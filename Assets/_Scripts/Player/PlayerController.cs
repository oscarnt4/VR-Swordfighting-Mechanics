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

    private PlayerStateMachine _stateMachine;
    private WalkingState _walkingState;
    private SprintingState _sprintingState;
    private CrouchingState _crouchingState;
    private DashingState _dashingState;

    private bool canDash = true;
    private float startingCameraYOffset;
    private float dashStartTime;
    private float dashEndTime;

    private enum PlayerState { Walking, Sprinting, Dashing, Jumping, Crouching }
    private PlayerState currentState;

    private void Awake()
    {
        xrRig = GetComponent<XROrigin>();
        moveProvider = GetComponent<DynamicMoveProvider>();
        characterController = GetComponent<CharacterController>();

        _stateMachine = PlayerStateMachine.Instance;
        _walkingState = new WalkingState(this);
        _sprintingState = new SprintingState(this);
        _crouchingState = new CrouchingState(this);
        _dashingState = new DashingState(this);
    }

    private void Start()
    {
        moveProvider.moveSpeed = walkSpeed;
        currentState = PlayerState.Walking;
        startingCameraYOffset = xrRig.CameraYOffset;
        _stateMachine.ChangeState(_walkingState);
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
        _stateMachine.Update();
    }

    private void OnSprint(InputAction.CallbackContext context)
    {
        _stateMachine.ChangeState(_sprintingState);
    }

    private void OnCrouch(InputAction.CallbackContext context)
    {
        if (_stateMachine.CurrentState is CrouchingState)
        {
            _stateMachine.ChangeState(_walkingState);
        }
        else
        {
            _stateMachine.ChangeState(_crouchingState);
        }
    }

    private void OnDash(InputAction.CallbackContext context)
    {
        _stateMachine.ChangeState(_dashingState);
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

    public void EnterWalk()
    {
        moveProvider.moveSpeed = walkSpeed;
    }

    public void EnterSprint()
    {
        moveProvider.moveSpeed = sprintSpeed;
    }

    public void EnterCrouch()
    {
        moveProvider.moveSpeed = crouchSpeed;
        //StartCoroutine(CrouchingCoroutine());
    }

    public void ExitCrouch()
    {
        //StartCoroutine(StandingCoroutine());
    }

    public void EnterDash()
    {
        dashStartTime = Time.time;
    }

    public bool ExecuteDash()
    {
        RaycastHit slopeHit;
        DetectSlope(out slopeHit);
        Vector3 dashDirection = Vector3.ProjectOnPlane(
            -new Vector3(xrRig.Camera.transform.forward.x, 0, xrRig.Camera.transform.forward.z).normalized,
            slopeHit.normal
            ).normalized;

        characterController.Move(dashDirection * (dashDistance / dashDuration) * Time.deltaTime);
        float dashTime = dashStartTime + dashDuration;
        //Debug.Log(Time.time + " || " + dashTime + " || " + (Time.time >= dashTime));

        return Time.time >= (dashStartTime + dashDuration);
    }

    public void ExitDash()
    {
        characterController.Move(Vector3.zero);
        dashEndTime = Time.time;
    }

    public bool IsGrounded()
    {
        return characterController.isGrounded;
    }

    public bool MoveForwardHeld()
    {
        return moveInput.action.ReadValue<Vector2>().y > 0.5f;
    }

    public bool OutsideDashWindow()
    {
        return Time.time >= dashEndTime + 0.5f;
    }

    private bool DetectSlope(out RaycastHit slopeHit)
    {
        return Physics.Raycast(moveProvider.headTransform.position + Vector3.down * characterController.height / 2f, Vector3.down, out slopeHit, characterController.height)
               && slopeHit.normal != Vector3.up;
    }

    public IEnumerator CrouchingCoroutine()
    {
        while (xrRig.CameraYOffset > startingCameraYOffset / 2f)
        {
            xrRig.CameraYOffset -= 2f * Time.deltaTime;
            yield return null;
        }
        xrRig.CameraYOffset = startingCameraYOffset / 2f;
    }

    public IEnumerator StandingCoroutine()
    {
        while (xrRig.CameraYOffset < startingCameraYOffset)
        {
            xrRig.CameraYOffset += 2f * Time.deltaTime;
            yield return null;
        }
        xrRig.CameraYOffset = startingCameraYOffset;
    }
}