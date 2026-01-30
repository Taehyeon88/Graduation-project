using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dice : MonoBehaviour
{
    public string Description => data.Description;
    public int FaceCount => data.NumberPerFace.Length;
    public int[] NumberPerFace { get; private set; }
    public Quaternion[] rotationPerFace { get; private set; }
    private DiceData data;
    private DiceModel model;
    public Dice(DiceData data)
    {
        this.data = data;
        model = data.DiceModel;
        NumberPerFace = data.NumberPerFace;
        rotationPerFace = model.SetUp(NumberPerFace);
    }
    public Transform CreateDice(Vector3 setupPosition, Transform parent)
    {
        return Instantiate(model, setupPosition, Quaternion.identity, parent).transform;
    }
    public void ChangeNumber(int faceIndex, int number)
    {
        NumberPerFace[faceIndex] = number;
        model.ChanageNumber(faceIndex, number);
    }
}
