using UnityEngine;
using System.Collections;

public class GiveRandomName : MonoBehaviour {

    Toilet toilet;
    public string nombre;

	void Awake () {
        toilet = GetComponent<Toilet>();

        if (nombre == "") toilet.nombre = (Random.insideUnitCircle).ToString();
        else toilet.nombre = nombre;

        Destroy(this);
	}
}
