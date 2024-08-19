using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(CapsuleCollider))]
public class MatchColliderToCharController : MonoBehaviour
{
    private CharacterController characterController;
    private CapsuleCollider capsuleCollider;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        UpdateCapsuleColliderDimensions();
    }

    private void Update()
    {
        UpdateCapsuleColliderDimensions();
    }

    private void UpdateCapsuleColliderDimensions()
    {
        capsuleCollider.radius = characterController.radius;
        capsuleCollider.height = characterController.height;
        capsuleCollider.center = characterController.center;
    }
}