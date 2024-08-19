using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;

public class ComplexEnemySwordDamage : Damage
{
    [Header("Stun")]
    [SerializeField] float stunTimePerDamageAmount = 0.15f;
    [Header("Positional Info")]
    [SerializeField] Transform swordTip;
    [SerializeField] Transform head;

    private ComplexEnemyController _enemy;

    private void Awake()
    {
        _enemy = this.transform.parent.GetComponentInParent<ComplexEnemyController>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision detected");
        // Detect stun
        if (collision.gameObject.TryGetComponent<BasicSwordDamage>(out BasicSwordDamage swordDamage))
        {
            Debug.Log("Sword collision");
            float collidedWithPointDistance = swordDamage.GetTipDistance();
            float thisPointDistance = this.GetTipDistance();

            if (thisPointDistance > collidedWithPointDistance)
            {
                Debug.Log("Stun implemented");
                _enemy.ImplementStun(DamageAmount * stunTimePerDamageAmount);
            }
            else if (swordDamage.DamageAmount > 0)
            {
                Debug.Log("Block performed");
                _enemy.BlockPerformed();
            }
        }
        // Inflict damage
        else if (collision.gameObject.TryGetComponent<IDamageable>(out IDamageable damageable))
        {
            Debug.Log("Damageable collision");
            InflictDamage(damageable);
        }

    }

    public float GetTipDistance()
    {
        if (head == null) return 0;
        return Vector3.Distance(head.position, swordTip.position); //work out a better way to get the head position and return infinity otherwise
    }
}
