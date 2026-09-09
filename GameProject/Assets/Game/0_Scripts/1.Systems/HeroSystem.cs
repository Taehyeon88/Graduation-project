using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroSystem : Singleton<HeroSystem>
{
    public Image selected_Hero_UI;
    public IReadOnlyList<HeroView> HeroViews => TokenSystem.Instance.HeroViews;
    public HeroView CurrentHero
    {
        get { return currentHero; }
        set
        {
            if (value == null 
                || currentHero == value) return;

            currentHero = value;

            if (selected_Hero_UI != null)
            {
                if (!selected_Hero_UI.gameObject.activeSelf)
                    selected_Hero_UI.gameObject.SetActive(true);
                selected_Hero_UI.sprite = (currentHero.TokenData as HeroData).simbolIcon; //선택된 영웅 이미지 변경
            }
            SkillSystem.Instance.UpdateSkillsUI(currentHero);                             //스킬 UI 업데이트 + 재정렬
        }
    }

    private HeroView currentHero;

    private void OnEnable()
    {
        ActionSystem.SubscribeReaction<TurnGA>(HeroTurnPreReaction, ReactionTiming.PRE);
        ActionSystem.SubscribeReaction<TurnGA>(EnemyTurnPreReaction, ReactionTiming.PRE);
    }
    private void OnDisable()
    {
        ActionSystem.UnsubscribeReaction<TurnGA>(HeroTurnPreReaction, ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<TurnGA>(EnemyTurnPreReaction, ReactionTiming.PRE);
    }

    //Reactions
    private void HeroTurnPreReaction(TurnGA turnGA)
    {
        if (turnGA.Type != TurnType.Player) return;

        //플레이어턴 시작 연출

        Debug.Log("플레이어 턴 시작");

        foreach (var hero in HeroViews)
        {
            hero.ResetMovePoint();   //각 영웅 이동 포인트 초기화

            //플레이어의 방어막 스택 삭제
            int armorStack = hero.GetStatusEffectStacks(StatusEffectType.ARMOR);
            if (armorStack > 0) hero.RemoveStatusEffect(StatusEffectType.ARMOR, armorStack);

            //상태 효과
            //악화
            float specialRate = 1;
            bool deteriaorateExist = hero.CheckStatusEffectExist(StatusEffectType.DETERIORATE);
            if (deteriaorateExist)
            {
                bool tdSEExist = hero.CheckStatusEffectExist(StatusEffectType.POISIONING)
                              || hero.CheckStatusEffectExist(StatusEffectType.BLEEDING);

                if (!tdSEExist)
                    hero.RemoveStatusEffect(StatusEffectType.DETERIORATE, 0);
            }
        }

        //마나 회복
        RefillManaGA refillManaGA = new();
        ActionSystem.Instance.AddReaction(refillManaGA);

        //스킬 사용 횟수 초기화
        RefillSkillLimitGA refillSkillLimitGA = new();
        ActionSystem.Instance.AddReaction(refillSkillLimitGA);
    }

    private void EnemyTurnPreReaction(TurnGA turnGA)
    {
        if (turnGA.Type != TurnType.Enemy) return;

        Debug.Log("플레이어 턴 종료");

        foreach (var hero in HeroViews)
        {
            //플레이어 상태효과 N감소
            foreach (var statusEffectType in hero.GetStatusEffects())
            {
                //기간제 및 조건제만 실행
                var mcType = StatusEffectSystem.Instance.GetMachanicsType(statusEffectType);
                if (mcType == SEMachanicsType.FixedTerm || mcType == SEMachanicsType.ConditionTerm)
                {
                    hero.RemoveStatusEffect(statusEffectType, 1);
                }
            }
        }
    }
}
