using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Your implemented technique inherits the InteractionTechnique class
public class MyTechnique : InteractionTechnique
{
    [SerializeField]
    int raycastMaxDistance = 1000;


    [SerializeField]
    private GameObject leftController;

    [SerializeField]
    private GameObject rightController;

    private LineRenderer lineRenderer;

    private Shelf hoveredShelf = null;
    private Shelf manipulatedShelf = null;

    private bool isTriggerPressed = false;
    private bool isTriggerPressedOnce = false;

    private void Start()
    {
        lineRenderer = rightController.GetComponent<LineRenderer>();
    }

    private void FixedUpdate()
    {
        Transform rightControllerTransform = rightController.transform;

        // Set the beginning of the line renderer to the position of the controller
        lineRenderer.SetPosition(0, rightControllerTransform.position);

        // detect single trigger press (use timer to prevent multiple presses)
        if (OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) > 0.1f)
        {
            if(!isTriggerPressed) {
                isTriggerPressedOnce = true;
            } else {
                isTriggerPressedOnce = false;
            }
            isTriggerPressed = true;
        }
        else
        {
            isTriggerPressed = false;
            isTriggerPressedOnce = false;
        }

        // Creating a raycast and storing the first hit if existing
        RaycastHit hit;
        bool hasHit = Physics.Raycast(rightControllerTransform.position, rightControllerTransform.forward, out hit, Mathf.Infinity);

        if (!hasHit)
        {
            // if we are not hitting anything, we should unselect the shelf we were hovering over
            if (hoveredShelf != null)
            {
                hoveredShelf.isSelected = false;
                hoveredShelf = null;
            }
        }
        else
        {
            // if we are hitting something, we should select the shelf we are hovering over
            GameObject hitObject = hit.collider.gameObject;
            if (hitObject.tag == "shelfHighlight")
            {
                GameObject shelf = hitObject.transform.parent.gameObject;
                if (hoveredShelf != null && hoveredShelf != shelf)
                {
                    hoveredShelf.isSelected = false;
                }
                hoveredShelf = shelf.GetComponent<Shelf>();
                hoveredShelf.isSelected = true;

                // Checking that the user pushed the trigger
                if (this.isTriggerPressedOnce)
                {
                    if (hoveredShelf != manipulatedShelf)
                    {
                        if(manipulatedShelf != null) manipulatedShelf.Release();
                        manipulatedShelf = hoveredShelf;
                        hoveredShelf.FlyToHand(rightController.transform);
                    }
                }
            }
        }

        // Determining the end of the LineRenderer depending on whether we hit an object or not
        if (hasHit)
        {
            lineRenderer.SetPosition(1, hit.point);
        }
        else
        {
            lineRenderer.SetPosition(1, raycastMaxDistance * rightControllerTransform.forward);
        }

        // DO NOT REMOVE
        // If currentSelectedObject is not null, this will send it to the TaskManager for handling
        base.CheckForSelection();
    }

}
