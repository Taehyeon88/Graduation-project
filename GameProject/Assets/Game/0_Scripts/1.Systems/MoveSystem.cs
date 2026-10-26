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
    private HashSet<Vector2Int> extendedDests = new();                //마지막 이동 범위 계산에서 발판 효과로만 갈 수 있는 칸
    private Dictionary<PerformMoveGA, IExtendedMoveTerrain> extendedMoves = new();   //예약된 확장 범위 이동과 그 발판 효과
    private HashSet<HeroView> extendedBonusUsed = new();              //이번 라운드에 확장 이동 보너스를 받은 영웅 (라운드당 1회)
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
        bool isExtendedMove = extendedMoves.Remove(performMoveGA, out IExtendedMoveTerrain extendedTerrain);
        if (performMoveGA.mover is HeroView hero)
        {
            if (!TryApplyMoveAP(hero, performMoveGA)) return;   //예약 후 AP가 부족해졌으면 이동 취소

            if (isExtendedMove && extendedBonusUsed.Add(hero))   //확장 범위 이동 보너스는 영웅당 라운드 1회, 이동이 끝난 뒤 적용
            {
                foreach (GameAction reaction in extendedTerrain.GetExtendedMoveReactions(hero))
                    performMoveGA.PostReactions.Add((reaction, null));
            }
        }

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
                    if (extendedDests.Contains(pos) && TryGetExtendedMoveTerrain(GetRoundStartPos(currentHero), out IExtendedMoveTerrain extendedTerrain))
                        extendedMoves[performMoveGA] = extendedTerrain;
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
        VisualGridCreator.Instance.RemoveVisualGrid(gameObject.GetInstanceID(), "Hero_Move_Start");
    }

    private void UpdateMoveRange(HeroView heroView)
    {
        heroMovePaths = CalcMovePaths(heroView);
        heroMoveRange = heroMovePaths.Keys.ToList();

        VisualGridCreator.Instance.RemoveVisualGrid(gameObject.GetInstanceID(), "Hero_Move");
        VisualGridCreator.Instance.RemoveVisualGrid(gameObject.GetInstanceID(), "Hero_Move_Start");
        Vector2Int start = GetRoundStartPos(heroView);
        foreach (var pos in heroMoveRange)
        {
            if (pos == start) continue;   //시작 위치는 Hero_Move_Start가 표시 (클릭 이동은 그대로 가능)
            VisualGridCreator.Instance.CreateVisualGrid(gameObject.GetInstanceID(), pos, "Hero_Move");
        }

        if (heroMoveRange.Count > 0)   //이동VG와 함께만, 3x3 중앙(라운드 시작 위치) 표시
            VisualGridCreator.Instance.CreateVisualGrid(gameObject.GetInstanceID(), start, "Hero_Move_Start");
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

        //이동 기준칸이 확장 이동 발판이고 아직 그 칸에 있으면, 기본 범위 밖의 조건을 만족하는 빈 칸으로 직접 이동 가능
        extendedDests.Clear();
        if (current == start && TryGetExtendedMoveTerrain(start, out IExtendedMoveTerrain extendedTerrain))
        {
            for (int x = 0; x < TokenSystem.Instance.gridWidth; x++)
            {
                for (int y = 0; y < TokenSystem.Instance.gridHeight; y++)
                {
                    Vector2Int pos = new(x, y);
                    if (pos == current || paths.ContainsKey(pos)) continue;
                    if (!TokenSystem.Instance.API.IsGridEmpty(pos) || EnemySystem.Instance.SetupPoses.Contains(pos)) continue;
                    if (!extendedTerrain.AllowsExtendedMove(heroView, start, pos)) continue;

                    paths.Add(pos, new List<Vector2Int> { pos });
                    extendedDests.Add(pos);
                }
            }
        }
        return paths;
    }

    //기준칸에 확장 이동 효과를 가진 발판이 있으면 그 효과 (효과 템플릿은 상태가 없어 사본이 아니라 데이터에서 조회)
    private bool TryGetExtendedMoveTerrain(Vector2Int pos, out IExtendedMoveTerrain extendedTerrain)
    {
        extendedTerrain = null;
        if (!TokenSystem.Instance.Setup.TryGetTerrain(pos, out TerrainView terrain)) return false;

        extendedTerrain = terrain.Data.Effect as IExtendedMoveTerrain;
        return extendedTerrain != null;
    }

    //시작 위치 중심 3x3 안, 빈 칸, 적 배치 구간 제외 (영웅 배치 구간은 이동 가능, 시작 위치는 복귀 목적지라 항상 허용)
    private bool CanMoveTo(Vector2Int pos, Vector2Int start)
    {
        if (Mathf.Abs(pos.x - start.x) > MoveRadius || Mathf.Abs(pos.y - start.y) > MoveRadius) return false;
        if (!TokenSystem.Instance.API.IsGridEmpty(pos)) return false;
        if (pos == start) return true;

        return !EnemySystem.Instance.SetupPoses.Contains(pos);
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
        extendedBonusUsed.Clear();
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
