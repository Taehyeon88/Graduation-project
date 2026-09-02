using UnityEngine;
using UnityEngine.UI;

public class PerkUI : MonoBehaviour
{
    [SerializeField] private Image image;
    public PerkItem perk { get; private set; }
    public void SetUp(PerkItem perk)
    {
        this.perk = perk;
        image.sprite = perk.Image;
    }
}
