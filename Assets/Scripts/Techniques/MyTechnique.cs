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

    private LineRenderer leftHandLineRenderer;
    private LineRenderer rightHandLineRenderer;

    private Shelf leftHoveredShelf = null;
    private Shelf rightHoveredShelf = null;
    private Shelf manipulatedShelf = null;

    private bool isLeftTriggerPressed = false;
    private bool isLeftTriggerPressedOnce = false;

    private bool isRightTriggerPressed = false;
    private bool isRightTriggerPressedOnce = false;

    private void Start()
    {
        leftHandLineRenderer = leftController.GetComponent<LineRenderer>();
        rightHandLineRenderer = rightController.GetComponent<LineRenderer>();
    }

    private void UpdateInputState()
    {
        // Manage trigger press from left controller
        if (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) > 0.1f)
        {
            if (!isLeftTriggerPressed)
            {
                isLeftTriggerPressedOnce = true;
            }
            else
            {
                isLeftTriggerPressedOnce = false;
            }
            isLeftTriggerPressed = true;
        }
        else
        {
            isLeftTriggerPressed = false;
            isLeftTriggerPressedOnce = false;
        }

        // Manage trigger press from right controller
        if (OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) > 0.1f)
        {
            if (!isRightTriggerPressed)
            {
                isRightTriggerPressedOnce = true;
            }
            else
            {
                isRightTriggerPressedOnce = false;
            }
            isRightTriggerPressed = true;
        }
        else
        {
            isRightTriggerPressed = false;
            isRightTriggerPressedOnce = false;
        }
    }

    private void FixedUpdate()
    {
        Transform rightControllerTransform = rightController.transform;

        // Set the beginning of the line renderer to the position of the controller
        leftHandLineRenderer.SetPosition(0, leftController.transform.position);
        rightHandLineRenderer.SetPosition(0, rightControllerTransform.position);

        UpdateInputState();

        // Creating a raycast and storing the first hit if existing
        RaycastHit rightHit;
        bool hasRightHit = Physics.Raycast(rightControllerTransform.position, rightControllerTransform.forward, out rightHit, Mathf.Infinity);

        if (!hasRightHit)
        {
            // if we are not hitting anything, we should unselect the shelf we were hovering over
            if (rightHoveredShelf != null)
            {
                rightHoveredShelf.isSelected = false;
                rightHoveredShelf = null;
            }
        }
        else
        {
            // if we are hitting something, we should select the shelf we are hovering over
            GameObject hitObject = rightHit.collider.gameObject;
            if (hitObject.tag == "shelfHighlight")
            {
                GameObject shelfObject = hitObject.transform.parent.gameObject;
                Shelf shelf = shelfObject.GetComponent<Shelf>();

                if (shelf == manipulatedShelf)
                {
                    // the ray hit the shelf that is currently being manipulated.
                    // We then perform a second raycast on the items of the shelf to see if we hit one of them (layer 6)
                    RaycastHit itemHit;
                    bool hasItemHit = Physics.Raycast(rightControllerTransform.position, rightControllerTransform.forward, out itemHit, Mathf.Infinity, 1 << 6);
                    if (hasItemHit)
                    {
                        // we hit an item, we should select it
                        GameObject item = itemHit.collider.gameObject;

                        if (item.tag == "selectableGroceryItem")
                        {
                            SelectableObject selectableObject = item.GetComponent<SelectableObject>();
                            if (selectableObject != null)
                            {
                                if (this.isRightTriggerPressedOnce)
                                {
                                    this.currentSelectedObject = item;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (rightHoveredShelf != null && rightHoveredShelf != shelf)
                    {
                        rightHoveredShelf.isSelected = false;
                    }
                    rightHoveredShelf = shelf;
                    rightHoveredShelf.isSelected = true;

                    // Checking that the user pushed the trigger
                    if (this.isRightTriggerPressedOnce)
                    {
                        if (rightHoveredShelf != manipulatedShelf)
                        {
                            if (manipulatedShelf != null) manipulatedShelf.Release();
                            manipulatedShelf = rightHoveredShelf;
                            rightHoveredShelf.FlyToHand(rightController.transform);
                        }
                    }
                }
            }
        }

        // Determining the end of the LineRenderer depending on whether we hit an object or not
        if (hasRightHit)
        {
            rightHandLineRenderer.SetPosition(1, rightHit.point);
        }
        else
        {
            rightHandLineRenderer.SetPosition(1, raycastMaxDistance * rightControllerTransform.forward);
        }


        // Creating a raycast and storing the first hit if existing
        RaycastHit leftHit;
        bool hasLeftHit = Physics.Raycast(leftController.transform.position, leftController.transform.forward, out leftHit, Mathf.Infinity);

        if (!hasLeftHit)
        {
            // if we are not hitting anything, we should unselect the shelf we were hovering over
            if (leftHoveredShelf != null)
            {
                leftHoveredShelf.isSelected = false;
                leftHoveredShelf = null;
            }
        }
        else
        {
            // if we are hitting something, we should select the shelf we are hovering over
            GameObject hitObject = leftHit.collider.gameObject;
            if (hitObject.tag == "shelfHighlight")
            {
                GameObject shelf = hitObject.transform.parent.gameObject;
                if (leftHoveredShelf != null && leftHoveredShelf != shelf)
                {
                    leftHoveredShelf.isSelected = false;
                }
                leftHoveredShelf = shelf.GetComponent<Shelf>();
                leftHoveredShelf.isSelected = true;

                // Checking that the user pushed the trigger
                if (this.isLeftTriggerPressedOnce)
                {
                    if (leftHoveredShelf != manipulatedShelf)
                    {
                        if (manipulatedShelf != null) manipulatedShelf.Release();
                        manipulatedShelf = leftHoveredShelf;
                        leftHoveredShelf.FlyToHand(leftController.transform);
                    }
                }
            }
        }

        if (hasLeftHit)
        {
            leftHandLineRenderer.SetPosition(1, leftHit.point);
        }
        else
        {
            leftHandLineRenderer.SetPosition(1, raycastMaxDistance * leftController.transform.forward);
        }

        // DO NOT REMOVE
        // If currentSelectedObject is not null, this will send it to the TaskManager for handling
        base.CheckForSelection();
    }

}
