using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BasicSwordDamage : Damage
{
    [SerializeField] float maxDamageSwingDistance = 1f;

    private XRSimpleInteractable simpleInteractable;
    private MomentumTracker momentumTracker;

    private bool momentumTrackerAttached = false;
    private float damageAmount = 0f;
    private DateTime timeOfLastHit = DateTime.Now;

    public override float DamageAmount => damageAmount;

    private void Awake()
    {
        simpleInteractable = GetComponent<XRSimpleInteractable>();
    }
    private void Start()
    {
        simpleInteractable.selectEntered.AddListener(AttachMomentumTracker);
        simpleInteractable.selectExited.AddListener(RemoveMomentumTracker);
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
        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        if (damageable != null && DateTime.Now > timeOfLastHit.AddSeconds(0.3))
        {
            if (momentumTrackerAttached) CalculateMomentumDamage();
            else CalculateVelocityDamage();
            InflictDamage(damageable);
            timeOfLastHit = DateTime.Now;
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

    private void CalculateMomentumDamage()
    {
        Debug.Log(momentumTracker.largestDistanceTravelled);
        damageAmount = (momentumTracker.largestDistanceTravelled > maxDamageSwingDistance ? 1 : momentumTracker.largestDistanceTravelled / maxDamageSwingDistance) * maxDamage; //Damage reduction based on swing distance
    }

    private void CalculateVelocityDamage()
    {
        Debug.Log("Veocity damage used");
        damageAmount = maxDamage;
    }
}
