using UnityEngine;

public class GlowstickItem : ItemBehaviour
{
    //Maybe make a throwable class that this can inherit from if we wanna throw a bunch of stuff. 

    public override void PrimaryInput()
    {
        base.PrimaryInput();

        _player.ConsumeCurrentItem();

        //Spawn and throw a glowstick here. 
        var spawnedGlowstick = Instantiate(gameObject, _player.PlayerHead.transform.position + _player.PlayerHead.transform.forward, _player.PlayerHead.transform.rotation);
        spawnedGlowstick.GetComponent<Collider>().enabled = true;
        var rb = spawnedGlowstick.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.AddForceAtPosition(_player.PlayerHead.transform.forward * 500, rb.transform.position + rb.transform.up * 0.2f);
        spawnedGlowstick.GetComponentInChildren<Light>().enabled = true;
    }
}
