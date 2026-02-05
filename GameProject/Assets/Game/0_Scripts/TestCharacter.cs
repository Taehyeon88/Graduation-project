using System.Collections;
using System.Collections.Generic;
using IsoTools;
using UnityEngine;

public class TestCharacter : MonoBehaviour
{
    int currentX = 0;
    int currentY = 0;

    private IsoObject isoObject;

    private void Start()
    {
        isoObject = GetComponent<IsoObject>();
        isoObject.transform.position = Vector3.zero;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentX += GetRandomValue();
            UpdatePosition();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentX -= GetRandomValue();
            UpdatePosition();
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentY += GetRandomValue();
            UpdatePosition();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentY -= GetRandomValue();
            UpdatePosition();
        }
    }
    private void UpdatePosition()
    {
        isoObject.position = new Vector3(currentX, currentY, 1);
    }

    private int GetRandomValue()
    {
        return UnityEngine.Random.Range(0, 10);
    }
}
