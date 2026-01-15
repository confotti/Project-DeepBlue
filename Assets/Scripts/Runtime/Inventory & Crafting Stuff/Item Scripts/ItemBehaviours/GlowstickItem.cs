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
        var spawnedGlowstick = Instantiate(gameObject, player.PlayerHead.transform.position + player.PlayerHead.transform.forward, player.PlayerHead.transform.rotation);
        spawnedGlowstick.GetComponent<Collider>().enabled = true;
        var rb = spawnedGlowstick.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.AddForceAtPosition(player.PlayerHead.transform.forward * 500, rb.transform.position + rb.transform.up * 0.2f);
        spawnedGlowstick.GetComponentInChildren<Light>().enabled = true;
    }
}
