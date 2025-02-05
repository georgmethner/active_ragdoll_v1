// Optimized RagdollBone.cs
using System;
using UnityEngine;

namespace Ragdoll
{
    public class RagdollBone : MonoBehaviour
    {
        Rigidbody _rb;
        Transform _transform;
        float _linearDamping;
        
        [SerializeField] Transform anim;
        [SerializeField] bool follow = true;
        [SerializeField] bool vital;

        [Header("Force Multipliers")]
        [SerializeField] float positionForceMultiplierMax = 300f;
        [SerializeField] float rotationTorqueMultiplierMax = 20f;
        [SerializeField] float positionForceVerticalMultiplier = 1;
        [SerializeField] float maxForce = 500f;
        [SerializeField] float maxTorque = 500f;
        
        public event Action<bool> OnBoneSet;
        public event Action<RagdollBone> OnBoneBreak;
        public event Action<HitInfo> OnBoneHit;
        
        public bool IsBroken { get; set; }
        public bool IsVital => vital;
        public bool IsActive => follow;
        
        GameObject bloodParticles;

        float _positionForceMultiplier;
        float _rotationTorqueMultiplier;

        void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _transform = transform;
            _linearDamping = _rb.linearDamping;
            
            _positionForceMultiplier = positionForceMultiplierMax;
            _rotationTorqueMultiplier = rotationTorqueMultiplierMax;
            
            InitializeChildren();
        }

        void FixedUpdate()
        {
            if (follow)
                ApplyBoneTransform();
        }
        
        void InitializeChildren()       // Initialize all children of the bone
        {
            foreach (Transform child in transform)
            {
                RagdollBone childBone = child.GetComponentInChildren<RagdollBone>();
                if (childBone != null)
                {
                    OnBoneSet += childBone.SetBone;
                    OnBoneHit += childBone.HitBone;
                }
            }
        }

        void ApplyBoneTransform()
        {
            Vector3 positionDelta = anim.position - _rb.position;
            positionDelta.y *= positionForceVerticalMultiplier;
            Vector3 positionForce = Vector3.ClampMagnitude(positionDelta * _positionForceMultiplier, maxForce);
            _rb.AddForce(positionForce);

            Quaternion rotationDelta = anim.rotation * Quaternion.Inverse(_rb.rotation);
            rotationDelta.ToAngleAxis(out float angle, out Vector3 axis);
            if (angle > 180f) angle -= 360f;
            
            if (!float.IsNaN(angle))
            {
                Vector3 torque = Vector3.ClampMagnitude(axis * (angle * Mathf.Deg2Rad * _rotationTorqueMultiplier), maxTorque);
                _rb.AddTorque(torque);
            }
        }

        public void SetBone(bool isActive = false)
        {
            if (IsBroken) return;
            follow = isActive;
            _rb.linearDamping = isActive ? _linearDamping : 0;
            OnBoneSet?.Invoke(isActive);
        }

        public virtual void HitBone(HitInfo hitInfo)
        {
            if (hitInfo.bone == null) return;
            Rigidbody rb = hitInfo.bone.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.mass = Mathf.Min(rb.mass + Mathf.Clamp(hitInfo.impulse * 0.005f, 0.5f, 5f), 20f);
            }

            float impulseFactor = Mathf.Clamp(hitInfo.impulse / 100, 0.1f, 1f);
            _positionForceMultiplier = Mathf.Max(_positionForceMultiplier * (0.85f + impulseFactor * 0.15f), positionForceMultiplierMax * 0.15f);
            _rotationTorqueMultiplier *= (0.85f + impulseFactor * 0.15f);
            positionForceVerticalMultiplier = Mathf.Max(positionForceVerticalMultiplier - (hitInfo.impulse * 0.01f), 0.3f);
            
            CharacterJoint joint = GetComponent<CharacterJoint>();
            if (joint != null && !hitInfo.inherited)
            {
                joint.breakForce = Mathf.Lerp(joint.breakForce, 0, impulseFactor);
                hitInfo.inherited = true;
            }

            OnBoneHit?.Invoke(hitInfo);
        }

        void OnJointBreak(float breakForce)
        {
            if (bloodParticles == null) return;
            Vector3 spawnPosition = _transform.position;
            GameObject bloodInstance = Instantiate(bloodParticles, spawnPosition, Quaternion.identity);
            
            if (transform.parent)
            {
                bloodInstance.transform.parent = transform.parent;
            }
            
            Destroy(bloodInstance, 10f);

            OnBoneBreak?.Invoke(this);
            SetBone();
            IsBroken = true;
        }

        void OnCollisionEnter(Collision other)
        {
            if (other.impulse.magnitude > 10f && other.gameObject.layer != LayerMask.NameToLayer("Ragdoll"))
            {
                HitBone(new HitInfo
                {
                    bone = this,
                    impulse = other.impulse.magnitude,
                    hit_point = other.contacts[0].point,
                    hit_direction = other.contacts[0].normal
                });
            }
        }

        public void SetBoneParams(GameObject _bloodParticles) => bloodParticles = _bloodParticles;
    }
}
