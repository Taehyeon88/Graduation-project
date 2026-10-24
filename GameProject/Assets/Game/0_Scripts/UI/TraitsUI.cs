using System.Collections.Generic;
using UnityEngine;

public class TraitsUI : MonoBehaviour
{
    [SerializeField] private TraitUI[] slots;

    //보유한 특성 수만큼 슬롯을 켜고 이미지를 할당, 나머지 슬롯은 끈다
    public void SetUp(IReadOnlyList<Trait> traits)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            bool has = i < traits.Count;
            slots[i].gameObject.SetActive(has);
            if (has) slots[i].SetUp(traits[i]);
        }
    }
}
