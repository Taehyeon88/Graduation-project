using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDUI : MonoBehaviour
{
    [Header("버튼들(사용자 반응형)")]
    [SerializeField] private Button endTurnButton;
    [SerializeField] private Button onSettingButton;
    [SerializeField] private Button checkDeckButton;

    [Header("시각 데이터들(실시간 갱신형)")]
    [SerializeField] private TMP_Text heroHpText;
    [SerializeField] private TMP_Text goldAmountText;
    [SerializeField] private TMP_Text turnCountText;
    [SerializeField] private TMP_Text manaAmountText;
    [SerializeField] private TMP_Text drawPileCardAmountText;
    [SerializeField] private TMP_Text discardPileCardAmountText;
    [SerializeField] private TMP_Text deckCountText;

    [Header("기타(실행형)")]
    [SerializeField] private GameObject feedBackPanel;
    [SerializeField] private Transform settingUI;
    [SerializeField] private CheckDeckUI checkDeckUI;

    private int heroHP => HeroSystem.Instance.HeroView.CurrentHealth;
    private int heroMaxHP => HeroSystem.Instance.HeroView.MaxHealth;
    //골드
    //턴 횟수
    private int manaAmount => ManaSystem.Instance.CurrentMana;
    private int maxMana => ManaSystem.Instance.MaxMana;
    private int drawPileCardAmount => CardSystem.Instance.drawPileCA;
    private int discardPileCardAmount => CardSystem.Instance.discardPileCA;

    private bool isSetting = false;
    private bool isCheckDeck = false;

    private void Update()
    {
        if (HeroSystem.Instance.HeroView != null)
        {
            heroHpText.SetText("{0}/{1}", heroHP, heroMaxHP);
        }
        //골드
        //턴
        manaAmountText.SetText("{0}/{1}", manaAmount, maxMana);
        drawPileCardAmountText.text = drawPileCardAmount.ToString();
        discardPileCardAmountText.text = discardPileCardAmount.ToString();
        deckCountText.text = GameSystem.Instance.Deck.Count.ToString();
    }

    private void OnEnable()
    {
        endTurnButton.onClick.AddListener(EndPlayerTurn);
        onSettingButton.onClick.AddListener(OnSettingUI);
        checkDeckButton.onClick.AddListener(OnCheckDeckUI);
    }
    private void OnDisable()
    {
        endTurnButton.onClick.RemoveListener(EndPlayerTurn);
        onSettingButton.onClick.RemoveListener(OnSettingUI);
        checkDeckButton.onClick.RemoveListener(OnCheckDeckUI);
    }

    //Button Event Methods
    private void EndPlayerTurn()
    {
        //플레이어 턴 종료 사운드 재생
        SoundSystem.Instance.PlaySound(28);

        EnemysTurnGA enemyTurnGA = new();
        ActionSystem.Instance.Perform(enemyTurnGA);
    }
    private void OnSettingUI()
    {
        Debug.Log("설정 활성화");
        isSetting = !isSetting;
        settingUI.gameObject.SetActive(isSetting);
    }

    private void OnCheckDeckUI()
    {
        isCheckDeck = !isCheckDeck;
        checkDeckUI.SetCheckDeckUI(isCheckDeck);
    }
}
