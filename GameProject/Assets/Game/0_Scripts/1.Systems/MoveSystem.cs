using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSystem : Singleton<MoveSystem>
{
    public IReadOnlyList<Vector2Int> Hero_MoveRange => hero_moveRange;

    private List<Vector2Int> hero_moveRange;
    private HeroView hero_mover;
    private Queue<PerformMoveGA> reserved_hero_moves = new();
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<PerformMoveGA>(PerformMoveGAPerformer);
        ActionSystem.AttachPerformer<MoveGA>(MoveGAPerformer);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<PerformMoveGA>();
        ActionSystem.DetachPerformer<MoveGA>();
    }

    private void Update()
    {
        //예약된 영웅 이동이 있고 액션이 종료 되면 다음 예약 실행
        if (reserved_hero_moves.Count <= 0 || ActionSystem.Instance.IsPerforming)
            return;

        var performMoveGA = reserved_hero_moves.Dequeue();
        ActionSystem.Instance.Perform(performMoveGA);
    }

    //Player
    public void PlayPlayerMoveMode(HeroView heroView)
    {
        if (hero_mover == null || hero_mover != heroView)
        {
            hero_mover = heroView;
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

                if (hero_moveRange.Contains(pos))
                {
                    var path = TokenSystem.Instance.API.GetShortestPath(currentHero, pos);
                    if (path != null)
                    {
                        int distance = path.Count;
                        currentHero.SpendMovePoint(distance);

                        PerformMoveGA performMoveGA = new(currentHero, path);
                        reserved_hero_moves.Enqueue(performMoveGA);
                        break;
                    }
                }
            }

            if (currentHero != hero_mover)      //변경된 영웅 대상으로 이동VG 업데이트
            {
                UpdateMoveRange(hero_mover);
                currentHero = hero_mover;
            }

            if (!Interactions.Instance.IsHeroMoveMode
                 ||Interactions.Instance.IsSkillTargetMode)   //이동 모드 종료
            {
                break;
            }

            yield return null;
        }
        //이동 모드 종료 후, 이동 데이터 초기화
        hero_mover = null;
        hero_moveRange = null;
        Interactions.Instance.IsHeroMoveMode = false;
        VisualGridCreator.Instance.RemoveVisualGrid(gameObject.GetInstanceID(), "Hero_Move");
    }

    private void UpdateMoveRange(HeroView heroView)
    {
        hero_moveRange = TokenSystem.Instance.API.GetCanMovePlace(heroView,
                                                    heroView.CurrentMovePoint);

        VisualGridCreator.Instance.RemoveVisualGrid(gameObject.GetInstanceID(), "Hero_Move");
        foreach (var pos in hero_moveRange)
            VisualGridCreator.Instance.CreateVisualGrid(gameObject.GetInstanceID(), pos, "Hero_Move");
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
        }
    }
}
