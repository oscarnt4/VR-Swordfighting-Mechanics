using UnityEngine;

public class StayWithinPlayerRange : ComplexEnemyBaseState
{
    public StayWithinPlayerRange(ComplexEnemyController _enemy, EnemyStateMachine _stateMachine) : base(_enemy, _stateMachine) { }

    public override void Enter()
    {
        Debug.Log("Enemy: Entering StayWithinPlayerRange State");
        _enemy.EnterStayWithinPlayerRange();
    }

    public override void Execute()
    {
        _enemy.ExecuteStayWithinPlayerRange();
    }

    public override void Exit()
    {
        Debug.Log("Enemy: Exiting StayWithinPlayerRange State");
        _enemy.ExitStayWithinPlayerRanger();
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

public class StayOutsidePlayerRange : ComplexEnemyBaseState
{
    public StayOutsidePlayerRange(ComplexEnemyController _enemy, EnemyStateMachine _stateMachine) : base(_enemy, _stateMachine) { }

    public override void Enter()
    {
        Debug.Log("Enemy: Entering StayOutsidePlayerRange State");
        _enemy.EnterStayOutsidePlayerRange();
    }

    public override void Execute()
    {
        _enemy.ExecuteStayOutsidePlayerRange();
    }

    public override void Exit()
    {
        Debug.Log("Enemy: Exiting StayOutsidePlayerRange State");
        _enemy.ExitStayOutsidePlayerRange();
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

public class Block : ComplexEnemyBaseState
{
    public Block(ComplexEnemyController _enemy, EnemyStateMachine _stateMachine) : base(_enemy, _stateMachine) { }

    public override void Enter()
    {
        Debug.Log("Enemy: Entering Block State");
        _enemy.EnterBlock();
    }

    public override void Execute()
    {
        _enemy.ExecuteBlock();
    }

    public override void Exit()
    {
        Debug.Log("Enemy: Exiting Block State");
        _enemy.ExitBlock();
    }

    public override bool CanEnter(IState currentState)
    {
        return _enemy.CanEnterBlock(currentState);
    }

    public override bool CanExit()
    {
        return true;
    }
}

public class BackDash : ComplexEnemyBaseState
{
    public BackDash(ComplexEnemyController _enemy, EnemyStateMachine _stateMachine) : base(_enemy, _stateMachine) { }

    public override void Enter()
    {
        Debug.Log("Enemy: Entering BackDash State");
        _enemy.EnterBackDash();
    }

    public override void Execute()
    {
        _enemy.ExecuteBackDash();
    }

    public override void Exit()
    {
        Debug.Log("Enemy: Exiting BackDash State");
        _enemy.ExitBackDash();
    }

    public override bool CanEnter(IState currentState)
    {
        return _enemy.CanEnterBackDash(currentState);
    }

    public override bool CanExit()
    {
        return true;
    }
}

public class RandomStationaryAttack : ComplexEnemyBaseState
{
    public RandomStationaryAttack(ComplexEnemyController _enemy, EnemyStateMachine _stateMachine) : base(_enemy, _stateMachine) { }

    public override void Enter()
    {
        Debug.Log("Enemy: Entering RandomStationaryAttack State");
    }

    public override void Execute()
    {
        _enemy.ExecuteRandomStationaryAttack();
    }

    public override void Exit()
    {
        Debug.Log("Enemy: Exiting RandomStationaryAttack State");
        _enemy.ExitRandomStationaryAttack();
    }

    public override bool CanEnter(IState currentState)
    {
        return _enemy.CanEnterRandomStationaryAttack(currentState);
    }

    public override bool CanExit()
    {
        return true;
    }
}

public class RandomSprintAttack : ComplexEnemyBaseState
{
    public RandomSprintAttack(ComplexEnemyController _enemy, EnemyStateMachine _stateMachine) : base(_enemy, _stateMachine) { }

    public override void Enter()
    {
        Debug.Log("Enemy: Entering RandomSprintAttack State");
    }

    public override void Execute()
    {
        _enemy.ExecuteRandomSprintAttack();
    }

    public override void Exit()
    {
        Debug.Log("Enemy: Exiting RandomSprintAttack State");
        _enemy.ExitRandomSprintAttack();
    }

    public override bool CanEnter(IState currentState)
    {
        return _enemy.CanEnterRandomSprintAttack(currentState);
    }

    public override bool CanExit()
    {
        return true;
    }
}

public class Stun : ComplexEnemyBaseState
{
    public Stun(ComplexEnemyController _enemy, EnemyStateMachine _stateMachine) : base(_enemy, _stateMachine) { }

    public override void Enter()
    {
        Debug.Log("Enemy: Entering Stun State");
        _enemy.EnterStun();
    }

    public override void Execute()
    {
        _enemy.ExecuteStun();
    }

    public override void Exit()
    {
        Debug.Log("Enemy: Exiting Stun State");
        _enemy.ExitStun();
    }

    public override bool CanEnter(IState currentState)
    {
        return _enemy.CanEnterStun(currentState);
    }

    public override bool CanExit()
    {
        return _enemy.CanExitStun();
    }
}
