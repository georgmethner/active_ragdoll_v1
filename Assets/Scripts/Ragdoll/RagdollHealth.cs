// Optimized RagdollHealth.cs
using System;
using UnityEngine;

namespace Ragdoll
{
	public class RagdollHealth : MonoBehaviour
	{
		[SerializeField] GameObject bloodParticles;
		[SerializeField] GameObject bloodDecal;
        
		[HideInInspector]
		public RagdollBone Pelvis;

		public bool IsDead { get; set; }
		
		public event Action OnEnterCrawl;

		void Awake()
		{
			SubscribeToAllBones();
		}

		void SubscribeToAllBones()
		{
			RagdollBone[] bones = GetComponentsInChildren<RagdollBone>();
			Pelvis = bones.Length > 0 ? bones[0] : null;
            
			foreach (RagdollBone bone in bones)
			{
				bone.SetBoneParams(bloodParticles);
				bone.OnBoneBreak += HandleBoneBreak;
				bone.OnBoneHit += HandleBoneHit;
			}
		}

		void HandleBoneBreak(RagdollBone bone)
		{
			if (bone.IsVital)
			{
				Pelvis.SetBone(false);
				IsDead = true;
			}
			else if (bone is LegBone)
			{
				OnEnterCrawl?.Invoke();
			}
		}

		void HandleBoneHit(HitInfo hitInfo)
		{
			GameObject decal = Instantiate(bloodDecal, hitInfo.hit_point, Quaternion.LookRotation(hitInfo.hit_direction));
			decal.transform.SetParent(hitInfo.bone.transform);
		}
	}
}