using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRSimpleInteractable))]
[RequireComponent(typeof(Rigidbody))]

public class GrabableObject : MonoBehaviour
{
    [Header("Pre Positioned Hands")]
    [SerializeField] GameObject leftGrabbedHand;
    [SerializeField] GameObject rightGrabbedHand;

    private XRSimpleInteractable simpleInteractable;
    private Rigidbody _rigidbody;

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
        if (args.interactorObject.transform.tag == "Left Hand")
        {
            leftGrabbedHand.SetActive(true);
        }
        else if (args.interactorObject.transform.tag == "Right Hand")
        {
            rightGrabbedHand.SetActive(true);
        }
        args.interactorObject.transform.GetChild(0).gameObject.SetActive(false);
        this.transform.SetParent(args.interactorObject.transform);
        args.interactorObject.transform.GetComponent<ConfigurableJoint>().connectedBody = _rigidbody;
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
        args.interactorObject.transform.GetComponent<ConfigurableJoint>().connectedBody = null;
        this.transform.SetParent(null);
        args.interactorObject.transform.GetChild(0).gameObject.SetActive(true);
    }
}
