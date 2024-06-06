using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class EnableObjectGrabHand : MonoBehaviour
{
    [SerializeField] GameObject leftHandModel;
    [SerializeField] GameObject rightHandModel;
    [SerializeField] GameObject leftGrabbedHand;
    [SerializeField] GameObject rightGrabbedHand;

    private XRGrabInteractable grabInteractible;

    private void Awake()
    {
        grabInteractible = GetComponent<XRGrabInteractable>();
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
        }
        else if (args.interactorObject.transform.tag == "Right Hand")
        {
            rightHandModel.SetActive(false);
            rightGrabbedHand.SetActive(true);
        }
    }

    private void DisableGrabHand(SelectExitEventArgs args)
    {
        if (args.interactorObject.transform.tag == "Left Hand")
        {
            leftHandModel.SetActive(true);
            leftGrabbedHand.SetActive(false);
        }
        else if (args.interactorObject.transform.tag == "Right Hand")
        {
            rightHandModel.SetActive(true);
            rightGrabbedHand.SetActive(false);
        }
    }
}
