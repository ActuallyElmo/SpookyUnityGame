using UnityEngine;
using System.Collections;

public interface IInteractable
{
    public void Interact();

    public static IEnumerator DisplayLockedMessage(string message)
    {
        const int displaySeconds = 2;

        HudController.Instance.EnableInteractionText(message, "");
        yield return new WaitForSeconds(displaySeconds);
        HudController.Instance.DisableInteractionText();
    }
}
