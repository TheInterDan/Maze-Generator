using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gyroscope : MonoBehaviour {

    Quaternion rotation;

	void Awake () {
        rotation = transform.rotation;	
	}
	
	void Update () {
        float y = transform.position.y;
        Vector3 newPosition = GridF.TransformGridToPoint(GridF.TransformPointToGrid(transform.parent.transform.position));
        newPosition.y = y;
        transform.position = newPosition;
        transform.rotation = rotation;
	}
}
