using UnityEngine;
using System.Collections;

public class LockpadController : MonoBehaviour, IInteractable
{
    [SerializeField] GameObject neededKey;
    [SerializeField] GameObject lockedDoor;
    private PickupItem key;
    
    void Start()
    {
        key = neededKey.GetComponent<PickupItem>();
    }

    private IEnumerator DisplayLockedMessage()
    {
        const int displaySeconds = 2;

        HudController.Instance.EnableInteractionText("You need a key for this lockpad", "");
        yield return new WaitForSeconds(displaySeconds);
        HudController.Instance.DisableInteractionText();
    }

    public void Interact()
    {
        if (!key.isHeld)
        {
            StartCoroutine(DisplayLockedMessage());
            return;
        }

        gameObject.GetComponent<Rigidbody>().isKinematic = false;
        gameObject.GetComponent<Rigidbody>().useGravity = true;
        gameObject.layer = 0;
    }
}
