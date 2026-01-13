using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class DoorRaycast : MonoBehaviour
{
    [SerializeField] private int rayDistance = 5;
    //[SerializeField] private LayerMask layerMask;
    //[SerializeField] private string excludeLayerName = null;

    [Header("Input Action Asset")]
    [SerializeField] private InputActionAsset playerControls;

    [Header("Action Map Name Reference")]
    [SerializeField] private string actionMapName = "Player";     // Action map used for player controls

    [Header("Action Name References")]
    [SerializeField] private string interact = "Interact";
    private InputAction interactAction;

    private const string doorTag = "Door";
    private const string doubleDoorTag = "DoubleDoor";
    private const string lockerTag = "Locker";
    private DoorController raycastedDoor = null;

    private void Awake()
    {
        InputActionMap mapReference = playerControls.FindActionMap(actionMapName);

        interactAction = mapReference.FindAction(interact);
    }

    private void Update()
    {
        RaycastHit hit;

        if(Physics.Raycast(transform.position, transform.forward, out hit, rayDistance))
        {
            if (hit.collider.CompareTag(doorTag) || hit.collider.CompareTag(lockerTag))
            {
                if (!raycastedDoor)
                {
                    raycastedDoor = hit.collider.gameObject.GetComponent<DoorController>();
                    Debug.Log("Door Found1");
                }

                if (interactAction.WasPressedThisFrame())
                {
                    raycastedDoor.Interact();
                }
            }
            else if (hit.collider.CompareTag(doubleDoorTag))
            {
                if (!raycastedDoor)
                {
                    raycastedDoor = hit.collider.gameObject.GetComponentInParent<DoorController>();
                    Debug.Log("Door Found2");
                    Debug.Log(raycastedDoor.gameObject.name);
                }

                if (interactAction.WasPressedThisFrame())
                {
                    raycastedDoor.Interact();
                }
            }
        }
        else
        {
            raycastedDoor = null;
        }
    }
}
