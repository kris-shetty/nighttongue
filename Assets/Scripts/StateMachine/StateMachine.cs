using UnityEngine;

public abstract class StateMachine : MonoBehaviour
{
    protected BaseState CurrentState;
    protected BaseState QueuedState = null;
    protected bool isTransitioningState = false;
    private void Start()
    {
        CurrentState.EnterState();
    }
    private void FixedUpdate()
    {
        CurrentState.FixedUpdateState();
        if (!isTransitioningState && QueuedState != null)
        {
            TransitionToState(QueuedState);
            QueuedState = null;
        }
    }

    public void QueueNextState(BaseState nextState)
    {
        QueuedState = nextState;
    }

    public void TransitionToState(BaseState nextState)
    {
        isTransitioningState = true;
        CurrentState.ExitState();
        CurrentState = nextState;
        CurrentState.EnterState();
        isTransitioningState = false;
    }

    void OnTriggerEnter(Collider other)
    {
        CurrentState.OnTriggerEnter(other);
    }

    void OnTriggerStay(Collider other)
    {
        CurrentState.OnTriggerStay(other);

    }

    void OnTriggerExit(Collider other)
    {
        CurrentState.OnTriggerExit(other);
    }
}