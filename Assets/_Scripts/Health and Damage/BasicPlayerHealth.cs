using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class BasicPlayerHealth : Health
{
    [SerializeField] PostProcessVolume _postProcessVolume;
    private ColorGrading screenTint;

    private void Awake()
    {
        _postProcessVolume.profile.TryGetSettings<ColorGrading>(out screenTint);
    }

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
    }

    private void UpdateColour()
    {
        Color colour = Color.HSVToRGB(0f, 1f - CurrentHealth / MaxHealth, 1f);
        screenTint.colorFilter.value = colour;//new Color(1 - CurrentHealth / MaxHealth, CurrentHealth / MaxHealth, 0f);
    }
}
