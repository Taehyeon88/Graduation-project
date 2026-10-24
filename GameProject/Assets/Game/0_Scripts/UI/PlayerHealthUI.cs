using TMPro;
using UnityEngine;

//상단 표시줄에 영웅 대장(아군 코어)의 현재 체력 표시
public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private TMP_Text healthText;

    private int shownHealth = -1;   //마지막으로 표시한 체력 (바뀔 때만 텍스트 갱신)

    private void Update()
    {
        if (HeroSystem.Instance == null) return;

        HeroCaptainView captain = HeroSystem.Instance.HeroCaptain;   //대장은 매치 셋업에서 생성되므로 그 전에는 null
        if (captain == null) return;

        int health = captain.CurrentHealth;
        int maxHealth = captain.MaxHealth;
        if (health == shownHealth) return;

        shownHealth = health;
        healthText.SetText($"{health}/{maxHealth}");
    }
}
