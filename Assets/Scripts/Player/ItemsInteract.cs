using UnityEngine;

public class ItemsInteract : MonoBehaviour
{
    [SerializeField] float pickUpRange = 3f;
    private const string objectLayer = "InteractObjects";

    private IInteractable foundObject = null;

    private PlayerInputHandler inputHandler;
    void Awake()
    {
        inputHandler = GetComponent<PlayerInputHandler>(); // Get input handler from the player
    }

    private void CheckRay()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));

        int layerMask = LayerMask.GetMask(objectLayer);

        if (Physics.Raycast(ray, out RaycastHit hit, pickUpRange, layerMask))
        {
            IInteractable interactableObject = hit.collider.GetComponent<IInteractable>();

            if(interactableObject != null)
            {
                foundObject = interactableObject;
                HudController.Instance.EnableInteractionText("(E)", "Interact");
            }
            
        }
        else {
            
            if(foundObject != null)
            {
                foundObject = null;
                HudController.Instance.DisableInteractionText();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        CheckRay();

        if (inputHandler != null && inputHandler.InteractTriggered && foundObject != null)
        {
            foundObject.Interact();
        }
    }
}
