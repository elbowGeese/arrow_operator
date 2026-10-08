using UnityEngine;

namespace ArrowOperator.Jeff
{
    public enum PowerupKind { Speed, DirectionalSpeed, Shield, ExtraTime }

    [RequireComponent(typeof(Collider))]
    public sealed class PowerupPickup : MonoBehaviour
    {
        public PowerupKind kind;
        [Min(1f)] public float multiplier = 1.8f;
        [Min(0.1f)] public float duration = 8f;
        [Min(0.1f)] public float extraSeconds = 10f;
        [Tooltip("The pickup's local +Z axis defines the boost direction in world space.")]
        public Transform directionSource;
        bool collected;

        void Reset() => GetComponent<Collider>().isTrigger = true;
        void Awake() => GetComponent<Collider>().isTrigger = true;
        void OnEnable() => collected = false;

        public bool TryCollect(ArrowPowerups player)
        {
            if (collected || !isActiveAndEnabled || Time.timeScale <= 0f || player == null || !player.isActiveAndEnabled) return false;
            bool granted;
            switch (kind)
            {
                case PowerupKind.Speed: granted = player.GiveSpeed(multiplier, duration); break;
                case PowerupKind.DirectionalSpeed: granted = player.GiveDirection((directionSource != null ? directionSource : transform).forward, multiplier, duration); break;
                case PowerupKind.Shield: granted = player.GiveShield(duration); break;
                case PowerupKind.ExtraTime: granted = player.GiveTime(extraSeconds); break;
                default: return false;
            }
            if (!granted) return false;
            collected = true;
            gameObject.SetActive(false);
            return true;
        }

        void OnTriggerEnter(Collider other) => TryCollect(other.GetComponentInParent<ArrowPowerups>());
    }
}
