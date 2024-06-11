using System;
using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

[RequireComponent(typeof(XROrigin))]
[RequireComponent(typeof(ActionBasedContinuousMoveProvider))]
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Values:")]
    [SerializeField] float walkSpeed = 2f;
    [SerializeField] float sprintSpeed = 4f;
    [SerializeField] float dashDuration = 0.2f;
    [SerializeField] float dashDistance = 1f;
    [SerializeField] float jumpHeight = 2f;
    [SerializeField] float crouchSpeed = 2f;

    [Header("Input Actions:")]
    [SerializeField] InputActionProperty moveInput;
    [SerializeField] InputActionProperty sprintInput;
    [SerializeField] InputActionProperty dashInput;
    [SerializeField] InputActionProperty jumpInput;
    [SerializeField] InputActionProperty crouchInput;

    private XROrigin xrRig;
    private DynamicMoveProvider moveProvider;
    private CharacterController characterController;

    private bool canDash = true;
    private float startingCameraYOffset;

    private enum PlayerState { Walking, Sprinting, Dashing, Jumping, Crouching }
    private PlayerState currentState;

    private void Awake()
    {
        xrRig = GetComponent<XROrigin>();
        moveProvider = GetComponent<DynamicMoveProvider>();
        characterController = GetComponent<CharacterController>();
    }

    private void Start()
    {
        moveProvider.moveSpeed = walkSpeed;
        currentState = PlayerState.Walking;
        startingCameraYOffset = xrRig.CameraYOffset;
    }

    private void OnEnable()
    {
        sprintInput.action.started += OnSprint;
        dashInput.action.started += OnDash;
        jumpInput.action.started += OnJump;
        crouchInput.action.started += OnCrouch;

        sprintInput.action.Enable();
        dashInput.action.Enable();
        jumpInput.action.Enable();
        crouchInput.action.Enable();
    }

    private void OnDisable()
    {
        sprintInput.action.started -= OnSprint;
        dashInput.action.started -= OnDash;
        jumpInput.action.started -= OnJump;
        crouchInput.action.started -= OnCrouch;

        sprintInput.action.Disable();
        dashInput.action.Disable();
        jumpInput.action.Disable();
        crouchInput.action.Disable();
    }

    private void Update()
    {
        MovementStateMachine();
        Debug.DrawRay(moveProvider.headTransform.position + Vector3.down * characterController.height / 2f, Vector3.down, Color.red); // Slope check raycast
    }

    private void MovementStateMachine()
    {
        switch (currentState)
        {
            case PlayerState.Walking:
                moveProvider.moveSpeed = walkSpeed;
                break;
            case PlayerState.Sprinting:
                if (moveInput.action.ReadValue<Vector2>().y <= 0.5f)
                {
                    currentState = PlayerState.Walking;
                    break;
                }
                moveProvider.moveSpeed = sprintSpeed;
                break;
            case PlayerState.Dashing:
                StartCoroutine(DashCoroutine());
                currentState = PlayerState.Walking;
                break;
            case PlayerState.Jumping:
                currentState = PlayerState.Walking;
                break;
            case PlayerState.Crouching:
                moveProvider.moveSpeed = crouchSpeed;
                break;
            default:
                currentState = PlayerState.Walking;
                break;
        }
    }

    private void OnSprint(InputAction.CallbackContext context)
    {
        if (currentState == PlayerState.Crouching)
        {
            StartCoroutine(StandingCoroutine());
            currentState = PlayerState.Walking;
        }
        if (currentState == PlayerState.Walking && characterController.isGrounded)
        {
            currentState = PlayerState.Sprinting;
        }
    }

    private void OnDash(InputAction.CallbackContext context)
    {
        if (currentState != PlayerState.Crouching && canDash && characterController.isGrounded)
        {
            currentState = PlayerState.Dashing;
        }
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (currentState != PlayerState.Crouching && characterController.isGrounded)
        {
            currentState = PlayerState.Jumping;
            Debug.Log("JUMPED");
            // Implement jump logic here
        }
    }

    private void OnCrouch(InputAction.CallbackContext context)
    {
        if (currentState == PlayerState.Crouching)
        {
            currentState = PlayerState.Walking;
            StartCoroutine(StandingCoroutine());
        }
        else
        {
            currentState = PlayerState.Crouching;
            StartCoroutine(CrouchingCoroutine());
        }
    }

    private IEnumerator DashCoroutine()
    {
        canDash = false;
        currentState = PlayerState.Dashing;

        RaycastHit slopeHit;
        DetectSlope(out slopeHit);
        Vector3 dashDirection = Vector3.ProjectOnPlane(
            -new Vector3(xrRig.Camera.transform.forward.x, 0, xrRig.Camera.transform.forward.z).normalized,
            slopeHit.normal
            ).normalized;

        float startTime = Time.time;

        while (Time.time < startTime + dashDuration)
        {
            characterController.Move(dashDirection * dashDistance / dashDuration * Time.deltaTime);
            yield return null;
        }

        currentState = PlayerState.Walking;
        yield return new WaitForSeconds(0.5f);
        canDash = true;
    }

    private bool DetectSlope(out RaycastHit slopeHit)
    {
        return Physics.Raycast(moveProvider.headTransform.position + Vector3.down * characterController.height / 2f, Vector3.down, out slopeHit, characterController.height)
               && slopeHit.normal != Vector3.up;
    }

    private IEnumerator CrouchingCoroutine()
    {
        while (xrRig.CameraYOffset > startingCameraYOffset / 2f)
        {
            xrRig.CameraYOffset -= 2f * Time.deltaTime;
            yield return null;
        }
        xrRig.CameraYOffset = startingCameraYOffset / 2f;
    }

    private IEnumerator StandingCoroutine()
    {
        while (xrRig.CameraYOffset < startingCameraYOffset)
        {
            xrRig.CameraYOffset += 2f * Time.deltaTime;
            yield return null;
        }
        xrRig.CameraYOffset = startingCameraYOffset;
    }
}