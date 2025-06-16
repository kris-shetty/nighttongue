using System.Collections.Generic;
using System;
using UnityEngine;

public abstract class StateMachine : MonoBehaviour
{
    protected BaseState currentState;
    protected BaseState queuedState = null;
    protected bool isTransitioningState = false;
    private void Start()
    {
        currentState.EnterState();
    }
    private void Update()
    {
        currentState.UpdateState();
        if (!isTransitioningState && queuedState != null)
        {
            TransitionToState(queuedState);
            queuedState = null;
        }
    }

    public void QueueNextState(BaseState nextState)
    {
        queuedState = nextState;
    }

    public void TransitionToState(BaseState nextState)
    {
        isTransitioningState = true;
        currentState.ExitState();
        currentState = nextState;
        currentState.EnterState();
        isTransitioningState = false;
    }

    void OnTriggerEnter(Collider other)
    {
        currentState.OnTriggerEnter(other);
    }

    void OnTriggerStay(Collider other)
    {
        currentState.OnTriggerStay(other);

    }

    void OnTriggerExit(Collider other)
    {
        currentState.OnTriggerExit(other);
    }
}