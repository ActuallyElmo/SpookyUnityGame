using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public bool isHeld = false;
    public string displayName = "Item";

    private Outline outline;

    private void Awake()
    {
        outline = GetComponent<Outline>();
        if (outline != null)
        {
            outline.enabled = true;   // always active in world
        }
    }

    private void Update()
    {
        if (outline != null)
        {
            outline.enabled = !isHeld;       // inactive when held
        }
    }
}
