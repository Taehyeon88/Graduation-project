using UnityEngine;

public enum TutorialScreen { Battle, Result, StageSelect, GrowthTree, HeroManage }

//단계가 시작되는 시점 (Immediate = 앞 단계가 끝나자마자). 자동 전투 안의 시점은 도달하면 전투를 멈추고 단계를 시작
public enum TutorialTrigger
{
    Immediate,
    BattleStart,        //전투 시작 (첫 드로우·행동력 지급 전)
    PlayerTurnStart,    //내 턴 시작
    AutoBattleStart,    //자동 전투 시작 (첫 유닛 행동 전)
    FirstUnitAction,    //첫 유닛 행동 직전 (이동·공격·코어 공격)
    SameSpeedTie,       //같은 속도 유닛이 나란히 있을 때
    CriticalAttack,     //치명타를 가진 영웅의 공격 직전
    CoreAttack,         //영웅의 코어 공격 직전
    EnemyTurnEnd,       //적 턴 종료 (라운드의 끝)
    CriticalGained,     //영웅이 치명타를 얻은 직후 (치명타 부여는 적 턴이 끝난 뒤에 처리됨)
    SceneEntered,       //Screen 필드의 화면 씬에 진입 (전투 밖 단계의 시작 시점)
}

//단계가 끝나는(넘어가는) 조건. 전투 외 조건(노드 구매·승급 확정 등)은 성장 파트에서 추가
public enum TutorialAdvance
{
    NextButton,     //안내 상자의 "다음" 또는 화면 클릭
    PlaceCard,      //CardHero 카드를 CellPos에 배치
    TurnEnd,        //전투 시작 버튼(턴 종료)
    DrawCard,       //카드 뽑기 버튼 (튜토리얼 지급은 제외)
    UseDagger,      //단도로 CellPos의 적 공격
    BattleVictory,  //전투 승리
    UIClick,        //AdvanceId 대상의 버튼 클릭
    TargetAppears,  //AdvanceId 대상이 활성화됨 (팝업이 열림 등)
    NodePurchased,  //AdvanceId 코드(예: C1)의 공용 트리 노드 구매
    PromotionDone,  //영웅 승급 확정
    StageClick,     //스테이지 선택 씬에서 AdvanceId 번호의 스테이지를 클릭해 정보 팝업이 열림 (그 스테이지의 월드 입력만 허용)
}

[System.Serializable]
public class TutorialStepData
{
    [field: SerializeField] public string Id { get; private set; }   //단계 번호 (예: battle_07)
    [field: SerializeField] public TutorialScreen Screen { get; private set; }
    [field: SerializeField, TextArea(1, 2)] public string Message { get; private set; }   //안내 문구 (두 줄 이내)

    [Header("대상")]
    [field: SerializeField] public string[] TargetIds { get; private set; }   //스포트라이트 UI ID. "@context"는 트리거 시점의 유닛들
    [field: SerializeField] public HeroData CardHero { get; private set; }    //배치·강조할 영웅 카드
    [field: SerializeField] public HeroData HeroTarget { get; private set; } //이미 배치된 이 영웅을 비춤
    [field: SerializeField] public Vector2Int CellPos { get; private set; }   //목표 칸 (배치 칸 또는 단도 대상 적의 칸)
    [field: SerializeField] public bool ShowCell { get; private set; }        //배치·단도 단계가 아니어도 CellPos를 비춤

    [Header("진행")]
    [field: SerializeField] public TutorialTrigger Trigger { get; private set; }
    [field: SerializeField] public TutorialAdvance Advance { get; private set; }
    [field: SerializeField] public string AdvanceId { get; private set; }   //UIClick·TargetAppears는 대상 ID, NodePurchased는 노드 코드
    [field: SerializeField] public bool LockInput { get; private set; } = true;   //안내 대상 외 입력 잠금 (자유 플레이 단계는 해제)

    [Header("공개와 연출")]
    [field: SerializeField] public string[] RevealIds { get; private set; }   //이 단계부터 보이게 할 UI ID (누적)
    [field: SerializeField] public string[] HideIdsOnComplete { get; private set; }   //이 단계가 끝나면 다시 숨길 UI ID (설명할 때만 잠깐 보여 주는 UI용)
    [field: SerializeField] public bool GrantCardOnStart { get; private set; }   //시작 시 뽑을 카드 더미 맨 위 1장을 행동력 없이 지급
    [field: SerializeField] public bool ShowGhost { get; private set; }   //고스트 시연 재생 (잘못된 행동 뒤에도 다시 재생)
    [field: SerializeField] public bool ResumeOnStart { get; private set; }      //시작하면서 멈춘 전투를 재개
    [field: SerializeField] public bool ResumeOnComplete { get; private set; }   //끝나면서 멈춘 전투를 재개
}
