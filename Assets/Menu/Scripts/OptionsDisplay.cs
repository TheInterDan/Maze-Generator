using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class OptionsDisplay : MonoBehaviour {

    public enum ToDisplay { Resolution, FullScreen, Minimap, Skin, Size, Seed, Enemies }
    public ToDisplay toDisplay;
    public Text targetText;

	void OnEnable () {
        UpdateTag();
    }

    void Awake()
    {       
        UpdateTag();
    }

    public void UpdateTag () {
        if (targetText == null) targetText = GetComponent<Text>();
        if (toDisplay == ToDisplay.Minimap)
        {
            targetText.text = Binary(Game.minimap);
        }
        if (toDisplay == ToDisplay.Enemies)
        {
            targetText.text = Binary(Game.enemies);
        }
        if (toDisplay == ToDisplay.FullScreen)
        {
            StartCoroutine(UpdateFullScreen()); //Because changes need one frame to happen
        }
        if (toDisplay == ToDisplay.Resolution)
        {
            StartCoroutine(UpdateScreenResolution());
        }
        if (toDisplay == ToDisplay.Skin)
        {
            targetText.text = ReadSkin();
        }
        if (toDisplay == ToDisplay.Size)
        {
            targetText.text = ReadSize();
        }
        if (toDisplay == ToDisplay.Seed)
        {
            if(LabyrinthGenerator.instance != null) targetText.text = LabyrinthGenerator.instance.seed.ToUpper();
            else targetText.text = Game.seed.ToUpper();
        }
    }

    IEnumerator UpdateFullScreen()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        targetText.text = Binary(Screen.fullScreen);
    }

    IEnumerator UpdateScreenResolution()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        int width, height;
        width = Screen.width;
        height = Screen.height;
        targetText.text = width.ToString() + " x " + height.ToString();
    }

    string Binary(bool option)
    {
        if (option) return "yes";
        return "no";
    }

    public string ReadSkin()
    {
        string mode = "default";
        switch (Game.skinMode)
        {
            case 0:
                {
                    mode = "hedge";
                    break;
                }
            case 1:
                {
                    mode = "dungeon";
                    break;
                }
            case 2:
                {
                    mode = "classic";
                    break;
                }
            case 3:
                {
                    mode = "bathroom";
                    break;
                }
        }
        return mode;
    }

    public string ReadSize()
    {
        string mode = "default";
        switch (Game.size)
        {
            case 15:
                {
                    mode = "small";
                    break;
                }
            case 31:
                {
                    mode = "default";
                    break;
                }
            case 63:
                {
                    mode = "big";
                    break;
                }
        }
        return mode;
    }
}
