using System.Collections.Generic; 
using UnityEngine;

public class MeshCombiner : MonoBehaviour
{
    private void Start()
    {
        //samla alla child meshfilters
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        Transform parentTransform = transform;

        for (int i = 0; i < meshFilters.Length; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            // Transform vertices relative to parent
            combine[i].transform = parentTransform.worldToLocalMatrix * meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);
        }

        //Skapar nya meshen
        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combine);

        //lägger till meshfilter o mesh renderer om dessa inte finns
        MeshFilter mf = gameObject.AddComponent<MeshFilter>();
        mf.mesh = combinedMesh;

        MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
        mr.sharedMaterial = meshFilters[0].GetComponent<MeshRenderer>().sharedMaterial;

        //lägger till en ny meshcollider
        MeshCollider mc = gameObject.AddComponent<MeshCollider>();
        mc.sharedMesh = combinedMesh;
        mc.convex = false; 

        gameObject.SetActive(true); 
    }
}
