public abstract class ComplexEnemyBaseState : IState
{
    protected ComplexEnemyController _enemy;
    protected EnemyStateMachine _stateMachine;

    public ComplexEnemyBaseState(ComplexEnemyController enemy, EnemyStateMachine stateMachine)
    {
        this._enemy = enemy;
        this._stateMachine = stateMachine;
    }
    public abstract void Enter();
    public abstract void Execute();
    public abstract void Exit();
    public abstract bool CanEnter(IState currentState);
    public abstract bool CanExit();
}
