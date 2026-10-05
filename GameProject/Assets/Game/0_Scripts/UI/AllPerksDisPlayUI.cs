using UnityEngine;
using UnityEngine.UI;

public class AllPerksDisPlayUI : MonoBehaviour
{
    [SerializeField] private DisPlayUI disPlayUIPrf;
    [SerializeField] private Image[] colorLabels;     //각 영웅 별, 색 라벨
    [SerializeField] private Image[] icons;           //각 영웅 별, 아이콘
    [SerializeField] private RectTransform[] parents; //각 영웅 별, 부모

    private HeroData[] heroDatas;
    public void SetUp(HeroData[] datas)
    {
        for (int i = 0; i < datas.Length; i++)
        {
            HeroData data = datas[i];
            colorLabels[i].color = data.heroColor;
            icons[i].sprite = data.simbolIcon;

            //표시 전용 PerkItem (구독하지 않음), 유닛 최대 특성 개수까지만 표시
            for (int p = 0; p < Mathf.Min(data.Perks.Count, CombatantView.MaxPerkCount); p++)
            {
                if (data.Perks[p] == null) continue;

                DisPlayUI disPlayUI = Instantiate(disPlayUIPrf, parents[i]);
                disPlayUI.SetUp(new PerkItem(data.Perks[p]));
            }
        }
        this.heroDatas = datas;
    }
}
