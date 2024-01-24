using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectableObject : MonoBehaviour
{
    private string objectName = "undefined";

    private Material defaultMaterial;
    // The material used to indicate to the user that this is the object to select
    private Material targetMaterial;
    // The material used to indicate to the user that this object was successfully selected
    private Material successMaterial;

    private GameObject boundingBoxHelper;

    private void Start()
    {
        defaultMaterial = this.GetComponent<MeshRenderer>().material;

        // set layer to "GroceryItems" (layer 6)
        this.gameObject.layer = 6;

        // raycast downward to set parent to the shelf below
        RaycastHit hit;
        float distance = 2.0f;
        Vector3 dir = Vector3.down;
        if (Physics.Raycast(transform.position, dir, out hit, distance))
        {
            transform.parent = hit.transform;
        }

        // create a new box
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.transform.position = this.transform.position;

        Vector3 bounds = this.GetComponent<Renderer>().bounds.size;
        Vector3 boxSize = bounds + new Vector3(0.01f, 0.01f, 0.01f);
        box.transform.localScale = boxSize;

        // remove the collider from the box
        Destroy(box.GetComponent<BoxCollider>());

        // move the box up by half its height
        //box.transform.position = new Vector3(box.transform.position.x, box.transform.position.y + boxHeight / 2 - shelfSize.y / 2, box.transform.position.z);

        // set the material of the box to be transparent
        Material material = new Material(Shader.Find("Transparent/Diffuse"));
        material.color = new Color(1, 1, 1, 0.3f);
        box.GetComponent<Renderer>().material = material;
        box.transform.parent = this.transform;
        box.SetActive(false);

        this.boundingBoxHelper = box;
    }

    public void DisplayBoundingBox(bool display)
    {
        this.boundingBoxHelper.SetActive(display);
    }

    public string GetObjectName()
    {
        return objectName;
    }

    public void SetObjectName(string objectName)
    {
        this.objectName = objectName;
    }

    public void SetTargetMaterial(Material material)
    {
        this.targetMaterial = material;
    }

    public void SetSuccessMaterial(Material material)
    {
        this.successMaterial = material;
    }

    public void SetAsTarget()
    {
        this.GetComponent<MeshRenderer>().material = targetMaterial;
    }

    public void SetAsSuccess()
    {
        this.GetComponent<MeshRenderer>().material = successMaterial;
        StartCoroutine(DelayResetMaterial());
    }

    public IEnumerator DelayResetMaterial()
    {
        // Reset to original material after 5 seconds
        yield return new WaitForSeconds(5f);
        this.GetComponent<MeshRenderer>().material = defaultMaterial;
    }
}
