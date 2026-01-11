using UnityEngine;

public class PlankController : MonoBehaviour, IInteractable
{
    [SerializeField] GameObject crowbarObject;
    private PickupItem crowbarItem;

    private string noCrowbarMessage = "You need a crowbar for this!";

    void Start()
    {
        crowbarItem = crowbarObject.GetComponent<PickupItem>();
    }
    public void Interact()
    {
        if (!crowbarItem.isHeld)
        {
            StartCoroutine(IInteractable.DisplayLockedMessage(noCrowbarMessage));
            return;
        }

        Rigidbody rigidBody =  gameObject.GetComponent<Rigidbody>();
        rigidBody.isKinematic = false;
        rigidBody.useGravity = true;
        gameObject.layer = 0;
    }
}
