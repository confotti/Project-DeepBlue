using UnityEngine;

public class FollowNoRotation : MonoBehaviour
{
    public Transform target;

    void LateUpdate()
    {
        if (target == null) return;

        // Follow position
        transform.position = target.position;

        // Keep fixed rotation
        transform.rotation = Quaternion.identity;
    }
} 