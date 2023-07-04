using UnityEngine;

public static class EasingFunctions
{

    public static float EaseOutCircle(float x)
    {
        return Mathf.Sqrt(1 - Mathf.Pow(x - 1, 2));
    }
}
