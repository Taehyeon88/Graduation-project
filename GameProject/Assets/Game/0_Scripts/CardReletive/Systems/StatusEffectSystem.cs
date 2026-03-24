using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectSystem : MonoBehaviour
{
    [SerializeField] private StatusEffectData[] effectDatas;
    private Dictionary<StatusEffectType, StatusEffectData> effectDataByType = new();
    void OnEnable()
    {
        //effectData들 타입별로 캐싱
        foreach (var data in effectDatas)
        {
            if (!effectDataByType.TryAdd(data.EffectType, data))
                Debug.LogError($"{data.name}데이터의 effectType이 {effectDataByType[data.EffectType].name}과 {data.EffectType}으로 충돌함");
        }

        ActionSystem.AttachPerformer<AddStatusEffectGA>(AddStatusEffectPerformer);
    }
    void OnDisable()
    {
        ActionSystem.DetachPerformer<AddStatusEffectGA>();
    }

    //Performers
    private IEnumerator AddStatusEffectPerformer(AddStatusEffectGA addStatusEffectGA)
    {
        foreach (var target in addStatusEffectGA.Targets)
        {
            if (target != null)
            {
                var infoes = effectDataByType[addStatusEffectGA.StatusEffectType].effectInfos;
                var sprite = effectDataByType[addStatusEffectGA.StatusEffectType].spriteImage;

                target.AddStatusEffect(addStatusEffectGA.StatusEffectType, addStatusEffectGA.StackCount, sprite, infoes);
                yield return null; //Add VFX for adding status effects
            }
        }
    }
}
