using UnityEngine;

public class ExitSubmarine : MonoBehaviour
{
    [SerializeField] private float offset;

    void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent<PlayerMovement>(out PlayerMovement pm))
        {
            if (other.transform.position.y < transform.position.y + offset) pm.StateMachine.ChangeState(pm.SwimmingState);
            else pm.StateMachine.ChangeState(pm.StandingState);
        }
    }
}
