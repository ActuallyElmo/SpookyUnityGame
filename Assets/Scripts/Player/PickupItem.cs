using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public bool isHeld = false;
    public string displayName = "Item";

    [Header("Hold Settings")]
    public Vector3 holdOffset;      // local position offset relative to the holdPoint
    public Vector3 holdRotation;    // local rotation applied when the item is held

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
