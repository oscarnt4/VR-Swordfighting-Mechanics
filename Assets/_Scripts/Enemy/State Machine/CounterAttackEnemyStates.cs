using UnityEngine;

public class EnemyChasingState : CounterAttackEnemyBaseState
{
    public EnemyChasingState(CounterAttackEnemyController _enemy, EnemyStateMachine _stateMachine) : base(_enemy, _stateMachine) { }
    public override void Enter()
    {
        Debug.Log("Enemy: Entering Chasing State");
        _enemy.EnterChase();
    }

    public override void Execute()
    {
        _enemy.ExecuteChase();
    }

    public override void Exit()
    {
        Debug.Log("Enemy: Exiting Chasing State");
        _enemy.ExitChase();
    }

    public override bool CanEnter(IState currentState)
    {
        return true;
    }

    public override bool CanExit()
    {
        return true;
    }
}

public class EnemyVerticalSlashState : CounterAttackEnemyBaseState
{
    public EnemyVerticalSlashState(CounterAttackEnemyController _enemy, EnemyStateMachine _stateMachine) : base(_enemy, _stateMachine) { }
    public override void Enter()
    {
        Debug.Log("Enemy: Entering Vertical Slash State");
    }

    public override void Execute()
    {
        _enemy.ExecuteVerticalSlash();
    }

    public override void Exit()
    {
        Debug.Log("Enemy: Exiting Vertical Slash State");
        _enemy.ExitVerticalSlash();
    }

    public override bool CanEnter(IState currentState)
    {
        return true;
    }

    public override bool CanExit()
    {
        return _enemy.CanExitVerticalSlash();
    }
}