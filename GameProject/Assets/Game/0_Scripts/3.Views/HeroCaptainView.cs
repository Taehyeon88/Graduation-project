using UnityEngine;

public class HeroCaptainView : CombatantView   //그리드 밖에 존재하는 아군 코어 (TokenGrid 미등록, Forward 없음 → 자동 전투 안 함)
{
    public void SetUp(HeroCaptainData heroCaptainData)
    {
        SetUpBase(heroCaptainData);
    }

    public void PlayHitShake()
    {
        SoundSystem.Instance.PlaySound(1);
        Utility.GetHitShakeTween(this);
    }

    //코어 공격 피해 (방어막·피해 보정 없이 체력만 차감)
    public void ReceiveCoreDamage(int amount)
    {
        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
    }
}
