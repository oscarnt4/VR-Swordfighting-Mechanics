public abstract class PlayerBaseState : IState
{
    protected PlayerController _player;
    protected PlayerStateMachine _stateMachine;

    public PlayerBaseState(PlayerController player)
    {
        this._player = player;
        this._stateMachine = PlayerStateMachine.Instance;
    }
    public abstract void Enter();
    public abstract void Execute();
    public abstract void Exit();
    public abstract bool CanEnter(IState currentState);
    public abstract bool CanExit();
}
