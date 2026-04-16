using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CheatSystem : Singleton<CheatSystem>
{
    //매턴 받는 코스트 99 | 몬스터 & 플레이어 체력 1000으로 변경
    //다음 드로우할 카드를 미리 정해 놓고 정해진 대로 뽑기
    [SerializeField] private RectTransform cheatUITrans;
    [SerializeField] private TMP_Text cardCheatModeText;
    [SerializeField] private List<CardData> Attack_Adjacent_CardDeck;
    [SerializeField] private List<CardData> Attack_Projection_CardDeck;
    [SerializeField] private List<CardData> Skill_Sheild_CardDeck;
    [SerializeField] private List<CardData> Skill_Area_CardDeck;
    [SerializeField] private List<CardData> Skill_Debuff_CardDeck;
    [SerializeField] private List<CardData> Skill_Move_CardDeck;

    private string[] cardsetCheatMode = 
    { 
        "기본",
        "패링&무기교체",
        "톱날 무기",
        "악화", 
        "스킬_영역",
        "스킬_디버프",
        "스킬_이동",
    };
    private int cardsetIndex;

    private void Start()
    {
        cheatUITrans.gameObject.SetActive(false);
    }

    public void StartCheat()
    {
        cheatUITrans.gameObject.SetActive(true);

        //매턴 받는 코스트 99로 변경
        ManaSystem.Instance.Cheat_ChangeMaxMana(99);

        RefillManaGA refillManaGA = new();
        if (ActionSystem.Instance.IsPerforming) ActionSystem.Instance.AddReaction(refillManaGA);
        else ActionSystem.Instance.Perform(refillManaGA);

        //몬스터 & 플레이어 체력 200으로 변경
        var tokens = TokenSystem.Instance.GetAllTokens();
        foreach (var token in tokens)
        {
            var target = token as CombatantView;
            if (target != null)
                target.Cheat_GetHealth(200);
        }

        cardCheatModeText.text = "Defualt";

        InteractionSystem.Instance.SetCheatInteraction(ChangeCheatMode);

        //다음 드로우할 카드를 미리 정해 놓고 정해진 대로 뽑기
    }

    private void ChangeCheatMode(bool isChanging)
    {
        if (isChanging)
        {
            int curIndex = cardsetIndex;
            cardsetIndex = cardsetIndex >= cardsetCheatMode.Length - 1 ? 0 : cardsetIndex + 1;
            string cheatName = cardsetCheatMode[cardsetIndex];

            cardCheatModeText.text = cheatName;

            var deck = cheatName switch
            {
                "패링&무기교체" => Attack_Adjacent_CardDeck,
                "톱날 무기" => Attack_Projection_CardDeck,
                "악화" => Skill_Sheild_CardDeck,
                "스킬_영역" => Skill_Area_CardDeck,
                "스킬_디버프" => Skill_Debuff_CardDeck,
                "스킬_이동" => Skill_Move_CardDeck,
                _=> null
            };

            if (!CardSystem.Instance.Cheat_ChangeCards(deck))
            {
                cardsetIndex = curIndex;
                cardCheatModeText.text = cardsetCheatMode[cardsetIndex];
            }
        }
    }
}
