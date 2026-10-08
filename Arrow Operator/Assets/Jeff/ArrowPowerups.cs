using UnityEngine;

namespace ArrowOperator.Jeff
{
    [DisallowMultipleComponent]
    public sealed class ArrowPowerups : MonoBehaviour
    {
        public CountdownTimer timer;
        [Min(1f)] public float maximumSpeedMultiplier = 4f;
        public bool bounceWithShield;
        [Min(0f)] public float bounceDuration = 0.2f;

        public float SpeedSeconds { get; private set; }
        public float DirectionSeconds { get; private set; }
        public float ShieldSeconds { get; private set; }
        public bool HasShield => isActiveAndEnabled && ShieldSeconds > 0f;
        public Vector3 BoostDirection { get; private set; } = Vector3.forward;
        public Vector3 LastVelocity { get; private set; }
        float speedMultiplier = 1f, directionMultiplier = 1f, bounceSeconds;
        Vector3 bounceVelocity;

        public static bool Positive(float value) => value > 0f && !float.IsNaN(value) && !float.IsInfinity(value);

        void Update() => Advance(Time.deltaTime);

        public void Advance(float seconds)
        {
            if (!Positive(seconds)) return;
            SpeedSeconds = Mathf.Max(0f, SpeedSeconds - seconds);
            DirectionSeconds = Mathf.Max(0f, DirectionSeconds - seconds);
            ShieldSeconds = Mathf.Max(0f, ShieldSeconds - seconds);
            bounceSeconds = Mathf.Max(0f, bounceSeconds - seconds);
            if (SpeedSeconds == 0f) speedMultiplier = 1f;
            if (DirectionSeconds == 0f) directionMultiplier = 1f;
            if (ShieldSeconds == 0f) bounceSeconds = 0f;
        }

        public bool GiveSpeed(float multiplier, float duration)
        {
            if (!Positive(multiplier) || multiplier < 1f || !Positive(duration)) return false;
            speedMultiplier = multiplier;
            SpeedSeconds = duration;
            return true;
        }

        public bool GiveDirection(Vector3 direction, float multiplier, float duration)
        {
            if (!Positive(direction.sqrMagnitude) || !Positive(multiplier) || multiplier < 1f || !Positive(duration)) return false;
            BoostDirection = direction.normalized;
            directionMultiplier = multiplier;
            DirectionSeconds = duration;
            return true;
        }

        public bool GiveShield(float duration)
        {
            if (!Positive(duration)) return false;
            ShieldSeconds = duration;
            return true;
        }

        public bool GiveTime(float seconds) => timer != null && timer.TryAddTime(seconds);

        // Called once by the 3D motor, before applying velocity to its Rigidbody.
        public Vector3 ModifyVelocity(Vector3 velocity)
        {
            if (!isActiveAndEnabled) return velocity;
            if (velocity.sqrMagnitude < 0.000001f)
            {
                bounceSeconds = 0f;
                return LastVelocity = Vector3.zero;
            }
            if (bounceSeconds > 0f && HasShield) return LastVelocity = bounceVelocity;
            float cap = Positive(maximumSpeedMultiplier) ? Mathf.Max(1f, maximumSpeedMultiplier) : 4f;
            float speed = Mathf.Min(speedMultiplier, cap);
            float direction = Mathf.Min(directionMultiplier, cap);
            float along = Mathf.Max(0f, Vector3.Dot(velocity, BoostDirection));
            Vector3 boosted = (velocity + BoostDirection * along * (direction - 1f)) * speed;
            return LastVelocity = Vector3.ClampMagnitude(boosted, velocity.magnitude * cap);
        }

        public bool TryProtect(Vector3 contactNormal)
        {
            if (!HasShield) return false;
            if (bounceWithShield && bounceSeconds <= 0f && contactNormal.sqrMagnitude > 0.000001f)
            {
                contactNormal.Normalize();
                if (Vector3.Dot(LastVelocity, contactNormal) < 0f)
                {
                    bounceVelocity = Vector3.Reflect(LastVelocity, contactNormal);
                    bounceSeconds = Positive(bounceDuration) ? bounceDuration : 0f;
                }
            }
            return true;
        }

        public void Clear()
        {
            SpeedSeconds = DirectionSeconds = ShieldSeconds = bounceSeconds = 0f;
            speedMultiplier = directionMultiplier = 1f;
            LastVelocity = bounceVelocity = Vector3.zero;
        }

        void OnDisable() => Clear();
    }
}
