using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectConfiguration : MonoBehaviour
{
    [SerializeField]
    private Material targetMaterial;

    [SerializeField]
    private Material successMaterial;

    private List<GameObject> selectableObjects;

    private void Awake()
    {
        selectableObjects = new List<GameObject>();
        for (int i = 0; i < this.transform.childCount; i++)
        {
            GameObject objectCategory = this.transform.GetChild(i).gameObject;
            for (int j = 0; j < objectCategory.transform.childCount; j++)
            {
                GameObject objectToConfigure = objectCategory.transform.GetChild(j).gameObject;
                selectableObjects.Add(objectToConfigure);

                // Add a SelectableObject script to the object
                SelectableObject selectableObject = objectToConfigure.AddComponent<SelectableObject>();
                selectableObject.SetObjectName("ObjectCategory_" + i.ToString() + "_Number_" + j.ToString());
                selectableObject.SetTargetMaterial(targetMaterial);
                selectableObject.SetSuccessMaterial(successMaterial);

                // Add a BoxCollider to the object
                BoxCollider boxCollider = objectToConfigure.AddComponent<BoxCollider>();
                boxCollider.isTrigger = true;

                // Add a kinematic RigidBody to the object
                Rigidbody rigidbody = objectToConfigure.AddComponent<Rigidbody>();
                rigidbody.isKinematic = true;
            }
        }
    }
}
