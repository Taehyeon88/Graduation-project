using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SPDUI : MonoBehaviour
{
    [SerializeField] private TMP_Text spdText;
    public void UpdateSPDUI(int amount, int maxAmount)
    {
        spdText.text = $"{amount}/{maxAmount}";
    }
}
