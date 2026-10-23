using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusEffectUI : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TMP_Text stackCountText;

    public void Set(Sprite sprite, int stackCount)
    {
        if (sprite != null) image.sprite = sprite;   //스택 감소 시에는 sprite가 null로 와서 기존 아이콘 유지
        stackCountText.text = stackCount.ToString();
    }
}
