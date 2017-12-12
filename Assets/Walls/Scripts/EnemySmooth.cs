using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySmooth : EnemyBase {

    public enum State { Normal, Turning };
    [HideInInspector]public State state = State.Normal;

    Vector2 moveDirection;

    new void Update()
    {
        switch (state)
        {
            case State.Normal:
                NormalUpdate();
                break;

            case State.Turning:
                Turn();
                break;

            default:
                break;
        }
    }

    void NormalUpdate()
    {
        //transform.Translate(new Vector3(direction.x, 0, direction.y) * Time.fixedDeltaTime * Time.timeScale * speed, Space.World);

        transform.Translate(Vector3.forward * Time.fixedDeltaTime * Time.timeScale * speed);

        currentPositionInGrid = TransformPointToGrid(transform.position);
        if ((int)currentPositionInGrid.x != (int)prevPositionInGrid.x || (int)currentPositionInGrid.y != (int)prevPositionInGrid.y)
        {
            List<Vector2> directionPosibilities = CheckSurroundings(direction);
            if (directionPosibilities.Count >= 1)
            {
                Vector2 newDirection = directionPosibilities[Random.Range(0, directionPosibilities.Count)];
                if (newDirection != direction)
                {
                    //transform.position = TransformGridToPoint(currentPositionInGrid);
                    direction = newDirection;
                }
            }
            else if (directionPosibilities.Count == 0)
            {
                direction = -direction;
                state = State.Turning;
            }

            prevPositionInGrid = TransformPointToGrid(transform.position);
        }

        float turnSpeed = Time.fixedDeltaTime * Time.timeScale * speed * (1.5f / LabyrinthGenerator.instance.scale) * 1.5f;
        moveDirection = new Vector2(Mathf.Lerp(moveDirection.x, direction.x, turnSpeed), Mathf.Lerp(moveDirection.y, direction.y, turnSpeed));

        transform.rotation = Quaternion.LookRotation(ToVector3(moveDirection));
    }

    void Turn()
    {
        //Advance to the center of the tile
        if (transform.position != TransformGridToPoint(currentPositionInGrid))
        {
            transform.position = Vector3.MoveTowards(transform.position, TransformGridToPoint(currentPositionInGrid), Time.fixedDeltaTime * Time.timeScale * speed);
        }
        //Stop and turn
        else if (transform.rotation != Quaternion.LookRotation(ToVector3(direction)))
        {
            float turnSpeed = Time.fixedDeltaTime * Time.timeScale * speed * LabyrinthGenerator.instance.scale * Mathf.Rad2Deg;
            //moveDirection = new Vector2(Mathf.Lerp(moveDirection.x, direction.x, turnSpeed), Mathf.Lerp(moveDirection.y, direction.y, turnSpeed));
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(ToVector3(direction)), turnSpeed);
        }
        //There, done, fine
        else {
            state = State.Normal;
            moveDirection = direction;
        }
    }
}
