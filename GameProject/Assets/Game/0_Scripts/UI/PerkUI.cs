using UnityEngine;
using UnityEngine.UI;

public class PerkUI : MonoBehaviour
{
    [SerializeField] private Image image;
    public Perk perk { get; private set; }
    public void SetUp(Perk perk)
    {
        this.perk = perk;
        image.sprite = perk.Image;
    }
}
