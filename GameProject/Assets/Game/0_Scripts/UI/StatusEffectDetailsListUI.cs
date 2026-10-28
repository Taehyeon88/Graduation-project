using System.Collections.Generic;
using UnityEngine;

public class StatusEffectDetailsListUI : MonoBehaviour
{
    [SerializeField] private StatusEffectDetailsUI detailsPrefab;

    private readonly List<StatusEffectDetailsUI> pool = new();

    //Publics
    public void Show(CombatantView view)
    {
        StatusEffectType[] types = view.GetStatusEffects();
        if (types.Length == 0)
        {
            Hide();
            return;
        }

        for (int i = 0; i < types.Length; i++)
        {
            if (i >= pool.Count) pool.Add(Instantiate(detailsPrefab, transform));

            StatusEffectData data = StatusEffectSystem.Instance.GetData(types[i]);
            pool[i].Set(data.spriteImage, data.name, view.GetStatusEffectStacks(types[i]), data.Discription);
            pool[i].gameObject.SetActive(true);
        }
        for (int i = types.Length; i < pool.Count; i++)
            pool[i].gameObject.SetActive(false);

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
