using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    [SerializeField] private RoomData[] roomDatas;
    [SerializeField] private Button[] buttons;

    private RoomData roomData;

    private void OnEnable()
    {
        for (int i = 0; i < roomDatas.Length; i++)
        {
            int index = i;
            roomData = roomDatas[index];
            buttons[index].onClick.AddListener(() =>
            {
                ChangeRoom(index);
            });
        }
    }
    private void ChangeRoom(int index)
    {
        RoomData roomData = roomDatas[index];
        GameSystem.Instance.SetCurrentRoomData(roomData);
    }
}
