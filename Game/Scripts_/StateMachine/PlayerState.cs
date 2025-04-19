using UnityEngine;

public abstract class PlayerState : ScriptableObject
{
    public abstract void OnEnter(PlayerStateMachine player);
    public abstract void OnUpdate(PlayerStateMachine player);
    public abstract void OnExit(PlayerStateMachine player);
}
