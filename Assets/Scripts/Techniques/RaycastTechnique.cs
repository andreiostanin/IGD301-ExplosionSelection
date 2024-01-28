using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastTechnique : InteractionTechnique
{
    [SerializeField]
    int raycastMaxDistance = 1000;

    [SerializeField]
    private GameObject rightController;

    private LineRenderer lineRenderer;

    private void Start()
    {
        lineRenderer = rightController.GetComponent<LineRenderer>();
    }

    private void FixedUpdate()
    {
        Transform rightControllerTransform = rightController.transform;
        
        // Set the beginning of the line renderer to the position of the controller
        lineRenderer.SetPosition(0, rightControllerTransform.position);

        // Creating a raycast and storing the first hit if existing
        RaycastHit hit;
        bool hasHit = Physics.Raycast(rightControllerTransform.position, rightControllerTransform.forward, out hit, Mathf.Infinity);

        // Checking that the user pushed the trigger
        if (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) > 0.1f && hasHit)
        {
            // Sending the selected object hit by the raycast
            currentSelectedObject = hit.collider.gameObject;
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
