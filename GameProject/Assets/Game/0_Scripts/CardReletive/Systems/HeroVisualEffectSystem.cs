using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroVisualEffectSystem : Singleton<HeroVisualEffectSystem>
{
    [SerializeField] private VisualEffectData[] visualEffectDatas;
    [SerializeField] private Dictionary<(CardType, CardSubType), VisualEffectData> effectDataById = new();

    private void OnEnable()
    {
        Initialize();
        ActionSystem.AttachPerformer<PlayHeroVisualEffectGA>(PlayVisualEffectAnimation);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<PlayHeroVisualEffectGA>();
    }

    private void Initialize()
    {
        foreach (var data in visualEffectDatas)
        {
            var key = (data.CardType, data.CardSubType);

            if (!effectDataById.TryAdd(key, data))
                Debug.LogError($"{data.name}데이터의 효과ID가 {effectDataById[key].name}과 {key}으로 충돌함");
        }
    }

    public IEnumerator PlayVisualEffectAnimation(PlayHeroVisualEffectGA playVEGA)
    {
        if (effectDataById.ContainsKey(playVEGA.CardTypes))
        {
            if (effectDataById[playVEGA.CardTypes].CustomSquences.Count < playVEGA.Step)
                Debug.LogError($"{playVEGA.EffectId}의 CustomSquences의 개수가 {playVEGA.Step}보다 적습니다.");

            var data = effectDataById[playVEGA.CardTypes].CustomSquences[playVEGA.Step];

            //사운드 재생
            SoundSystem.Instance.PlaySound(data.SoundId);
            //모션 재생
            var sqe = data.CustomSquence.GetCustomSquence
                (
                     HeroSystem.Instance.HeroView,
                     TokenSystem.Instance.GetTokenPosition(HeroSystem.Instance.HeroView),
                     playVEGA.TargetPoses
                );
            sqe.Play();
            yield return sqe.WaitForCompletion(); 
        }
    }

    //publics
    public void PlayVisualEffectPreGameAction(CardType cardType, CardSubType cardSubType, List<Vector2Int> targetPoses, bool isPrivateLogic = false)
    {
        (CardType, CardSubType) type = (cardType, cardSubType);

        if (!CheckCanPlayVisualEffect(type, isPrivateLogic)) return;

        //시작 모션
        var casterPos = TokenSystem.Instance.GetTokenPosition(HeroSystem.Instance.HeroView);
        PlayHeroVisualEffectGA playVisualEffectGA = new(type, 0, targetPoses);
        ActionSystem.Instance.AddReaction(playVisualEffectGA);

        //피격 이펙트 전달
        DamageSystem.Instance.DamageVFX = GetHitVEInfo(type).Item2;
        DamageSystem.Instance.DamageSoundId = GetHitVEInfo(type).Item1;
    }

    public void PlayVisualEffectPostGameAction(CardType cardType, CardSubType cardSubType, List<Vector2Int> targetPoses, bool isPrivateLogic = false)
    {
        (CardType, CardSubType) type = (cardType, cardSubType);

        if (!CheckCanPlayVisualEffect(type, isPrivateLogic)) return;

        //회수 모션
        PlayHeroVisualEffectGA playVisualEffectGA2 = new(type, 1, targetPoses);
        ActionSystem.Instance.AddReaction(playVisualEffectGA2);
    }

    public (int, GameObject) GetHitVEInfo((CardType, CardSubType) cardTyps)
    {
        if (effectDataById.ContainsKey(cardTyps))
        {
            var soundId = effectDataById[cardTyps].HitSoundId;
            var vfx = effectDataById[cardTyps].HitVFX;
            return (soundId, vfx);
        }
        return default;
    }

    public bool CheckCanPlayVisualEffect((CardType, CardSubType) cardTyps, bool isPrivateLogic)
    {
        if (effectDataById.ContainsKey(cardTyps))
        {
            if (effectDataById[cardTyps].UsePrivateLogic != isPrivateLogic)
                return false;

            return true;
        }
        return false;
    }
}
