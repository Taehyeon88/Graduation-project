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
    }
}
