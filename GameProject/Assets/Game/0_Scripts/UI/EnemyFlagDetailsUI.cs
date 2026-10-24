using TMPro;
using UnityEngine;

public class EnemyFlagDetailsUI : MonoBehaviour
{
    [SerializeField] private TMP_Text health;   //BG/Health

    //Publics
    public void Show(int currentHealth, int maxHealth)
    {
        health.text = $"{currentHealth}/{maxHealth}";
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
