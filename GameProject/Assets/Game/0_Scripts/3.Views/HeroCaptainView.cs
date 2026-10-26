using UnityEngine;

public class HeroCaptainView : CombatantView   //그리드 밖에 존재하는 아군 코어 (TokenGrid 미등록, Forward 없음 → 자동 전투 안 함)
{
    public void SetUp(HeroCaptainData heroCaptainData)
    {
        SetUpBase(heroCaptainData);
    }

    //이어지는 체력 복원 (최대 체력은 증가분만 반영, 현재 체력은 1~최대 사이로 보정)
    public void RestoreHealth(int current, int max)
    {
        int delta = max - MaxHealth;
        if (delta > 0) ModifyStat(StatType.HEALTH, delta);
        CurrentHealth = Mathf.Clamp(current, 1, MaxHealth);
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
