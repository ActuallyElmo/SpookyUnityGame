using UnityEngine;

public class PlayerHidingManager : MonoBehaviour
{
    public bool isCompletlyHidden;
    public bool isPartiallyHidden;

    void Start()
    {
        isPartiallyHidden = false;
        isCompletlyHidden = false;
    }

    public void SetHiddenStatus(bool isCompletlyHidden)
    {
        this.isCompletlyHidden = isCompletlyHidden;
    }

    public void SetPartiallyHiddenStatus(bool isPartiallyHidden)
    {
        this.isPartiallyHidden = isPartiallyHidden;
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "HiddenLocation")
        {
            SetHiddenStatus(true);
        }
        if(other.gameObject.tag == "PartiallyHiddenLocation")
        {
            SetPartiallyHiddenStatus(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.gameObject.tag == "HiddenLocation")
        {
            SetHiddenStatus(false);
        }
        if(other.gameObject.tag == "PartiallyHiddenLocation")
        {
            SetPartiallyHiddenStatus(false);
        }
    }
}
