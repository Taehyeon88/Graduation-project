using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TokenManager : Singleton<TokenManager>
{
    public const float CellSize = 1f;
    [SerializeField] private TokenPreview previewPrefab;
    [SerializeField] private Token TokenPrefab;
    [SerializeField] private TokenGrid grid;

    public HeroView HeroView { get; private set; }
    public List<EnemyView> EnemyViews { get; private set; }
    public bool startSetting { get; private set; } = false;

    private TokenPreview preview;
    private TokenData tokenData;
    public void StartSetting(TokenData tokenData)
    {
        this.tokenData = tokenData;
        startSetting = true;
    }

    private void Update()
    {
        if (!startSetting) return;

        Vector3 mousePosition = GetMouseWorldPosition();
        if (preview != null)
        {
            HandlePreView(mousePosition);
        }
        else
        {
            preview = CreatTokenPreview(tokenData, mousePosition);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            preview.Rotate(90f);
        }
    }
    private void HandlePreView(Vector3 mouseWorldPosition)
    {
        preview.transform.position = mouseWorldPosition;
        List<Vector3> TokenPositions = preview.TokenModel.GetAllBuildingPositions();
        bool canSet = grid.CanSet(TokenPositions);
        if (canSet)
        {
            preview.transform.position = GetSnappedCenterPosition(TokenPositions);
            preview.ChangeState(TokenPreview.TokenPreViewState.Positive);
            if (Input.GetMouseButtonDown(0))
            {
                PlaceToken(TokenPositions);
            }
        }
        else
        {
            preview.ChangeState(TokenPreview.TokenPreViewState.Negative);
        }
    }
    private void PlaceToken(List<Vector3> setPositions)
    {
        Token token = Instantiate(TokenPrefab, preview.transform.position, Quaternion.identity, transform);
        token.SetUp(preview.Data, preview.TokenModel.Rotation);
        grid.SetToken(token, setPositions);
        Destroy(preview.gameObject);

        if (token is HeroView heroView)
        {
            this.HeroView = heroView;
        }
        else if (token is EnemyView enemyView)
        {
            EnemyViews.Add(enemyView);
        }

        ResetValue();
    }
    private Vector3 GetSnappedCenterPosition(List<Vector3> allTokenPositions)
    {
        List<int> sx = allTokenPositions.Select(p => Mathf.FloorToInt(p.x)).ToList();
        List<int> sz = allTokenPositions.Select(p => Mathf.FloorToInt(p.z)).ToList();

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
    private TokenPreview CreatTokenPreview(TokenData data, Vector3 position)
    {
        TokenPreview preview = Instantiate(previewPrefab, position, Quaternion.identity);
        preview.SetUp(data);
        return preview;
    }
    private void ResetValue()
    {
        tokenData = null;
        startSetting = false;
        preview = null;

    }
}
