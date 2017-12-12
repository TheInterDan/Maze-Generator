using UnityEngine;
using System.Collections;

public class ToiletGoal : MonoBehaviour {

    public GameObject toilet;
    // Use this for initialization
    void Start () {
        Toilet toiletToilet = gameObject.AddComponent<Toilet>();
        toiletToilet.nombre = LabyrinthGenerator.instance.seed;
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
