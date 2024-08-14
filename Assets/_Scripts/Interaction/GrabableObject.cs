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

    private int originalLayer;

    private GameObject hand;//test

    private void Awake()
    {
        simpleInteractable = GetComponent<XRSimpleInteractable>();
        _rigidbody = GetComponent<Rigidbody>();
        originalLayer = this.gameObject.layer;
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

        //Match anchor position
        Vector3 offset = args.interactorObject.transform.position - grabAnchor.transform.position;
        this.transform.position += offset;

        //Match anchor rotation
        Quaternion rotationOffset = args.interactorObject.transform.rotation * Quaternion.Inverse(grabAnchor.transform.rotation);
        this.transform.rotation = rotationOffset * this.transform.rotation;

        //Attach sword to controller hands
        this.transform.SetParent(args.interactorObject.transform);
        SetLayerForAllDescendants(this.gameObject, args.interactorObject.transform.gameObject.layer);
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
        SetLayerForAllDescendants(this.gameObject, originalLayer);
        this.transform.SetParent(null);
        args.interactorObject.transform.GetChild(0).gameObject.SetActive(true);
        this.GetComponent<Rigidbody>().useGravity = true;
    }

    private void SetLayerForAllDescendants(GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach(Transform child in obj.transform)
        {
            SetLayerForAllDescendants(child.gameObject,layer);
        }
    }
}
