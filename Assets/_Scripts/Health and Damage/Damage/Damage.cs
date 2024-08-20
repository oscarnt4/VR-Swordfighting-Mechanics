using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damage : MonoBehaviour, IDamaging
{
    [SerializeField] protected float maxDamage = 10f;

    public virtual float DamageAmount => maxDamage;

    public virtual Vector3 CenterOfMass => this.transform.position;

    public virtual void InflictDamage(IDamageable target)
    {
        if (target != null)
        {
            target.TakeDamage(maxDamage);
        }
    }
}
