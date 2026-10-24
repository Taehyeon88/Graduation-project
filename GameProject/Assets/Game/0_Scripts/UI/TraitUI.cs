using UnityEngine;
using UnityEngine.UI;

public class TraitUI : MonoBehaviour
{
    [SerializeField] private Image image;
    public Trait trait { get; private set; }
    public void SetUp(Trait trait)
    {
        this.trait = trait;
        image.sprite = trait.Image;
        image.enabled = trait.Image != null;   //이미지가 없는 특성은 슬롯만 켜고 흰 사각형은 숨김
    }
}
