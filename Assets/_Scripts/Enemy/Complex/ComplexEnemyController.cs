using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class ComplexEnemyController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float walkSpeed = 2f;
    [SerializeField] float turnSpeed = 2f;
    [SerializeField] float distanceWithinPlayerRange = 3f;
    [SerializeField] float distanceOutsidePlayerRange = 6f;
    [SerializeField] float dashDistance = 1f;
    [SerializeField] float dashDuration = 0.2f;
    [SerializeField] float delayBetweenDashes = 1f;
    [Header("Body")]
    [SerializeField] float armLength = 0.5f;
    [SerializeField] GameObject hand;
    [Header("Combat")]
    [SerializeField] float blockReactionDistance = 1.7f;
    [SerializeField] float blockSpeed = 5f;
    [SerializeField] float blockRotationSpeed = 50f;
    [SerializeField] float delayBetweenBlocks = 1f;
    [SerializeField] int oneOverBackdashProbability = 4;

    // State Machine
    private EnemyStateMachine _stateMachine;
    private StayWithinPlayerRange _stayWithinPlayerRangeState;
    private StayOutsidePlayerRange _stayOutsidePlayerRangeState;
    private Block _blockState;
    private BackDash _backDashState;
    private RandomStationaryAttack _randomStationaryAttackState;
    private RandomSprintAttack _randomSprintAttackState;
    private Stun _stunState;

    private NavMeshAgent _navMeshAgent;
    private Damage[] potentialThreats;
    private Damage currentThreat;
    private Animator _animator;

    Vector3 handStartLocalPosition;
    Quaternion handStartLocalRotation;

    private float stateEndTime;
    private float navMeshBuffer = 0.1f;
    private bool blockPerformed = false;
    private float blockDashPerformedTime = 0f;
    private float dashEndTime;
    private bool isSlashing = false;
    private float stunEndTime;
    private float currentStunLength;
    private float navMeshOriginalAcceleration;

    private void Awake()
    {
        // Initialise state machine and states
        _stateMachine = new EnemyStateMachine();
        _stayWithinPlayerRangeState = new StayWithinPlayerRange(this, _stateMachine);
        _stayOutsidePlayerRangeState = new StayOutsidePlayerRange(this, _stateMachine);
        _blockState = new Block(this, _stateMachine);
        _backDashState = new BackDash(this, _stateMachine);
        _randomStationaryAttackState = new RandomStationaryAttack(this, _stateMachine);
        _randomSprintAttackState = new RandomSprintAttack(this, _stateMachine);
        _stunState = new Stun(this, _stateMachine);
    }

    private void Start()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();

        potentialThreats = FindObjectsOfType<Damage>();

        handStartLocalPosition = hand.transform.localPosition;
        handStartLocalRotation = Quaternion.identity;
        _navMeshAgent.speed = walkSpeed;
        navMeshOriginalAcceleration = _navMeshAgent.acceleration;

        _stateMachine.ChangeState(_stayWithinPlayerRangeState);
    }

    private void Update()
    {
        LookAtPlayer();
        _stateMachine.Update();
    }

    // ..::STATE FUNCTIONS::..

    // STAY WITHIN PLAYER RANGE

    public void EnterStayWithinPlayerRange()
    {
        RandomiseStateTimeWindow(1f, 3f);

        _navMeshAgent.updateRotation = false;
        _navMeshAgent.SetDestination(Camera.main.transform.position);
    }

    public void ExecuteStayWithinPlayerRange()
    {
        ReturnHandToStartPosition();
        FollowPlayerAtSpecificDistance(distanceWithinPlayerRange);
        DetermineRandomStateTransition();
        DetectIncommingAttack();
    }

    public void ExitStayWithinPlayerRanger()
    {
        _navMeshAgent.SetDestination(this.transform.position);
    }

    // STAY OUTSIDE PLAYER RANGE

    public void EnterStayOutsidePlayerRange()
    {
        RandomiseStateTimeWindow(1f, 3f);

        _navMeshAgent.updateRotation = false;
        _navMeshAgent.SetDestination(Camera.main.transform.position);
    }

    public void ExecuteStayOutsidePlayerRange()
    {
        ReturnHandToStartPosition();
        FollowPlayerAtSpecificDistance(distanceOutsidePlayerRange);
        DetermineRandomStateTransition();
        DetectIncommingAttack();
    }

    public void ExitStayOutsidePlayerRange()
    {
        _navMeshAgent.SetDestination(this.transform.position);
    }

    // BLOCK

    public void EnterBlock()
    {
        blockPerformed = false;
    }

    public void ExecuteBlock()
    {
        // Find target position in local space
        Vector3 threatLocalPosition = this.transform.InverseTransformPoint(currentThreat.CenterOfMass);
        Vector3 direction = (threatLocalPosition - handStartLocalPosition).normalized;

        Vector3 localTargetPosition = handStartLocalPosition + new Vector3(direction.x, direction.y, 0f) * armLength;

        // Find target rotation in local space
        float radianZAngle = Mathf.Atan2(direction.y, direction.x);
        float eulerZAngle = radianZAngle * Mathf.Rad2Deg;
        // Ensure that hand is upright
        if (eulerZAngle > 90f) eulerZAngle -= 180f;
        else if (eulerZAngle < -90f) eulerZAngle += 180f;

        Quaternion localTargetRotation = Quaternion.Euler(0f, 0f, eulerZAngle);

        // Update hand position and rotation
        UpdateHandLocalPosition(localTargetPosition);
        UpdateHandLocalRotation(localTargetRotation);

        // Check if block performed or if threat is beyond reaction distance
        if (Vector3.Distance(this.transform.position, currentThreat.CenterOfMass) > blockReactionDistance 
            || currentThreat.transform.parent.GetComponent<MomentumTracker>().largestDistanceTravelled < 0.25 
            || blockPerformed)
        {
            RandomStateTransition();
        }
    }

    public void ExitBlock()
    {
        blockPerformed = false;
    }

    public bool CanEnterBlock(IState currentState)
    {
        return (currentState is StayWithinPlayerRange || currentState is StayOutsidePlayerRange) &&
                    Time.time > blockDashPerformedTime + delayBetweenBlocks;
    }

    // BACK DASH

    public void EnterBackDash()
    {
        blockDashPerformedTime = Time.time;
        _navMeshAgent.enabled = false;
        dashEndTime = Time.time + dashDuration;
    }

    public void ExecuteBackDash()
    {
        ReturnHandToStartPosition();
        RaycastHit slopeHit;
        DetectSlope(out slopeHit);
        Vector3 dashDirection = Vector3.ProjectOnPlane(-new Vector3(this.transform.forward.x, 0, this.transform.forward.z).normalized, slopeHit.normal).normalized;

        this.transform.position += (dashDirection * (dashDistance / dashDuration) * Time.deltaTime);

        if (Time.time >= dashEndTime) _stateMachine.ChangeState(_stayOutsidePlayerRangeState);
    }

    public void ExitBackDash()
    {
        _navMeshAgent.enabled = true;
        _navMeshAgent.SetDestination(this.transform.position);
    }

    public bool CanEnterBackDash(IState currentState)
    {
        return currentState is StayWithinPlayerRange &&
                 Time.time > blockDashPerformedTime + delayBetweenDashes;
    }

    // RANDOM STATIONARY ATTACK

    public void ExecuteRandomStationaryAttack()
    {
        if (!isSlashing && UpdateHandLocalPosition(handStartLocalPosition) && UpdateHandLocalRotation(handStartLocalRotation))
        {
            isSlashing = true;
            _animator.enabled = true;

            int randomIndex = Random.Range(0, 4);

            if (randomIndex == 0) _animator.Play("Enemy_SlashVerticalDown1", 0, 0f);
            else if (randomIndex == 1) _animator.Play("Enemy_SlashVerticalDown2", 0, 0f);
            else if (randomIndex == 2) _animator.Play("Enemy_SlashHorizontal", 0, 0f);
            else if (randomIndex == 3) _animator.Play("Enemy_DoubleCrossSlash", 0, 0f);
        }

        if (isSlashing)
        {
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

            if (stateInfo.normalizedTime >= 1.0f || _animator.enabled == false)
            {
                _animator.Rebind();
                _animator.enabled = false;
                RandomStateTransition();
                Debug.Log("slash complete");
            }
        }
    }

    public void ExitRandomStationaryAttack()
    {
        isSlashing = false;
    }

    public bool CanEnterRandomStationaryAttack(IState currentState)
    {
        return currentState is StayWithinPlayerRange || currentState is Block || currentState is RandomStationaryAttack || currentState is RandomSprintAttack;
    }

    // RANDOM SPRINT ATTACK

    public void ExecuteRandomSprintAttack()
    {
        if (!isSlashing && UpdateHandLocalPosition(handStartLocalPosition) && UpdateHandLocalRotation(handStartLocalRotation))
        {
            isSlashing = true;
            _animator.enabled = true;

            int randomIndex = Random.Range(0, 4);

            if (randomIndex == 0) _animator.Play("Enemy_SlashVerticalDown1", 0, 0f);
            else if (randomIndex == 1) _animator.Play("Enemy_SlashVerticalDown2", 0, 0f);
            else if (randomIndex == 2) _animator.Play("Enemy_SlashHorizontal", 0, 0f);
            else if (randomIndex == 3) _animator.Play("Enemy_DoubleCrossSlash", 0, 0f);
        }

        if (isSlashing)
        {
            _navMeshAgent.acceleration = 500f;
            _navMeshAgent.speed = 10f;
            FollowPlayerAtSpecificDistance(distanceWithinPlayerRange);

            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

            if (stateInfo.normalizedTime >= 1.0f || _animator.enabled == false)
            {
                _animator.Rebind();
                _animator.enabled = false;
                RandomStateTransition();
            }
        }
    }

    public void ExitRandomSprintAttack()
    {
        isSlashing = false;
        _navMeshAgent.acceleration = navMeshOriginalAcceleration;
        _navMeshAgent.speed = walkSpeed;
    }

    public bool CanEnterRandomSprintAttack(IState currentState)
    {
        return currentState is StayOutsidePlayerRange;
    }

    // STUN

    public void EnterStun()
    {
        stunEndTime = Time.time + currentStunLength;
        _animator.speed = 0f;
    }

    public void ExecuteStun()
    {
        if (Time.time >= stunEndTime)
        {
            _animator.speed = 1f;
            RandomStateTransition();
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
        return currentState is RandomStationaryAttack;
    }

    public bool CanExitStun()
    {
        return Time.time >= stunEndTime;
    }

    // ..::STATE TRANSITION LOGIC::..

    private void RandomiseStateTimeWindow(float shortestTime, float longestTime)
    {
        stateEndTime = Time.time + Random.Range(shortestTime, longestTime);
    }

    private void DetermineRandomStateTransition()
    {
        if (Time.time > stateEndTime)
        {
            RandomStateTransition();
        }
    }

    private void RandomStateTransition()
    {
        int rnd = Random.Range(0, 11);

        switch (rnd)
        {
            // 4/11 probability to stay within player range
            case 0:
            case 1:
            case 2:
            case 3:
                _stateMachine.ChangeState(_stayWithinPlayerRangeState);
                break;
            // 1/11 probability to stay outside player range
            case 4:
                _stateMachine.ChangeState(_stayOutsidePlayerRangeState);
                break;
            // 2/11 Probability to perform a random stationary attack
            case 5:
            case 6:
                _stateMachine.ChangeState(_randomStationaryAttackState);
                break;
            // 4/11 probability to perform a random sprinting attack
            case 7:
            case 8:
            case 9:
            case 10:
                _stateMachine.ChangeState(_randomSprintAttackState);
                break;
            default:
                _stateMachine.ChangeState(_stayWithinPlayerRangeState);
                break;

        }
    }

    private void DetectIncommingAttack()
    {
        foreach (Damage threat in potentialThreats)
        {
            if (threat.tag != "Enemy" 
                && Vector3.Distance(this.transform.position, threat.CenterOfMass) <= blockReactionDistance 
                && threat.transform.parent.GetComponent<MomentumTracker>().largestDistanceTravelled > 0.25f)
            {
                currentThreat = threat;

                int rnd = Random.Range(0, oneOverBackdashProbability); // Probability of a back dash

                if (rnd == 0)
                {
                    _stateMachine.ChangeState(_backDashState);
                }
                else
                {
                    _stateMachine.ChangeState(_blockState);
                }
                break;
            }
        }
    }

    public void BlockPerformed()
    {
        blockPerformed = true;
        blockDashPerformedTime = Time.time;
    }

    public void ImplementStun(float stunLength)
    {
        currentStunLength = stunLength;
        _stateMachine.ChangeState(_stunState);
    }

    // ..::OTHER FUNCTIONS::..

    private void LookAtPlayer()
    {
        // Calculate player direction
        Quaternion lookRotation = Quaternion.LookRotation(Camera.main.transform.position - this.transform.position, Vector3.up);
        lookRotation.eulerAngles = new Vector3(0f, lookRotation.eulerAngles.y, 0f);

        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, lookRotation, turnSpeed * Time.deltaTime);
    }


    private void FollowPlayerAtSpecificDistance(float distanceToFollow)
    {
        // Calculate distance from player
        float playerDistance = Vector3.Distance(this.transform.position, Camera.main.transform.position);

        if (playerDistance > distanceToFollow + navMeshBuffer)
        {
            // Set player as destination
            _navMeshAgent.SetDestination(Camera.main.transform.position);
        }
        else if (playerDistance < distanceToFollow - navMeshBuffer)
        {
            // Calculate opposite direction to player
            Vector3 retreatDirection = this.transform.position - Camera.main.transform.position;
            // Calculate point behind enemy
            Vector3 retreatPosition = transform.position + retreatDirection.normalized;

            _navMeshAgent.SetDestination(retreatPosition);
        }
        else
        {
            // Stop moving
            _navMeshAgent.SetDestination(this.transform.position);
        }
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

    private void ReturnHandToStartPosition()
    {
        UpdateHandLocalPosition(handStartLocalPosition);
        UpdateHandLocalRotation(handStartLocalRotation);
    }

    private bool DetectSlope(out RaycastHit slopeHit)
    {
        return Physics.Raycast(this.transform.position /*+ Vector3.down * characterController.height / 2f*/, Vector3.down, out slopeHit, this.transform.localScale.y)
               && slopeHit.normal != Vector3.up;
    }
}
