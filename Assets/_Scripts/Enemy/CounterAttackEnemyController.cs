using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class CounterAttackEnemyController : MonoBehaviour
{
    [Header("Weapon")]
    [SerializeField] float armLength = 0.9f;
    [SerializeField] float blockReactionDistance = 2f;
    [SerializeField] float blockSpeed = 10f;
    [SerializeField] float blockRotationSpeed = 50f;
    [SerializeField] GameObject hand;
    [Header("Movement")]
    [SerializeField] float turnSpeed = 2f;
    [SerializeField] float stoppingDistanceFromPlayer = 3f;

    private EnemyStateMachine _stateMachine;
    private EnemyChasingState _chasingState;
    private EnemyVerticalSlashState _verticalSlashState;
    private EnemyStunState _stunState;

    private Damage[] potentialThreats;
    private NavMeshAgent _navMeshAgent;
    private Animator _animator;

    private Vector3 handStartLocalPosition;
    private Quaternion handStartLocalRotation;
    private Vector3 localTargetPosition;
    private Quaternion localTargetRotation;

    private bool isSlashing = false;
    private float currentStunTime = 0f;
    private float stunStartTime = 0f;

    private void Awake()
    {
        _stateMachine = new EnemyStateMachine();
        _chasingState = new EnemyChasingState(this, _stateMachine);
        _verticalSlashState = new EnemyVerticalSlashState(this, _stateMachine);
        _stunState = new EnemyStunState(this, _stateMachine);
    }

    void Start()
    {
        potentialThreats = FindObjectsOfType<Damage>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();

        _animator.enabled = false;

        handStartLocalPosition = hand.transform.localPosition;
        handStartLocalRotation = Quaternion.identity;

        _stateMachine.ChangeState(_chasingState);

    }

    void Update()
    {
        _stateMachine.Update();
    }

    public void EnterChase()
    {
        _navMeshAgent.SetDestination(Camera.main.transform.position);
        _animator.enabled = false;
    }

    public void ExecuteChase()
    {
        ExecuteBlock();
        LookAtPlayer();
        if (Vector3.Distance(this.transform.position, Camera.main.transform.position) < stoppingDistanceFromPlayer)
        {
            _navMeshAgent.isStopped = true;
        }
        else
        {
            _navMeshAgent.isStopped = false;
            _navMeshAgent.SetDestination(Camera.main.transform.position);
        }
    }

    public void ExitChase()
    {
        _navMeshAgent.SetDestination(this.transform.position);
        _navMeshAgent.isStopped = true;
    }

    public void ExecuteVerticalSlash()
    {
        if (!isSlashing && UpdateHandLocalPosition(handStartLocalPosition) && UpdateHandLocalRotation(handStartLocalRotation))
        {
            isSlashing = true;
            _animator.enabled = true;
            _animator.Play("Enemy_SlashVerticalDown", 0, 0f);
        }

        if (isSlashing)
        {
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("Enemy_SlashVerticalDown"))
            {
                Debug.Log(stateInfo.normalizedTime);
                if (stateInfo.normalizedTime >= 1.0f)
                {
                    _animator.Play("Default", 0, 0f);
                    _animator.Rebind();
                    _stateMachine.ChangeState(_chasingState);
                    //_stateMachine.ChangeState(_verticalSlashState); //test
                }
            }
        }
    }

    public void ExitVerticalSlash()
    {
        isSlashing = false;
    }

    public bool CanEnterVerticalSlash(IState currentState)
    {
        return !(currentState is EnemyVerticalSlashState);
    }

    public void EnterStun()
    {
        stunStartTime = Time.time;
        _animator.speed = 0f;
    }

    public void ExecuteStun()
    {
        if(Time.time >= stunStartTime + 2f/*currentStunTime*/)
        {
            _animator.speed = 1f;
            _stateMachine.ChangeState(_chasingState);
        }
    }

    public void ExitStun()
    {
        _animator.Play("Default", 0, 0f);
        _animator.Rebind();
        _animator.enabled = false;
    }

    public bool CanEnterStun(IState currentState)
    {
        return currentState is EnemyVerticalSlashState;
    }

    public bool CanExitStun()
    {
        return Time.time >= stunStartTime + 2f/*currentStunTime*/;
    }

    private void ExecuteBlock()
    {
        localTargetPosition = handStartLocalPosition;
        localTargetRotation = handStartLocalRotation;

        foreach (Damage threat in potentialThreats)
        {
            if (/*threat.DamageAmount > 0 &&*/ threat.tag != "Enemy" && Vector3.Distance(this.transform.position, threat.CenterOfMass) <= blockReactionDistance)
            {
                Vector3 threatLocalPosition = this.transform.InverseTransformPoint(threat.CenterOfMass);
                Vector3 direction = (threatLocalPosition - handStartLocalPosition).normalized;
                localTargetPosition = handStartLocalPosition + new Vector3(direction.x, direction.y, 0f) * armLength;

                float radianZAngle = Mathf.Atan2(direction.y, direction.x);
                float eulerZAngle = radianZAngle * Mathf.Rad2Deg;

                if (eulerZAngle > 90f) eulerZAngle -= 180f;
                else if (eulerZAngle < -90f) eulerZAngle += 180f;

                localTargetRotation = Quaternion.Euler(0f, 0f, eulerZAngle);
                break;
            }
        }
        UpdateHandLocalPosition(localTargetPosition);
        UpdateHandLocalRotation(localTargetRotation);
    }

    private bool UpdateHandLocalPosition(Vector3 target)
    {
        hand.transform.localPosition = Vector3.MoveTowards(hand.transform.localPosition, target, blockSpeed * Time.deltaTime);
        if (Vector3.Distance(hand.transform.localPosition, target) < 0.01f)
        {
            hand.transform.localPosition = target;
            return true;
        }
        return false;
    }

    private bool UpdateHandLocalRotation(Quaternion target)
    {
        hand.transform.localRotation = Quaternion.Slerp(hand.transform.localRotation, target, blockRotationSpeed * Time.deltaTime);

        if (Quaternion.Angle(target, hand.transform.localRotation) < 0.01f)
        {
            hand.transform.localRotation = target;
            return true;
        }
        return false;
    }
    private void LookAtPlayer()
    {
        Quaternion lookRotation = Quaternion.LookRotation(Camera.main.transform.position - this.transform.position, Vector3.up);
        lookRotation.eulerAngles = new Vector3(0f, lookRotation.eulerAngles.y, 0f);
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, lookRotation, turnSpeed * Time.deltaTime);
    }

    public void ImplementStun(float stunTime)
    {
        currentStunTime = stunTime;
        _stateMachine.ChangeState(_stunState);
    }

    public void Block()
    {
        _stateMachine.ChangeState(_verticalSlashState);
    }
}
