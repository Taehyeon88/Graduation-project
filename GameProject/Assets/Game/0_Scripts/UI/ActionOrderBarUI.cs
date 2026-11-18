using System.Collections.Generic;
using UnityEngine;

//튜토리얼 전용: 이번 라운드에 아직 행동하지 않은 유닛을 속도 순으로 나열
public class ActionOrderBarUI : MonoBehaviour
{
    [SerializeField] private GameObject content;              //튜토리얼 중에만 켜는 내용 (루트는 TutorialTarget이 공개 여부를 관리)
    [SerializeField] private RectTransform slotParent;
    [SerializeField] private ActionOrderSlotUI slotTemplate;  //비활성 템플릿

    private readonly List<ActionOrderSlotUI> slots = new();

    private void LateUpdate()
    {
        bool tutorialOn = TutorialSystem.Instance != null && TutorialSystem.Instance.IsActive;
        if (content.activeSelf != tutorialOn)
            content.SetActive(tutorialOn);
        if (!tutorialOn) return;

        IReadOnlyList<CombatantView> queue = AutoBattleSystem.Instance != null ? AutoBattleSystem.Instance.BattleQueue : null;
        int count = 0;
        if (queue != null)
        {
            foreach (CombatantView unit in queue)
            {
                if (unit == null) continue;

                if (count >= slots.Count)
                    slots.Add(Instantiate(slotTemplate, slotParent));
                slots[count].gameObject.SetActive(true);
                slots[count].SetUp(unit, count == 0);
                count++;
            }
        }

        for (int i = count; i < slots.Count; i++)
            slots[i].gameObject.SetActive(false);
    }
}
