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

        // raycast downward to set parent to the shelf below (layer 8)
        RaycastHit hit;
        float distance = 2.0f;
        Vector3 dir = Vector3.down;
        if (Physics.Raycast(transform.position, dir, out hit, distance, 1 << 8))
        {
            transform.parent = hit.transform;
        }

        // create a new box to representing the box collider (same scale and orientation as the object, with bounds of the box collider)
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.transform.localScale = this.GetComponent<BoxCollider>().size + new Vector3(0.01f, 0.01f, 0.01f);
        box.transform.rotation = this.transform.rotation;
        box.transform.position = this.transform.position + this.GetComponent<BoxCollider>().center;

        // set the box to be a child of the object
        box.transform.parent = this.transform;


        // remove the box collider from the box
        Destroy(box.GetComponent<BoxCollider>());

        // set the material of the box to be transparent
        Material material = new Material(Shader.Find("Transparent/Diffuse"));
        material.color = new Color(1, 1, 1, 0.3f);
        box.GetComponent<Renderer>().material = material;
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
