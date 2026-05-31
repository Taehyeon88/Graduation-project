using UnityEngine;
using TMPro;
using System.Collections;

public class TutorialUI : MonoBehaviour
{
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private float duration = 0.12f;
    [SerializeField] private GameObject backPanel;
    [SerializeField] private GameObject blockUI;
    [SerializeField] private GameObject backPanel2;

    private WaitForSecondsRealtime WaitForSeconds;

    public IEnumerator ApplyContents(string title, string description)
    {
        if (WaitForSeconds == null)
            Initialized();

        descriptionText.SetText(title + "\n");

        char[] chars = description.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            descriptionText.text += chars[i];
            yield return WaitForSeconds;
        }
    }

    public void SetBackPanelActive(bool active) => backPanel.SetActive(active);
    public void SetBlockUIActive(bool active) => blockUI.SetActive(active);
    public void SetTextEmpty() => descriptionText.SetText(string.Empty);
    public void SetBackPanel2Active(bool active) => backPanel2.SetActive(active);

    private void Initialized()
    {
        WaitForSeconds = new WaitForSecondsRealtime(duration);
    }
}
