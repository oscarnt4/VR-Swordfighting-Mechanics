using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemyHealth : Health
{
    [SerializeField] Renderer _renderer;
    [SerializeField] GameObject sword;

    public override void Start()
    {
        base.Start();
        UpdateColour();
    }

    public override void TakeDamage(float amount)
    {
        base.TakeDamage(amount);
        UpdateColour();
    }

    public override void Heal(float amount)
    {
        base.Heal(amount);
        UpdateColour();
    }

    protected override void Die()
    {
        base.Die();
        sword.transform.parent = null;
    }

    private void UpdateColour()
    {
        Color colour = new Color(1 - CurrentHealth / MaxHealth, CurrentHealth / MaxHealth, 0f);
        _renderer.material.color = colour;
    }
}
