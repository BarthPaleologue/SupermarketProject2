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

    private void Start()
    {
        defaultMaterial = this.GetComponent<MeshRenderer>().material;

        // set tag to "groceryItem"
        this.tag = "groceryItem";

        // raycast downward to set parent to the shelf below
        RaycastHit hit;
        float distance = 2.0f;
        Vector3 dir = Vector3.down;
        if (Physics.Raycast(transform.position, dir, out hit, distance))
        {
            transform.parent = hit.transform;
        }
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
