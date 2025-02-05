using UnityEngine;

namespace Ragdoll
{
	/// <summary>
	/// Handles the movement and animation of the Ragdoll character.
	/// Provides public properties to check character states from other scripts.
	/// </summary>
	public class RagdollMovement : MonoBehaviour
	{
		[Header("Movement Variables")]
		[SerializeField] float speed = 5f;

		[Header("Ragdoll Variables")]
		[SerializeField] Animator anim;
		[SerializeField] Transform camArm;

		Rigidbody rb;
		RagdollHealth health;
		
		bool grounded;
		bool crawling;

		// Animator Hashes
		static readonly int RunningProp = Animator.StringToHash("running");
		static readonly int GroundedProp = Animator.StringToHash("grounded");
		static readonly int JumpProp = Animator.StringToHash("jump");
		static readonly int YVelProp = Animator.StringToHash("y_vel");
		static readonly int CrawlProp = Animator.StringToHash("crawl");

		public bool IsRunning { get; set; }
		public bool IsGrounded => grounded;
		public bool IsCrawling => crawling;
		public float VerticalVelocity => rb.linearVelocity.y;

		void Start()
		{
			rb = GetComponent<Rigidbody>();
			health = GetComponent<RagdollHealth>();
			
			health.OnEnterCrawl += OnEnterCrawl;
		}

		void Update()
		{
			HandleMovement();
			HandleJump();
			HandleCrawlState();
		}

		void FixedUpdate()
		{
			UpdateGroundedState();
			UpdateAnimatorValues();
		}

		/// <summary>
		/// Handles movement input and updates the animator.
		/// </summary>
		void HandleMovement()
		{
			float move = Input.GetAxisRaw("Vertical");
			float strafe = Input.GetAxisRaw("Horizontal");

			Vector3 moveDir = camArm.forward * move + camArm.right * strafe;
			moveDir.y = 0;

			if (move != 0 || strafe != 0)
			{
				Vector3 velocity = rb.linearVelocity;
				Vector3 horizontalVelocity = moveDir * speed;
				rb.linearVelocity = new Vector3(horizontalVelocity.x, velocity.y, horizontalVelocity.z);

				IsRunning = true;
				anim.SetBool(RunningProp, true);
				anim.transform.forward = moveDir.normalized;
			}
			else
			{
				IsRunning = false;
				anim.SetBool(RunningProp, false);
			}
		}

		/// <summary>
		/// Handles jump input and animation triggers.
		/// </summary>
		void HandleJump()
		{
			if (Input.GetKeyDown(KeyCode.Space) && grounded && !crawling)
			{
				rb.AddForce(Vector3.up * 5, ForceMode.Impulse);
				anim.SetBool(GroundedProp, false);
				anim.SetTrigger(JumpProp);
			}
		}

		/// <summary>
		/// Updates the grounded state using a raycast.
		/// </summary>
		void UpdateGroundedState()
		{
			grounded = Physics.Raycast(transform.position, Vector3.down, 0.55f);
			anim.SetBool(GroundedProp, grounded);
		}

		/// <summary>
		/// Updates animator values like vertical velocity.
		/// </summary>
		void UpdateAnimatorValues()
		{
			anim.SetFloat(YVelProp, rb.linearVelocity.y);
		}
		
		/// <summary>
		/// Handles the event when the character enters the crawl state.
		/// </summary>
		void OnEnterCrawl()
		{
			anim.SetTrigger(CrawlProp);
			crawling = true;
			speed = 2;
		}
		
		/// <summary>
		/// Handles the crawl state.
		/// </summary>
		void HandleCrawlState()
		{
			if (!crawling) return;

			bool shouldFollow = IsRunning;
			if (health.Pelvis.IsActive != shouldFollow)
			{
				if (!health.IsDead)
					health.Pelvis.SetBone(shouldFollow);
			}
		}

	}
}