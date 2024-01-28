using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Implemented technique inherits the InteractionTechnique class
public class MyTechnique : InteractionTechnique
{
    
    [SerializeField] private GameObject explosionRadiusIndicator;
    
    [SerializeField] private float movementSpeed = 4f;
    private OVRCameraRig cameraRig;
    
    [SerializeField] private float maxExplosionRadius = 4f;
    [SerializeField] private float minExplosionRadius = 0.5f;
    [SerializeField] private float explosionRadiusChangeSpeed = 1f;
    [SerializeField] private float explosionRadius = 1f;
    [SerializeField] private float maxExplosionForce = 75f;
    [SerializeField] private float falloffCoefficient = 0.05f; 
    
    [SerializeField] private float freezeDelay = 0.75f;
    [SerializeField] private float freezeDistanceThreshold = 2.8f;
    
    [SerializeField] private float baseDispersionAmount = 0.3f;
    [SerializeField] private float maxDistanceForDispersion = 35f;

    [SerializeField] private GameObject rightController;
    
    private Dictionary<Rigidbody, (Vector3 position, Quaternion rotation)> initialTransforms = new Dictionary<Rigidbody, (Vector3, Quaternion)>();
    private bool hasExploded = false;
    private Vector3 explosionPosition;
    private Coroutine freezeCoroutine;
    private bool isFreezeCoroutineRunning = false;
    private bool isFrozen = false;
    
    private LineRenderer lineRenderer;
    private bool isTriggerReleased = true;
    
    private void Start()
    {
        lineRenderer = rightController.GetComponent<LineRenderer>();
        explosionRadius = Mathf.Clamp(explosionRadius, minExplosionRadius, maxExplosionRadius);
        
        cameraRig = FindObjectOfType<OVRCameraRig>();
    }

    private void FixedUpdate()
    {
        Transform rightControllerTransform = rightController.transform;
        
        lineRenderer.SetPosition(0, rightControllerTransform.position);
        
        RaycastHit hit;
        bool hasHit = Physics.Raycast(rightControllerTransform.position, rightControllerTransform.forward, out hit, Mathf.Infinity);
        
        // Check if the trigger is pressed to initiate explosion
        if (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch) > 0.1f)
        {
            if (isTriggerReleased){
                // A raycast from the controller
                isTriggerReleased = false;
                if (hasHit && !isFreezeCoroutineRunning)
                {
                    // Hit point
                    explosionPosition = hit.point;

                    // Revert objects back to their initial positions
                    RevertObjects();
                    
                    // Explosion
                    Explode(explosionPosition);
                    
                    hasExploded = true;
                    isFrozen = false;
                }
            }
        }
        else
        {
            isTriggerReleased = true; // Reset the flag when the trigger is released
        }

        // If still in flight - check distance
        if (hasExploded && !isFrozen)
        {
            CheckDistanceForFreeze();
        }
        
        // Check for manual selection after explosion
        if (hasExploded && OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch) > 0.1f)
        {
            SelectObjectManually();
        }
        
        // Revert objects
        if (hasExploded && OVRInput.GetDown(OVRInput.RawButton.A) && OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch)==0)
        {
            RevertObjects();
            isFrozen = false;
        }

        // For raycast visualisation
        if (hasHit)
        {
            lineRenderer.SetPosition(1, hit.point);
            if (Vector3.Distance(hit.point, rightController.transform.position) < 1.5f)
            {
                explosionRadiusIndicator.SetActive(false);
            } else explosionRadiusIndicator.SetActive(true);

            explosionRadiusIndicator.transform.position = hit.point;
        }
        else
        {
            explosionRadiusIndicator.SetActive(false);
            lineRenderer.SetPosition(1, 1000 * rightControllerTransform.forward);
        }
        
        explosionRadiusIndicator.transform.localScale = Vector3.one * explosionRadius;

        // Movement/sphere radius on joystick
        if (!OVRInput.Get(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.RTouch))
        {
            MovePlayer();
        } else
        {
            ScaleExplosionRadius();
        }
        
        // DO NOT REMOVE
        // If currentSelectedObject is not null, this will send it to the TaskManager for handling
        // Then it will set currentSelectedObject back to null
        base.CheckForSelection();
    }
    
    private void Explode(Vector3 explosionPosition)
    {
        Collider[] colliders = Physics.OverlapSphere(explosionPosition, explosionRadius);

        // For eavh object inside the explosion
        foreach (Collider col in colliders)
        {
            Rigidbody rb = col.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = false;

                // Store initial transform
                initialTransforms[rb] = (rb.transform.position, rb.transform.rotation);
                
                Vector3 direction = (rightController.transform.position - rb.transform.position).normalized;
                float distance = Vector3.Distance(rb.transform.position, explosionPosition);
                float distancePlayer = Vector3.Distance(rb.transform.position, rightController.transform.position);

                float dynamicDispersionAmount = baseDispersionAmount * Mathf.Clamp01(1 - (distancePlayer / maxDistanceForDispersion));
                Vector3 dispersedDirection = direction + Random.insideUnitSphere * dynamicDispersionAmount;
                
                float force = Mathf.Clamp(maxExplosionForce / (1 + falloffCoefficient * distance), 0f, maxExplosionForce);
                rb.isKinematic = false;
                rb.AddForce(dispersedDirection * force, ForceMode.Impulse);
                
                rb.angularVelocity = Random.insideUnitSphere * maxExplosionForce;

            }
        }
        
        if (freezeCoroutine != null)
        {
            StopCoroutine(freezeCoroutine);
        }
        
        freezeCoroutine = StartCoroutine(FreezeObjects());
    }

    private IEnumerator FreezeObjects()
    {
        isFreezeCoroutineRunning = true;
        yield return new WaitForSeconds(freezeDelay);

        Freeze();
        isFrozen = true;
        
        isFreezeCoroutineRunning = false;   
    }

    private void CheckDistanceForFreeze()
    {
        foreach (var pair in initialTransforms)
        {
            Rigidbody rb = pair.Key;
            if (rb != null)
            {
                float distance = Vector3.Distance(rb.transform.position, rightController.transform.position);
                if (distance < freezeDistanceThreshold)
                {
                    isFreezeCoroutineRunning = false;
                    
                    if (freezeCoroutine != null)
                        StopCoroutine(freezeCoroutine);

                    // Call the Freeze function directly
                    Freeze();
                    isFrozen = true;
                    
                    return; // No need to check other objects
                }
            }
        }
    }
    
    private void Freeze()
    {
        foreach (var pair in initialTransforms)
        {
            Rigidbody rb = pair.Key;
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }
        }
    }
    
    private void RevertObjects()
    {
        hasExploded = false;
        foreach (var pair in initialTransforms)
        {
            Rigidbody rb = pair.Key;
            if (rb != null)
            {
                // Reset position and rotation
                rb.transform.position = pair.Value.position;
                rb.transform.rotation = pair.Value.rotation;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        initialTransforms.Clear();
    }
    
    private void SelectObjectManually()
    {
        Collider[] colliders = Physics.OverlapSphere(rightController.transform.position, 0.1f);

        foreach (Collider collider in colliders)
        {
            GameObject collidingObject = collider.gameObject;

            Rigidbody rb = collidingObject.GetComponent<Rigidbody>();
            if (rb != null && initialTransforms.ContainsKey(rb))
            {
                // Select the object
                currentSelectedObject = collidingObject;
                return;
            }
        }
    }
    
    private void ScaleExplosionRadius()
    {
        float verticalInput = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch).y;

        // Change explosion radius based on input
        explosionRadius += verticalInput * explosionRadiusChangeSpeed * Time.deltaTime;
        explosionRadius = Mathf.Clamp(explosionRadius, minExplosionRadius, maxExplosionRadius);
    }
    
    private void MovePlayer()
    {
        if (cameraRig == null || cameraRig.gameObject == null)
            return;

        // Get joystick input from the right controller
        float horizontalInput = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch).x;
        float verticalInput = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch).y;

        Vector3 forwardDirection = cameraRig.centerEyeAnchor.forward;
        Vector3 rightDirection = cameraRig.centerEyeAnchor.right;
        forwardDirection.y = 0; // Ignore vertical components for horizontal movement
        rightDirection.y = 0;
        
        // Calculate movement direction based on input
        Vector3 movementDirection = (forwardDirection * verticalInput + rightDirection * horizontalInput).normalized;

        // Apply movement to the player's transform
        cameraRig.transform.position += movementDirection * movementSpeed * Time.deltaTime;
    }
} 