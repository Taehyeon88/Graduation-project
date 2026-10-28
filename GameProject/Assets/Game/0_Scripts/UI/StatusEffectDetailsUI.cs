using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusEffectDetailsUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text description;

    public void Set(Sprite sprite, string titleText, int stackCount, string descriptionText)
    {
        icon.sprite = sprite;
        title.text = $"{titleText} x{stackCount}";
        description.text = descriptionText;
    }
}
