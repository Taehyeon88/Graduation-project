using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public string Description => data.Description;
    public int Cost => data.Cost;

    private BuildingModel buildingModel;
    private BuildingData data;
    public void SetUp(BuildingData data, float rotationStep)
    {
        this.data = data;
        buildingModel = Instantiate(data.buildingModel, transform.position, Quaternion.identity);
        buildingModel.Rotate(rotationStep);
    }
}
