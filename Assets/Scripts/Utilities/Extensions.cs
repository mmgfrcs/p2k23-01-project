using UnityEngine;

public static class Extensions
{
    public static Vector3Int ToVector3Int(this Vector2Int v2)
    {
        return new Vector3Int(v2.x, v2.y, 0);
    }
    
    public static Vector2Int ToVector2Int(this Vector3Int v3)
    {
        return new Vector2Int(v3.x, v3.y);
    }
    
    public static Vector3 ToVector3(this Vector3Int v3)
    {
        return new Vector3(v3.x, v3.y, v3.z);
    }
}
