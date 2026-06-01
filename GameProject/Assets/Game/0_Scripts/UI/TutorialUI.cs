using System;
using UnityEngine;
using TMPro;
using System.Collections;

public class TutorialUI : MonoBehaviour
{
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private float duration = 0.12f;
    [SerializeField] private GameObject backPanel;
    [SerializeField] private GameObject blockUI;
    [SerializeField] private GameObject blockUI2;
    [SerializeField] private GameObject blockUI3;

    private WaitForSecondsRealtime WaitForSeconds;

    public IEnumerator ApplyContents(string title, string description)
    {
        if (WaitForSeconds == null)
            Initialized();

        descriptionText.SetText(title + "\n");

        char[] chars = description.ToCharArray();
        int jumpIndex = Array.IndexOf(chars, 'n');
        for (int i = 0; i < chars.Length; i++)
        {
            if (i == jumpIndex - 1)
            {
                descriptionText.text += "\n";
                i++;
            }
            else
            {
                descriptionText.text += chars[i];
            }
            yield return WaitForSeconds;
        }
    }

    public void SetBackPanelActive(bool active) => backPanel.SetActive(active);
    public void SetBlockUIActive(bool active) => blockUI.SetActive(active);
    public void SetTextEmpty() => descriptionText.SetText(string.Empty);
    public void SetBlockUI2Active(bool active) => blockUI2.SetActive(active);
    public void SetBlockUI3Active(bool active) => blockUI3.SetActive(active);

    private void Initialized()
    {
        WaitForSeconds = new WaitForSecondsRealtime(duration);
    }
}
