using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class CombatantView : MonoBehaviour
{
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private SpriteRenderer spriteRenderer;
    public int MaxHealth { get; private set; }
    public int CurrentHealth { get; private set; }
    public void SetUpBase(int health, Sprite image)
    {
        MaxHealth = CurrentHealth = health;
        spriteRenderer.sprite = image;
        UpdateHealthText();
    }
    public void UpdateHealthText()
    {
        healthText.text = $"HP : {CurrentHealth}";
    }
    public void Damage(int amount)
    {
        CurrentHealth = Mathf.Max(CurrentHealth - amount, 0);
        transform.DOShakePosition(0.2f, 0.5f);
        UpdateHealthText();
    }
}
