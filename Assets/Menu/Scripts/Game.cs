using UnityEngine;
using System.Collections;

public static class Game {

    public static string seed = "default";
    public static bool minimap = true;
    public static int skinMode = 0;
    public static int size = 31;
    public static bool randomMode = false;
    public static bool enemies = true;

    public static string GenerateString()
    {
        Random.InitState(System.DateTime.Now.Month + System.DateTime.UtcNow.Second * System.DateTime.Now.Millisecond + (int)Time.time);
        char[] newString = new char[6];
        newString[0] = ClampChar(Random.Range(0, 1000));
        newString[1] = ClampChar(Random.Range(0, 1000));
        newString[2] = ClampChar(Random.Range(0, 1000));
        newString[3] = ClampChar(Random.Range(0, 1000));
        newString[4] = ClampChar(Random.Range(0, 1000));
        newString[5] = ClampChar(Random.Range(0, 1000));
        return new string(newString);
    }

    static char ClampChar(int charToClamp)
    {
        char charToReturn;
        charToClamp = (Mathf.Abs(charToClamp) % /*36*/ 26) + 1;

        /*if (charToClamp <= 10)
        {
            charToReturn = (char)(charToClamp + 47);
        }
        else*/ charToReturn = (char)(charToClamp + 54 + (10)); //the 2 and the z and the 5 and the S are the same in this font so its better to dont give numbers

        return charToReturn;
    }
}
