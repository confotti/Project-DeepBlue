using UnityEngine;

public class Flashlight : MonoBehaviour
{
    public Light flashLight; 

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F))
        {
            flashLight.enabled = !flashLight.enabled; 
        }
    }
}
