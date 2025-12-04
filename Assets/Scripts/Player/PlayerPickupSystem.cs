using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class PlayerPickupSystem : MonoBehaviour
{
    public Transform holdPoint;                      
    public float pickUpRange = 3f;

    private GameObject heldObject;
    private Rigidbody heldRb;
    private PlayerInputHandler inputHandler;  

    void Awake()
    {
        inputHandler = GetComponent<PlayerInputHandler>();
    }

    void Update()
    {
        if (inputHandler != null)
        {
            if (inputHandler.InteractTriggered)
            {
                if (heldObject == null)
                    TryPickUpObject();
            }

            if (inputHandler.DropTriggered)
            {
                if (heldObject != null)
                    DropObject();
            }
        }
    }

    void TryPickUpObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width/2, Screen.height/2));
        Debug.DrawRay(ray.origin, ray.direction * pickUpRange, Color.red);
        int layerMask = LayerMask.GetMask("Interactable");
        if (Physics.Raycast(ray, out RaycastHit hit, pickUpRange, layerMask))
        {
            Debug.Log("Raycast hit: " + hit.collider.name);
            PickupItem interactObj = hit.collider.GetComponent<PickupItem>();
            if (interactObj != null && interactObj.isHeld == false)
            {
                PickUpObject(hit.collider.gameObject);
            }
        }
        else
        {
            Debug.Log("Raycast didn't hit anything.");
        }
    }


    void PickUpObject(GameObject obj)
    {
        heldObject = obj;
        heldRb = obj.GetComponent<Rigidbody>();

        obj.GetComponent<PickupItem>().isHeld = true;

        if (heldRb != null)
        {
            heldRb.isKinematic = true;
        }

        /*foreach(Collider col in obj.GetComponents<Collider>())
        {
            col.enabled = false;
        }*/

        obj.transform.SetParent(holdPoint);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 90));
    }

    void DropObject()
    {
        if (heldObject != null)
        {
            heldObject.transform.SetParent(null);

            heldObject.GetComponent<PickupItem>().isHeld = false;

            if (heldRb != null)
            {
                heldRb.isKinematic = false;
            }

            /*foreach(Collider col in heldObject.GetComponents<Collider>())
            {
                col.enabled = true;
            }*/

            heldObject = null;
            heldRb = null;
        }
    }
}
