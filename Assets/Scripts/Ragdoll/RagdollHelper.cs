using UnityEngine;

namespace Ragdoll
{
	public struct HitInfo
	{
		public RagdollBone bone;
		public float impulse;
		public Vector3 hit_point;
		public Vector3 hit_direction;
		public bool inherited;
	}
}
