using UnityEngine;

public class ExitSubmarine : MonoBehaviour
{
    void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent<PlayerMovement>(out PlayerMovement pm))
        {
            if (other.transform.position.y < transform.position.y) pm.StateMachine.ChangeState(pm.SwimmingState);
            else pm.StateMachine.ChangeState(pm.StandingState);
        }
    }
}
