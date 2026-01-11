using UnityEngine;

public class LockpadController : MonoBehaviour, IInteractable
{
    [SerializeField] GameObject neededKey;
    [SerializeField] GameObject lockedDoor;
    private PickupItem key;

    private const string lockedMessage = "You need a key for this lockpad";


    void Start()
    {
        key = neededKey.GetComponent<PickupItem>();
    }

    

    public void Interact()
    {
        if (!key.isHeld)
        {
            StartCoroutine(IInteractable.DisplayLockedMessage(lockedMessage));
            return;
        }

        gameObject.GetComponent<Rigidbody>().isKinematic = false;
        gameObject.GetComponent<Rigidbody>().useGravity = true;
        gameObject.layer = 0;
    }
}
