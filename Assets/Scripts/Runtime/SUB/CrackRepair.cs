using UnityEngine;
using UnityEngine.Events;

public class CrackRepair : MonoBehaviour
{
    private bool repaired = false;
    
    //Might do something with the interactText in the future. 
    //public string InteractText => repaired ? "Already Repaired" : "Repair Crack";


    public void Repair()
    {
        if (repaired) return;

        repaired = true;

        gameObject.SetActive(false);
    }
} 