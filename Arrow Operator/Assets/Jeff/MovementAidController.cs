using UnityEngine;

namespace ArrowOperator.Jeff
{
    [DisallowMultipleComponent]
    public sealed class MovementAidController : MonoBehaviour
    {
        [Tooltip("Optional. Time pickups are not consumed until a running timer is available.")]
        public CountdownTimer timer;
        public bool bounceWithShield = true;
        [Min(0f)] public float bounceDuration = 0.2f;
        [Min(1f)] public float maximumMultiplier = 4f;
        public MovementAidState State { get; } = new MovementAidState();
        public Vector3 BoostDirection { get; private set; } = Vector3.right;
        public Vector3 LastVelocity { get; private set; }
        Vector3 bounceVelocity;
        float bounceRemaining;

        void Update()
        {
            State.Tick(Time.deltaTime);
            bounceRemaining = Mathf.Max(0f, bounceRemaining - Time.deltaTime);
        }

        public bool GrantSpeed(float multiplier, float seconds) => State.GrantSpeed(multiplier, seconds);
        public bool GrantShield(float seconds) => State.GrantShield(seconds);

        public bool GrantDirection(Vector3 direction, float multiplier, float seconds)
        {
            if (!MovementAidState.Positive(direction.sqrMagnitude)) return false;
            if (!State.GrantDirection(multiplier, seconds)) return false;
            BoostDirection = direction.normalized;
            return true;
        }

        public bool GrantTime(float seconds) => timer != null && timer.TryAddTime(seconds);

        // Call exactly once from the movement owner. This component never moves the player each frame.
        public Vector3 ModifyVelocity(Vector3 baseVelocity)
        {
            if (baseVelocity.sqrMagnitude < 0.000001f)
            {
                LastVelocity = Vector3.zero;
                return Vector3.zero;
            }
            if (bounceRemaining > 0f)
                return LastVelocity = bounceVelocity;
            float alignment = Vector3.Dot(baseVelocity.normalized, BoostDirection);
            float multiplier = Mathf.Min(State.GetMultiplier(alignment), Mathf.Max(1f, maximumMultiplier));
            return LastVelocity = baseVelocity * multiplier;
        }

        // Called by the hazard BEFORE its normal death path. Works with existing trigger obstacles.
        public bool TryProtect(Collider hazard)
        {
            if (!isActiveAndEnabled || !State.HasShield || hazard == null) return false;
            if (!bounceWithShield) return true;

            Collider body = GetComponent<Collider>();
            Vector3 normal = transform.position - hazard.ClosestPoint(transform.position);
            float distance = 0f;
            if (body != null && Physics.ComputePenetration(body, body.transform.position, body.transform.rotation,
                hazard, hazard.transform.position, hazard.transform.rotation, out Vector3 separation, out distance))
                normal = separation;
            if (normal.sqrMagnitude < 0.000001f) normal = -LastVelocity;
            if (normal.sqrMagnitude < 0.000001f) normal = Vector3.up;
            normal.Normalize();

            // Exit the overlap so expiry inside a wall cannot silently bypass the hazard.
            Vector3 position = transform.position + normal * (distance + 0.05f);
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null) rb.position = position;
            transform.position = position;
            bounceVelocity = Vector3.Dot(LastVelocity, normal) < 0f
                ? Vector3.Reflect(LastVelocity, normal) : LastVelocity;
            bounceRemaining = bounceDuration;
            return true;
        }
    }
}
