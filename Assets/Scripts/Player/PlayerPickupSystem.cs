using UnityEngine;

public class PlayerPickupSystem : MonoBehaviour
{
    public Transform holdPoint;                  
    public PlayerInputHandler inputHandler;      
    public float pickUpRange = 3f;

    private GameObject heldObject;
    private Rigidbody heldRb;

    void Update()
    {
        if (inputHandler != null)
        {
            if (inputHandler.PickupTriggered)
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
    int layerMask = LayerMask.GetMask("Pickup");
    if (Physics.Raycast(ray, out RaycastHit hit, pickUpRange, layerMask))
    {
        Debug.Log("Raycast hit: " + hit.collider.name);
        InteractableObject interactObj = hit.collider.GetComponent<InteractableObject>();
        if (interactObj != null && interactObj.canBePickedUp)
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

        if (heldRb != null)
        {
            heldRb.useGravity = false;
            heldRb.isKinematic = true;
        }

        obj.transform.SetParent(holdPoint);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
    }

    void DropObject()
    {
        if (heldObject != null)
        {
            heldObject.transform.SetParent(null);

            if (heldRb != null)
            {
                heldRb.useGravity = true;
                heldRb.isKinematic = false;
            }

            heldObject = null;
            heldRb = null;
        }
    }
}
