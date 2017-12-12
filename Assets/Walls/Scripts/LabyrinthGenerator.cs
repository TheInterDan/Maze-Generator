using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LabyrinthGenerator : MonoBehaviour {

    [HideInInspector]
    public static LabyrinthGenerator instance;

    public GameObject player, goal, enemyType;

    public string seed = "weed";

    public int size = 15;
    public float scale = 2;
    public float height = 2;
    public bool capMode = false;
    public Texture2D mapTexture;

    float[,] map;
    List<HallInfo> grounds = new List<HallInfo>();
    List<HallInfo> walls = new List<HallInfo>();
    MeshObject groundMeshObject;
    MeshObject wallMeshObject;
    MeshObject ceilMeshObject;

    public Vector2 origin, end, originDirection, endDirection;

    void Awake()
    {
        instance = this;

        /*  {
              seed = seed.ToLower();
              int seedNumber = 0;
              foreach (char character in seed)
              {
                  seedNumber += character;
              }
              Random.InitState(seedNumber);
              print(seedNumber);
          }     */

        //if (mapTexture == null)
        seed = Game.seed;
        size = Game.size;

        GenerateTexture();
    }

    void Start() {         
        TextureToMap();

        GetGroundFromMap();
        GetWallsFromMap();

        groundMeshObject = new MeshObject((grounds.Count * 4), grounds.Count * 2 * 3);
        wallMeshObject = new MeshObject(walls.Count * 4, walls.Count * 2 * 3);

        DrawGround();
        DrawWalls();
        DrawCeil();

        CombineInstance[] combineMesh = new CombineInstance[3];
        combineMesh[0].mesh = groundMeshObject.GenerateMesh();
        combineMesh[1].mesh = wallMeshObject.GenerateMesh();
        combineMesh[2].mesh = ceilMeshObject.GenerateMesh();

        Mesh generatedMesh = new Mesh();
        generatedMesh.CombineMeshes(combineMesh, false, false);

        GetComponent<MeshFilter>().sharedMesh = generatedMesh;
        GetComponent<MeshCollider>().sharedMesh = generatedMesh;

        player.transform.position = new Vector3(((origin.x * 2) + 1) * scale, height * scale / 2, ((origin.y * 2) + 1) * scale);
        float angle = Mathf.Atan2(originDirection.x, originDirection.y) * Mathf.Rad2Deg;
        player.transform.eulerAngles = new Vector3(0, angle, 0);

        goal.transform.position = new Vector3(((end.x * 2) + 1) * scale, 0, ((end.y * 2) + 1) * scale);
        //endDirection = new Vector2(1,1);
        angle = Mathf.Atan2(endDirection.x, endDirection.y) * Mathf.Rad2Deg;
        goal.transform.eulerAngles = new Vector3(0, angle, 0);

        if(Game.enemies){
            GameObject enemy = Instantiate(enemyType);
            enemy.transform.position = GridF.TransformGridToPoint(end * 2 + Vector2.one + endDirection / 2);
            enemy.transform.eulerAngles = new Vector3(0, angle, 0);
        }
    }

    void Update() {

    }

    void GenerateTexture()
    {
        //Initialize texture
        Texture2D newMap = new Texture2D(size, size, TextureFormat.RGBA32, false);
        newMap.filterMode = FilterMode.Point;
        newMap.wrapMode = TextureWrapMode.Clamp;
        {
            Color[] colors = new Color[size * size];
            for(int i = 0; i < colors.Length; i++)
            {
                colors[i] = Color.black;
            }
            newMap.SetPixels(colors);
        }

        Color paintColor = Color.white;

        //Initialize variables
        int pathsMapSize = Mathf.FloorToInt(size / 2f);
        bool[,] pathsMap = new bool[pathsMapSize, pathsMapSize];
        List<Vector2> mainPath = new List<Vector2>();

        //Initialize seed
        {
            seed = seed.ToLower();
            int seedNumber = 0;
            foreach (char character in seed)
            {
                seedNumber += character;
            }
            Random.InitState(seedNumber);
        }

        /*for (int x = 0; x < pathsMap.GetLength(0); x++)
        {
            for (int y = 0; y < pathsMap.GetLength(0); y++)
            {
                newMap.SetPixel(x * 2 + 1, y * 2 + 1, new Color(0.5f,0.5f,0.5f));
            }
        } */ //Draw grid       

        //Draw Main Path
        origin = new Vector2(Random.Range(0, pathsMapSize), Random.Range(0, pathsMapSize));
        end = new Vector2(Random.Range(0, pathsMapSize), Random.Range(0, pathsMapSize));
        while(origin == end) end = new Vector2(Random.Range(0, pathsMapSize), Random.Range(0, pathsMapSize));

        Vector2 pointer = origin;
        Vector2 prevPosition = pointer;
        Vector2 direction;
        {
            int x = Random.Range(0, 2);
            int sign = Random.Range(-1, 1);
            direction = new Vector2(x * Mathf.Sign(sign), (x - 1) * Mathf.Sign(sign));
        }

        int rotationDirection = 0;
        int corners = 0;
        bool turn = false;
        int turnsWithoutCorner = 0;
        int turnsWithCorner = 0;
        int pathTries = 0;

        int maxPathLength = (pathsMapSize * pathsMapSize) / 2;

        mainPath.Add(origin);
        pathsMap[(int)origin.x, (int)origin.y] = true;

        //paintColor = Color.cyan;
        //FIRST PASS (main path)______________________________________
        while (pointer != end)
        {
            if(!turn && Random.Range(0,pathsMapSize/2) < turnsWithoutCorner && turnsWithCorner < 3) //decides if turning. it doesn't turn if there have been a turn in the previous 3 turns
            {
                direction = RotateVector(direction, Random.Range(-1,1));
                turn = true;
                turnsWithCorner++;               
            }

            if (mainPath.Count > maxPathLength)
            {           
                Vector2 relativePosition = -new Vector2(Mathf.RoundToInt(pointer.x - end.x), Mathf.RoundToInt(pointer.y - end.y));
                if (relativePosition.x == 0)
                {
                    direction = - new Vector2(0, Mathf.Sign(pointer.y - end.y));                   
                }
                else if (relativePosition.y == 0)
                {
                    direction = - new Vector2(Mathf.Sign(pointer.x - end.x), 0);
                }
                else
                {
                    relativePosition = - new Vector2(Mathf.Sign(pointer.x - end.x), Mathf.Sign(pointer.y - end.y));
                    if (direction.x != relativePosition.x || direction.y != relativePosition.y)
                    {
                        int randomNumber = Random.Range(0, 2);
                        direction = new Vector2((int)(Mathf.Abs(randomNumber - 1) * relativePosition.x), (int)(randomNumber * relativePosition.y));                     
                    }
                }
            }

            Vector2 nextPosition = pointer + direction;

            if (((nextPosition.x < pathsMapSize && nextPosition.y < pathsMapSize && nextPosition.x >= 0 && nextPosition.y >= 0) &&                  //if we are inside the map
                (!pathsMap[(int)nextPosition.x, (int)nextPosition.y] || pathTries > 4 ||                                                            //and the map is not painted yet            
                (Random.Range(0f, 2 - turnsWithCorner) < (1 - 1f/corners) && nextPosition != prevPosition)) &&                                      //(or there's no other way or random chance)
                (corners > pathsMapSize / 4 || nextPosition != end)) || mainPath.Count > maxPathLength)                                             //and is not the end or the end is allowed
            {
                pathTries = 0;
                if (turn) { turn = false; corners++; turnsWithoutCorner = 0; }
                else { turnsWithoutCorner++; turnsWithCorner = 0; }

                if (mainPath.Count == 0) originDirection = -direction;
                if (nextPosition == end) endDirection = -direction;

                //draw________________________________________
                Vector2 nextPositionTextureSpace = nextPosition * 2 + Vector2.one;

                newMap.SetPixel((int)nextPositionTextureSpace.x, (int)nextPositionTextureSpace.y, paintColor);
                nextPositionTextureSpace -= direction;
                newMap.SetPixel((int)nextPositionTextureSpace.x, (int)nextPositionTextureSpace.y, paintColor);

                pathsMap[(int)nextPosition.x, (int)nextPosition.y] = true;

                prevPosition = pointer;
                pointer = nextPosition;
                mainPath.Add(pointer);

                //paintColor += new Color(0, -0.005f, 0.005f); //cool effect
            }
            else
            {
                if (pathTries == 0)
                {
                    //rotationDirection = Random.Range(-1, 1);
                    rotationDirection = -rotationDirection;
                }
                pathTries++;
                direction = RotateVector(direction, rotationDirection);
                turn = true;              
            }
        }
        
        //paintColor = Color.green;
        //SECOND PASS (Add to main path)_________________________________________________
        foreach (Vector2 coordinate in mainPath)
        {
            if (coordinate != mainPath[0] && coordinate != mainPath[mainPath.Count-1])
            {
                rotationDirection = Random.Range(-1, 1);
                bool thereIsRoom = false;
                for (int i = 0; i < 4; i++)
                {
                    direction = RotateVector(direction, rotationDirection);
                    Vector2 nextPosition = coordinate + direction;
                    if ((nextPosition.x < pathsMapSize && nextPosition.y < pathsMapSize && nextPosition.x >= 0 && nextPosition.y >= 0) &&
                        !pathsMap[(int)nextPosition.x, (int)nextPosition.y])
                    {
                        thereIsRoom = true;
                        break;
                    }
                }

                int emptyTiles = 0;
                foreach (bool tile in pathsMap)
                {
                    emptyTiles++;
                }

                if (thereIsRoom && (int)Random.Range(0, (emptyTiles / (pathsMapSize * pathsMapSize)) * pathsMapSize) > pathsMapSize / 4f) //probability to open new path
                {
                    pointer = coordinate;
                    Vector2 nextPosition;
                    for (int i = 0; i < Random.Range(0, (emptyTiles / (pathsMapSize * pathsMapSize)) * pathsMapSize * 1.5f); i++) //longitude chance
                    {
                        nextPosition = pointer + direction;

                        if ((nextPosition.x < pathsMapSize && nextPosition.y < pathsMapSize && nextPosition.x >= 0 && nextPosition.y >= 0)
                            && (!pathsMap[(int)nextPosition.x, (int)nextPosition.y] || Random.Range(0, i*1.5f) > pathsMapSize / 4f)) //pass through other paths chance
                        {
                            pathsMap[(int)nextPosition.x, (int)nextPosition.y] = true;
                           
                            Vector2 nextPositionTextureSpace = nextPosition * 2 + Vector2.one;
                            newMap.SetPixel((int)nextPositionTextureSpace.x, (int)nextPositionTextureSpace.y, paintColor);
                            nextPositionTextureSpace -= direction;
                            newMap.SetPixel((int)nextPositionTextureSpace.x, (int)nextPositionTextureSpace.y, paintColor);

                            prevPosition = pointer;
                            pointer = nextPosition;

                            if (Random.Range(0, 2 + (int)(1 / (2f / ((i / 2f) + 1)))) == 0) //turn chance
                            {
                                direction = RotateVector(direction, Random.Range(-1, 1));
                            }
                        }
                        else
                        {
                            direction = RotateVector(direction, Random.Range(-1, 1));
                            i--;
                        }
                    }
                }
            }
        }

        //paintColor = Color.yellow;
        //LAST PASS (fill)______________________________________________________________________________       
        int passDirection = +1;
        if ((!pathsMap[1, 0] && !pathsMap[0, 1]) || (!pathsMap[pathsMapSize - 2, 0] && !pathsMap[pathsMapSize - 1, 1])) passDirection = -1;

        int repetitions = 1;
        if (passDirection < 0 && ((!pathsMap[1, pathsMapSize - 1] && !pathsMap[0, pathsMapSize - 2]) || 
            (!pathsMap[pathsMapSize - 2, pathsMapSize - 1] && !pathsMap[pathsMapSize - 1, pathsMapSize - 2])))
            repetitions = 2;

        for (int repetition = 0; repetition < repetitions; repetition++)
        {         
            for(int y = 0; y < pathsMapSize; y++)
            {               
                for (int x = 0; x < pathsMapSize; x++)
                {
                    //int map_x = (pathsMapSize * (-passDirection + 1) / 2) + x;
                    //int map_y = (pathsMapSize * (-passDirection + 1) / 2) + y;

                    int map_x = x;
                    int map_y = y;
                    if (passDirection < 0) { map_x = pathsMapSize - 1 - x; map_y = pathsMapSize - 1 - y; }

                    if (!pathsMap[map_x, map_y])
                    {
                        int new_x = 0;
                        int new_y = 0;
                        bool foundOne = false;

                        rotationDirection = Random.Range(-1, 1);
                        for (int i = 0; i < Random.Range(0, 3); i++)
                        {
                            direction = RotateVector(direction, rotationDirection);
                        }

                        for (int i = 0; i < 4; i++)
                        {
                            direction = RotateVector(direction, 1);
                            new_x = (int)(map_x + direction.x);
                            new_y = (int)(map_y + direction.y);
                            if (new_x >= 0 && new_y >= 0 && new_x < pathsMapSize && new_y < pathsMapSize &&
                                new Vector2(new_x, new_y) != end && new Vector2(new_x, new_y) != origin)
                            {
                                if (pathsMap[new_x, new_y]) { foundOne = true; break; }
                            }
                        }
                        if (!foundOne) //&& repetition == repetitions-1)
                        {
                            //for (int i = 0; i < 5; i++)
                            //{
                            //    direction = RotateVector(direction, 1);
                            //    new_x = (int)(map_x + direction.x);
                            //    new_y = (int)(map_y + direction.y);
                            //    if (new_x >= 0 && new_y >= 0 && new_x < pathsMapSize && new_y < pathsMapSize)
                            //    {
                            //        break;
                            //    }
                            //}

                            //Vector2 relativePosition = -new Vector2(Mathf.Sign(map_x - (pathsMapSize/2)), Mathf.Sign(map_y - (pathsMapSize/2)));
                            //int randomNumber = Random.Range(0, 2);
                            //direction = new Vector2((int)(Mathf.Abs(randomNumber - 1) * relativePosition.x), (int)(randomNumber * relativePosition.y));
                            //foundOne = true;
                        }
                        if (foundOne)
                        {
                            Vector2 nextPositionTextureSpace = new Vector2(map_x, map_y) * 2 + Vector2.one; ;
                            newMap.SetPixel((int)nextPositionTextureSpace.x, (int)nextPositionTextureSpace.y, paintColor);
                            nextPositionTextureSpace += direction;
                            newMap.SetPixel((int)nextPositionTextureSpace.x, (int)nextPositionTextureSpace.y, paintColor);
                            nextPositionTextureSpace = new Vector2(new_x, new_y) * 2 + Vector2.one;
                            newMap.SetPixel((int)nextPositionTextureSpace.x, (int)nextPositionTextureSpace.y, paintColor);

                            pathsMap[map_x, map_y] = true;
                            //paintColor -= new Color(-0.001f, 0.001f, 0.001f);
                        }                       
                    }
                }               
            }           
            passDirection = -passDirection;
        }
        
        newMap.SetPixel((int)(origin.x * 2 + 1), (int)(origin.y * 2 + 1), Color.white);
        newMap.SetPixel((int)(end.x * 2 + 1), (int)(end.y * 2 + 1), Color.white);

        newMap.Apply();

        //GetComponent<MeshRenderer>().sharedMaterial.mainTexture = newMap;
        mapTexture = newMap;        
    }

    Vector2 RotateVector(Vector2 vectorToRotate, int direction)
    {
        Vector2 vector = Quaternion.Euler(0, 0, 90 * Mathf.Sign(direction)) * vectorToRotate;
        return new Vector2(Mathf.Round(vector.x), Mathf.Round(vector.y));
    }

    void TextureToMap()
    {
        map = new float[mapTexture.width, mapTexture.height];
        Color[] pixels = mapTexture.GetPixels();

        for (int i = 0; i < pixels.Length; i++)
        {
            int column = i % mapTexture.width;
            int row = i / mapTexture.width;
            map[column, row] = pixels[i].r;
        }
    }

    void GetGroundFromMap(bool capping = false)
    {
        int groundColor = 1;
        if (capping) groundColor = 0;
        for (int y = 0; y < map.GetLength(1); y++)
        {
            for (int x = 0; x < map.GetLength(0); x++)
            {
                if (map[x, y] == groundColor)
                {                   
                    if (!CheckGroundTiles(new Vector2(x, y)))
                    {
                        Vector2 beginning = new Vector2(x, y);
                        int offset = 0;

                        while ((x + offset + 1) < map.GetLength(0) && map[x + offset + 1, y] == groundColor && !CheckGroundTiles(new Vector2(x + offset + 1, y)))
                        {
                            offset++;
                        }

                        if (offset != 0)
                        {
                            grounds.Add(new HallInfo(beginning, beginning + Vector2.right * offset));
                        }
                        else
                        {
                            offset = 0;
                            while ((y + offset + 1) < map.GetLength(1) && map[x, y + offset + 1] == groundColor && !CheckGroundTiles(new Vector2(x, y + offset + 1)))
                            {
                                offset++;
                            }
                            grounds.Add(new HallInfo(beginning, beginning + Vector2.up * offset));                            
                        }
                    }
                }
            }
        }
    }

    bool CheckGroundTiles(Vector2 point)
    {
        bool pointIsMapped = false;
        /*for (int i = 0; i < grounds.Count; i++)
        {
            if (grounds[i].CheckPoint(new Vector2(x, y)))
            {
                pointIsMapped = true;
            }
        }*/
        foreach (HallInfo groundTile in grounds)
        {
            if (groundTile.CheckPoint(point))
            {
                pointIsMapped = true;
            }
        }
        return pointIsMapped;
    }

    void GetWallsFromMap()
    {
        List<Corner> corners = new List<Corner>();

        for (int y = 0; y < map.GetLength(1) - 1; y++)
        {
            for (int x = 0; x < map.GetLength(0) - 1; x++)
            {
                //Check tiles 4 by 4
                float[,] sample = new float[2, 2];
                sample[0, 0] = map[x + 0, y + 0];
                sample[1, 0] = map[x + 1, y + 0];
                sample[0, 1] = map[x + 0, y + 1];
                sample[1, 1] = map[x + 1, y + 1];

                Corner corner;
                if(CheckCorner(sample, out corner, x, y))
                {
                    corners.Add(corner);
                    if (corner.doubleCorner == true) corners.Add(corner);                 
                }
            }
        }

        while (corners.Count > 0)
        {
            if (corners[0].hallsToDrawCount <= 0) { corners.RemoveAt(0); }
            else {
                HallInfo hallInfo;

                if (corners[0].horizontalHall > 0) //if the horizontal hall hasn't been drawn
                {
                    Vector2 beginningPos1, endPos1, normal1;
                    beginningPos1 = corners[0].position;
                    endPos1 = Vector2.zero;
                    normal1 = corners[0].direction1;
                    for (int i = 1; i < corners.Count; i++)
                    {
                        if (corners[0].position.y == corners[i].position.y)
                        {
                            endPos1 = corners[i].position;
                            Corner cornerCopy = corners[i];
                            cornerCopy.HallDrawed("horizontal");
                            corners[i] = cornerCopy;
                            break;
                        }
                    }
                    if (endPos1 != Vector2.zero)
                    {
                        hallInfo = new HallInfo(beginningPos1, endPos1, normal1);
                        walls.Add(hallInfo);                        
                    }

                    corners[0].HallDrawed("horizontal");
                }

                if (corners[0].verticalHall > 0) //vertical hall
                {
                    Vector2 beginningPos2, endPos2, normal2;
                    beginningPos2 = corners[0].position;
                    endPos2 = Vector2.zero;
                    normal2 = corners[0].direction2;
                    for (int i = 1; i < corners.Count; i++)
                    {
                        if (corners[0].position.x == corners[i].position.x)
                        {
                            endPos2 = corners[i].position;
                            Corner cornerCopy = corners[i];
                            cornerCopy.HallDrawed("vertical");
                            corners[i] = cornerCopy;
                            break;
                        }
                    }
                    if (endPos2 != Vector2.zero)
                    {
                        hallInfo = new HallInfo(beginningPos2, endPos2, normal2);
                        walls.Add(hallInfo);
                        corners[0].HallDrawed("vertical");
                    }                   
                }

                corners.RemoveAt(0);
            }
        }
    }

    bool CheckCorner(float[,] sample, out Corner corner, int map_x, int map_y)
    {
        corner = new Corner(new Vector2(map_x, map_y));
        int wallTilesQuantity = 0;

        Vector2 singleTileCase = Vector2.zero;
        Vector2 tripleTileCase = Vector2.zero;

        for (int y = 0; y < sample.GetLength(1); y++)
        {
            for (int x = 0; x < sample.GetLength(0); x++)
            {
                if (sample[x, y] == 0)
                {
                    wallTilesQuantity++;
                    singleTileCase = new Vector2(x, y);
                }
                else tripleTileCase = new Vector2(x, y);
            }
        }

        //discard non-corners
        if (wallTilesQuantity == 0 || wallTilesQuantity == 4) return false;        
        //examine corner
        else if (wallTilesQuantity == 1)
        {
            singleTileCase = singleTileCase * 2 - Vector2.one;
            corner.direction1 = new Vector2(0, -singleTileCase.y);
            corner.direction2 = new Vector2(-singleTileCase.x, 0);
            return true;
        }
        else if (wallTilesQuantity == 3)
        {
            tripleTileCase = tripleTileCase * 2 - Vector2.one;
            corner.direction1 = new Vector2(0, tripleTileCase.y);
            corner.direction2 = new Vector2(tripleTileCase.x, 0);                      
            return true;
        }
        else
        {
            if (!(sample[0, 0] != sample[0, 1] && sample[0, 0] != sample[1, 0]))
            {
                return false;
            }
            else {
                corner.direction1 = Vector2.up;
                corner.direction2 = Vector2.right;
                corner.doubleCorner = true;
                return true;
            }
        }       
    }

    void DrawCeil()
    {
        if (!capMode)
        {
            Vector3 offset, a, b, c, d;
            offset = new Vector3(-0.5f,0,-0.5f) * scale;

            a = new Vector3(0, height*scale, 0);
            b = new Vector3(scale*size, height*scale, 0);
            c = new Vector3(0, height*scale, scale*size);
            d = new Vector3(scale*size, height*scale, scale*size);

            a += offset; b += offset; c += offset; d += offset;

            ceilMeshObject = new MeshObject(4 + 4*4, 2*3 + 3*2*4);
            ceilMeshObject.AddQuad(a, b, c, d);
            //ceilMeshObject.AddQuad(new Vector3(0, height, 0), new Vector3(1, height, 0), new Vector3(0, height, 1), new Vector3(1, height, scale));
        }
        else
        {
            grounds = new List<HallInfo>();
            GetGroundFromMap(true);
            ceilMeshObject = new MeshObject((grounds.Count * 4 + 4*4), grounds.Count * 2 * 3 + 3*2*4);
            AddCeilBorders();
            DrawGround(ref ceilMeshObject, height * scale);

        }
    }

    void AddCeilBorders()
    {
        Vector3 min = new Vector3(0, -50f,0);
        Vector3 borderHeight = new Vector3(0,height * scale,0);

        Vector3 offset, a, b, c, d;
        offset = new Vector3(-0.5f, 0, -0.5f) * scale;

        a = new Vector3(0, 0, 0);
        b = new Vector3(scale * size, 0, 0);
        c = new Vector3(0, 0, scale * size);
        d = new Vector3(scale * size, 0, scale * size);

        a += offset; b += offset; c += offset; d += offset;

        ceilMeshObject.AddQuad(b + borderHeight, b + min, a + borderHeight, a + min);
        ceilMeshObject.AddQuad(c + min, d + min, c + borderHeight, d + borderHeight);
        ceilMeshObject.AddQuad(a + borderHeight, a + min, c + borderHeight, c + min);
        ceilMeshObject.AddQuad(d + borderHeight, d + min, b + borderHeight, b + min);
    }

    void DrawGround()
    {
        DrawGround(ref groundMeshObject, 0);
    }

    void DrawGround(ref MeshObject meshObject, float groundHeight)
    {
        Vector2 bottomOffset, topOffset, rightOffset, leftOffset;
        bottomOffset = Vector2.down * 0.5f;
        topOffset = Vector2.up * 0.5f;
        leftOffset = Vector2.left * 0.5f;
        rightOffset = Vector2.right * 0.5f;

        foreach (HallInfo groundTile in grounds)
        {
            Vector2 beginningPoint = groundTile.GetBeginning;
            Vector2 endPoint = groundTile.GetEnd;
            Vector2 direction = groundTile.GetDirection;

            Vector2 a, b, c, d;

            a = beginningPoint + bottomOffset + leftOffset;
            d = endPoint + topOffset + rightOffset;            

            if (direction == Vector2.up)
            {
                b = beginningPoint + bottomOffset + rightOffset;
                c = endPoint + topOffset + leftOffset;               
            }
            else
            {
                b = endPoint + bottomOffset + rightOffset;
                c = beginningPoint + topOffset + leftOffset;
            }

            a *= scale;
            b *= scale;
            c *= scale;
            d *= scale;

            meshObject.AddQuad(new Vector3(c.x, groundHeight, c.y), new Vector3(d.x, groundHeight, d.y), new Vector3(a.x, groundHeight, a.y), new Vector3(b.x, groundHeight, b.y));
        }
    }

    void DrawWalls()
    {
        Vector3 offset;
        offset = Vector3.one * 0.5f;
        offset.y = 0;

        foreach (HallInfo wallTile in walls)
        {
            Vector2 beginningPoint = wallTile.GetBeginning;
            Vector2 endPoint = wallTile.GetEnd;
            Vector2 normal = wallTile.GetNormal;

            Vector3 a, b, c, d;

            a = new Vector3(beginningPoint.x, 0, beginningPoint.y) + offset;            
            b = new Vector3(endPoint.x, 0, endPoint.y) + offset;
            c = new Vector3(beginningPoint.x, height, beginningPoint.y) + offset;
            d = new Vector3(endPoint.x, height, endPoint.y) + offset;

            a *= scale;
            b *= scale;
            c *= scale;
            d *= scale;

            if ((normal.x < 0 || normal.y > 0)) wallMeshObject.AddQuad(a,b,c,d);
            else wallMeshObject.AddQuad(d, b, c, a);
        }
    }

    struct HallInfo
    {
        Vector2 beginningPosition;
        Vector2 endPosition;
        Vector2 normal;

        public HallInfo(Vector2 beginningPosition, Vector2 endPosition)
        {
            this.beginningPosition = beginningPosition;
            this.endPosition = endPosition;
            normal = Vector2.zero;
        }

        public HallInfo(Vector2 beginningPosition, Vector2 endPosition, Vector2 normal)
        {
            this.beginningPosition = beginningPosition;
            this.endPosition = endPosition;
            this.normal = normal;
        }

        public Vector2 GetDirection
        {
            get { return (endPosition - beginningPosition).normalized; }
        }

        public Vector2 GetNormal
        {
            get { return normal; }
        }

        public Vector2 GetBeginning
        {
            get { return beginningPosition; }
        }

        public Vector2 GetEnd
        {
            get { return endPosition; }
        }

        public Vector2 GetPosition
        {
            get { return beginningPosition; }
        }

        public bool CheckPoint(Vector2 point)
        {
            if (point == beginningPosition || point == endPosition) return true;
            return Vector2.Distance(beginningPosition, point) + Vector2.Distance(point, endPosition) == Vector2.Distance(beginningPosition, endPosition);
        }       
    }

    struct Corner
    {
        public Vector2 position, direction1, direction2;
        public bool doubleCorner;
        public int horizontalHall, verticalHall;

        public Corner(Vector2 position)
        {
            this.position = position;
            direction1 = Vector2.zero;
            direction2 = Vector2.zero;
            doubleCorner = false;
            horizontalHall = 1;
            verticalHall = 1;
        }

        public Corner(Vector2 position, Vector2 direction1, Vector2 direction2, bool doubleDirection = false)
        {
            this.position = position;
            this.direction1 = direction1;
            this.direction2 = direction2;
            this.doubleCorner = doubleDirection;
            horizontalHall = 1;
            verticalHall = 1;
        }

        public void HallDrawed(string horizontalOrVertical)
        {
            if (horizontalOrVertical == "horizontal") horizontalHall--;
            else verticalHall--;
        }

        public int hallsToDrawCount
        {
            get { return horizontalHall + verticalHall; }
        }
    }
}


//TOOLS FOR CREATING THE MESH
public class MeshObject
{
    Vector3[] vertices;
    Vector2[] uvs;
    int vertexIndex = 0;

    int[] triangles;
    int triangleIndex = 0;

    //CONSTRUCTOR____________________________________________
    public MeshObject(int verticesLength, int trianglesLength)
    {
        vertices = new Vector3[verticesLength];
        uvs = new Vector2[verticesLength];
        triangles = new int[trianglesLength];
    }

    //POINTER FUNCTIONS______________________________________
    public bool SetVertexPointer(int newIndex)
    {
        return SetIndex(newIndex, ref vertexIndex, vertices.Length);
    }

    public bool SetTrianglePointer(int newIndex)
    {
        return SetIndex(newIndex, ref triangleIndex, triangles.Length / 3);
    }

    bool SetIndex(int newIndex, ref int destinationIndex, int arrayLength)
    {
        if (newIndex < 0)
        {
            destinationIndex = 0;
            Debug.Log("Index can't be lower than 0");
            return false;
        }

        if (newIndex < arrayLength)
        {
            destinationIndex = newIndex;
            return true;
        }

        destinationIndex = arrayLength - 1;
        return false;
    }

    //VERTEX FUNCTIONS_______________________________________
    public void AddVertex(Vector3 point)
    {
        AddVertex(point, vertexIndex);
    }

    public void AddVertex(Vector3 point, Vector2 uv)
    {
        AddVertex(point, vertexIndex, uv);
    }

    public void AddVertex(Vector3 point, int newVertexIndex)
    {
        AddVertex(point, vertexIndex, Vector2.zero);
    }

    public void AddVertex(Vector3 point, int newVertexIndex, Vector2 uv)
    {
        SetVertexPointer(newVertexIndex);
        vertices[vertexIndex] = point;
        uvs[vertexIndex] = uv;
        SetVertexPointer(newVertexIndex + 1);
    }

    //TRIANGLE FUNCTIONS_____________________________________
    public void AddTriangle(int a, int b, int c)
    {
        AddTriangle(a, b, c, triangleIndex);
    }

    public void AddTriangle(int a, int b, int c, int newTriangleIndex)
    {
        SetTrianglePointer(newTriangleIndex);
        triangles[triangleIndex * 3    ] = a;
        triangles[triangleIndex * 3 + 1] = b;
        triangles[triangleIndex * 3 + 2] = c;
        SetTrianglePointer(newTriangleIndex + 1);
    }

    public void AddTriangle(Vector3 a, Vector3 b, Vector3 c)
    {
        AddTriangle(vertexIndex, a, b, c, triangleIndex);
    }

    public void AddTriangle(Vector3 a, Vector3 b, Vector3 c, int newTriangleIndex)
    {
        AddTriangle(vertexIndex, a, b, c, newTriangleIndex);
    }

    public void AddTriangle(int newVertexIndex, Vector3 a, Vector3 b, Vector3 c)
    {
        AddTriangle(newVertexIndex, a, b, c, triangleIndex);
    }

    public void AddTriangle(int newVertexIndex, Vector3 a, Vector3 b, Vector3 c, int newTriangleIndex)
    {
        SetVertexPointer(newVertexIndex);
        int indexA = vertexIndex;
        AddVertex(a);
        int indexB = vertexIndex;
        AddVertex(b);
        int indexC = vertexIndex;
        AddVertex(c);
        AddTriangle(indexA, indexB, indexC, newTriangleIndex);
    }

    //QUAD FUNCTIONS________________________________________
    public void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        AddQuad(a, b, c, d, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
    }

    public void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector2 uvA, Vector2 uvB, Vector2 uvC, Vector2 uvD)
    {
        AddTriangle(vertexIndex + 0, vertexIndex + 1, vertexIndex + 2);
        AddTriangle(vertexIndex + 1, vertexIndex + 3, vertexIndex + 2);
        AddVertex(a, uvA);
        AddVertex(b, uvB);
        AddVertex(c, uvC);
        AddVertex(d, uvD);
    }

    //GENERATE MESH_________________________________________
    public Mesh GenerateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        return mesh;
    }
}
