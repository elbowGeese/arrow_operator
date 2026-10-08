using System;

namespace ArrowOperator.Jeff
{
    // Engine-independent rules: repeat pickups refresh rather than multiply forever.
    public sealed class MovementAidState
    {
        public float SpeedSeconds { get; private set; }
        public float DirectionSeconds { get; private set; }
        public float ShieldSeconds { get; private set; }
        public float SpeedMultiplier { get; private set; } = 1f;
        public float DirectionMultiplier { get; private set; } = 1f;
        public bool HasShield => ShieldSeconds > 0f;

        public static bool Positive(float value) => value > 0f && !float.IsNaN(value) && !float.IsInfinity(value);

        public bool GrantSpeed(float multiplier, float seconds)
        {
            if (!Positive(seconds) || !Positive(multiplier) || multiplier < 1f) return false;
            SpeedMultiplier = multiplier;
            SpeedSeconds = seconds;
            return true;
        }

        public bool GrantDirection(float multiplier, float seconds)
        {
            if (!Positive(seconds) || !Positive(multiplier) || multiplier < 1f) return false;
            DirectionMultiplier = multiplier;
            DirectionSeconds = seconds;
            return true;
        }

        public bool GrantShield(float seconds)
        {
            if (!Positive(seconds)) return false;
            ShieldSeconds = seconds;
            return true;
        }

        public void Tick(float seconds)
        {
            if (!Positive(seconds)) return;
            SpeedSeconds = Math.Max(0f, SpeedSeconds - seconds);
            DirectionSeconds = Math.Max(0f, DirectionSeconds - seconds);
            ShieldSeconds = Math.Max(0f, ShieldSeconds - seconds);
            if (SpeedSeconds == 0f) SpeedMultiplier = 1f;
            if (DirectionSeconds == 0f) DirectionMultiplier = 1f;
        }

        public float GetMultiplier(float alignment)
        {
            if (float.IsNaN(alignment)) alignment = 0f;
            alignment = Math.Max(0f, Math.Min(1f, alignment));
            return SpeedMultiplier * (1f + (DirectionMultiplier - 1f) * alignment);
        }
    }
}
