using DG.Tweening;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TurnPopUpUI : MonoBehaviour
{
    [Header("Element")]
    [SerializeField] private Image turn_Pop_Image;
    [SerializeField] private Transform hero_PopUp_UI;
    [SerializeField] private TMP_Text hero_Turn_Text;
    [SerializeField] private TMP_Text hero_Current_Turn_Text;
    [SerializeField] private Transform enemy_PopUp_UI;
    [SerializeField] private TMP_Text enemy_Turn_Text;

    [Header("Direct Element")]
    [SerializeField] private float fade_in_duration = 3.4f;
    [SerializeField] private float fade_out_duration = 3.4f;
    [SerializeField] private float wait_duration = 0.5f;

    private Sequence player_fade_Squ;
    private Sequence enemy_fade_Squ;

    private void Start()
    {
        turn_Pop_Image.gameObject.SetActive(true);
        Initialize();
        InitPlayerSquence();
        InitEnemySquence();
    }
    public Sequence GetTurnPopUpTween(TurnType turnType, int current_Turn_Number)
    {
        if (turnType == TurnType.Player)
        {
            Initialize();
            hero_PopUp_UI.gameObject.SetActive(true);
            hero_Current_Turn_Text.SetText($"{current_Turn_Number}Turn");

            return player_fade_Squ;
        }
        else if (turnType == TurnType.Enemy)
        {
            SoundSystem.Instance.PlaySound(4001);
            Initialize();
            enemy_PopUp_UI.gameObject.SetActive(true);

            return enemy_fade_Squ;
        }

        return null;
    }

    private void Initialize()
    {
        hero_PopUp_UI.gameObject.SetActive(false);
        enemy_PopUp_UI.gameObject.SetActive(false);
    }

    private void InitPlayerSquence()
    {
        turn_Pop_Image.DOFade(0, 0.01f);

        player_fade_Squ = DOTween.Sequence();

        Tween fade_in = turn_Pop_Image.DOFade(1f, fade_in_duration);
        Tween fade_in_text = hero_Turn_Text.DOFade(1f, fade_in_duration);
        Tween fade_in_text2 = hero_Current_Turn_Text.DOFade(1f, fade_in_duration);

        Tween fade_out = turn_Pop_Image.DOFade(0.0f, fade_out_duration);
        Tween fade_out_text = hero_Turn_Text.DOFade(0.0f, fade_out_duration);
        Tween fade_out_text2 = hero_Current_Turn_Text.DOFade(0.0f, fade_out_duration);

        player_fade_Squ.Join(fade_in)
                       .Join(fade_in_text)
                       .Join(fade_in_text2)
                       .Insert(fade_in_duration + wait_duration, fade_out)
                       .Insert(fade_in_duration + wait_duration, fade_out_text)
                       .Insert(fade_in_duration + wait_duration, fade_out_text2)
                       .AppendCallback(() =>
                       {
                           Initialize();
                       })
                       .SetAutoKill(false)
                       .Pause();

    }
    private void InitEnemySquence()
    {
        turn_Pop_Image.DOFade(0, 0.01f);

        enemy_fade_Squ = DOTween.Sequence();

        Tween fade_in = turn_Pop_Image.DOFade(1f, fade_in_duration);
        Tween fade_in_text = enemy_Turn_Text.DOFade(1f, fade_in_duration);

        Tween fade_out = turn_Pop_Image.DOFade(0.0f, fade_out_duration);
        Tween fade_out_text = enemy_Turn_Text.DOFade(0.0f, fade_out_duration);

        enemy_fade_Squ.Join(fade_in)
                       .Join(fade_in_text)
                       .Insert(fade_in_duration + wait_duration, fade_out)
                       .Insert(fade_in_duration + wait_duration, fade_out_text)
                       .AppendCallback(() =>
                       {
                           Initialize();
                       })
                       .SetAutoKill(false)
                       .Pause();

    }

    private void OnDestroy()
    {
        player_fade_Squ.Kill();
        enemy_fade_Squ.Kill();
    }
}
