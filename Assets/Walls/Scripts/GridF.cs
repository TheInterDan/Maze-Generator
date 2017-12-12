using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GridF {

    static public bool CheckTile(Vector2 tile)
    {
        return CheckTile((int)tile.x, (int)tile.y);
    }

    static public bool CheckTile(int x, int y)
    {
        return LabyrinthGenerator.instance.mapTexture.GetPixel(x, y).r == 1;
    }

    static public Vector2 RotateVector(Vector2 vectorToRotate, int direction = 1)
    {
        Vector2 vector = Quaternion.Euler(0, 0, 90 * Mathf.Sign(direction)) * vectorToRotate;
        return new Vector2(Mathf.Round(vector.x), Mathf.Round(vector.y));
    }

    static public Vector3 ToVector3(Vector2 vector2)
    {
        return new Vector3(vector2.x, 0, vector2.y);
    }

    static public Vector2 TransformPointToGrid(Vector3 realPoint)
    {
        return TransformPointToGrid(new Vector2(realPoint.x, realPoint.z));
    }

    static public Vector2 TransformPointToGrid(Vector2 realPoint)
    {
        Vector2 point = realPoint / LabyrinthGenerator.instance.scale;
        point.x = Mathf.Round(point.x);
        point.y = Mathf.Round(point.y);
        return point;
    }

    static public Vector3 TransformGridToPoint(Vector2 gridPoint)
    {
        return TransformGridToPoint(new Vector3(gridPoint.x, 0, gridPoint.y));
    }

    static public Vector3 TransformGridToPoint(Vector3 gridPoint)
    {
        return (gridPoint * LabyrinthGenerator.instance.scale);
    }

    static public Vector3 Direction2ToDirection3(Vector2 direction)
    {
        return new Vector3(direction.x, 0, direction.y);
    }
}