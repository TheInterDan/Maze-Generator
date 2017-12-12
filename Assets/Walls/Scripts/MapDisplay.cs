using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;

public class MapDisplay : MonoBehaviour {

    public bool minimap = false;
    bool minimapState;
    float alpha;
    bool initialized = false;

	void Start () {
        
    }
	
    void OnEnable()
    {
        if (!initialized && !(LabyrinthMap.instance == null) && !(LabyrinthMap.instance.display == null))
        {
            GetComponent<RawImage>().texture = LabyrinthMap.instance.display;
        }
    }

	void Update () {
        if (!initialized)
        {
            GetComponent<RawImage>().texture = LabyrinthMap.instance.display;
            alpha = GetComponent<RawImage>().color.a;
            minimapState = Game.minimap;
            if (minimap) UpdateMinimap();
            initialized = true;
        }

        if (minimap)
        {
            if (minimapState != Game.minimap)
            {
                minimapState = Game.minimap;
                UpdateMinimap();
            }
        }
	}

    void UpdateMinimap()
    {
        float newAlpha = 0;
        if (minimapState) newAlpha = alpha;
        Color color = GetComponent<RawImage>().color;
        color.a = newAlpha;
        GetComponent<RawImage>().color = color;
    }

    public void SaveMap()
    {
        Texture2D display, scaledDisplay;
        float divider = 4;
        if (Game.size == 15) divider = 16;
        if (Game.size == 31) divider = 8;
        LabyrinthMap.instance.PrepareScreenshot();
        display = LabyrinthMap.instance.display;
        scaledDisplay = new Texture2D(256, 256);

        for(int y = 0; y < 256; y++)
        {
            for (int x = 0; x < 256; x++)
            {
                scaledDisplay.SetPixel(x, y, display.GetPixel(Mathf.Abs((int)(x / divider - 0.5f)), Mathf.Abs(((int)(y / divider - 0.5f)))));
            }
        }

        byte[] bytes = scaledDisplay.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/../" + LabyrinthGenerator.instance.seed + ".png", bytes);
    }
}
