using TMPro;
using UnityEngine;

public class CoreUI : MonoBehaviour
{
    [SerializeField] private TMP_Text myHP_Text;
    [SerializeField] private TMP_Text enemyHP_Text;

    private string myHPCache;
    private string enemyHPCache;

    private void Update()
    {
        CoreSystem core = CoreSystem.Instance;
        if (core == null) return;

        SetTextIfChanged(myHP_Text, ref myHPCache, $"{core.PlayerCoreHealth}/{core.PlayerCoreMaxHealth}");
        SetTextIfChanged(enemyHP_Text, ref enemyHPCache, $"{core.EnemyCoreHealth}/{core.EnemyCoreMaxHealth}");
    }

    private void SetTextIfChanged(TMP_Text text, ref string cache, string value)
    {
        if (text == null || cache == value) return;
        cache = value;
        text.SetText(value);
    }
}
