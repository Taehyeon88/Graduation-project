using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HUDUI : MonoBehaviour
{
    [Header("버튼들(사용자 반응형)")]
    [SerializeField] private Button endTurnButton;
    [SerializeField] private Button onSettingButton;
    [SerializeField] private Button checkDeckButton;
    [SerializeField] private Button endGameButton;

    [Header("시각 데이터들(실시간 갱신형)")]
    [SerializeField] private TMP_Text heroHpText;
    [SerializeField] private TMP_Text goldAmountText;
    [SerializeField] private TMP_Text turnCountText;
    [SerializeField] private TMP_Text waveCountText;
    [SerializeField] private TMP_Text manaAmountText;
    [SerializeField] private TMP_Text drawPileCardAmountText;
    [SerializeField] private TMP_Text discardPileCardAmountText;
    [SerializeField] private TMP_Text deckCountText;

    [Header("기타(실행형)")]
    [SerializeField] private GameObject feedBackPanel;
    [SerializeField] private SettingUI settingUI;
    [SerializeField] private CheckDeckUI checkDeckUI;

    private int heroHP => HeroSystem.Instance.HeroView == null? 
        GameSystem.Instance.CurrentHp : 
        HeroSystem.Instance.HeroView.CurrentHealth;
    private int heroMaxHP => HeroSystem.Instance.HeroView == null ?
        GameSystem.Instance.MaxHp :
        HeroSystem.Instance.HeroView.MaxHealth;
    //골드
    private int turnCount => WaveSystem.Instance.CurrentTurn;
    private int waveCount => WaveSystem.Instance.RemainWaveTurn;
    private int manaAmount => ManaSystem.Instance.CurrentMana;
    private int maxMana => ManaSystem.Instance.MaxMana;
    private int drawPileCardAmount => CardSystem.Instance.drawPileCA;
    private int discardPileCardAmount => CardSystem.Instance.discardPileCA;

    private bool isSetting = false;
    private bool isCheckDeck = false;


    private void Update()
    {
        heroHpText.SetText("{0}/{1}", heroHP, heroMaxHP);
        //골드
        turnCountText.SetText("턴: {0}", turnCount);
        waveCountText.SetText("다음 웨이브: {0}턴", waveCount);
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
        endGameButton.onClick.AddListener(EndGame);

        ActionSystem.SubscribeReaction<PlayCardTargetingGA>(PlayCardTargetingGAPreReaction, ReactionTiming.PRE);
        ActionSystem.SubscribeReaction<PlayCardTargetingGA>(PlayCardTargetingGAPostReaction, ReactionTiming.POST);
    }
    private void OnDisable()
    {
        endTurnButton.onClick.RemoveListener(EndPlayerTurn);
        onSettingButton.onClick.RemoveListener(OnSettingUI);
        checkDeckButton.onClick.RemoveListener(OnCheckDeckUI);
        endGameButton.onClick.RemoveListener(EndGame);

        ActionSystem.UnsubscribeReaction<PlayCardTargetingGA>(PlayCardTargetingGAPreReaction, ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<PlayCardTargetingGA>(PlayCardTargetingGAPostReaction, ReactionTiming.POST);
    }

    private void OnSettingUI()
    {
        Debug.Log("설정 활성화");
        
        isSetting = !settingUI.IsActive;
        endGameButton.gameObject.SetActive(isSetting);
        settingUI.SetActiveSettingUI(isSetting);
    }

    private void OnCheckDeckUI()
    {
        isCheckDeck = !isCheckDeck;
        checkDeckUI.SetCheckDeckUI(isCheckDeck);
    }

    private void EndGame()
    {
        SceneManager.LoadScene("StartScene");
    }

    //Button Event Methods
    PlayCardTargetingGA playCardTargetingGA = null;
    private void EndPlayerTurn()
    {
        if (playCardTargetingGA != null)
        {
            CardSystem.Instance.CancelTargetMode = true;  //타겟 모드 강제 취소

            //플레이어 턴 종료 사운드 재생
            if (!ActionSystem.Instance.IsPerforming)
                SoundSystem.Instance.PlaySound(28);

            EnemysTurnGA enemyTurnGA = new();
            playCardTargetingGA.PostReactions.Add((enemyTurnGA, null));
        }
        else
        {
            //플레이어 턴 종료 사운드 재생
            if (!ActionSystem.Instance.IsPerforming)
                SoundSystem.Instance.PlaySound(28);

            EnemysTurnGA enemyTurnGA = new();
            ActionSystem.Instance.Perform(enemyTurnGA);
        }
    }

    //SubScribers
    private void PlayCardTargetingGAPreReaction(PlayCardTargetingGA playCardTargetingGA)
    {
        this.playCardTargetingGA = playCardTargetingGA;
    }
    private void PlayCardTargetingGAPostReaction(PlayCardTargetingGA playCardTargetingGA)
    {
        this.playCardTargetingGA = null;
    }
}
