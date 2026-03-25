using UnityEngine;

/// <summary>
/// Deterministic seed-based value generation.
/// Adapted from Infinity Square Space PositionSeed() concept.
/// Given a position and seed, returns a repeatable pseudo-random value.
/// </summary>
public static class SeedUtility
{
    public static float ValueAt(Vector3 position, int seed, float factor = 1f)
    {
        float combined = position.x * 73.137f + position.y * 27.941f + position.z * 51.413f + seed * 17.31f;
        float raw = Mathf.Abs(Mathf.Cos(combined * factor));
        return raw;
    }

    public static int IntAt(Vector3 position, int seed, int max)
    {
        if (max <= 0) return 0;
        float val = ValueAt(position, seed, 1.73f);
        return Mathf.FloorToInt(val * max) % max;
    }

    public static bool ChanceAt(Vector3 position, int seed, float probability)
    {
        return ValueAt(position, seed, 2.17f) < probability;
    }
}
