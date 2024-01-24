using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum State
{
    Idle,
    ManipulatingLeft,
    ManipulatingRight,
}

public enum Hand
{
    Left,
    Right
}

// Your implemented technique inherits the InteractionTechnique class
public class MyTechnique : InteractionTechnique
{
    [SerializeField]
    int raycastMaxDistance = 1000;

    [SerializeField]
    private GameObject OVRCameraRig;

    [SerializeField]
    private GameObject leftController;

    [SerializeField]
    private GameObject rightController;

    private LineRenderer leftHandLineRenderer;
    private LineRenderer rightHandLineRenderer;

    private GameObject leftTeleportTarget;
    private GameObject rightTeleportTarget;

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


        Material material = new Material(Shader.Find("Transparent/Diffuse"));
        material.color = new Color(0, 0, 1, 0.5f);

        // Creating a teleport target for the left controller
        leftTeleportTarget = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        leftTeleportTarget.transform.localScale = new Vector3(2.0f, 2.0f, 2.0f);
        leftTeleportTarget.GetComponent<Renderer>().material = material;
        leftTeleportTarget.GetComponent<Collider>().enabled = false;
        leftTeleportTarget.SetActive(false);

        // Creating a teleport target for the right controller
        rightTeleportTarget = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rightTeleportTarget.transform.localScale = new Vector3(2.0f, 2.0f, 2.0f);
        rightTeleportTarget.GetComponent<Renderer>().material = material;
        rightTeleportTarget.GetComponent<Collider>().enabled = false;
        rightTeleportTarget.SetActive(false);
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

    private void HandleRightShelfSelection()
    {
        // Creating a raycast and storing the first hit if existing
        RaycastHit rightHit;
        bool hasRightHit = Physics.Raycast(rightController.transform.position, rightController.transform.forward, out rightHit, Mathf.Infinity);

        if (!hasRightHit)
        {
            // if we are not hitting anything, we should unselect the shelf we were hovering over
            if (rightHoveredShelf != null)
            {
                rightHoveredShelf.SetHighlighted(false);
                rightHoveredShelf = null;
            }

            if (this.isRightTriggerPressedOnce && manipulatedShelf != null)
            {
                manipulatedShelf.Release();
                manipulatedShelf = null;
                state = State.Idle;
            }
        }
        else
        {
            // if we are hitting something, we should select the shelf we are hovering over
            GameObject hitObject = rightHit.collider.gameObject;
            if (hitObject.tag == "Ground")
            {
                if (rightTeleportTarget != null)
                {
                    rightTeleportTarget.SetActive(true);
                    rightTeleportTarget.transform.position = rightHit.point;
                    if (this.isRightTriggerPressedOnce)
                    {
                        OVRCameraRig.transform.position = new Vector3(rightHit.point.x, OVRCameraRig.transform.position.y, rightHit.point.z);
                    }
                }
            }
            else
            {
                if (rightTeleportTarget != null)
                {
                    rightTeleportTarget.SetActive(false);
                }
            }
            if (hitObject.tag == "shelfHighlight")
            {
                GameObject shelfObject = hitObject.transform.parent.gameObject;
                Shelf shelf = shelfObject.GetComponent<Shelf>();

                if (shelf == manipulatedShelf)
                {
                    // As we are selecting an item on the manipulated shelf, we are not hovering over another shelf
                    if (rightHoveredShelf != null)
                    {
                        rightHoveredShelf.SetHighlighted(false);
                        rightHoveredShelf = null;
                    }
                }
                else
                {
                    if (rightHoveredShelf != null && rightHoveredShelf != shelf)
                    {
                        rightHoveredShelf.SetHighlighted(false);
                    }
                    rightHoveredShelf = shelf;
                    rightHoveredShelf.SetHighlighted(true);

                    // Checking that the user pushed the trigger
                    if (this.isRightTriggerPressedOnce)
                    {
                        if (rightHoveredShelf != manipulatedShelf)
                        {
                            if (manipulatedShelf != null) manipulatedShelf.Release();
                            manipulatedShelf = rightHoveredShelf;
                            rightHoveredShelf.FlyToHand(rightController.transform, Hand.Right);

                            state = State.ManipulatingRight;
                        }
                    }

                    rightHandLineRenderer.SetPosition(1, rightHit.point);
                }
            }
        }
    }

    private void HandleLeftShelftSelection()
    {


        // Creating a raycast and storing the first hit if existing
        RaycastHit leftHit;
        bool hasLeftHit = Physics.Raycast(leftController.transform.position, leftController.transform.forward, out leftHit, Mathf.Infinity);

        if (!hasLeftHit)
        {
            // if we are not hitting anything, we should unselect the shelf we were hovering over
            if (leftHoveredShelf != null)
            {
                leftHoveredShelf.SetHighlighted(false);
                leftHoveredShelf = null;
            }

            if (this.isLeftTriggerPressedOnce && manipulatedShelf != null)
            {
                manipulatedShelf.Release();
                manipulatedShelf = null;
                state = State.Idle;
            }
        }
        else
        {
            // if we are hitting something, we should select the shelf we are hovering over
            GameObject hitObject = leftHit.collider.gameObject;
            if (hitObject.tag == "Ground")
            {
                if (leftTeleportTarget != null)
                {
                    leftTeleportTarget.SetActive(true);
                    leftTeleportTarget.transform.position = leftHit.point;
                    if (this.isLeftTriggerPressedOnce)
                    {
                        OVRCameraRig.transform.position = new Vector3(leftHit.point.x, OVRCameraRig.transform.position.y, leftHit.point.z);
                    }
                }
            }
            else
            {
                if (leftTeleportTarget != null)
                {
                    leftTeleportTarget.SetActive(false);
                }
            }
            if (hitObject.tag == "shelfHighlight")
            {
                GameObject shelf = hitObject.transform.parent.gameObject;
                if (leftHoveredShelf != null && leftHoveredShelf != shelf)
                {
                    leftHoveredShelf.SetHighlighted(false);
                }
                leftHoveredShelf = shelf.GetComponent<Shelf>();
                leftHoveredShelf.SetHighlighted(true);

                // Checking that the user pushed the trigger
                if (this.isLeftTriggerPressedOnce)
                {
                    if (leftHoveredShelf != manipulatedShelf)
                    {
                        if (manipulatedShelf != null) manipulatedShelf.Release();
                        manipulatedShelf = leftHoveredShelf;
                        leftHoveredShelf.FlyToHand(leftController.transform, Hand.Left);

                        state = State.ManipulatingLeft;
                    }
                }

                leftHandLineRenderer.SetPosition(1, leftHit.point);
            }
        }
    }

    private void FixedUpdate()
    {

        // Update line renderer to default position (this will be overwritten if we hit something)
        rightHandLineRenderer.SetPosition(0, rightController.transform.position + rightController.transform.forward * 0.05f);
        rightHandLineRenderer.SetPosition(1, raycastMaxDistance * rightController.transform.forward);

        leftHandLineRenderer.SetPosition(0, leftController.transform.position + leftController.transform.forward * 0.05f);
        leftHandLineRenderer.SetPosition(1, raycastMaxDistance * leftController.transform.forward);

        UpdateInputState();

        bool isRightShelfSelectionNeeded = true;
        bool isLeftShelfSelectionNeeded = true;

        if (state == State.ManipulatingLeft)
        {
            // We perform a raycast from the right controller on the layer "SelectableGroceryItems" (layer 7)
            RaycastHit itemHit;
            bool hasItemHit = Physics.Raycast(rightController.transform.position, rightController.transform.forward, out itemHit, Mathf.Infinity, 1 << 7);
            if (hasItemHit)
            {
                GameObject item = itemHit.collider.gameObject;

                SelectableObject selectableObject = item.GetComponent<SelectableObject>();
                if (selectableObject != null)
                {
                    if (this.isRightTriggerPressedOnce)
                    {
                        this.currentSelectedObject = item;
                    }
                }

                rightHandLineRenderer.SetPosition(1, itemHit.point);
                isRightShelfSelectionNeeded = false;

                if (rightHoveredShelf != null)
                {
                    rightHoveredShelf.SetHighlighted(false);
                    rightHoveredShelf = null;
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

                SelectableObject selectableObject = item.GetComponent<SelectableObject>();
                if (selectableObject != null)
                {
                    if (this.isLeftTriggerPressedOnce)
                    {
                        this.currentSelectedObject = item;
                    }
                }

                leftHandLineRenderer.SetPosition(1, itemHit.point);
                isLeftShelfSelectionNeeded = false;

                if (leftHoveredShelf != null)
                {
                    leftHoveredShelf.SetHighlighted(false);
                    leftHoveredShelf = null;
                }
            }
        }

        if (isRightShelfSelectionNeeded) HandleRightShelfSelection();
        if (isLeftShelfSelectionNeeded) HandleLeftShelftSelection();

        // DO NOT REMOVE
        // If currentSelectedObject is not null, this will send it to the TaskManager for handling
        base.CheckForSelection();
    }

}
