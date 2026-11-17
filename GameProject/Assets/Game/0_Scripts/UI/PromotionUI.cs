using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//스테이지 선택 씬의 영웅 승급 화면: 보유 영웅 목록 → 영웅 상세(능력치·특성 칸) → 승급 2택1 → 되돌릴 수 없음 확인
//반복 요소(영웅 목록·선택지)는 비활성 템플릿을 복제해 채운다 (규칙 판단·실행은 PromotionSystem)
public class PromotionUI : MonoBehaviour
{
    [SerializeField] private Button openButton;   //화면 열기 (항상 표시)
    [SerializeField] private GameObject panel;
    [SerializeField] private Button closeButton;
    [SerializeField] private TMP_Text insigniaText;

    [Header("보유 영웅 목록")]
    [SerializeField] private RectTransform heroListContent;
    [SerializeField] private Button heroButtonTemplate;   //자식 TMP_Text에 이름·승급 단계
    [SerializeField] private Color heroNormalColor = new(0.25f, 0.25f, 0.3f, 1f);
    [SerializeField] private Color heroSelectedColor = new(0.45f, 0.4f, 0.2f, 1f);

    [Header("영웅 상세")]
    [SerializeField] private TMP_Text heroNameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private TMP_Text[] traitSlotTexts;   //특성 칸 순서 (칸 1, 칸 2)
    [SerializeField] private Button promoteButton;
    [SerializeField] private TMP_Text promoteButtonText;
    [SerializeField] private TMP_Text promoteStateText;   //승급할 수 없는 이유

    [Header("승급 선택 (2택1)")]
    [SerializeField] private GameObject choicePanel;
    [SerializeField] private RectTransform optionContent;
    [SerializeField] private Button optionTemplate;       //자식 TMP_Text에 선택지 설명
    [SerializeField] private Button choiceCancelButton;

    [Header("확인")]
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private TMP_Text confirmText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button confirmCancelButton;

    public bool IsOpen => panel.activeSelf;

    private readonly List<Button> heroButtons = new();     //복제한 영웅 버튼 (리스너는 ClearSpawned에서 해제)
    private readonly List<Button> optionButtons = new();   //복제한 선택지 버튼 (〃)
    private HeroData selectedHero;
    private PromotionOption pendingOption;

    private PromotionSystem Promotion => PromotionSystem.Instance;
    private GameSystem Game => GameSystem.Instance;

    private void Awake()
    {
        panel.SetActive(false);
        choicePanel.SetActive(false);
        confirmPanel.SetActive(false);
        heroButtonTemplate.gameObject.SetActive(false);
        optionTemplate.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        openButton.onClick.AddListener(Open);
        closeButton.onClick.AddListener(Close);
        promoteButton.onClick.AddListener(OpenChoice);
        choiceCancelButton.onClick.AddListener(CloseChoice);
        confirmButton.onClick.AddListener(ConfirmPromote);
        confirmCancelButton.onClick.AddListener(CloseConfirm);
    }

    private void OnDisable()
    {
        openButton.onClick.RemoveListener(Open);
        closeButton.onClick.RemoveListener(Close);
        promoteButton.onClick.RemoveListener(OpenChoice);
        choiceCancelButton.onClick.RemoveListener(CloseChoice);
        confirmButton.onClick.RemoveListener(ConfirmPromote);
        confirmCancelButton.onClick.RemoveListener(CloseConfirm);
        if (IsOpen) Promotion.Promoted -= OnPromoted;
    }

    //Publics
    public void Open()
    {
        if (IsOpen) return;

        Promotion.Promoted += OnPromoted;
        selectedHero = Promotion.GetOwnedHeroes().FirstOrDefault();
        panel.SetActive(true);
        Refresh();
    }

    public void Close()
    {
        if (!IsOpen) return;

        Promotion.Promoted -= OnPromoted;
        CloseConfirm();
        CloseChoice();
        ClearSpawned(heroButtons);
        panel.SetActive(false);
    }

    //Privates
    private void Refresh()
    {
        insigniaText.SetText($"보유 휘장 {Game.Insignias}");
        RefreshHeroList();
        RefreshDetail();
    }

    private void RefreshHeroList()
    {
        ClearSpawned(heroButtons);
        foreach (HeroData heroData in Promotion.GetOwnedHeroes())
        {
            Button button = Spawn(heroButtonTemplate, heroListContent, heroButtons);
            button.GetComponentInChildren<TMP_Text>(true).SetText($"{heroData.Name}  승급 {Promotion.GetLevel(heroData)}/{PromotionSystem.MaxLevel}");
            button.image.color = heroData == selectedHero ? heroSelectedColor : heroNormalColor;
            button.onClick.AddListener(() => SelectHero(heroData));
        }
    }

    private void SelectHero(HeroData heroData)
    {
        selectedHero = heroData;
        RefreshHeroList();
        RefreshDetail();
    }

    private void RefreshDetail()
    {
        Hero hero = selectedHero != null ? Game.Deck.FirstOrDefault(h => h.Data == selectedHero) : null;
        if (hero == null)
        {
            heroNameText.SetText("보유 영웅 없음");
            levelText.SetText("");
            statsText.SetText("");
            foreach (TMP_Text slotText in traitSlotTexts) slotText.SetText("");
            promoteButton.interactable = false;
            promoteButtonText.SetText("승급");
            promoteStateText.SetText("");
            return;
        }

        HeroPromotion promotion = Game.GetPromotion(selectedHero);
        heroNameText.SetText(selectedHero.Name);
        levelText.SetText($"승급 {promotion.Level} / {PromotionSystem.MaxLevel}");
        statsText.SetText($"체력 {hero.MaxHealth} (승급 +{promotion.HealthBonus})   공격력 {hero.Damage}   속도 {hero.Speed}   AP {selectedHero.APCost}");

        for (int i = 0; i < traitSlotTexts.Length; i++)
        {
            TraitData trait = i < CombatantView.MaxTraitCount ? promotion.GetSlotTrait(i) : null;
            traitSlotTexts[i].SetText(trait != null
                ? $"칸 {i + 1}: {trait.name}{(promotion.IsEnhanced(i) ? " (강화)" : "")}\n<size=80%>{trait.Description}</size>"
                : $"칸 {i + 1}: 빈 칸");
        }

        bool isMax = Promotion.IsMaxLevel(selectedHero);
        int cost = Promotion.GetCost(selectedHero);
        promoteButton.interactable = Promotion.CanPromote(selectedHero);
        promoteButtonText.SetText(isMax ? "최대 단계" : $"승급 (휘장 {cost})");
        promoteStateText.SetText(GetBlockedReason(isMax, cost));
    }

    private string GetBlockedReason(bool isMax, int cost)
    {
        if (isMax) return "더 이상 승급할 수 없습니다";
        if (Promotion.GetOptions(selectedHero).Count == 0) return "고를 수 있는 선택지가 없습니다";
        if (Game.Insignias < cost) return $"휘장이 부족합니다 (필요 {cost})";
        return "";
    }

    private void OpenChoice()
    {
        if (!Promotion.CanPromote(selectedHero)) return;

        ClearSpawned(optionButtons);
        foreach (PromotionOption option in Promotion.GetOptions(selectedHero))
        {
            Button button = Spawn(optionTemplate, optionContent, optionButtons);
            button.GetComponentInChildren<TMP_Text>(true).SetText(DescribeOption(option));
            button.onClick.AddListener(() => OpenConfirm(option));
        }
        choicePanel.SetActive(true);
    }

    private void CloseChoice()
    {
        ClearSpawned(optionButtons);
        choicePanel.SetActive(false);
    }

    private void OpenConfirm(PromotionOption option)
    {
        pendingOption = option;
        confirmText.SetText($"{selectedHero.Name} 승급 {Promotion.GetLevel(selectedHero) + 1}단계\n{GetOptionTitle(option)}\n\n휘장 {Promotion.GetCost(selectedHero)}개를 사용합니다.\n되돌릴 수 없습니다. 승급할까요?");
        confirmPanel.SetActive(true);
    }

    private void CloseConfirm()
    {
        pendingOption = null;
        confirmPanel.SetActive(false);
    }

    private void ConfirmPromote()
    {
        PromotionOption option = pendingOption;
        CloseConfirm();
        CloseChoice();
        if (option != null) Promotion.Promote(selectedHero, option);   //성공하면 Promoted로 갱신
    }

    private void OnPromoted(HeroData heroData)
    {
        Refresh();
    }

    private static string GetOptionTitle(PromotionOption option)
        => option.IsEnhance ? $"[특성 강화] {option.Before.name} → {option.After.name}" : $"[새 특성] {option.After.name}";

    private static string DescribeOption(PromotionOption option)
        => $"{GetOptionTitle(option)}\n\n<size=85%>{option.After.Description}</size>";

    private Button Spawn(Button template, RectTransform parent, List<Button> spawned)
    {
        Button instance = Instantiate(template, parent);
        instance.gameObject.SetActive(true);
        spawned.Add(instance);
        return instance;
    }

    //Spawn 후 붙인 onClick 리스너를 해제하고 제거 (등록·해제 대칭)
    private static void ClearSpawned(List<Button> spawned)
    {
        foreach (Button button in spawned)
        {
            button.onClick.RemoveAllListeners();
            Destroy(button.gameObject);
        }
        spawned.Clear();
    }
}
