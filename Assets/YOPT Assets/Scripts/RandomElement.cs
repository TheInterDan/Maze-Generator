using UnityEngine;
using System.Collections;

public class RandomElement : MonoBehaviour {

    public bool homogeneous = true;

    public bool allMaterials = true;
    public bool[] specificMaterials;

    public Color[] color;

    public Texture[] texture;

    public Material[] material;



    private Toilet mToilet;
    private Material[] mMaterials;

	void LateUpdate () {
        mToilet = GetComponentInParent<Toilet>();

        //Cogemos la lista de materiales
        mMaterials = GetComponent<MeshRenderer>().materials;
        //La editamos
        SetRandom();
        //La devolvemos
        GetComponent<MeshRenderer>().materials = mMaterials;

        Destroy(this);
	}
	
	void SetRandom()
    {
        int nombre = Toilet.CalculateNumber(mToilet.nombre);
       

        int i = 0;
        int number = 0;

        //renderer.materials gives just a copy of the materials array, not a reference.Changing the materials in it doesn't have any effect on the actual materials of the renderer.
        //What works is to substitute the whole array at once with a new preconfigured materials array:

        foreach (Material mat in mMaterials)
        {
            if (allMaterials || specificMaterials[i])             
            {   

                if (homogeneous)
                    number = nombre + 1;

                else number = (int)Mathf.Floor(nombre * (i + 1) / 3.0f);

                if (material.Length > 0)
                    mMaterials[i] = material[Mathf.Abs((nombre + number) % material.Length)];              

                if (texture.Length > 0)
                    mat.mainTexture = texture[Mathf.Abs((nombre - (int)(Mathf.Floor(nombre/3.0f))) % texture.Length)];

                if (color.Length > 0)
                    mat.color = color[Mathf.Abs((number) % color.Length)];
            }

            i++;
        }
    }

}
