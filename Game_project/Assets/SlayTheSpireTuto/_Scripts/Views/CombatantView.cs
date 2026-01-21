using System.Collections;
using System.Collections.Generic;
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
}
