using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum State
{
    Idle,
    ManipulatingLeft,
    ManipulatingRight,
}

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

    private State state = State.Idle;

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

        // Update line renderer to default position (this will be overwritten if we hit something)
        rightHandLineRenderer.SetPosition(0, rightControllerTransform.position + rightControllerTransform.forward * 0.05f);
        rightHandLineRenderer.SetPosition(1, raycastMaxDistance * rightControllerTransform.forward);

        leftHandLineRenderer.SetPosition(0, leftController.transform.position + leftController.transform.forward * 0.05f);
        leftHandLineRenderer.SetPosition(1, raycastMaxDistance * leftController.transform.forward);

        UpdateInputState();

        bool isShelfSelectionNeeded = true;

        if (state == State.ManipulatingLeft)
        {
            // We perform a raycast from the right controller on the layer "SelectableGroceryItems" (layer 7)
            RaycastHit itemHit;
            bool hasItemHit = Physics.Raycast(rightControllerTransform.position, rightControllerTransform.forward, out itemHit, Mathf.Infinity, 1 << 7);
            if (hasItemHit)
            {
                GameObject item = itemHit.collider.gameObject;

                if (item.tag == "groceryItem")
                {
                    SelectableObject selectableObject = item.GetComponent<SelectableObject>();
                    if (selectableObject != null)
                    {
                        if (this.isRightTriggerPressedOnce)
                        {
                            this.currentSelectedObject = item;
                        }
                    }

                    rightHandLineRenderer.SetPosition(1, itemHit.point);
                    isShelfSelectionNeeded = false;
                }
            }
        }
        else if (state == State.ManipulatingRight)
        {
            // We perform a raycast from the left controller on the layer "SelectableGroceryItems" (layer 7)
            RaycastHit itemHit;
            bool hasItemHit = Physics.Raycast(leftController.transform.position, leftController.transform.forward, out itemHit, Mathf.Infinity, 1 << 7);
            if (hasItemHit)
            {
                GameObject item = itemHit.collider.gameObject;

                if (item.tag == "groceryItem")
                {
                    SelectableObject selectableObject = item.GetComponent<SelectableObject>();
                    if (selectableObject != null)
                    {
                        if (this.isLeftTriggerPressedOnce)
                        {
                            this.currentSelectedObject = item;
                        }
                    }

                    leftHandLineRenderer.SetPosition(1, itemHit.point);
                    isShelfSelectionNeeded = false;
                }
            }
        }

        if (isShelfSelectionNeeded)
        {

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
                        // As we are selecting an item on the manipulated shelf, we are not hovering over another shelf
                        if (rightHoveredShelf != null)
                        {
                            rightHoveredShelf.isSelected = false;
                            rightHoveredShelf = null;
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

                                state = State.ManipulatingRight;
                            }
                        }

                        rightHandLineRenderer.SetPosition(1, rightHit.point);
                    }
                }
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

                            state = State.ManipulatingLeft;
                        }
                    }

                    leftHandLineRenderer.SetPosition(1, leftHit.point);
                }
            }
        }


        // DO NOT REMOVE
        // If currentSelectedObject is not null, this will send it to the TaskManager for handling
        base.CheckForSelection();
    }

}
