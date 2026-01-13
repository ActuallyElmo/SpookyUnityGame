using TMPro;
using UnityEngine;

public class HudController : MonoBehaviour
{
    [SerializeField] TMP_Text interactionText;
    [SerializeField] TMP_Text itemNameText;

    public static HudController Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        interactionText.gameObject.SetActive(false);
        itemNameText.gameObject.SetActive(false);
    }

    public void EnableInteractionText(string actionText, string itemName)
    {
        interactionText.text = actionText;
        itemNameText.text = itemName;

        interactionText.gameObject.SetActive(true);
        itemNameText.gameObject.SetActive(true);
    }

    public void DisableInteractionText()
    {
        interactionText.gameObject.SetActive(false);
        itemNameText.gameObject.SetActive(false);
    }
}