using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuPanel : MonoBehaviour {

    public bool mainPanel = false;
    public bool startActive = false;

    public MenuPanel prevPanel;

    bool initiated = false;

	void Awake () {
        transform.localPosition = Vector3.zero;
        gameObject.SetActive(startActive);
    }
	
    void Start()
    {
       /* if (!initiated)
        {
            gameObject.SetActive(startActive);
            initiated = true;
        }*/
    }

    void OnEnable()
    {
        if (mainPanel) ResetPanel();
    }

    public void ResetPanel()
    {
        if (mainPanel)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                MenuPanel panel = transform.GetChild(i).GetComponent<MenuPanel>();
                if (panel != null) panel.ResetPanel();
            }
        }
        else gameObject.SetActive(startActive);
    }

    public void ChangePanel(MenuPanel otherPanel)
    {
        ChangePanel(otherPanel, true);
    }

    public void ShowPanel(MenuPanel otherPanel)
    {
        otherPanel.gameObject.SetActive(true);
    }

    public void HidePanel(MenuPanel otherPanel)
    {
        otherPanel.gameObject.SetActive(false);
    }

    public void ChangePanel(MenuPanel otherPanel, bool savePrevious)
    {
        otherPanel.gameObject.SetActive(true); 
        if (savePrevious) otherPanel.prevPanel = this;          
        gameObject.SetActive(false);
    }

    public void Back()
    {
        if (prevPanel != null) ChangePanel(prevPanel, false);
    }

    public void UpdateSeed(Text newSeed)
    {
        Game.seed = newSeed.text;
        //print(Game.seed);
    }

    public void ChangeScene(bool randomize = false)
    {
        //print(System.DateTime.Now.Second * 100000 + System.DateTime.Now.Second * 100 + System.DateTime.Now.Minute * 10000 + System.DateTime.Now.Year / 100 + Time.time * 100 + Time.time);
        if (Game.randomMode) {
            Game.seed = Game.GenerateString();
            SceneManager.LoadScene(Random.Range(1, 5));
        }
        else SceneManager.LoadScene(Game.skinMode + 1);
    }

    public void Reset()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void ChangeMinimap()
    {
        Game.minimap = !Game.minimap;
    }

    public void ChangeEnemies()
    {
        Game.enemies = !Game.enemies;
    }

    public void ChangeFullScreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }

    public void ChangeResolution(int i)
    {
        switch (i)
        {
            case 0:
                {
                    //1920x1080
                    Screen.SetResolution(1920, 1080, Screen.fullScreen);
                    break;
                }
            case 1:
                {
                    //1600x900
                    Screen.SetResolution(1600, 900, Screen.fullScreen);
                    break;
                }
            case 2:
                {
                    //1280x720
                    Screen.SetResolution(1280, 720, Screen.fullScreen);
                    break;
                }
            case 3:
                {
                    //568x320
                    Screen.SetResolution(568, 320, Screen.fullScreen);
                    break;
                }
        }
    }

    public void ChangeSkin(int i)
    {
        Game.skinMode = i;
    }

    public void ChangeSize(int i)
    {
        Game.size = i;
    }

    public void RandomMode(bool doIt)
    {
        Game.randomMode = doIt;
    }

    public void Quit()
    {
        Application.Quit();
    }
}
