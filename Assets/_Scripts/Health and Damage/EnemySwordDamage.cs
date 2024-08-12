using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;

public class EnemySwordDamage : Damage
{
    [Header("Stun")]
    [SerializeField] float stunTimePerDamageAmount = 0.015f;
    [Header("Positional Info")]
    [SerializeField] Transform swordTip;
    [SerializeField] Transform head;


    private CounterAttackEnemyController _enemy;
    private void Awake()
    {
        _enemy = this.transform.parent.GetComponentInParent<CounterAttackEnemyController>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Detect stun
        if (collision.gameObject.TryGetComponent<BasicSwordDamage>(out BasicSwordDamage swordDamage))
        {
            float collidedWithPointDistance = swordDamage.GetTipDistance();
            float thisPointDistance = this.GetTipDistance();

            if (thisPointDistance > collidedWithPointDistance)
            {
                _enemy.ImplementStun(DamageAmount * stunTimePerDamageAmount);
            }
            else if (swordDamage.DamageAmount > 0)
            {
                _enemy.Block();
            }
        }
        // Inflict damage
        else if (collision.gameObject.TryGetComponent<IDamageable>(out IDamageable damageable))
        {
            InflictDamage(damageable);
        }

    }

    public float GetTipDistance()
    {
        if (head == null) return 0; //Mathf.Infinity;
        return Vector3.Distance(head.position, swordTip.position); //work out a better way to get the head position and return infinity otherwise
    }
}
