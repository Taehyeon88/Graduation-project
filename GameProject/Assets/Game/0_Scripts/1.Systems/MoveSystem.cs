using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MoveSystem : Singleton<MoveSystem>
{
    public IReadOnlyList<Vector2Int> Hero_MoveRange => heroMoveRange;

    private const int MoveRadius = 1;   //라운드 시작 위치 기준 이동 반경 (1 = 주변 8칸, 3x3)

    private List<Vector2Int> heroMoveRange;
    private Dictionary<Vector2Int, List<Vector2Int>> heroMovePaths;   //이동 가능 칸 -> 현재 위치에서의 경로
    private HeroView heroMover;
    private Queue<PerformMoveGA> reservedHeroMoves = new();
    private Dictionary<HeroView, Vector2Int> roundStartPos = new();   //영웅별 이번 라운드 시작 위치 (첫 조회 시 기록)
    private HashSet<HeroView> movePaid = new();                       //시작 위치를 떠나며 AP를 낸 영웅 (복귀 시 환급 대상)
    private HashSet<HeroView> placedThisRound = new();                //이번 라운드에 카드로 배치된 영웅 (이동 불가)
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<PerformMoveGA>(PerformMoveGAPerformer);
        ActionSystem.AttachPerformer<MoveGA>(MoveGAPerformer);
        ActionSystem.SubscribeReaction<TurnGA>(TurnGAPreReaction, ReactionTiming.PRE);
        ActionSystem.SubscribeReaction<PlayCardGA>(PlayCardGAPostReaction, ReactionTiming.POST);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<PerformMoveGA>();
        ActionSystem.DetachPerformer<MoveGA>();
        ActionSystem.UnsubscribeReaction<TurnGA>(TurnGAPreReaction, ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<PlayCardGA>(PlayCardGAPostReaction, ReactionTiming.POST);
    }

    private void Update()
    {
        //예약된 영웅 이동이 있고 액션이 종료 되면 다음 예약 실행
        if (reservedHeroMoves.Count <= 0
            || ActionSystem.Instance.IsPerforming 
            || TurnSystem.Instance.CurrentTurn == TurnType.GameSetUp
            || TurnSystem.Instance.CurrentTurn == TurnType.StartBattle)
            return;

        var performMoveGA = reservedHeroMoves.Dequeue();
        if (performMoveGA.mover is HeroView hero && !TryApplyMoveAP(hero, performMoveGA)) return;   //예약 후 AP가 부족해졌으면 이동 취소

        ActionSystem.Instance.Perform(performMoveGA);
    }

    //Player
    public void PlayPlayerMoveMode(HeroView heroView)
    {
        if (Interactions.Instance.IsCardTargetMode) return;   //카드 타겟 모드 중, 이동 모드 진입 불가
        if (placedThisRound.Contains(heroView)) return;       //배치된 라운드에는 이동 불가
        if (IsAtRoundStart(heroView) && !APSystem.Instance.HasEnoughAP()) return;   //시작 위치에서 떠나려면 AP 필요 (이미 떠난 영웅은 AP 없이 이동 가능)

        if (heroMover == null || heroMover != heroView)
        {
            heroMover = heroView;
            if (!Interactions.Instance.IsHeroMoveMode)
            {
                Interactions.Instance.IsHeroMoveMode = true;    //이동 모드 활성화
                StartCoroutine(PlayerMoveMode(heroView));
            }
        }
        else
        {
            Interactions.Instance.IsHeroMoveMode = false;   //이동 모드 비활성화
        }
    }

    private IEnumerator PlayerMoveMode(HeroView heroView)
    {
        HeroView currentHero = heroView;
        UpdateMoveRange(currentHero);

        while (true)
        {
            if (Interactions.Instance.GridSelected)
            {
                Vector3 isoPos = TokenSystem.Instance.IsoWorld.MouseIsoTilePosition(1);
                Vector2Int pos = Utility.IsoVectorToVector2Int(isoPos);

                if (heroMovePaths.TryGetValue(pos, out var path))
                {
                    PerformMoveGA performMoveGA = new(currentHero, path);
                    reservedHeroMoves.Enqueue(performMoveGA);
                    break;
                }
            }

            if (currentHero != heroMover)      //변경된 영웅 대상으로 이동VG 업데이트
            {
                UpdateMoveRange(heroMover);
                currentHero = heroMover;
            }

            if (!Interactions.Instance.IsHeroMoveMode)   //이동 모드 종료
            {
                break;
            }

            yield return null;
        }
        //이동 모드 종료 후, 이동 데이터 초기화
        heroMover = null;
        heroMoveRange = null;
        heroMovePaths = null;
        Interactions.Instance.IsHeroMoveMode = false;
        VisualGridCreator.Instance.RemoveVisualGrid(gameObject.GetInstanceID(), "Hero_Move");
    }

    private void UpdateMoveRange(HeroView heroView)
    {
        heroMovePaths = CalcMovePaths(heroView);
        heroMoveRange = heroMovePaths.Keys.ToList();

        VisualGridCreator.Instance.RemoveVisualGrid(gameObject.GetInstanceID(), "Hero_Move");
        foreach (var pos in heroMoveRange)
            VisualGridCreator.Instance.CreateVisualGrid(gameObject.GetInstanceID(), pos, "Hero_Move");
    }

    //라운드 시작 위치 기준 3x3 안에서, 현재 위치로부터 이동 가능한 칸과 그 경로 (8방향 BFS, 범위 밖 우회 금지)
    private Dictionary<Vector2Int, List<Vector2Int>> CalcMovePaths(HeroView heroView)
    {
        Vector2Int start = GetRoundStartPos(heroView);
        Vector2Int current = TokenSystem.Instance.API.GetTokenPosition(heroView);
        Dictionary<Vector2Int, Vector2Int> parent = new();
        Queue<Vector2Int> queue = new();
        queue.Enqueue(current);
        parent[current] = current;

        while (queue.Count > 0)
        {
            Vector2Int cur = queue.Dequeue();
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    Vector2Int next = cur + new Vector2Int(dx, dy);
                    if (next == cur || parent.ContainsKey(next)) continue;
                    if (!CanMoveTo(next, start)) continue;

                    parent[next] = cur;
                    queue.Enqueue(next);
                }
            }
        }

        Dictionary<Vector2Int, List<Vector2Int>> paths = new();
        foreach (var dest in parent.Keys)
        {
            if (dest == current) continue;

            List<Vector2Int> path = new();
            for (Vector2Int p = dest; p != current; p = parent[p])
                path.Add(p);
            path.Reverse();
            paths.Add(dest, path);
        }
        return paths;
    }

    //시작 위치 중심 3x3 안, 빈 칸, 배치 구간 제외 (시작 위치는 복귀 목적지라 배치 구간이어도 허용)
    private bool CanMoveTo(Vector2Int pos, Vector2Int start)
    {
        if (Mathf.Abs(pos.x - start.x) > MoveRadius || Mathf.Abs(pos.y - start.y) > MoveRadius) return false;
        if (!TokenSystem.Instance.API.IsGridEmpty(pos)) return false;
        if (pos == start) return true;

        return !HeroSystem.Instance.SetupPoses.Contains(pos)
            && !EnemySystem.Instance.SetupPoses.Contains(pos);
    }

    private Vector2Int GetRoundStartPos(HeroView heroView)
    {
        if (!roundStartPos.TryGetValue(heroView, out Vector2Int start))
        {
            start = TokenSystem.Instance.API.GetTokenPosition(heroView);
            roundStartPos[heroView] = start;
        }
        return start;
    }

    private bool IsAtRoundStart(HeroView heroView)
    {
        return TokenSystem.Instance.API.GetTokenPosition(heroView) == GetRoundStartPos(heroView);
    }

    //시작 위치를 떠나면 AP 1 소모, 소모했던 영웅이 시작 위치로 돌아오면 AP 1 환급, 그 외 이동은 AP 없음
    private bool TryApplyMoveAP(HeroView heroView, PerformMoveGA performMoveGA)
    {
        Vector2Int dest = performMoveGA.path[^1];
        Vector2Int start = GetRoundStartPos(heroView);

        if (IsAtRoundStart(heroView) && dest != start)
        {
            if (!APSystem.Instance.HasEnoughAP()) return false;

            performMoveGA.PreReactions.Add((new SpendAPGA(), null));
            movePaid.Add(heroView);
        }
        else if (dest == start && movePaid.Remove(heroView))
        {
            performMoveGA.PreReactions.Add((new RefillAPGA(1), null));
        }
        return true;
    }

    //Reactions

    private void TurnGAPreReaction(TurnGA turnGA)
    {
        if (turnGA.Type != TurnType.Player) return;

        roundStartPos.Clear();   //새 라운드: 시작 위치 / AP 지불 / 배치 기록 초기화
        movePaid.Clear();
        placedThisRound.Clear();
    }

    //카드로 배치된 영웅은 해당 라운드 동안 이동 불가 (전투 전 초기 배치는 PlayCardGA를 거치지 않아 제외)
    private void PlayCardGAPostReaction(PlayCardGA playCardGA)
    {
        if (TokenSystem.Instance.API.GetTokenByPosition(playCardGA.TargetPos) is HeroView hero)
            placedThisRound.Add(hero);
    }

    //Performer

    private IEnumerator PerformMoveGAPerformer(PerformMoveGA performMoveGA)
    {
        CombatantView mover = performMoveGA.mover;
        List<Vector2Int> path = performMoveGA.path;

        if (mover == null) yield break;   //파괴된 몬스터 예외처리

        //대상 이동 처리
        foreach (Vector2Int p in path)
        {
            MoveGA moveGA = new(mover, p);
            ActionSystem.Instance.AddReaction(moveGA);
        }

        yield return null;
    }
    private IEnumerator MoveGAPerformer(MoveGA moveGA)
    {
        Token mover = moveGA.mover;
        Vector2Int position = moveGA.movePosition;
        if (mover != null)
        {
            yield return TokenSystem.Instance.Main.MoveToken(mover, position);
            SoundSystem.Instance.PlaySound(2);
        }
    }
}
