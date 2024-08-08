using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySwordDamage : Damage
{
    [SerializeField] Transform head;
    [SerializeField] Transform swordTip;

    private float damageAmount = 0f;
    public override float DamageAmount => damageAmount;

    private void OnCollisionEnter(Collision collision)
    {
        BasicSwordDamage swordDamage = collision.gameObject.GetComponent<BasicSwordDamage>();
        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();

        // Implement stun
        if (swordDamage != null)
        {

            float collidedWithPointDistance = swordDamage.GetTipDistance();
            float thisPointDistance = this.GetTipDistance();

            if (thisPointDistance > collidedWithPointDistance)
            {
                StartCoroutine(ImplementStun());
            }
        }
        // Inflict damage
        else if (damageable != null)
        {
            InflictDamage(damageable);
        }
    }

    public override void InflictDamage(IDamageable target)
    {
        if (target != null)
        {
            target.TakeDamage(damageAmount);
        }
    }

    public void SetDamageAmount(float amount)
    {
        damageAmount = amount;
    }

    public float GetTipDistance()
    {
        if (head == null) return 0;
        return Vector3.Distance(head.position, swordTip.position); //work out a better way to get the head position and return 0 otherwise
    }

    private IEnumerator ImplementStun()
    {
        yield return new WaitForSeconds(0.1f);
    }
}
