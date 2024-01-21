using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum ShelfState
{
    Idle,
    Manipulating,
    Releasing
}

public class Shelf : MonoBehaviour
{
    public bool isSelected = false;

    public Material material;

    [SerializeField] private Vector3 originalPosition;
    [SerializeField] private Quaternion originalRotation;

    [SerializeField] private ShelfState state = ShelfState.Idle;

    // Start is called before the first frame update
    void Start()
    {
        originalPosition = this.transform.position;
        originalRotation = this.transform.rotation;

        // get the size of the shelf
        Vector3 shelfSize = this.GetComponent<Renderer>().bounds.size;

        // create a new box
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);

        // add tag to box
        box.tag = "shelfHighlight";
        
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

        if(state == ShelfState.Manipulating) {
            // do some stuff
        }

        // The shelf is flying away
        if(state == ShelfState.Releasing) {
            this.transform.position = Vector3.MoveTowards(this.transform.position, originalPosition, 0.4f);
            this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, originalRotation, 15.0f);

            if(this.transform.position == originalPosition && this.transform.rotation == originalRotation) {
                state = ShelfState.Idle;
            }
        }
    }
}
