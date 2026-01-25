using TMPro;
using UnityEngine;

public class ManaUI : MonoBehaviour
{
    [SerializeField] private TMP_Text mana;
    public void UpdateManaText(int currentMana)
    {
        this.mana.text = currentMana.ToString();
    }
}
