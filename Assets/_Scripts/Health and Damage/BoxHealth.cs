using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxHealth : Health
{
    protected override void Die()
    {
        base.Die();
        Destroy(this.gameObject);
    }
}
