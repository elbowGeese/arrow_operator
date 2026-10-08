using UnityEngine;

namespace ArrowOperator.Jeff
{
    public enum MovementAidKind { Speed, DirectionalSpeed, Shield, ExtraTime }

    [RequireComponent(typeof(Collider))]
    public sealed class MovementAidPickup : MonoBehaviour
    {
        public MovementAidKind kind;
        [Min(1f)] public float multiplier = 1.8f;
        [Min(0.1f)] public float duration = 6f;
        [Min(0.1f)] public float extraSeconds = 10f;
        [Tooltip("World-space direction. A right-pointing pickup boosts +X movement.")]
        public Vector3 direction = Vector3.right;
        bool collected;

        void Reset() => GetComponent<Collider>().isTrigger = true;

        public bool TryCollect(MovementAidController player)
        {
            if (collected || !isActiveAndEnabled || Time.timeScale <= 0f || player == null || !player.isActiveAndEnabled)
                return false;
            bool granted;
            switch (kind)
            {
                case MovementAidKind.Speed: granted = player.GrantSpeed(multiplier, duration); break;
                case MovementAidKind.DirectionalSpeed: granted = player.GrantDirection(direction, multiplier, duration); break;
                case MovementAidKind.Shield: granted = player.GrantShield(duration); break;
                case MovementAidKind.ExtraTime: granted = player.GrantTime(extraSeconds); break;
                default: return false;
            }
            if (!granted) return false;
            collected = true; // Guard compound colliders in the same physics step.
            gameObject.SetActive(false);
            return true;
        }

        void OnTriggerEnter(Collider other) => TryCollect(other.GetComponentInParent<MovementAidController>());
    }
}
