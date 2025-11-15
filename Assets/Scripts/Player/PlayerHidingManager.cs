using UnityEngine;

public class PlayerHidingManager : MonoBehaviour
{
    public bool isCompletlyHidden;     // True when player is in a full hiding spot
    public bool isPartiallyHidden;     // True when player is in a partial hiding spot

    void Start()
    {
        isPartiallyHidden = false;     // Initialize hiding states
        isCompletlyHidden = false;
    }

    public void SetHiddenStatus(bool isCompletlyHidden)
    {
        this.isCompletlyHidden = isCompletlyHidden;   // Update full hiding status
    }

    public void SetPartiallyHiddenStatus(bool isPartiallyHidden)
    {
        this.isPartiallyHidden = isPartiallyHidden;   // Update partial hiding status
    }

    void OnTriggerEnter(Collider other)
    {
        // Detect entry into hiding zones based on tags
        if (other.gameObject.tag == "HiddenLocation")
        {
            SetHiddenStatus(true);
        }
        if (other.gameObject.tag == "PartiallyHiddenLocation")
        {
            SetPartiallyHiddenStatus(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Reset hiding status when leaving the area
        if (other.gameObject.tag == "HiddenLocation")
        {
            SetHiddenStatus(false);
        }
        if (other.gameObject.tag == "PartiallyHiddenLocation")
        {
            SetPartiallyHiddenStatus(false);
        }
    }
}
