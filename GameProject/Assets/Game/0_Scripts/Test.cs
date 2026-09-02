using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    private RectTransform rectTransform;

    [SerializeField] private int currnetMana = 0;
    [SerializeField] private int maxMana = 3;

    [SerializeField] private TMP_Text mana_Text;
    [SerializeField] private Slider mana_Slider;

    [SerializeField] private float refill_duration = 0.2f;
    [SerializeField] private Ease refill_ease = Ease.Linear;
    [SerializeField] private float spend_duration = 0.1f;
    [SerializeField] private Ease spend_ease = Ease.Linear;

    private Tween RefillManaTween;
    private Tween SpendManaTween;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        RefillManaTween = DOTween.To(() =>
                mana_Slider.value,
                v => mana_Slider.value = v,
                currnetMana / (float)maxMana,
                refill_duration
            ).SetEase(refill_ease).OnComplete(() => Debug.Log("트윈 종료")).SetAutoKill(false);

        Debug.Log($"목표 : {currnetMana / (float)maxMana}");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            RefillManaTween.Restart();
        }
    }
}
