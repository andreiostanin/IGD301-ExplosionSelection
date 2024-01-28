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
