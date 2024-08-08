using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;

public class BasicSwordDamage : Damage
{
    [Header("Damage")]
    [SerializeField] float maxDamageSwingDistance = 1.15f;
    [SerializeField] float zeroDamageThreshold = 0.5f;
    [SerializeField] float discreteDamageIncrement = 0.1f;
    [SerializeField] bool fixedDamageAmount = false;
    [SerializeField] float timeBectweenConsecutiveHits = 0.3f;
    [Header("Stun")]
    [SerializeField] float stunTimePerDamageAmount = 0.015f;
    [SerializeField] Transform swordTip;
    [SerializeField] Transform head;
    [Header("Visuals")]
    [SerializeField] Renderer _renderer;
    [SerializeField] BlockingTrigger enemyBlockingTrigger;

    private XRSimpleInteractable simpleInteractable;
    private MomentumTracker momentumTracker;
    private GrabableObject grabableObject;

    private bool momentumTrackerAttached = false;
    private float damageAmount = 0f;
    private DateTime timeOfLastHit = DateTime.Now;
    private bool isStunned = false;
    private bool canAttack = true;

    public override float DamageAmount => damageAmount;

    private void Awake()
    {
        simpleInteractable = GetComponent<XRSimpleInteractable>();
        grabableObject = GetComponent<GrabableObject>();
    }
    private void Start()
    {
        head = Camera.main.transform;
        simpleInteractable.selectEntered.AddListener(AttachMomentumTracker);
        simpleInteractable.selectExited.AddListener(RemoveMomentumTracker);
        if (this.transform.parent.GetComponent<MomentumTracker>() != null)
        {
            StartCoroutine(AttachTrackerCoroutine());
        }
    }
    private void Update()
    {
        if (momentumTracker != null)
        {
            //Debug
            Color color = Color.Lerp(Color.green, Color.red, momentumTracker.largestDistanceTravelled > maxDamageSwingDistance ? 1 : momentumTracker.largestDistanceTravelled / maxDamageSwingDistance);
            _renderer.material.color = Color.HSVToRGB(color.grayscale, 1f, 1f);
        }
        if (!canAttack)
        {
            canAttack = CalculateMomentumDamage() == 0;
        }
    }

    public override void InflictDamage(IDamageable target)
    {
        if (target != null)
        {
            target.TakeDamage(damageAmount);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isStunned && canAttack)
        {
            BasicSwordDamage swordDamage = collision.gameObject.GetComponent<BasicSwordDamage>();
            IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();

            // Implement stun
            if (swordDamage != null)
            {
                canAttack = false;

                float collidedWithPointDistance = swordDamage.GetTipDistance();
                float thisPointDistance = this.GetTipDistance();

                if (thisPointDistance > collidedWithPointDistance)
                {
                    StartCoroutine(ImplementStun());
                } 
                else
                {
                    if(enemyBlockingTrigger != null) enemyBlockingTrigger.EndBlock();
                }
            }
            // Inflict damage
            else if (damageable != null && DateTime.Now > timeOfLastHit.AddSeconds(timeBectweenConsecutiveHits))
            {
                canAttack = false;

                if (momentumTrackerAttached) CalculateMomentumDamage();
                else CalculateVelocityDamage();

                InflictDamage(damageable);
                timeOfLastHit = DateTime.Now;
            }
        }
    }

    private void AttachMomentumTracker(SelectEnterEventArgs args)
    {
        StartCoroutine(AttachTrackerCoroutine());
    }

    private IEnumerator AttachTrackerCoroutine()
    {
        yield return new WaitForSeconds(0.1f);
        momentumTracker = this.transform.parent.GetComponent<MomentumTracker>();
        momentumTrackerAttached = momentumTracker != null;
    }

    private void RemoveMomentumTracker(SelectExitEventArgs args)
    {
        momentumTracker = null;
        momentumTrackerAttached = false;
    }

    private float CalculateMomentumDamage()
    {
        float distanceTravelledRatio = Mathf.Clamp01(momentumTracker.largestDistanceTravelled / maxDamageSwingDistance);
        float damageRatio = -1f;
        if (fixedDamageAmount)
        {
            if (distanceTravelledRatio < zeroDamageThreshold) damageRatio = 0f;
            else damageRatio = 1f;
        }
        else
        {
            if (distanceTravelledRatio >= 0 && distanceTravelledRatio < zeroDamageThreshold) damageRatio = 0f; //Check if distanceTravelledRatio is too low to cause damage
            if (distanceTravelledRatio >= 1f) damageRatio = 1f; // Return 1 is distance ratio it higher than or equal to 1

            float lowerLimit = zeroDamageThreshold;
            while (damageRatio == -1f && lowerLimit < 1f) // Check which threshold the distance ratio falls within and then output the x^2 value of the lower limit as the damage ratio
            {
                if (distanceTravelledRatio >= lowerLimit && distanceTravelledRatio < lowerLimit + discreteDamageIncrement)
                {
                    damageRatio = Mathf.Pow(lowerLimit, 2f);
                    break;
                }
                lowerLimit += discreteDamageIncrement;
            }
        }

        return damageAmount = damageRatio * maxDamage; //Damage reduction based on swing distance
    }

    private float CalculateVelocityDamage()
    {
        Debug.Log("Veocity damage used");
        return damageAmount = maxDamage;
    }

    public float GetTipDistance()
    {
        if (head == null) return 0; //Mathf.Infinity;
        return Vector3.Distance(head.position, swordTip.position); //work out a better way to get the head position and return infinity otherwise
    }

    private IEnumerator ImplementStun()
    {
        isStunned = true;
        float stunTime = CalculateMomentumDamage() * stunTimePerDamageAmount;// * 10;
        ActionBasedController handController = momentumTracker.GetComponentInParent<ActionBasedController>();
        //grabableObject.FreezeGrabbedObject();
        handController.enabled = false;
        yield return new WaitForSeconds(stunTime);
        //grabableObject.ReturnObjectToGrabPosition();
        handController.enabled = true;
        isStunned = false;
    }

    private void MoveSwordIntoGrabbedPosition()// move sword into correct hand position before attaching wrist (also used for after the stun)
    {

    }

}
