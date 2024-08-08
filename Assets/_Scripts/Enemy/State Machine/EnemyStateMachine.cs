using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateMachine
{
    private IState _currentState;
    private IState _previousState;
    public IState CurrentState => _currentState;
    public IState PreviousState => _previousState;

    public void ChangeState(IState newState)
    {
        if (_currentState != null && !(_currentState.CanExit() && newState.CanEnter(_currentState)))
        {
            return;
        }

        if (_currentState != null)
        {
            _currentState.Exit();
            _previousState = _currentState;
        }
        _currentState = newState;
        _currentState.Enter();
    }

    public void Update()
    {
        if (_currentState != null)
        {
            _currentState.Execute();
        }
    }
}
