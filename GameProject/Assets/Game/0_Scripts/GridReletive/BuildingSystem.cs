using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuildingSystem : MonoBehaviour
{
    public const float CellSize = 1f;
    [SerializeField] private BuildingData buildingData1;
    [SerializeField] private BuildingData buildingData2;
    [SerializeField] private BuildingData buildingData3;
    [SerializeField] private BuildingPreview previewPrefab;
    [SerializeField] private Building builingPrefab;
    [SerializeField] private BuildingGrid grid;

    private BuildingPreview preview;

    private void Update()
    {
        Vector3 mousePosition = GetMouseWorldPosition();
        if (preview != null)
        {
            HandlePreView(mousePosition);
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                preview = CreatBuildingPreview(buildingData1, mousePosition);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                preview = CreatBuildingPreview(buildingData2, mousePosition);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                preview = CreatBuildingPreview(buildingData3, mousePosition);
            }
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            preview.Rotate(90f);
        }
    }
    private void HandlePreView(Vector3 mouseWorldPosition)
    {
        preview.transform.position = mouseWorldPosition;
        List<Vector3> buildPositions = preview.BuildingModel.GetAllBuildingPositions();
        bool canBuild = grid.CanBuild(buildPositions);
        if (canBuild)
        {
            preview.transform.position = GetSnappedCenterPosition(buildPositions);
            preview.ChangeState(BuildingPreview.BuildingPreViewState.Positive);
            if (Input.GetMouseButtonDown(0))
            {
                PlaceBuilding(buildPositions);
            }
        }
        else
        {
            preview.ChangeState(BuildingPreview.BuildingPreViewState.Negative);
        }
    }
    private void PlaceBuilding(List<Vector3> buildPositions)
    {
        Building building = Instantiate(builingPrefab, preview.transform.position, Quaternion.identity);
        building.SetUp(preview.Data, preview.BuildingModel.Rotation);
        grid.SetBuilding(building, buildPositions);
        Destroy(preview.gameObject);
        preview = null;
    }
    private Vector3 GetSnappedCenterPosition(List<Vector3> allBuildingPositions)
    {
        List<int> sx = allBuildingPositions.Select(p => Mathf.FloorToInt(p.x)).ToList();
        List<int> sz = allBuildingPositions.Select(p => Mathf.FloorToInt(p.z)).ToList();

        float centerX = (sx.Min() + sx.Max()) / 2f + CellSize / 2f;
        float centerz = (sz.Min() + sz.Max()) / 2f + CellSize / 2f;
        return new Vector3(centerX, 0, centerz);
    }
    private Vector3 GetMouseWorldPosition()
    {
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (groundPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }
    private BuildingPreview CreatBuildingPreview(BuildingData data, Vector3 position)
    {
        BuildingPreview preview = Instantiate(previewPrefab, position, Quaternion.identity);
        preview.SetUp(data);
        return preview;
    }
}
