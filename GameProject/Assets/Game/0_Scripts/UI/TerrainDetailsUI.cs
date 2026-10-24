using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TerrainDetailsUI : MonoBehaviour
{
    [Serializable]
    private class Panel   //지형지물·장애물 공용 항목 (BG 하나가 한 종류를 보여준다)
    {
        [SerializeField] private GameObject background;
        [SerializeField] private Image portrait;   //DiamondFrame/Masked_Content
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text description;

        public void Show(Sprite sprite, string name, string desc)
        {
            background.SetActive(true);
            portrait.sprite = sprite;
            title.text = name;
            description.text = desc;
        }

        public void Hide() => background.SetActive(false);
    }

    [Header("Terrain_BG")]
    [SerializeField] private Panel terrain;

    [Header("Obstacle_BG")]
    [SerializeField] private Panel obstacle;
    [SerializeField] private TMP_Text obstacleAttack;   //Stats/Attack/Amount
    [SerializeField] private TMP_Text obstacleHealth;   //Stats/Health/Amount

    //Publics
    public void ShowTerrain(TerrainData data)
    {
        obstacle.Hide();
        terrain.Show(data.Sprite, data.Name, data.Description);
        gameObject.SetActive(true);
    }

    public void ShowObstacle(ObstacleView view)
    {
        terrain.Hide();
        ObstacleData data = (ObstacleData)view.TokenData;
        obstacle.Show(data.Sprite, data.Name, data.Description);
        obstacleAttack.text = view.BaseDamage.ToString();
        obstacleHealth.text = $"{view.CurrentHealth}/{view.MaxHealth}";
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
