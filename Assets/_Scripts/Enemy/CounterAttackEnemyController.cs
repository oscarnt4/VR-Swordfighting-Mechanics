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

    private Damage[] potentialThreats;
    private NavMeshAgent _navMeshAgent;

    private Vector3 handStartLocalPosition;
    private Quaternion weaponStartLocalRotation;
    private Vector3 localTargetPosition;
    private Quaternion localTargetRotation;

    private void Awake()
    {
        _stateMachine = new EnemyStateMachine();
        _chasingState = new EnemyChasingState(this, _stateMachine);
    }

    void Start()
    {
        potentialThreats = FindObjectsOfType<Damage>();
        _navMeshAgent = GetComponent<NavMeshAgent>();

        handStartLocalPosition = hand.transform.localPosition;
        weaponStartLocalRotation = Quaternion.identity;

        _stateMachine.ChangeState(_chasingState);
    }

    void Update()
    {
        _stateMachine.Update();
    }

    public void EnterChase()
    {
        _navMeshAgent.SetDestination(Camera.main.transform.position);
    }

    public void ExecuteChase()
    {
        ExecuteBlock();
        LookAtPlayer();
        if(Vector3.Distance(this.transform.position, Camera.main.transform.position) < stoppingDistanceFromPlayer)
        {
            _navMeshAgent.isStopped = true;
        } else
        {
            _navMeshAgent.isStopped = false;
            _navMeshAgent.SetDestination(Camera.main.transform.position);
        }
    }

    private void ExecuteBlock()
    {
        localTargetPosition = handStartLocalPosition;
        localTargetRotation = weaponStartLocalRotation;

        foreach (Damage threat in potentialThreats)
        {
            if (/*threat.DamageAmount > 0 &&*/ threat.tag != "Enemy" && Vector3.Distance(this.transform.position, threat.transform.position) <= blockReactionDistance)
            {
                Vector3 threatLocalPosition = this.transform.InverseTransformPoint(threat.transform.position);
                Vector3 direction = (threatLocalPosition - handStartLocalPosition).normalized;
                localTargetPosition = handStartLocalPosition + new Vector3(direction.x, direction.y, 0f) * armLength;
                Debug.Log("Start Position: " + handStartLocalPosition + " || Target Position: " + localTargetPosition);

                float radianZAngle = Mathf.Atan2(direction.y, direction.x);
                float eulerZAngle = radianZAngle * Mathf.Rad2Deg;

                if (eulerZAngle > 90f) eulerZAngle -= 180f;
                else if (eulerZAngle < -90f) eulerZAngle += 180f;

                localTargetRotation = Quaternion.Euler(0f, 0f, eulerZAngle);
                break;
            }
        }
        UpdateHandLocalPosition(localTargetPosition);
        UpdateWeaponLocalRotation(localTargetRotation);
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

    private bool UpdateWeaponLocalRotation(Quaternion target)
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
}
