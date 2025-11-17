using UnityEngine;

public class ExitSubmarine : MonoBehaviour
{
    void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent<PlayerMovement>(out PlayerMovement pm))
        {
            if (other.transform.position.y < transform.position.y) pm.SetCurrentState(PlayerMovement.States.swimming);
            else pm.SetCurrentState(PlayerMovement.States.standing);
        }
    }
}
