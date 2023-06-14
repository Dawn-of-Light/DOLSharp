using System.Numerics;

namespace DOL.GS
{
    static class Extensions
    {
        public static bool IsInRange(this Vector3 value, Vector3 target, float range)
        {
            // SH: Removed Z checks when one of the two Z values is zero(on ground)
            if (value.Z == 0 || target.Z == 0)
                return Vector2.DistanceSquared(value.ToVector2(), target.ToVector2()) <= range * range;
            return Vector3.DistanceSquared(value, target) <= range * range;
        }

        public static Vector2 ToVector2(this Vector3 value)
        {
            return new Vector2(value.X, value.Y);
        }
    }
}
