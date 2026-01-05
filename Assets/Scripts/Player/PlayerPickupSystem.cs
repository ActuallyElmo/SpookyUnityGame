using UnityEngine;

public class PlayerPickupSystem : MonoBehaviour
{
    public Transform holdPoint;                      // Point where the picked-up object will be held in front of the player
    public float pickUpRange = 3f;                   // Maximum distance the player can pick up an object
    private PickupItem currentHoverItem;

    private GameObject heldObject;                   // Reference to the currently held object
    private Rigidbody heldRb;                        // Rigidbody of the held object
    private PlayerInputHandler inputHandler;         // Handles player input for picking/dropping

    [Header("Throw Settings")]
    [SerializeField] private float throwForce;
    [SerializeField] private float upwardForce;

    void Awake()
    {
        inputHandler = GetComponent<PlayerInputHandler>(); // Get input handler from the player
    }

    void Update()
    {
        CheckHover();
        // Only run if input handler exists
        if (inputHandler != null)
        {
            // Check for interact button press (pick up)
            if (inputHandler.InteractTriggered)
            {
                // Only pick up if nothing is already held
                if (heldObject == null)
                    TryPickUpObject();
            }

            // Check for drop button press
            if (inputHandler.DropTriggered)
            {
                // Only drop if an object is currently held
                if (heldObject != null)
                    DropObject();
            }
        }
    }

    void CheckHover()
    {
        // Ray from the center of the screen (player view)
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));

        int layerMask = LayerMask.GetMask("Interactable");

        if (Physics.Raycast(ray, out RaycastHit hit, pickUpRange, layerMask))
        {
            PickupItem item = hit.collider.GetComponent<PickupItem>();

            // Valid interactable object under crosshair
            if (item != null && !item.isHeld)
            {
                if (currentHoverItem != item)
                {
                    currentHoverItem = item;
                    HudController.Instance.EnableInteractionText("(E)", item.displayName);
                }
                return;
            }
        }

        // No interactable object in view -> hide UI
        if (currentHoverItem != null)
        {
            currentHoverItem = null;
            HudController.Instance.DisableInteractionText();
        }
    }

    void TryPickUpObject()
    {
        // Cast a ray from the center of the screen (player's view)
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        Debug.DrawRay(ray.origin, ray.direction * pickUpRange, Color.red);

        int layerMask = LayerMask.GetMask("Interactable"); // Restrict raycast to interactable objects

        // Perform raycast
        if (Physics.Raycast(ray, out RaycastHit hit, pickUpRange, layerMask))
        {
            Debug.Log("Raycast hit: " + hit.collider.name);

            // Check if the hit object has a PickupItem script
            PickupItem interactObj = hit.collider.GetComponent<PickupItem>();
            if (interactObj != null && interactObj.isHeld == false)
            {
                PickUpObject(hit.collider.gameObject); // Pick up the object
            }
        }
        else
        {
            Debug.Log("Raycast didn't hit anything.");
        }
    }

    void PickUpObject(GameObject obj)
    {
        heldObject = obj;                                     // Store reference to held object
        heldRb = obj.GetComponent<Rigidbody>();               // Get its Rigidbody

        obj.GetComponent<PickupItem>().isHeld = true;         // Mark as held

        if (heldRb != null)
        {
            heldRb.isKinematic = true;                       // Disable physics while holding
        }

        // Disable object's colliders if needed:
        // (commented out but useful if object clips into environment)
        /*
        foreach(Collider col in obj.GetComponents<Collider>())
        {
            col.enabled = false;
        }
        */

        obj.transform.SetParent(holdPoint);                  // Attach it to the hold point
        obj.transform.localPosition = Vector3.zero;          // Reset position to hold point
        obj.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 90)); // Adjust rotation if needed
    }

    void DropObject()
    {
        if (heldObject != null)
        {
            heldObject.transform.SetParent(null); // Detach from player

            heldObject.GetComponent<PickupItem>().isHeld = false; // Mark as not held

            if (heldRb != null)
            {
                heldRb.isKinematic = false; // Re-enable physics

                // Apply throw force
                Vector3 throwDirection = Camera.main.transform.forward;
                heldRb.AddForce(
                    throwDirection * throwForce + Vector3.up * upwardForce,
                    ForceMode.Impulse
                );
            }

            // Re-enable colliders if you disabled them earlier
            /*
            foreach(Collider col in heldObject.GetComponents<Collider>())
            {
                col.enabled = true;
            }
            */

            heldObject = null;
            heldRb = null;
        }
    }
}