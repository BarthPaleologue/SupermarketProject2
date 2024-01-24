using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum ShelfState
{
    Idle,
    FlyingToHand,
    Manipulating,
    Releasing
}

public class Shelf : MonoBehaviour
{
    public bool isSelected = false;

    public Material material;

    [SerializeField] private Vector3 originalPosition;
    [SerializeField] private Quaternion originalRotation;
    [SerializeField] private Vector3 originalScale;

    [SerializeField] private float flySpeed = 0.4f;
    [SerializeField] private float rotateSpeed = 15.0f;
    [SerializeField] private float scaleSpeed = 0.2f;

    [SerializeField] private ShelfState state = ShelfState.Idle;

    Transform targetParentHand = null;
    Vector3 targetScale = new Vector3(0.1f, 0.1f, 0.1f);

    GameObject highlightBox;

    // Start is called before the first frame update
    void Start()
    {
        originalPosition = this.transform.position;
        originalRotation = this.transform.rotation;
        originalScale = this.transform.localScale;

        // get the size of the shelf
        Vector3 shelfSize = this.GetComponent<Renderer>().bounds.size;

        // create a new box
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);

        // add tag to box
        box.tag = "shelfHighlight";

        // set layer of box to "ShelvesHighlights" (layer 3)
        box.layer = 3;
        
        // position the box at the same position as the shelf
        box.transform.position = this.transform.position;

        // set the height of the box
        float boxHeight = 2.2f;
        Vector3 boxSize = new Vector3(shelfSize.x, boxHeight, shelfSize.z);
        box.transform.localScale = boxSize * 1.02f;

        // move the box up by half its height
        box.transform.position = new Vector3(box.transform.position.x, box.transform.position.y + boxHeight / 2 - shelfSize.y / 2, box.transform.position.z);

        // set the material of the box to be transparent
        material = new Material(Shader.Find("Transparent/Diffuse"));

        // change the color of the box
        material.color = new Color(1, 0, 0, 0.5f);

        // set the material of the box
        box.GetComponent<Renderer>().material = material;

        // make the box a child of the shelf
        box.transform.parent = this.transform;

        this.highlightBox = box;
    }

    public void FlyToHand(Transform hand)
    {
        state = ShelfState.FlyingToHand;
        this.targetParentHand = hand;

        float distance = (hand.position - this.transform.position).magnitude;
        flySpeed = distance / 10;

        // for every child that has layer "GroceryItems" (layer 6), set it to layer "SelectedGroceryItems" (layer 7)
        foreach (Transform child in this.transform)
        {
            if (child.gameObject.layer == 6)
            {
                child.gameObject.layer = 7;
            }
        }

        // disable the highlight box
        this.highlightBox.SetActive(false);
    }

    public void Release() {
        state = ShelfState.Releasing;
        this.transform.parent = null;
        this.targetParentHand = null;

        // for every child that has layer "SelectedGroceryItems" (layer 7), set it to layer "GroceryItems" (layer 6)
        foreach (Transform child in this.transform)
        {
            if (child.gameObject.layer == 7)
            {
                child.gameObject.layer = 6;
            }
        }

        // show the highlight box
        this.highlightBox.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (isSelected)
        {
            material.color = new Color(0, 1, 0, 0.5f);
        }
        else
        {
            material.color = new Color(1, 1, 1, 0.2f);
        }

        if(state == ShelfState.FlyingToHand) {
            if(this.targetParentHand == null) {
                Debug.Log("targetParentHand is null");
            }

            Vector3 targetPosition = this.targetParentHand.position;
            // move the shelf in the forward direction of the hand and a little bit in the up direction of the hand
            targetPosition += this.targetParentHand.forward * 0.2f;
            targetPosition += this.targetParentHand.up * 0.1f;

            Quaternion targetRotation = this.targetParentHand.rotation;
            // rotate the shelf 90 degrees around the y axis
            targetRotation *= Quaternion.Euler(0, 90, 0);

            this.transform.position = Vector3.MoveTowards(this.transform.position, targetPosition, this.flySpeed);
            this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, targetRotation, this.rotateSpeed);
            this.transform.localScale = Vector3.MoveTowards(this.transform.localScale, this.targetScale, this.scaleSpeed);

            if(this.transform.position == targetPosition && this.transform.rotation == targetRotation && this.transform.localScale == this.targetScale) {
                state = ShelfState.Manipulating;
                this.transform.parent = targetParentHand;
            }
        }

        if(state == ShelfState.Manipulating) {
            // do some stuff
        }

        // The shelf is flying away
        if(state == ShelfState.Releasing) {
            this.transform.position = Vector3.MoveTowards(this.transform.position, originalPosition, this.flySpeed * 2);
            this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, originalRotation, this.rotateSpeed * 2);
            this.transform.localScale = Vector3.MoveTowards(this.transform.localScale, originalScale, this.scaleSpeed * 2);

            if(this.transform.position == originalPosition && this.transform.rotation == originalRotation && this.transform.localScale == originalScale) {
                state = ShelfState.Idle;
            }
        }
    }
}
