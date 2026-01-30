using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DiceModel : MonoBehaviour
{
    [SerializeField] private TMP_Text[] numberTexts;

    public Quaternion[] SetUp(int[] numbers)
    {
        Quaternion[] rotations = new Quaternion[numbers.Length];
        if (numbers.Length == numberTexts.Length)
        {
            for (int i = 0; i < numbers.Length; i++)
            {
                numberTexts[i].text = numbers[i].ToString();
                Vector3 direction = numberTexts[i].gameObject.transform.position - gameObject.transform.position;
                Quaternion targetRot = Quaternion.FromToRotation(direction, Vector3.up) * transform.rotation;
                rotations[i] = targetRot;
                
            }
            return rotations;
        }
        Debug.LogError($"DiceModel : {gameObject.name}과 Dice의 숫자 개수가 다릅니다.");
        return null;
    }

    public void ChanageNumber(int index, int number)
    {
        numberTexts[index].text = number.ToString();
    }
}
