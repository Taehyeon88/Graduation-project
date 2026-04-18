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
            var sqe = data.CustomSquence.GetCustomSquence(playVEGA.Mover, playVEGA.CurrentPos, playVEGA.Direction);
            sqe.Play();
            yield return sqe.WaitForCompletion(); 
        }
        else
        {
            yield return null;
        }
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
}
