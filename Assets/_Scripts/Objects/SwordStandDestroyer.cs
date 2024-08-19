using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SwordStandDestroyer : MonoBehaviour
{
    [SerializeField] XRSimpleInteractable _interactable;

    void Update()
    {
        if(_interactable.isSelected) Destroy(this.gameObject);
    }
}
