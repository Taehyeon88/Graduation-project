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

            foreach (var perk in data.Hero.Perks)
            {
                DisPlayUI disPlayUI = Instantiate(disPlayUIPrf, parents[i]);
                disPlayUI.SetUp(perk);
            }
        }
        this.heroDatas = datas;
    }
}
