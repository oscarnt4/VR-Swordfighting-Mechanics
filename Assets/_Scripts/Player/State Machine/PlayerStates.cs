using UnityEngine;

public class WalkingState : PlayerBaseState
{
    public WalkingState(PlayerController _player) : base(_player) { }
    public override void Enter()
    {
        Debug.Log("Entering Walking State");
        _player.EnterWalk();
    }

    public override void Execute() { }

    public override void Exit()
    {
        Debug.Log("Exiting Walking State");
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

public class SprintingState : PlayerBaseState
{
    public SprintingState(PlayerController _player) : base(_player) { }
    public override void Enter()
    {
        Debug.Log("Entering Sprinting State");
        _player.EnterSprint();
    }

    public override void Execute()
    {
        if (!_player.MoveForwardHeld())
        {
            _stateMachine.ChangeState(new WalkingState(_player));
        }
    }

    public override void Exit()
    {
        Debug.Log("Exiting Sprinting State");
    }

    public override bool CanEnter(IState currentState)
    {
        if (currentState is WalkingState || currentState is CrouchingState)
        {
            return _player.IsGrounded() && _player.MoveForwardHeld();
        }
        return false;
    }

    public override bool CanExit()
    {
        return true;
    }
}

public class CrouchingState : PlayerBaseState
{
    public CrouchingState(PlayerController _player) : base(_player) { }
    public override void Enter()
    {
        Debug.Log("Entering Crouching State");
        _player.EnterCrouch();
    }

    public override void Execute() { }

    public override void Exit()
    {
        Debug.Log("Exiting Crouching State");
        _player.ExitCrouch();
    }

    public override bool CanEnter(IState currentState)
    {
        return currentState is WalkingState || currentState is SprintingState;
    }

    public override bool CanExit()
    {
        return true; //may need to add condition for if ceiling is too low to stand
    }
}

public class DashingState : PlayerBaseState
{
    private bool dashComplete = false;
    public DashingState(PlayerController _player) : base(_player) { }
    public override void Enter()
    {
        Debug.Log("Entering Dashing State");
        _player.EnterDash();
        dashComplete = false;
    }

    public override void Execute()
    {
        dashComplete = _player.ExecuteDash();
        Debug.Log(dashComplete + " || " + _player.IsGrounded());
        if (dashComplete)
        {
            _stateMachine.ChangeState(new WalkingState(_player));
        }
    }

    public override void Exit()
    {
        Debug.Log("Exiting Dashing State");
        _player.ExitDash();
    }

    public override bool CanEnter(IState currentState)
    {
        return currentState is WalkingState && _player.OutsideDashWindow();
    }

    public override bool CanExit()
    {
        return dashComplete;
    }
}
