using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRSimpleInteractable))]
[RequireComponent(typeof(Rigidbody))]

public class GrabableObject : MonoBehaviour
{
    [Header("Pre Positioned Hands")]
    [SerializeField] GameObject leftGrabbedHand;
    [SerializeField] GameObject rightGrabbedHand;
    [Header("Grab Position")]
    [SerializeField] GameObject grabAnchor;

    private XRSimpleInteractable simpleInteractable;
    private Rigidbody _rigidbody;
    private Vector3 currentVelocity;
    private ConfigurableJoint attachedJoint;

    private GameObject hand;//test

    private void Awake()
    {
        simpleInteractable = GetComponent<XRSimpleInteractable>();
        _rigidbody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        simpleInteractable.selectEntered.AddListener(EnableGrab);
        simpleInteractable.selectExited.AddListener(DisableGrab);
        leftGrabbedHand.SetActive(false);
        rightGrabbedHand.SetActive(false);
    }

    private void EnableGrab(SelectEnterEventArgs args)
    {
        //Activate sword hands
        if (args.interactorObject.transform.tag == "Left Hand")
        {
            leftGrabbedHand.SetActive(true);
        }
        else if (args.interactorObject.transform.tag == "Right Hand")
        {
            rightGrabbedHand.SetActive(true);
        }
        //Deactivate controller hands
        args.interactorObject.transform.GetChild(0).gameObject.SetActive(false);

        //Match anchor rotation
        //Quaternion rotationOffset = Quaternion.Inverse(args.interactorObject.transform.rotation) * grabAnchor.transform.rotation;
        //this.transform.rotation = rotationOffset * this.transform.rotation;
        //Match anchor position
        Vector3 offset = args.interactorObject.transform.position - grabAnchor.transform.position;
        this.transform.position += offset;
        //Attach sword to controller hands
        this.transform.SetParent(args.interactorObject.transform);
        attachedJoint = args.interactorObject.transform.GetComponent<ConfigurableJoint>();
        attachedJoint.connectedBody = _rigidbody;
    } 

    private void DisableGrab(SelectExitEventArgs args)
    {
        if (args.interactorObject.transform.tag == "Left Hand")
        {
            leftGrabbedHand.SetActive(false);
        }
        else if (args.interactorObject.transform.tag == "Right Hand")
        {
            rightGrabbedHand.SetActive(false);
        }
        attachedJoint.connectedBody = null;
        this.transform.SetParent(null);
        args.interactorObject.transform.GetChild(0).gameObject.SetActive(true);
        this.GetComponent<Rigidbody>().useGravity = true;
    }

    public void ReturnObjectToGrabPosition()
    {

    }
}
