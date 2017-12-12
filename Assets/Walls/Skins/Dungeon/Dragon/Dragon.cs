using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dragon : MonoBehaviour {

    public EnemySmooth enemy;
    public Transform head;
    public AnimationCurve widthCurve;
    public float height = 1.5f;

    int vertexNumber = 8;
    MeshHelper mesh = new MeshHelper();

    struct Point2
    {
        Vector2 _position;
        Vector2 _direction;
        float _angle;
        public float size;
        //Constructors
        public Point2(Vector2 position)
        {
            _position = position;
            _direction = Vector2.up;
            _angle = GetAngle(_direction);
            size = 1;
        }

        public Point2(Vector2 position, Vector2 direction)
        {
            _position = position;
            _direction = direction;
            _angle = GetAngle(_direction);
            size = 1;
        }
        //Setters/Getters
        public Vector2 position
        {
            set { _position = value;}
            get { return _position; }
        }

        public Vector2 direction
        {
            set { _direction = value; _angle = GetAngle(_direction); }
            get { return _direction; }
        }

        public float angle
        {
            set { _angle = Mathf.Round(value) % 360;}
            get { return _angle; }
        }

        public void RoundAngle(Vector2 prevDirection)
        {
            angle = GetAngle(prevDirection + direction);
        }

        //Helper functions
        public static float GetAngle(Vector2 direction)
        {
            float angle = Vector2.Angle(Vector2.up, direction) * Mathf.Sign(direction.x);
            if (angle < 0) angle += 360;
            return angle;
        }

        public static float GetAngle(Vector2 firstPoint, Vector2 secondPoint)
        {
            return GetAngle(GetDirection(firstPoint, secondPoint));
        }

        public static Vector2 GetDirection(Vector2 firstPoint, Vector2 secondPoint)
        {
            return secondPoint - firstPoint;
        }

        public static Vector2 GetDirection(float angle)
        {
            return Quaternion.Euler(0, 0, angle) * Vector2.up;
        }

        public static Vector2 RoundVector(Vector2 vectorToRound)
        {
            vectorToRound.x = Mathf.RoundToInt(vectorToRound.x);
            vectorToRound.y = Mathf.RoundToInt(vectorToRound.y);
            return vectorToRound;
        }
    }

    Vector2 prevPositionInGrid, currentPositionInGrid;
    int maxControlPoints = 3;
    List<Point2> controlPoints = new List<Point2>();

    Vector3 prevWall, nextWall;
    float betweenWallsDistance;

    bool moved = false;
    bool turning = false;

    //Program ---------------------------------------------------------------------
    void Start () {
        GetComponent<MeshFilter>().sharedMesh = mesh.CreateMesh();

        currentPositionInGrid = GridF.TransformPointToGrid(transform.position);
        prevPositionInGrid = currentPositionInGrid;

        nextWall = GetGridWall(currentPositionInGrid, enemy.direction);
        prevWall = transform.position;

        Point2 newPoint = new Point2(prevPositionInGrid, enemy.direction);
        for(int i =0; i < maxControlPoints; i++) AddPoint(newPoint);       
    }

	void Update () {
        //Always (before change)

        //When changes tile update information
        currentPositionInGrid = GridF.TransformPointToGrid(transform.position);
        if (currentPositionInGrid != prevPositionInGrid)
        {       
            Point2 newPoint = new Point2(prevPositionInGrid, Point2.GetDirection(prevPositionInGrid, currentPositionInGrid));
            if (controlPoints.Count > 1) newPoint.RoundAngle(controlPoints[controlPoints.Count - 1].direction);
            AddPoint(newPoint);
            ChangeTile();
        }
        //Always
        Draw();
        /*if (Input.GetKeyDown(KeyCode.Space))
        {
            foreach(Point2 point in controlPoints)
            {
                GameObject newObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                newObject.transform.position = GridF.TransformGridToPoint(point.position);
                newObject.transform.rotation = Quaternion.Euler(0, point.angle, 0);
            }
        }*/
	}

    void ChangeTile()
    {
        moved = true;
        prevPositionInGrid = currentPositionInGrid;
        prevWall = nextWall;
        if (enemy.state != EnemySmooth.State.Turning) nextWall = GetGridWall(currentPositionInGrid, enemy.direction, head.position.y);
        else { nextWall = GetGridWall(currentPositionInGrid, -enemy.direction, head.position.y);  turning = true; }
        betweenWallsDistance = (prevWall - nextWall).magnitude;
        betweenWallsDistance = betweenWallsDistance == 0? 1f : betweenWallsDistance; //to avoid dividing by 0
        
    }

    void Draw()
    {
        //Update head
        {
            Vector3 headPosition = head.position;
            headPosition.y = CalculateHeight(head.position);
            head.position = headPosition;
        }

        mesh.Clear();
        if (controlPoints.Count > 0)
        {
            float addedDistance = 0;

            if (moved)
            {
                addedDistance = ((head.transform.position - prevWall).magnitude / betweenWallsDistance);
                if (turning && enemy.state != EnemySmooth.State.Turning) { 
                    turning = false; ChangeTile();
                }
            }

            List<int[]> rings = new List<int[]>();
            
            if (controlPoints.Count == maxControlPoints)
            {
                {
                    Point2 point = controlPoints[0];
                    Vector3 realPointLocalPosition = (GridF.TransformGridToPoint(point.position) + GridF.Direction2ToDirection3(point.direction) * addedDistance * LabyrinthGenerator.instance.scale);
                    realPointLocalPosition += CalculateHeight(realPointLocalPosition) * Vector3.up - transform.position;

                    float width = widthCurve.Evaluate(0);                    
                    rings.Add(mesh.AddArray(CreateRing(width, realPointLocalPosition, point.angle), 0));

                    if (addedDistance < 0.5f)
                    {
                        Point2 point2 = controlPoints[1];
                        float distanceToHead = 0.5f - addedDistance;
                        realPointLocalPosition = (GridF.TransformGridToPoint(point.position) + CalculateHeight(GridF.TransformGridToPoint(point.position)) * Vector3.up) - transform.position;
                        Vector3 realPointLocalPosition2 = (GridF.TransformGridToPoint(point2.position) + Vector3.up * CalculateHeight(GridF.TransformGridToPoint(point2.position))) - transform.position;

                        width = widthCurve.Evaluate(distanceToHead / (controlPoints.Count - 0.5f));                    
                        rings.Add(mesh.AddArray(CreateRing(width, (realPointLocalPosition + realPointLocalPosition2)/ 2, (point.angle + point2.angle)/2), distanceToHead));
                    }
                }

                for (int i = 1; i < controlPoints.Count; i++)
                {
                    Point2 point = controlPoints[i];
                    float distanceToHead = i - addedDistance;
                    Vector3 realPointLocalPosition = (GridF.TransformGridToPoint(point.position) + CalculateHeight(GridF.TransformGridToPoint(point.position)) * Vector3.up) - transform.position;

                    float width = widthCurve.Evaluate(distanceToHead / (controlPoints.Count - 0.5f));
                    rings.Add(mesh.AddArray(CreateRing(width, realPointLocalPosition, point.angle), distanceToHead));
                    //rings.Add(mesh.AddArray(CreateRing(1, realPointLocalPosition + (new Vector3(point.direction.x, 0, point.direction.y) * addedDistance), point.angle)));

                    if (i == controlPoints.Count - 1)
                    {
                        distanceToHead = i + 0.5f - addedDistance;
                        /*Quaternion headAngle = head.rotation * Quaternion.Euler(0,point.angle,0);
                        while (Mathf.Abs(headAngle - point.angle) > 360)
                        {
                            if (headAngle < point.angle) headAngle += 360;
                            else headAngle -= 360;
                        }
                        Debug.Log(headAngle.eulerAngles.y);*/
                        width = widthCurve.Evaluate(distanceToHead / (controlPoints.Count - 0.5f));
                        realPointLocalPosition = (realPointLocalPosition + transform.InverseTransformPoint(head.position)) / 2;
                        rings.Add(mesh.AddArray(CreateRing(width, realPointLocalPosition, point.angle), distanceToHead));

                        distanceToHead -= 0.25f;
                        width = widthCurve.Evaluate(distanceToHead / (controlPoints.Count - 0.5f));
                        rings.Add(mesh.AddArray(CreateRing(width, ((realPointLocalPosition + transform.InverseTransformPoint(head.position)) / 2), head.rotation), distanceToHead));
                    }

                    else {
                        distanceToHead = i + 0.5f - addedDistance;
                        Point2 point2 = controlPoints[i + 1];
                        Vector3 realPointLocalPosition2 = (GridF.TransformGridToPoint(point2.position) + CalculateHeight(GridF.TransformGridToPoint(point2.position)) * Vector3.up) - transform.position;
                        width = widthCurve.Evaluate(distanceToHead / (controlPoints.Count - 0.5f));
                        rings.Add(mesh.AddArray(CreateRing(width, (realPointLocalPosition + realPointLocalPosition2)/2, point.angle), distanceToHead));
                    }
                }
            }
            //rings.Add(mesh.AddArray(CreateRing(widthCurve.Evaluate(1), transform.InverseTransformPoint(head.position), head.rotation), maxControlPoints - 0.5f));

            for (int i = 1; i < rings.Count; i++)
            {
                mesh.ConectRings(rings[i], rings[i - 1]);
            }

            mesh.Cap(rings[0]);
            mesh.Cap(rings[rings.Count - 1], true);

            mesh.UpdateMesh();
        }
    }

    float CalculateHeight(Vector3 position)
    {
        position = position / LabyrinthGenerator.instance.scale;
        return (Mathf.Sin(position.x) + Mathf.Cos(position.z)) / 4 + height;
    }

    Vector3 GetGridWall(Vector2 gridPosition, Vector2 direction, float height = 0)
    {
        Vector3 toReturn = GridF.TransformGridToPoint(gridPosition) + GridF.Direction2ToDirection3(direction * 0.5f * LabyrinthGenerator.instance.scale);
        toReturn.y = height;
        return toReturn;
    }

    /*Vector2 NearestPointOnLine(Vector2 linePnt, Vector2 lineDir, Vector2 pnt)
    {
        lineDir.Normalize();
        Vector2 v = pnt - linePnt;
        float d = Vector2.Dot(v, lineDir);
        return linePnt + lineDir * d;
    }*/

    void AddPoint(Point2 newPoint)
    {
        if (controlPoints.Count >= maxControlPoints) controlPoints.RemoveAt(0);
        controlPoints.Add(newPoint);
    }

    //RING CREATION -------------------------------------------------------------------------------------
    Vector3[] CreateRing(Vector3 offset)
    {
        return CreateRing(1, offset, Quaternion.identity);
    }

    Vector3[] CreateRing(float radius, Vector3 offset)
    {
        return CreateRing(vertexNumber, radius, offset, Quaternion.identity);
    }

    Vector3[] CreateRing(float radius, Vector3 offset, Quaternion rotation)
    {
        float angle = rotation.eulerAngles.y;
        angle = angle < 0? angle + 360 : angle;
        return CreateRing(radius, offset, Mathf.Round(angle));
    }

    Vector3[] CreateRing(float radius, Vector3 offset, float rotation)
    {
        return CreateRing(vertexNumber, radius, offset, Quaternion.Euler(0,rotation,0));
    }

    Vector3[] CreateRing(int ringVertexNumber, float radius, Vector3 offset, Quaternion rotation)
    {
        Vector3[] newRing = new Vector3[ringVertexNumber];
        float increment = 2 * Mathf.PI / ringVertexNumber;
        float angle = 0;

        for (int i = 0; i < ringVertexNumber; i++)
        {
            newRing[i] = rotation * new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0) + offset;
            angle += increment;
        }

        return newRing;
    }
}
