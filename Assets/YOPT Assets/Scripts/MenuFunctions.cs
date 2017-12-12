using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;

public class MenuFunctions : MonoBehaviour {

    public Text entradaNombre;
    public Text salidaNombre;

    public GameObject panel;
    public GameObject options;
    public GameObject saveButton;

    public GameObject camara;

    public GameObject toilet;
    public Texture2D tex;

    void Start()
    {
        panel.SetActive(true);
        options.SetActive(false);
    }

    /*public void Photo()
    {
        saveButton.SetActive(false);
        StartCoroutine(TakePhoto()); ;
    }

    IEnumerator TakePhoto()
    {
        yield return new WaitForEndOfFrame();
        
        tex = new Texture2D(Screen.width, Screen.height);
        tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        tex.Apply();

        // Encode texture into PNG
        byte[] bytes = tex.EncodeToPNG();
        Destroy(tex);

        // Write to a file in the project folder
        File.WriteAllBytes(Application.dataPath + "/../SavedScreen.png", bytes);

        saveButton.SetActive(true);
    }*/

    public void Vale()
    {
        CreateToilet(entradaNombre.text);
        salidaNombre.text = entradaNombre.text;

        panel.SetActive(false);
        options.SetActive(true);
        camara.GetComponent<CameraControl>().enabled = true;
    }

    public void New()
    {
        Destroy(toilet);

        panel.SetActive(true);
        options.SetActive(false);
        camara.GetComponent<CameraControl>().enabled = false;

        entradaNombre.GetComponentInParent<InputField>().text = "";
    }

    void CreateToilet(string nombre)
    {
        toilet = new GameObject("Toilet");
        Toilet toiletToilet = toilet.AddComponent<Toilet>();
        toiletToilet.nombre = nombre;
    }
}