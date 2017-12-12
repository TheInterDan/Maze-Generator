using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class LabyrinthMap : MonoBehaviour {

    public Transform player;
    Vector3 prevPlayerPosition;

    int mapSize = 1;
    float gridScale = 1;

    Texture2D realMap;
    public Texture2D display;
    [HideInInspector] public static LabyrinthMap instance;
    int[,] discoveredArea;

    Color[] backGround;

    bool initialized = false;
    bool end = false;

    List<EnemyBase> enemies = new List<EnemyBase>();

    void Start () {
        instance = this;

        mapSize = LabyrinthGenerator.instance.size;
        display = new Texture2D(mapSize, mapSize, TextureFormat.RGBA32, false);
        display.filterMode = FilterMode.Point;
        display.wrapMode = TextureWrapMode.Clamp;
        //GetComponent<RawImage>().texture = display;
        realMap = LabyrinthGenerator.instance.mapTexture;

        gridScale = LabyrinthGenerator.instance.scale;
        prevPlayerPosition = TransformPointToGrid(player.position);

        enemies = FindObjectsOfType<EnemyBase>().ToList();

        discoveredArea = new int[mapSize, mapSize];
        
        backGround = new Color[mapSize * mapSize];
        for (int i = 0; i < mapSize * mapSize; i++)
        {
            backGround[i] = Color.black;
        }      
    }
	
	void Update () {
        if (!initialized)
        {
            prevPlayerPosition = -Vector3.one; 
            UpdateMapDisplay();
            display.SetPixels(backGround);
            initialized = true;
        }
        if (!end)
        {
            Vector3 playerPosition = TransformPointToGrid(player.position);
            //if (Mathf.Floor(playerPosition.x) != Mathf.Floor(prevPlayerPosition.x) || Mathf.Floor(playerPosition.z) != Mathf.Floor(prevPlayerPosition.z))
            //{
                prevPlayerPosition = playerPosition;
                AddPointToDiscoveredArea(playerPosition);
                UpdateMapDisplay();
            //}

            if (player.GetComponent<UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController>().end)
            {
                UpdateMapDisplay(false);
                end = true;
            }
        }
    }

    Vector3 TransformPointToGrid(Vector3 realPoint)
    {
        return (realPoint / gridScale) + Vector3.one * 0.5f;
    }

    void UpdateMapDisplay(bool showPlayer = true)
    {
        
        for (int y = 0; y < mapSize; y++)
        {
            for (int x = 0; x < mapSize; x++)
            {
                if (discoveredArea[x, y] == 1)
                {
                    display.SetPixel(x, y, realMap.GetPixel(x, y));
                }
            }
        }
        if (showPlayer)
        {
            display.SetPixel(Mathf.FloorToInt(prevPlayerPosition.x), Mathf.FloorToInt(prevPlayerPosition.z), Color.cyan);
            foreach (EnemyBase enemy in enemies)
            {
                Vector3 enemyPosition = TransformPointToGrid(enemy.transform.position);
                enemyPosition = new Vector3(Mathf.FloorToInt(enemyPosition.x), 0, Mathf.FloorToInt(enemyPosition.z));
                if (discoveredArea[(int)enemyPosition.x, (int)enemyPosition.z] == 1)
                    display.SetPixel((int)enemyPosition.x, (int)enemyPosition.z, Color.red);
            }
        }
        display.Apply();
    }

    void AddPointToDiscoveredArea(Vector3 point)
    {
        int x = Mathf.FloorToInt(point.x);
        int y = Mathf.FloorToInt(point.z);
        discoveredArea[Mathf.Clamp(x, 0, mapSize), Mathf.Clamp(y, 0, mapSize)] = 1;
        discoveredArea[Mathf.Clamp(x, 0, mapSize), Mathf.Clamp(y+1, 0, mapSize)] = 1;
        discoveredArea[Mathf.Clamp(x+1, 0, mapSize), Mathf.Clamp(y, 0, mapSize)] = 1;
        discoveredArea[Mathf.Clamp(x, 0, mapSize), Mathf.Clamp(y-1, 0, mapSize)] = 1;
        discoveredArea[Mathf.Clamp(x-1, 0, mapSize), Mathf.Clamp(y, 0, mapSize)] = 1;
    }

    public void PrepareScreenshot()
    {
        UpdateMapDisplay(false);
    }
}
