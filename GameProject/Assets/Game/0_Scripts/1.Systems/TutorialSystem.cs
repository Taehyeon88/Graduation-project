using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialSystem : Singleton<TutorialSystem>   //GAMEMASTER 하위에서 씬을 넘나듦
{
    private const string StartSceneName = "StartScene";   //시작 화면 씬 (SettingUI와 같은 이름)

    [SerializeField] private TutorialSequenceData sequence;

    public static event Action<TutorialStepData> StepStarted;           //단계 시작 (안내 상자·스포트라이트 표시용)
    public static event Action<TutorialStepData> StepCompleted;
    public static event Action<TutorialStepData> WrongActionPerformed;  //조건과 다른 행동을 취소했을 때 (고스트 시연 재생용)
    public static event Action TutorialEnded;

    public bool IsActive { get; private set; }
    public TutorialStepData CurrentStep { get; private set; }
    public IReadOnlyList<Token> ContextTokens => contextTokens;   //현재 단계를 시작시킨 유닛들 ("@context" 대상)

    private readonly HashSet<string> revealedIds = new();
    private readonly List<Token> contextTokens = new();
    private int stepIndex = -1;
    private bool isWaitingTrigger;   //다음 단계가 시작 시점을 기다리는 중
    private bool isFlowPaused;       //단계가 전투 진행을 멈춰 둔 상태
    private bool isSettingOpen;      //설정창이 열려 멈춘 상태
    private bool isGranting;         //튜토리얼이 카드를 지급하는 중 (카드 뽑기 버튼 입력과 구분)
    private Button clickButton;      //UIClick 단계에서 클릭을 감시 중인 버튼
    private bool startOnSceneEntered;   //개발용: 다음 씬 진입 때 대기 중인 단계를 트리거와 무관하게 시작

    private void OnEnable()
    {
        if (Instance != this) return;

        GrowthNodeSystem.Purchased += OnNodePurchased;
        PromotionSystem.PromotionCompleted += OnPromotionCompleted;
        SceneManager.sceneLoaded += OnSceneLoaded;
        ActionSystem.SubscribeReaction<TurnGA>(TurnGAPreReaction, ReactionTiming.PRE);
        ActionSystem.SubscribeReaction<TurnGA>(TurnGAPostReaction, ReactionTiming.POST);
        ActionSystem.SubscribeReaction<AutoBattleTurnGA>(AutoBattleTurnGAPreReaction, ReactionTiming.PRE);
        ActionSystem.SubscribeReaction<AutoBattleGA>(AutoBattleGAPreReaction, ReactionTiming.PRE);
        ActionSystem.SubscribeReaction<MoveGA>(MoveGAPreReaction, ReactionTiming.PRE);
        ActionSystem.SubscribeReaction<DealDamageGA>(DealDamageGAPreReaction, ReactionTiming.PRE);
        ActionSystem.SubscribeReaction<DealDamageGA>(DealDamageGAPostReaction, ReactionTiming.POST);
        ActionSystem.SubscribeReaction<AttackCoreGA>(AttackCoreGAPreReaction, ReactionTiming.PRE);
        ActionSystem.SubscribeReaction<PlayCardGA>(PlayCardGAPostReaction, ReactionTiming.POST);
        ActionSystem.SubscribeReaction<DrawCardsGA>(DrawCardsGAPostReaction, ReactionTiming.POST);
        ActionSystem.SubscribeReaction<GameClearGA>(GameClearGAPostReaction, ReactionTiming.POST);
        ActionSystem.SubscribeReaction<AddStatusEffectGA>(AddStatusEffectGAPostReaction, ReactionTiming.POST);
    }

    private void OnDisable()
    {
        if (Instance != this) return;

        GrowthNodeSystem.Purchased -= OnNodePurchased;
        PromotionSystem.PromotionCompleted -= OnPromotionCompleted;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnbindClick();
        ActionSystem.UnsubscribeReaction<TurnGA>(TurnGAPreReaction, ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<TurnGA>(TurnGAPostReaction, ReactionTiming.POST);
        ActionSystem.UnsubscribeReaction<AutoBattleTurnGA>(AutoBattleTurnGAPreReaction, ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<AutoBattleGA>(AutoBattleGAPreReaction, ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<MoveGA>(MoveGAPreReaction, ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<DealDamageGA>(DealDamageGAPreReaction, ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<DealDamageGA>(DealDamageGAPostReaction, ReactionTiming.POST);
        ActionSystem.UnsubscribeReaction<AttackCoreGA>(AttackCoreGAPreReaction, ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<PlayCardGA>(PlayCardGAPostReaction, ReactionTiming.POST);
        ActionSystem.UnsubscribeReaction<DrawCardsGA>(DrawCardsGAPostReaction, ReactionTiming.POST);
        ActionSystem.UnsubscribeReaction<GameClearGA>(GameClearGAPostReaction, ReactionTiming.POST);
        ActionSystem.UnsubscribeReaction<AddStatusEffectGA>(AddStatusEffectGAPostReaction, ReactionTiming.POST);
    }

    private void Update()
    {
        if (Instance != this || !IsActive || CurrentStep == null) return;

        //대상이 아직 생기지 않았을 수 있어(패널이 열리는 중 등) 매 프레임 확인
        if (CurrentStep.Advance == TutorialAdvance.UIClick)
        {
            BindClick();
        }
        else if (CurrentStep.Advance == TutorialAdvance.TargetAppears
            && TutorialTarget.TryGet(CurrentStep.AdvanceId, out TutorialTarget target)
            && target.gameObject.activeInHierarchy)
        {
            CompleteStep();
        }
    }

    //Statics (튜토리얼이 없거나 진행 중이 아니면 모두 허용)
    public static bool IsGuided => Instance != null && Instance.IsActive
        && (Instance.CurrentStep == null || Instance.CurrentStep.LockInput);   //단계 사이에 비는 순간도 잠금 유지

    public static bool IsRevealed(string id) => Instance == null || !Instance.IsActive || Instance.revealedIds.Contains(id);

    public static bool AllowsCard(Card card)
    {
        if (!IsGuided) return true;

        TutorialStepData step = Instance.CurrentStep;
        return step != null && step.Advance == TutorialAdvance.PlaceCard && card.Unit.Data == step.CardHero;
    }

    public static bool AllowsPlacement(Card card, Vector2Int pos)
    {
        if (!IsGuided) return true;
        return AllowsCard(card) && pos == Instance.CurrentStep.CellPos;
    }

    public static bool AllowsTurnEnd() => !IsGuided || (Instance.CurrentStep != null && Instance.CurrentStep.Advance == TutorialAdvance.TurnEnd);

    public static bool AllowsDraw() => !IsGuided || (Instance.CurrentStep != null && Instance.CurrentStep.Advance == TutorialAdvance.DrawCard);

    //스테이지 선택 씬: 지금이 "스테이지를 클릭하는" 단계인가 (이 단계에서만 월드 입력을 일부 허용)
    public static bool IsStageClickStep => IsGuided && Instance.CurrentStep != null && Instance.CurrentStep.Advance == TutorialAdvance.StageClick;

    //스테이지 번호의 월드 입력(호버·클릭) 허용 여부: 안내 중이 아니면 허용, 안내 중에는 StageClick 단계의 목표 스테이지만
    public static bool AllowsStageInput(int stageNumber)
        => !IsGuided || (IsStageClickStep && Instance.CurrentStep.AdvanceId == stageNumber.ToString());

    //스테이지 정보 팝업이 열렸음을 알림 — StageClick 단계의 목표 스테이지면 단계 완료
    public static void NotifyStagePopupOpened(int stageNumber)
    {
        if (Instance != null && Instance.IsActive && IsStageClickStep && Instance.CurrentStep.AdvanceId == stageNumber.ToString())
            Instance.CompleteStep();
    }

    //target이 null이면 "지금 단도를 쓸 수 있는 단계인가"만 판정 (드래그 시작용)
    public static bool AllowsDagger(Token target)
    {
        if (!IsGuided) return true;

        TutorialStepData step = Instance.CurrentStep;
        if (step == null || step.Advance != TutorialAdvance.UseDagger) return false;
        return target == null || TokenSystem.Instance.API.GetTokenPosition(target) == step.CellPos;
    }

    //조건과 다른 행동을 취소했음을 알림 (고스트 시연 다시 재생)
    public static void NotifyWrongAction()
    {
        if (IsGuided && Instance.CurrentStep != null)
            WrongActionPerformed?.Invoke(Instance.CurrentStep);
    }

    //새 게임·메인 메뉴 복귀로 진행 데이터가 초기화될 때 튜토리얼도 처음 상태로
    public static void ResetProgress()
    {
        if (Instance != null && Instance.IsActive)
            Instance.End();
    }

#if UNITY_EDITOR
    //개발용(스테이지 런처): 전투 파트를 건너뛰고 전투 뒤 스테이지 선택 씬 진입 단계(첫 SceneEntered)부터 이어서 시작
    public static bool DebugResumeAfterBattle()
    {
        if (Instance == null || Instance.sequence == null || Instance.sequence.Steps == null) return false;

        TutorialStepData[] steps = Instance.sequence.Steps;
        for (int i = 0; i < steps.Length; i++)
        {
            if (steps[i].Trigger != TutorialTrigger.SceneEntered) continue;

            Instance.UnbindClick();
            Instance.IsActive = true;
            Instance.CurrentStep = null;
            Instance.revealedIds.Clear();
            Instance.contextTokens.Clear();
            Instance.isFlowPaused = false;
            Instance.isSettingOpen = false;
            Instance.stepIndex = i;
            Instance.isWaitingTrigger = true;
            TutorialTarget.RefreshAll();
            return true;
        }
        return false;
    }

    //개발용(스테이지 런처): 지정한 ID의 단계부터 이어서 시작 (다음 씬 진입 때 시작). 앞 단계들이 공개한 UI는 이미 공개된 상태로 맞춤
    public static bool DebugResumeAtStep(string stepId)
    {
        if (Instance == null || Instance.sequence == null || Instance.sequence.Steps == null) return false;

        TutorialStepData[] steps = Instance.sequence.Steps;
        int index = Array.FindIndex(steps, s => s.Id == stepId);
        if (index < 0) return false;

        Instance.UnbindClick();
        Instance.IsActive = true;
        Instance.CurrentStep = null;
        Instance.revealedIds.Clear();
        Instance.contextTokens.Clear();
        for (int i = 0; i < index; i++)
        {
            if (steps[i].RevealIds != null)
                foreach (string id in steps[i].RevealIds) Instance.revealedIds.Add(id);
            if (steps[i].HideIdsOnComplete != null)
                foreach (string id in steps[i].HideIdsOnComplete) Instance.revealedIds.Remove(id);
        }
        Instance.isFlowPaused = false;
        Instance.isSettingOpen = false;
        Instance.stepIndex = index;
        Instance.isWaitingTrigger = true;
        Instance.startOnSceneEntered = true;
        TutorialTarget.RefreshAll();
        return true;
    }
#endif

    //화면 씬(스테이지 선택 등)에 진입했음을 알림 — 씬 오브젝트가 모두 준비된 뒤 호출
    public static void NotifySceneEntered(TutorialScreen screen)
    {
        if (Instance == null || !Instance.IsActive || !Instance.isWaitingTrigger) return;

        TutorialStepData next = Instance.sequence.Steps[Instance.stepIndex];
        bool resume = Instance.startOnSceneEntered;   //개발용: 중간 단계부터 이어서 시작할 때는 씬에 들어오는 즉시 그 단계를 시작
        if (resume || (next.Trigger == TutorialTrigger.SceneEntered && next.Screen == screen))
        {
            Instance.startOnSceneEntered = false;
            Instance.StartStep(next);
        }
    }

    public static void NotifySettingOpen(bool open)
    {
        if (Instance != null && Instance.IsActive)
            Instance.SetSettingOpen(open);
    }

    public static void NotifySameSpeedTie(List<CombatantView> group)
    {
        if (Instance != null)
            Instance.Fire(TutorialTrigger.SameSpeedTie, group.ToArray());
    }

    //Publics
    //안내 상자의 "다음" 또는 화면 클릭에서 호출
    public void Next()
    {
        if (isSettingOpen || CurrentStep == null || CurrentStep.Advance != TutorialAdvance.NextButton) return;
        CompleteStep();
    }

    //현재 단계의 스포트라이트 대상 Transform 수집 (등록되지 않은 ID는 건너뜀)
    public void GetTargetTransforms(List<Transform> results)
    {
        results.Clear();
        if (CurrentStep == null || CurrentStep.TargetIds == null) return;

        foreach (string targetId in CurrentStep.TargetIds)
        {
            if (targetId == "@context")
            {
                foreach (var token in contextTokens)
                    if (token != null) results.Add(token.transform);
            }
            else if (TutorialTarget.TryGet(targetId, out var target))
            {
                results.Add(target.transform);
            }
        }
    }

    //Privates
    private void Begin()
    {
        if (sequence == null || sequence.Steps == null || sequence.Steps.Length == 0) return;

        IsActive = true;
        CurrentStep = null;
        revealedIds.Clear();
        contextTokens.Clear();
        stepIndex = -1;
        isFlowPaused = false;
        isSettingOpen = false;
        isGranting = false;
        TutorialTarget.RefreshAll();   //공개 전 UI 숨김

        PrepareNextStep();
        Fire(TutorialTrigger.BattleStart);
    }

    private void End()
    {
        UnbindClick();
        startOnSceneEntered = false;
        IsActive = false;
        CurrentStep = null;
        isWaitingTrigger = false;
        isFlowPaused = false;
        isSettingOpen = false;
        ApplyPause();
        TutorialTarget.RefreshAll();   //숨겼던 UI 모두 공개
        TutorialEnded?.Invoke();
    }

    private void PrepareNextStep()
    {
        stepIndex++;
        if (stepIndex >= sequence.Steps.Length)
        {
            End();
            return;
        }

        if (sequence.Steps[stepIndex].Trigger == TutorialTrigger.Immediate)
            StartStep(sequence.Steps[stepIndex]);
        else
            isWaitingTrigger = true;
    }

    private void Fire(TutorialTrigger trigger, params Token[] context)
    {
        if (!IsActive || !isWaitingTrigger || sequence.Steps[stepIndex].Trigger != trigger) return;
        StartStep(sequence.Steps[stepIndex], context);
    }

    private void StartStep(TutorialStepData step, params Token[] context)
    {
        isWaitingTrigger = false;
        CurrentStep = step;
        contextTokens.Clear();
        foreach (var token in context)
            if (token != null) contextTokens.Add(token);

        if (step.Trigger != TutorialTrigger.Immediate && step.Trigger != TutorialTrigger.PlayerTurnStart && step.Trigger != TutorialTrigger.SceneEntered)
            isFlowPaused = true;   //자동 전투 안의 시점은 안내가 끝날 때까지 멈춤
        if (step.RevealIds != null)
        {
            foreach (string id in step.RevealIds)
                revealedIds.Add(id);
        }
        if (step.ResumeOnStart)
            isFlowPaused = false;

        ApplyPause();
        TutorialTarget.RefreshAll();
        if (step.GrantCardOnStart)
            StartCoroutine(GrantCardRoutine());

        StepStarted?.Invoke(step);
    }

    private void CompleteStep()
    {
        UnbindClick();
        TutorialStepData step = CurrentStep;
        if (step.ResumeOnComplete)
            isFlowPaused = false;

        if (step.HideIdsOnComplete != null && step.HideIdsOnComplete.Length > 0)
        {
            foreach (string id in step.HideIdsOnComplete)
                revealedIds.Remove(id);
            TutorialTarget.RefreshAll();
        }

        CurrentStep = null;
        contextTokens.Clear();
        ApplyPause();
        StepCompleted?.Invoke(step);

        PrepareNextStep();
    }

    private void TryAdvance(TutorialAdvance advance)
    {
        if (CurrentStep != null && CurrentStep.Advance == advance)
            CompleteStep();
    }

    private void BindClick()
    {
        if (clickButton != null) return;
        if (!TutorialTarget.TryGet(CurrentStep.AdvanceId, out TutorialTarget target)) return;

        clickButton = target.GetComponent<Button>();
        if (clickButton != null)
            clickButton.onClick.AddListener(OnAdvanceButtonClicked);
    }

    private void UnbindClick()
    {
        if (clickButton != null)
            clickButton.onClick.RemoveListener(OnAdvanceButtonClicked);
        clickButton = null;
    }

    private void OnAdvanceButtonClicked() => TryAdvance(TutorialAdvance.UIClick);

    //시작 화면으로 돌아오면(설정창의 시작 화면 이동 등) 튜토리얼을 종료 — 남겨 두면 어두운 층이 시작 화면 버튼을 막음. 새 게임이 진행 데이터를 지우므로 튜토리얼도 처음부터 다시 시작
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == StartSceneName && IsActive)
            End();
    }

    private void OnNodePurchased(GrowthNodeData node)
    {
        if (CurrentStep != null && CurrentStep.Advance == TutorialAdvance.NodePurchased && node.Code == CurrentStep.AdvanceId)
            CompleteStep();
    }

    private void OnPromotionCompleted(HeroData hero) => TryAdvance(TutorialAdvance.PromotionDone);

    private void SetSettingOpen(bool open)
    {
        isSettingOpen = open;
        ApplyPause();
    }

    //전투 진행 정지: 단계가 멈춰 둔 경우 또는 설정창이 열린 경우
    private void ApplyPause()
    {
        if (ActionSystem.Instance != null)
            ActionSystem.Instance.SetPause(IsActive && (isFlowPaused || isSettingOpen));
    }

    //뽑을 카드 더미 맨 위 1장을 행동력 없이 손으로 (카드 뽑기 AP 소모는 버튼에서 PRE로 붙이므로 GA만 실행하면 됨)
    private IEnumerator GrantCardRoutine()
    {
        isGranting = true;
        while (ActionSystem.Instance.IsPerforming)
            yield return null;

        ActionSystem.Instance.Perform(new DrawCardsGA(1), () => isGranting = false);
    }

    private static bool IsUnitActionOf(DealDamageGA ga) => ga.IsBasicAttack && ga.Caster != null;

    //Reactions
    private void TurnGAPreReaction(TurnGA turnGA)
    {
        //튜토리얼 스테이지의 전투 시작마다 처음부터 (도중에 껐다 켜도 전투 파트 처음부터)
        if (turnGA.Type == TurnType.StartBattle && GameSystem.Instance.TutorialBattle != null)
            Begin();
    }

    private void TurnGAPostReaction(TurnGA turnGA)
    {
        if (turnGA.Type == TurnType.Player) Fire(TutorialTrigger.PlayerTurnStart);
        else if (turnGA.Type == TurnType.Enemy) Fire(TutorialTrigger.EnemyTurnEnd);
    }

    private void AutoBattleTurnGAPreReaction(AutoBattleTurnGA ga) => TryAdvance(TutorialAdvance.TurnEnd);

    private void AutoBattleGAPreReaction(AutoBattleGA ga) => Fire(TutorialTrigger.AutoBattleStart);

    private void MoveGAPreReaction(MoveGA ga)
    {
        if (!ga.isKnockBack) Fire(TutorialTrigger.FirstUnitAction, ga.mover);
    }

    private void DealDamageGAPreReaction(DealDamageGA ga)
    {
        if (!IsUnitActionOf(ga)) return;

        Fire(TutorialTrigger.FirstUnitAction, ga.Caster);
        if (ga.Caster is HeroView hero && hero.CheckStatusEffectExist(StatusEffectType.CRITICAL))
            Fire(TutorialTrigger.CriticalAttack, ga.Caster);
    }

    private void AttackCoreGAPreReaction(AttackCoreGA ga)
    {
        if (ga.Caster == null) return;   //단도 등 스킬 공격은 제외

        Fire(TutorialTrigger.FirstUnitAction, ga.Caster);
        Fire(TutorialTrigger.CoreAttack, ga.Caster);
    }

    private void DealDamageGAPostReaction(DealDamageGA ga)
    {
        if (ga.Caster == null && ga.IgnoreDodge)   //단도 (자동 전투 공격은 시전자가 있음)
        {
            if (CurrentStep != null && CurrentStep.Advance == TutorialAdvance.UseDagger)
                CompleteStep();
        }
    }

    private void PlayCardGAPostReaction(PlayCardGA ga)
    {
        if (CurrentStep == null || CurrentStep.Advance != TutorialAdvance.PlaceCard) return;
        if (ga.Card.Unit.Data == CurrentStep.CardHero && ga.TargetPos == CurrentStep.CellPos)
            CompleteStep();
    }

    private void DrawCardsGAPostReaction(DrawCardsGA ga)
    {
        if (!isGranting) TryAdvance(TutorialAdvance.DrawCard);
    }

    private void GameClearGAPostReaction(GameClearGA ga) => TryAdvance(TutorialAdvance.BattleVictory);

    private void AddStatusEffectGAPostReaction(AddStatusEffectGA ga)
    {
        if (ga.StatusEffectType == StatusEffectType.CRITICAL && ga.Targets != null)
            Fire(TutorialTrigger.CriticalGained, ga.Targets.ToArray());
    }
}
