using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRGrabInteractable))]

public class EnableObjectGrabHand : MonoBehaviour
{
    [Header("VR Hand Models to Disable")]
    [SerializeField] GameObject leftHandModel;
    [SerializeField] GameObject rightHandModel;
    [Header("Pre Positioned Hands")]
    [SerializeField] GameObject leftGrabbedHand;
    [SerializeField] GameObject rightGrabbedHand;

    private XRGrabInteractable grabInteractible;
    private Transform swordParent;

    private void Awake()
    {
        grabInteractible = GetComponent<XRGrabInteractable>();
        swordParent = this.transform.parent;
    }

    void Start()
    {
        grabInteractible.selectEntered.AddListener(EnableGrabHand);
        grabInteractible.selectExited.AddListener(DisableGrabHand);
        leftGrabbedHand.SetActive(false);
        rightGrabbedHand.SetActive(false);
    }

    private void EnableGrabHand(SelectEnterEventArgs args)
    {
        if (args.interactorObject.transform.tag == "Left Hand")
        {
            leftHandModel.SetActive(false);
            leftGrabbedHand.SetActive(true);
            swordParent.SetParent(args.interactorObject.transform);
            this.transform.parent = swordParent;
        }
        else if (args.interactorObject.transform.tag == "Right Hand")
        {
            rightHandModel.SetActive(false);
            rightGrabbedHand.SetActive(true);
            swordParent.SetParent(args.interactorObject.transform);
            this.transform.parent = swordParent;
        }
    }

    private void DisableGrabHand(SelectExitEventArgs args)
    {
        if (args.interactorObject.transform.tag == "Left Hand")
        {
            leftHandModel.SetActive(true);
            leftGrabbedHand.SetActive(false);
            swordParent.SetParent(null, true);
        }
        else if (args.interactorObject.transform.tag == "Right Hand")
        {
            rightHandModel.SetActive(true);
            rightGrabbedHand.SetActive(false);
            swordParent.SetParent(null, true);
        }
    }
}
