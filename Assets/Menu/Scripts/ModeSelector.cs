using UnityEngine;
using System.Collections;

public class ModeSelector : MonoBehaviour {

    public GameObject toSwitch;

	void Start () {
        if (Game.randomMode)
        {
            toSwitch.transform.parent = transform.parent;
            toSwitch.transform.position = transform.position;
            Destroy(gameObject);
        }
        else Destroy(this);
	}
	
	void Update () {
	
	}
}
