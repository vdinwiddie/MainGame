using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventForwarder : MonoBehaviour
{
    public PlayerStateMachine stateMachine;

    public void OnAnimationEvent(string eventName)
    {
        if (stateMachine.currentState is IReceivesAnimationEvent receiver)
        {
            receiver.OnAnimationEvent(eventName, stateMachine);
        }
    }
}
