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
    [SerializeField] private Button onSpdModeButton;  //이동 모드로 전환 버튼

    [Header("시각 데이터들(실시간 갱신형)")]
    [SerializeField] private TMP_Text heroHpText;
    [SerializeField] private TMP_Text goldAmountText;
    [SerializeField] private TMP_Text turnCountText;
    [SerializeField] private TMP_Text waveCountText;
    [SerializeField] private TMP_Text manaAmountText;
    [SerializeField] private TMP_Text drawPileCardAmountText;
    [SerializeField] private TMP_Text discardPileCardAmountText;
    [SerializeField] private TMP_Text deckCountText;
    [SerializeField] private TMP_Text spdAmountText;   //이동 포인트 텍스트
    [SerializeField] private Slider spdResourceSlider; //이동 자원 슬라이드 

    [Header("기타(실행형)")]
    [SerializeField] private GameObject feedBackPanel;
    [SerializeField] private Transform settingUI;
    [SerializeField] private CheckDeckUI checkDeckUI;

    private int heroHP => HeroSystem.Instance.HeroView.CurrentHealth;
    private int heroMaxHP => HeroSystem.Instance.HeroView.MaxHealth;
    //골드
    private int turnCount => WaveSystem.Instance.CurrentTurn;
    private int waveCount => WaveSystem.Instance.RemainWaveTurn;
    private int manaAmount => ManaSystem.Instance.CurrentMana;
    private int maxMana => ManaSystem.Instance.MaxMana;
    private int drawPileCardAmount => CardSystem.Instance.drawPileCA;
    private int discardPileCardAmount => CardSystem.Instance.discardPileCA;
    private int spdAmount => SPDSystem.Instance.currentSPD;
    private int maxspdResource => SPDSystem.Instance.maxResourceCount;
    private int currentspdResource => SPDSystem.Instance.currentResourceCount;

    private bool isSetting = false;
    private bool isCheckDeck = false;

    private void Update()
    {
        if (HeroSystem.Instance.HeroView != null)
        {
            heroHpText.SetText("{0}/{1}", heroHP, heroMaxHP);
        }
        //골드
        turnCountText.SetText("턴: {0}", turnCount);
        waveCountText.SetText("다음 웨이브: {0}턴", waveCount);
        manaAmountText.SetText("{0}/{1}", manaAmount, maxMana);
        drawPileCardAmountText.text = drawPileCardAmount.ToString();
        discardPileCardAmountText.text = discardPileCardAmount.ToString();
        deckCountText.text = GameSystem.Instance.Deck.Count.ToString();
        spdAmountText.SetText(spdAmount.ToString());
        spdResourceSlider.maxValue = maxspdResource;
        spdResourceSlider.value = currentspdResource;
    }

    private void OnEnable()
    {
        endTurnButton.onClick.AddListener(EndPlayerTurn);
        onSettingButton.onClick.AddListener(OnSettingUI);
        checkDeckButton.onClick.AddListener(OnCheckDeckUI);
        onSpdModeButton.onClick.AddListener(SwitchMoveMode);
    }
    private void OnDisable()
    {
        endTurnButton.onClick.RemoveListener(EndPlayerTurn);
        onSettingButton.onClick.RemoveListener(OnSettingUI);
        checkDeckButton.onClick.RemoveListener(OnCheckDeckUI);
        onSpdModeButton.onClick.RemoveListener(SwitchMoveMode);
    }

    //Button Event Methods
    private void EndPlayerTurn()
    {
        //플레이어 턴 종료 사운드 재생
        if(!ActionSystem.Instance.IsPerforming)
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

    private void SwitchMoveMode()
    {
        UISystem.Instance?.ToggleMoveMode();
    }
}
