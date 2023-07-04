using UnityEngine;

public static class CompassDirectionUtil
{
    public static Vector3 CompassDirectionToVector(CompassDirection direction)
    {
        switch (direction)
        {
            case CompassDirection.North:
                return Vector3.up;
            case CompassDirection.East:
                return Vector3.right;
            case CompassDirection.South:
                return Vector3.down;
            case CompassDirection.West:
                return Vector3.left;
            default:
                return Vector3.zero;
        }
    }

    public static Vector3Int CompassDirectionToIntVector(CompassDirection direction)
    {
        switch (direction)
        {
            case CompassDirection.North:
                return Vector3Int.up;
            case CompassDirection.East:
                return Vector3Int.right;
            case CompassDirection.South:
                return Vector3Int.down;
            case CompassDirection.West:
                return Vector3Int.left;
            default:
                return Vector3Int.zero;
        }
    }
}
