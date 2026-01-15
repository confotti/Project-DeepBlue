using UnityEngine;

public class GlowstickItem : ItemBehaviour
{
    //Maybe make a throwable class that this can inherit from if we wanna throw a bunch of stuff. 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void PrimaryInput()
    {
        base.PrimaryInput();

        player.ConsumeCurrentItem();
        
        //Spawn and throw a glowstick here. 
    }
}
