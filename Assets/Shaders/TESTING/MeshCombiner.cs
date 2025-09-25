using System.Collections.Generic; 
using UnityEngine;

public class MeshCombiner : MonoBehaviour
{
    private void Start()
    {
        //samla alla child meshfilters
        MeshFilter[] meshfilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshfilters.Length]; 

        for (int i = 0; i < meshfilters.Length; i++)
        {
            combine[i].mesh = meshfilters[i].sharedMesh;
            combine[i].transform = meshfilters[i].transform.localToWorldMatrix;
            meshfilters[i].gameObject.SetActive(false); //stänger av the children (vet inte om är bäst?)
        }

        //Skapar nya meshen
        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combine);

        //lägger till meshfilter o mesh renderer om dessa inte finns
        MeshFilter mf = gameObject.AddComponent<MeshFilter>();
        mf.mesh = combinedMesh;

        MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
        mr.sharedMaterial = meshfilters[0].GetComponent<MeshRenderer>().sharedMaterial;

        //lägger till en ny meshcollider
        MeshCollider mc = gameObject.AddComponent<MeshCollider>();
        mc.sharedMesh = combinedMesh;

        gameObject.SetActive(true); 
    }
}
