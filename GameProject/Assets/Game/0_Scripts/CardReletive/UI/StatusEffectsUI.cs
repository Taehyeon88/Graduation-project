using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectsUI : MonoBehaviour
{
    [SerializeField] private StatusEffectUI statusEffectPrefab;

    private Dictionary<StatusEffectType, StatusEffectUI> statusEffectUIs = new();

    private const int MaxCount = 4;
    public void UpdateStatusEffect(StatusEffectType statusEffectType, int stackCount, Sprite sprite = null)
    {
        if (stackCount == 0)
        {
            if (statusEffectUIs.ContainsKey(statusEffectType))
            {
                StatusEffectUI statusEffectUI = statusEffectUIs[statusEffectType];
                statusEffectUIs.Remove(statusEffectType);
                Destroy(statusEffectUI.gameObject);
            }
        }
        else
        {
            if (statusEffectUIs.Count >= MaxCount) return;   //최대 개수를 넘어갈 경우, 반환처리

            if (!statusEffectUIs.ContainsKey(statusEffectType))
            {
                StatusEffectUI statusEffectUI = Instantiate(statusEffectPrefab, transform);
                statusEffectUIs.Add(statusEffectType, statusEffectUI);
            }
            statusEffectUIs[statusEffectType].Set(sprite, stackCount);
        }
    }
}
