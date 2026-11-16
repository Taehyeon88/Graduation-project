using System.Collections.Generic;

public class StageResult   //승리한 한 판의 기록 — 별 조건 판정 입력 (StarSystem이 GameClearGA 시점에 만든다)
{
    public int Rounds { get; }                                     //승리까지 걸린 라운드 수
    public int CaptainHealth { get; }                              //승리 시 용병대장 남은 체력
    public IReadOnlyList<Hero> BaseHeroes { get; }                 //시작 덱 중 스테이지 기본 제공 카드
    public IReadOnlyList<Hero> DeckHeroes { get; }                 //시작 덱 전체 (기본 제공 + 선택)
    public IReadOnlyList<Hero> PlacedHeroes { get; }               //이번 판에 카드로 배치한 영웅
    public IReadOnlyDictionary<EnemyData, int> KillCounts { get; } //몬스터 종류별 처치 수

    public StageResult(int rounds, int captainHealth, IReadOnlyList<Hero> baseHeroes, IReadOnlyList<Hero> deckHeroes,
        IReadOnlyList<Hero> placedHeroes, IReadOnlyDictionary<EnemyData, int> killCounts)
    {
        Rounds = rounds;
        CaptainHealth = captainHealth;
        BaseHeroes = baseHeroes;
        DeckHeroes = deckHeroes;
        PlacedHeroes = placedHeroes;
        KillCounts = killCounts;
    }
}
