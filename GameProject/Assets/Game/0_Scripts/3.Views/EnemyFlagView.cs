using TMPro;
using UnityEngine;

public class EnemyFlagView : Token   //그리드 밖에 존재하는 적 코어 (TokenGrid 미등록, 표시 전용)
{
    [SerializeField] private TMP_Text healthText;

    public void SetUp(EnemyFlagData enemyFlagData)
    {
        SetUpBaseBase(enemyFlagData);
    }

    public void PlayHitShake()
    {
        SoundSystem.Instance.PlaySound(1);
        Utility.GetHitShakeTween(this);
    }

    public void UpdateHealth(int currentHealth)
    {
        if (healthText != null)
            healthText.SetText(currentHealth.ToString());
    }
}
