using UnityEngine;

public class DustParticles : MonoBehaviour
{
    public ParticleSystem particles;
    public Rigidbody playerRb;
    public float moveThreshold = 0.1f;

    void Update()
    {
        float speed = playerRb.linearVelocity.magnitude;

        if (speed > moveThreshold)
        {
            if (!particles.isPlaying)
                particles.Play();
        }
        else
        {
            if (particles.isPlaying)
                particles.Stop(); // stops emission, lets existing particles fade
        }
    }
}