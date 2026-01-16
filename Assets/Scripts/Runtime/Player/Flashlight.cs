using UnityEngine;

public class Flashlight : MonoBehaviour
{
    public Light flashLight; 
    public InventoryItemData flashlightItemData;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (!GetComponent<PlayerInventoryHolder>().InventorySystem.ContainsItem(flashlightItemData, out var ab)) return;
            flashLight.enabled = !flashLight.enabled;
        }

        if (!GetComponent<PlayerInventoryHolder>().InventorySystem.ContainsItem(flashlightItemData, out var a)) flashLight.enabled = false;
    }
}
