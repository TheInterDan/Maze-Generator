using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ivy : MonoBehaviour {

    public int vertexNumber = 16;
    public int testRadius = 1;

    public GameObject flower;
    GameObject nextFlower;

    public AnimationCurve growRate, unGrowRate;

    LabyrinthGenerator generatorInstance;
    float scale = 1;

    Vector2 positionInGrid;

    int[] ring1, ring2, ring3, ring4, ring5;
    bool[,] grid = new bool[3, 3];
    int[,] pointsGrid = new int[5,5];
    List<List<Vector2>> walls = new List<List<Vector2>>();  

    MeshHelper mesh = new MeshHelper();

    bool flowerGrowing = false;
    float prevFlowerGrowingDistance = 0;

	void Start () {
        /*Vector3[] ring = CreateRing();

        for(int i = 0; i < ring.Length; i++)
        {
            GameObject newGameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            newGameObject.transform.SetParent(transform);
            newGameObject.transform.localPosition = ring[i];
        }*/

        /*ring1 = mesh.AddArray(CreateRing(Vector3.zero));
        ring2 = mesh.AddArray(CreateRing(Vector3.forward));
        mesh.ConectRings(ring1, ring2);*/

        GetComponent<MeshFilter>().sharedMesh = mesh.CreateMesh();

        generatorInstance = LabyrinthGenerator.instance;
        scale = generatorInstance.scale;
        flower = Instantiate(flower);
        nextFlower = Instantiate(flower);

        positionInGrid = GridF.TransformPointToGrid(transform.position);
        GetSurroundings();
        GetChains();       
    }

    void Draw(float radius = 0.8f, float sinOffset = 0, float sinOffsetMultiplicator = 1, float amplitude = 0.5f)
    {
        //float refScale = 0.1f;
        List<int[]> rings = new List<int[]>();
        

        Vector3 gridOffset = GridF.TransformGridToPoint(positionInGrid) - transform.position;
        gridOffset.y = 0;

        foreach (List<Vector2> thread in walls)
        {
            foreach (Vector2 point in thread)
            {
                int pointX = (int)point.x;
                int pointY = (int)point.y;

                Vector2 transformOffset = ((point + Vector2.one) / 2 - Vector2.one * 1.5f);
                Vector3 offset = new Vector3(transformOffset.x, 0, transformOffset.y);
                float y = Mathf.Sin((offset.x + sinOffset * sinOffsetMultiplicator + positionInGrid.x) * 2 ) * amplitude 
                        + Mathf.Cos((offset.z - sinOffset * sinOffsetMultiplicator + positionInGrid.y) * 2 ) * amplitude;

                Vector3 position = offset * scale + gridOffset + Vector3.up * y;
                float distance = (transform.position - transform.TransformPoint(position)).magnitude;
                distance = Mathf.Clamp((scale - 0.15f * scale) - distance, 0, scale);

                rings.Add(mesh.AddArray(CreateRing(radius * distance, position, Quaternion.Euler(0, 45 * (pointsGrid[pointX, pointY] - 1), 0))));

                //GameObject newObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                //newObj.transform.position = transform.position + offset;
                //newObj.transform.rotation = Quaternion.Euler(0, 45 * (pointsGrid[pointX, pointY] - 1), 90);
                //newObj.transform.localScale = Vector3.one * refScale;
                //refScale += 0.1f;
            }
            for (int i = 1; i < rings.Count; i++)
            {
                mesh.ConectRings(rings[i], rings[i - 1]);
            }
            mesh.Cap(rings[0]);
            mesh.Cap(rings[rings.Count - 1], true);

            rings.Clear();
        }

        mesh.UpdateMesh();
    }

    void Update()
    {
        Compass();
        PositionateFlowers();

        if (positionInGrid != GridF.TransformPointToGrid(transform.position))
        {
            positionInGrid = GridF.TransformPointToGrid(transform.position);
            GetSurroundings();
            GetChains();
        }

        mesh.Clear(); Draw(); Draw(0.7f, Mathf.PI / 2, 1.2f, 0.45f);       
    }

    void PositionateFlowers()
    {
        Vector3 position = transform.position;
        position.y = 0;
        
        float distance = (position - GridF.TransformGridToPoint(transform.parent.GetComponent<EnemyBase>().currentPositionInGrid)).magnitude;
        distance = (scale - Mathf.Clamp(distance, 0, scale)) / scale;

        //if (positionInGrid != GridF.TransformPointToGrid(transform.position)) flowerGrowing = true;
        if (transform.parent.GetComponent<EnemyBase>().currentPositionInGrid != transform.parent.GetComponent<EnemyBase>().prevPositionInGrid) flowerGrowing = true;
        else if (prevFlowerGrowingDistance > distance) flowerGrowing = false;

        prevFlowerGrowingDistance = distance;

        if (!flowerGrowing) distance = unGrowRate.Evaluate(distance);
        else distance = growRate.Evaluate(distance);          

        flower.transform.position = GridF.TransformGridToPoint(transform.parent.GetComponent<EnemyBase>().currentPositionInGrid) /*+ Vector3.up * transform.position.y*/;
        flower.transform.localScale = Vector3.one * distance;

        distance = (position - GridF.TransformGridToPoint(transform.parent.GetComponent<EnemyBase>().currentPositionInGrid + transform.parent.GetComponent<EnemyBase>().direction)).magnitude;
        distance = (scale - Mathf.Clamp(distance, 0, scale)) / scale;
        distance = growRate.Evaluate(distance);

        nextFlower.transform.position = GridF.TransformGridToPoint(transform.parent.GetComponent<EnemyBase>().currentPositionInGrid + transform.parent.GetComponent<EnemyBase>().direction) /*+ Vector3.up * transform.position.y*/;
        nextFlower.transform.localScale = Vector3.one * distance;
    }

    void Compass()
    {        
        transform.rotation = Quaternion.identity;
    }

    void GetSurroundings()
    {
        int centerX = (int)positionInGrid.x;
        int centerY = (int)positionInGrid.y;
        //GENERATE GRID
        for (int y = 0; y < grid.GetLength(1); y++)
        {
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                grid[x, y] = GridF.CheckTile(centerX + (x-1), centerY + (y-1));
            }
        }
        //GENERATE POINTS GRID
        //Ignore halls
        for (int y = 0; y < pointsGrid.GetLength(1); y++)
        {
            for (int x = 0; x < pointsGrid.GetLength(0); x++)
            {
                if (x % 2 == 0 && y % 2 == 0) { pointsGrid[x, y] = 0; }
                else pointsGrid[x, y] = 1;
            }
        }
        //Get walls
        for (int y = 0; y < pointsGrid.GetLength(1); y++)
        {
            for (int x = 0; x < pointsGrid.GetLength(0); x++)
            {
                if (pointsGrid[x, y] != 0)
                {
                    if (x % 2 == 0)
                    {
                        if (grid[x / 2, (y + 1) / 2] == grid[x / 2, (y + 1) / 2 - 1]) pointsGrid[x, y] = 0;
                        else pointsGrid[x, y] = 3;
                    }
                    if (y % 2 == 0)
                    {
                        if (grid[(x + 1) / 2, y/2] == grid[(x + 1) / 2 - 1, y/2]) pointsGrid[x, y] = 0;
                        else pointsGrid[x, y] = 1;
                    }
                }
            }
        }
        //Get corners
        /*for(int y = 0; y < 2; y++)
        {
            for (int x = 0; x < 2; x++)
            {
                int cornerValue = 0;
                bool[,] sample = new bool[2, 2];
                int occupiedTiles = 0;

                for(int sampleY = 0; sampleY < 2; sampleY++)
                {
                    for (int sampleX = 0; sampleX < 2; sampleX++)
                    {
                        sample[sampleX, sampleY] = grid[x+sampleX, y+sampleY];
                        if(!sample[sampleX, sampleY])
                        {
                            occupiedTiles++;
                        }
                    }
                }

                if (occupiedTiles == 3)
                {
                    for (int sampleY = 0; sampleY < 2; sampleY++)
                    {
                        for (int sampleX = 0; sampleX < 2; sampleX++)
                        {
                            sample[sampleX, sampleY] = !sample[sampleX, sampleY];
                        }
                    }
                    occupiedTiles=1;
                }

                if (occupiedTiles == 1)
                {
                    for (int sampleY = 0; sampleY < 2; sampleY++)
                    {
                        for (int sampleX = 0; sampleX < 2; sampleX++)
                        {
                            if(sample[sampleX, sampleY]) 
                            {
                                if (sampleX % 2 != 0 || sampleY % 2 != 0) cornerValue = 4;
                                else cornerValue = 2;
                            }
                        }
                    }
                }

                if (occupiedTiles == 2)
                {
                    if (sample[0, 0] == sample[1, 0])
                    {
                        cornerValue = 3;
                    }
                    else if (sample[0, 0] == sample[0, 1])
                    {
                        cornerValue = 1;
                    }
                    else
                    {
                        if (sample[0, 0]) cornerValue = 2;
                        else cornerValue = 4;
                    }
                }
                pointsGrid[1 + x * 2, 1 + y * 2] = cornerValue;
            }
        }*/ //Who needs corners
    }

    void GetChains()
    {
        walls.Clear();
        Vector2 checkDirection, pointer, prevCheckDirection;

        for (int y = 0; y < pointsGrid.GetLength(1); y++)
        {
            for (int x = 0; x < pointsGrid.GetLength(0); x++)
            {
                if (((x == 0 || x == pointsGrid.GetLength(0) - 1) || (y == 0 || y == pointsGrid.GetLength(1) - 1)) &&
                    pointsGrid[x, y] != 0 && !CheckWallsList(new Vector2(x, y)))
                {
                    List<Vector2> thread = new List<Vector2>();

                    checkDirection = Vector2.right;
                    prevCheckDirection = Vector2.zero;
                    pointer = new Vector2(x, y);

                    bool keepGoing = true;

                    while (keepGoing)
                    {
                        thread.Add(pointer);
                        keepGoing = false;
                        for (int i = 0; i < 4; i++)
                        {
                            Vector2 checkPosition = pointer + checkDirection;
                            if ((InsideIndex((int)checkPosition.x, pointsGrid.GetLength(0)) && InsideIndex((int)checkPosition.y, pointsGrid.GetLength(1)))
                                && pointsGrid[(int)checkPosition.x, (int)checkPosition.y] != 0 && !CheckWallsList(checkPosition))
                            {
                                //Debug.Log("I'm here");
                                keepGoing = true;
                                pointer = checkPosition;
                                prevCheckDirection = -checkDirection;
                                break;
                            }
                            else {
                                //Debug.Log("I'm here rotating the vector you know that's what I do I wouldn't fail you, right?");
                                checkDirection = GridF.RotateVector(checkDirection);
                                if (checkDirection == prevCheckDirection) checkDirection = GridF.RotateVector(checkDirection); //rotate 2 times to avoid backwards movement
                            }
                        }
                    }
                    walls.Add(thread);
                }
            }
        }
        //Debug.Log(walls.Count);
        //CORRECT CHAINS       
        /*foreach (List<Vector2> thread in walls)
        {
            int prevValue = 0;
            foreach (Vector2 point in thread)
            {
                int x, y;
                x = (int)point.x; y = (int)point.y;
                if (prevValue != 0)
                {
                    int add = prevValue > 0 ? 0 : 1; //adjusting for the 0
                    int difference = (prevValue + add) - pointsGrid[x, y];

                    if(Mathf.Abs(difference) > 1)
                    {
                        int newValue = prevValue + (int)Mathf.Sign(difference); 
                        if (newValue == 0) newValue = -1; //adjusting for the 0
                        pointsGrid[x, y] = newValue;
                    }
                }
                prevValue = pointsGrid[x,y];
            }
        }*/
        foreach (List<Vector2> thread in walls)
        {
            if (thread.Count > 1)
            {
                Vector2 prevDirection = thread[1] - thread[0];
                pointsGrid[(int)thread[0].x, (int)thread[0].y] = (int)GetValueFromAngle(Vector2.Angle(Vector2.up, prevDirection) * Mathf.Sign(prevDirection.x));

                for (int i = 1; i < thread.Count; i++)
                {
                    int x, y;
                    x = (int)thread[i].x; y = (int)thread[i].y;

                    if (i != thread.Count - 1)
                    {                       
                        Vector2 currentDirection = thread[i + 1] - thread[i];
                        Vector2 angle = prevDirection + currentDirection;
                        pointsGrid[x, y] = (int)GetValueFromAngle(Vector2.Angle(Vector2.up, angle) * Mathf.Sign(angle.x));

                        prevDirection = currentDirection;
                    }
                    else
                    {
                        pointsGrid[x, y] = (int)GetValueFromAngle(Vector2.Angle(Vector2.up, prevDirection) * Mathf.Sign(prevDirection.x));
                    }
                }
            }
        }
    }

    float GetAngleFromValue(float value)
    {
        //int addCorrection = value > 0 ? 0 : 1;
        return 45 * (value - 1);
    }

    float GetValueFromAngle(float angle)
    {
        float newAngle = angle < 0 ? angle + 360 : angle;
        float value = Mathf.Round(newAngle / 45);
        //int addCorrection = value > 0 ? 1 : 0;
        return (value % 8) + 1;
    }

    bool InsideIndex(int value, int indexLength)
    {
        return (value >= 0 && value < indexLength);
    }

    bool CheckWallsList(Vector2 index)
    {
        foreach(List<Vector2> thread in walls)
        {
            foreach(Vector2 threadIndex in thread)
            {
                if (threadIndex == index) return true;
            }
        }

        return false;
    }

    bool CheckList(List<Vector2> thread, Vector2 index)
    {
        foreach (Vector2 threadIndex in thread)
        {
            if (threadIndex == index) return true;
        }
        return false;
    }

    void TestUpdate()
    {
        mesh.Clear();

        Vector3 worldPos = transform.position;

        int[] ring1, ring2, ring3, ring4, ring5;

        ring1 = mesh.AddArray(CreateRing(0, Vector3.back * 6 + Vector3.up * Mathf.Sin((worldPos.z - 6) * 0.5f)));
        ring2 = mesh.AddArray(CreateRing(1.5f, Vector3.back * 3 + Vector3.up * Mathf.Sin((worldPos.z - 3) * 0.5f), Quaternion.Euler(0, 45, 0)));
        ring3 = mesh.AddArray(CreateRing(2, Vector3.zero + Vector3.up * Mathf.Sin(worldPos.z * 0.5f)));
        ring4 = mesh.AddArray(CreateRing(1.5f, Vector3.forward * 3 + Vector3.up * Mathf.Sin((worldPos.z + 3) * 0.5f)));
        ring5 = mesh.AddArray(CreateRing(0, Vector3.forward * 6 + Vector3.up * Mathf.Sin((worldPos.z + 6) * 0.5f)));

        mesh.ConectRings(ring2, ring1);
        mesh.ConectRings(ring3, ring2);
        mesh.ConectRings(ring4, ring3);
        mesh.ConectRings(ring5, ring4);

        mesh.UpdateMesh();
    }

    //RING CREATION -------------------------------------------------------------------------------------
    Vector3[] CreateRing(Vector3 offset)
    {
        return CreateRing(testRadius, offset, Quaternion.identity);
    }

    Vector3[] CreateRing(float radius, Vector3 offset)
    {
        return CreateRing(vertexNumber, radius, offset, Quaternion.identity);
    }

    Vector3[] CreateRing(float radius, Vector3 offset, Quaternion rotation)
    {
        return CreateRing(vertexNumber, radius, offset, rotation);
    }

    Vector3[] CreateRing(int ringVertexNumber, float radius, Vector3 offset, Quaternion rotation)
    {
        Vector3[] newRing = new Vector3[ringVertexNumber];
        float increment = 2 * Mathf.PI / ringVertexNumber;
        float angle = 0;

        for(int i = 0; i < ringVertexNumber; i++)
        {
            newRing[i] = rotation * new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0) + offset;
            angle += increment;
        }

        return newRing;
    }    
}
