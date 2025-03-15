using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraSettings : MonoBehaviour
{
    void Start()
    {
        PhysicsRaycaster physicsRaycaster = GameObject.FindObjectOfType<PhysicsRaycaster>();

        if (physicsRaycaster == null)
            Camera.main.gameObject.AddComponent<PhysicsRaycaster>();
    }
}