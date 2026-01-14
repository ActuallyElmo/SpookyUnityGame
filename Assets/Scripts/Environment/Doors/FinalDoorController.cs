using System;
using UnityEngine;

public class FinalDoorController : MonoBehaviour, IInteractable
{
    public bool isKeyLocked = true;
    public bool isLockpadLocked = true;
    public bool isKeypadLocked = true;
    public int planksNumber = 2;

    [SerializeField] PickupItem masterKey;
    private const string noKeyMessage = "You need the master key for this";

    private void checkDoorUnlock()
    {
        bool lockedCondition = isKeyLocked || isLockpadLocked || isKeypadLocked || (planksNumber > 0);
        //lockedCondition = false;
        if (!lockedCondition)
        {
            Rigidbody rigidbody = GetComponent<Rigidbody>();
            rigidbody.isKinematic = false;
            rigidbody.useGravity = true;
            rigidbody.AddForce(0f, 0f, -5f, ForceMode.Impulse);
            PlayerSingleton.instance.GetComponent<PlayerDeathHandler>().OnWin();
        }
    }

    public void unlockKey()
    {
        isKeyLocked = false;
        checkDoorUnlock();
    }

    public void unlockLockpad()
    {
        Debug.Log("Unlocked lockpad###");
        isLockpadLocked = false;
        checkDoorUnlock();
    }

    public void unlockKeypad()
    {
        isKeypadLocked= false;
        checkDoorUnlock();
    }

    public void dropPlank()
    {
        if (planksNumber > 0)
        {
            planksNumber--;
        }
        checkDoorUnlock();
    }

    public void Interact()
    {
        checkDoorUnlock();
        if (!masterKey.isHeld)
        {
            StartCoroutine(IInteractable.DisplayLockedMessage(noKeyMessage));
            return;
        }

        unlockKey();
        gameObject.layer = 0;
    }
}
