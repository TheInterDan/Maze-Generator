using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;

public class TextClamp : MonoBehaviour {

    public UnityEvent afterClamp;

    InputField textToClamp;
    string prevText;

    void Start () {
        textToClamp = GetComponent<InputField>();
    }
	
	void Update () {
        if (textToClamp.text != prevText) Clamp();
	}

    void Clamp()
    {      
        if (textToClamp != null)
        {
            textToClamp.text = textToClamp.text.ToUpper();
            string clampedText = "";
            for (int i = 0; i < textToClamp.text.Length; i++)
            {
                int c = textToClamp.text[i];
                if ((c >= 65 && c <= 90) || (c >= 48 && c <= 57) || c==32)
                {
                    clampedText += (char)c;
                }
            }
            textToClamp.text = clampedText;
        }
        prevText = textToClamp.text;

        afterClamp.Invoke();
    }
}
