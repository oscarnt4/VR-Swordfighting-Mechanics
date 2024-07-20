public interface IState
{
    void Enter();
    void Execute();
    void Exit();
    bool CanEnter(IState currentState);
    bool CanExit();
}
