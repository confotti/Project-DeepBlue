using System.Collections.Generic; 
using UnityEngine;

public class InteractiveGrass : MonoBehaviour
{
    public GameObject player;
    public List<Material> mats; 

    private void Update()
    {
        if (player == null || mats == null) return;

        Vector3 playerPos = player.transform.position; 

        foreach (Material mat in mats)
        {
            if (mat != null)
            {
                mat.SetVector("_TramplePosition", playerPos); 
            }
        }
    }
}
