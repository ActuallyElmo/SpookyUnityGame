using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class DoorRaycast : MonoBehaviour
{
    [SerializeField] private int rayDistance = 5;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private string excludeLayerName = null;

    [Header("Input Action Asset")]
    [SerializeField] private InputActionAsset playerControls;

    [Header("Action Map Name Reference")]
    [SerializeField] private string actionMapName = "Player";     // Action map used for player controls

    [Header("Action Name References")]
    [SerializeField] private string interact = "Interact";
    private InputAction interactAction;

    private const string doorTag = "Door";
    private DoorController raycastedDoor = null;

    private void Awake()
    {
        InputActionMap mapReference = playerControls.FindActionMap(actionMapName);

        interactAction = mapReference.FindAction(interact);
    }

    private void Update()
    {
        RaycastHit hit;

        int mask = 1 << LayerMask.NameToLayer(excludeLayerName) | layerMask.value;

        if(Physics.Raycast(transform.position, transform.forward, out hit, rayDistance, mask))
        {
            if (hit.collider.CompareTag(doorTag))
            {
                if (!raycastedDoor)
                {
                    raycastedDoor = hit.collider.gameObject.GetComponent<DoorController>();
                    Debug.Log("Door Found");
                }

                if (interactAction.WasPressedThisFrame())
                {
                    raycastedDoor.PlayAnimation();
                }
            }
        }
        else
        {
            raycastedDoor = null;
        }
    }
}
