using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : MonoBehaviour {

    public float speed = 8;

    protected Texture2D map;
    public Vector2 prevPositionInGrid, currentPositionInGrid;
    public Vector2 direction;
    bool initialized = false;

	void Start () {
        map = LabyrinthGenerator.instance.mapTexture;

        currentPositionInGrid = TransformPointToGrid(transform.position);

        List<Vector2> directionPosibilities = CheckSurroundings();
        direction = directionPosibilities[Random.Range(0, directionPosibilities.Count)];

        //currentPositionInGrid = TransformPointToGrid(transform.position - LabyrinthGenerator.instance.scale * ToVector3(direction) * 0.5f);
        prevPositionInGrid = currentPositionInGrid;
    }

	protected void Update () {
        transform.Translate(new Vector3(direction.x, 0, direction.y) * Time.fixedDeltaTime * Time.timeScale * speed, Space.World);

        currentPositionInGrid = TransformPointToGrid(transform.position - LabyrinthGenerator.instance.scale * ToVector3(direction) * 0.5f);
        if((int)currentPositionInGrid.x != (int)prevPositionInGrid.x || (int)currentPositionInGrid.y != (int)prevPositionInGrid.y)
        {
            List<Vector2> directionPosibilities = CheckSurroundings(direction);
            if (directionPosibilities.Count >= 1)
            {
                Vector2 newDirection = directionPosibilities[Random.Range(0, directionPosibilities.Count)];
                if(newDirection != direction)
                {
                    transform.position = TransformGridToPoint(currentPositionInGrid);
                    direction = newDirection;
                }
            }
            else if (directionPosibilities.Count == 0)
            {
                direction = -direction;
            }

            prevPositionInGrid = TransformPointToGrid(transform.position);
        }
    }

    protected List<Vector2> CheckSurroundings()
    {
        List<Vector2> emptyTiles = new List<Vector2>();
        Vector2 checkDirection = Vector2.up;
        for (int i = 0; i < 4; i++)
        {
            if(CheckTile(currentPositionInGrid + checkDirection))
            {
                emptyTiles.Add(checkDirection);
            }
            checkDirection = RotateVector(checkDirection);
        }
        return emptyTiles;
    }

    protected List<Vector2> CheckSurroundings(Vector2 currentDirection)
    {
        List<Vector2> emptyTiles = CheckSurroundings();
        emptyTiles.Remove(-currentDirection);

        //Debug.Log("Enemy: " + currentPositionInGrid);
        //Debug.Log("Player: " + TransformPointToGrid(LabyrinthMap.instance.player.position));
        //Debug.Log("CurrentEnemyDirection: " + direction);
        //Debug.Log("DirectionToTake: " + (TransformPointToGrid(LabyrinthMap.instance.player.position) - currentPositionInGrid).normalized);

        for (int i = 0; i < emptyTiles.Count; i++)
        {
            Vector2 testDirection = emptyTiles[i];
            if ((TransformPointToGrid(LabyrinthMap.instance.player.position ) - currentPositionInGrid).normalized == testDirection)
            {
                emptyTiles.Clear();
                emptyTiles.Add(testDirection);
            }
        }
        return emptyTiles;
    }

    protected bool CheckTile(Vector2 tile)
    {
        return map.GetPixel((int)tile.x, (int)tile.y).r == 1;        
    }

    protected Vector2 RotateVector(Vector2 vectorToRotate, int direction = 1)
    {
        Vector2 vector = Quaternion.Euler(0, 0, 90 * Mathf.Sign(direction)) * vectorToRotate;
        return new Vector2(Mathf.Round(vector.x), Mathf.Round(vector.y));
    }

    protected Vector3 ToVector3(Vector2 vector2)
    {
        return new Vector3(vector2.x, 0, vector2.y);
    }

    protected Vector2 TransformPointToGrid(Vector3 realPoint)
    {
        return TransformPointToGrid(new Vector2(realPoint.x, realPoint.z));
    }

    protected Vector2 TransformPointToGrid(Vector2 realPoint)
    {
        Vector2 point = realPoint / LabyrinthGenerator.instance.scale;
        point.x = Mathf.Round(point.x);
        point.y = Mathf.Round(point.y);
        return point;
    }

    protected Vector3 TransformGridToPoint(Vector2 gridPoint)
    {
        return TransformGridToPoint(new Vector3(gridPoint.x, 0, gridPoint.y));
    }

    protected Vector3 TransformGridToPoint(Vector3 gridPoint)
    {
        return (gridPoint * LabyrinthGenerator.instance.scale);
    }
}
