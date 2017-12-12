using UnityEngine;
using System.Collections;

public class Toilet : MonoBehaviour {

    public string nombre;
    static public int flush = 7;
    static public int sit=8;
    static public int throne=8;

	void Start () {
        CreateToilet(nombre);
	}
	
	void Update () {
	
	}

    void CreateToilet(string value)
    {
        int rando = CalculateNumber(value);
        string number = "000";

        //Flush
        number=Calculate(rando,flush,0);
        InstantiatePart("Toilet/Flush/"+number);

        //Sit
        number = Calculate(rando, sit,1);
        InstantiatePart("Toilet/Sit/"+number);

        //Throne
        number = Calculate(rando, throne,2);
        InstantiatePart("Toilet/Throne/"+number);
    }

    void InstantiatePart(string path)
    {
        GameObject obj = Instantiate(Resources.Load(path) as GameObject);       
        obj.transform.parent = transform;
        obj.transform.position = transform.position;
        obj.transform.localEulerAngles = new Vector3(0,90,0);
        obj.transform.localScale = Vector3.one;
    }

    string Calculate(int number, int max, int time)
    {
        //Apply seed
        Random.seed=number+time;
        number = (int)Mathf.Round(Random.value*1000);

        //Clamp the number
        number = number % max;
        if (number == 0) number = max;

        //Format the number to string
        if (number < 1000)
        {
            string strng = number.ToString("000");
            return strng;
        }
        //Return 000 in case of error
        else return "000";
    }

    static public int CalculateNumber(string strng)
    {
        string value=strng.ToLower();  

        int result = 0;
        for (int i = 0; i < value.Length; i++)
        {
            char letter = value[i];
            result = 10 * result + (letter);
        }
        return result;
    }

}
