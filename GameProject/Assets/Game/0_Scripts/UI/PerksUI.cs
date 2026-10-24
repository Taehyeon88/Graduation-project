using System.Collections.Generic;
using UnityEngine;

public class PerksUI : MonoBehaviour
{
    [SerializeField] private PerkUI perkUIPrefab;

    public void SetUp(IReadOnlyList<Perk> perks)
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        foreach (Perk perk in perks)
        {
            PerkUI perkUI = Instantiate(perkUIPrefab, transform);
            perkUI.Set(perk);
        }
    }
}
