using UnityEngine;
using System.Collections;

public class Goal : MonoBehaviour {

    public bool enemyTrigger = false;

	void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController>() != null)
        {
            if (!enemyTrigger)
            {
                other.GetComponent<UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController>().End();
            }
            else
            {
                other.GetComponent<UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController>().Kill();
            }
        }
    }
}
