using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerkSystem : Singleton<PerkSystem>
{
    [SerializeField] private PerksUI perksUI;
    public readonly List<Perk> perks = new();
    public void AddPerk(Perk perk)
    {
        perks.Add(perk);
        UISystem.Instance.UpdatePerkUI(perk, true);
        perk.OnAdd();
    }
    public void RemovePerk(Perk perk)
    {
        perks.Remove(perk);
        UISystem.Instance.UpdatePerkUI(perk, false);
        perk.OnRemove();
    }
}
