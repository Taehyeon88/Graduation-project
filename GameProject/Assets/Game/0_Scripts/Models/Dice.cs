using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dice
{
    public string Description => data.Description;
    public int FaceCount => data.NumberPerFace.Length;
    public int[] NumberPerFace { get; private set; }
    public Quaternion[] rotationPerFace { get; private set; }
    public readonly DiceData data;
    private DiceModel model;
    public Dice(DiceData data)
    {
        this.data = data;
        model = data.DiceModel;
        NumberPerFace = data.NumberPerFace;
        rotationPerFace = model.SetUp(NumberPerFace);
    }

    public void ChangeNumber(int faceIndex, int number)
    {
        NumberPerFace[faceIndex] = number;
        model.ChanageNumber(faceIndex, number);
    }
}
