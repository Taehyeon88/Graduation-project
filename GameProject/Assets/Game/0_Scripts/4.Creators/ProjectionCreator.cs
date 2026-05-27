using IsoTools;
using UnityEngine;

public class ProjectionCreator : Singleton<ProjectionCreator>
{
    [SerializeField] private IsoObject skeletonArrow;
    [SerializeField] private IsoObject ratGeneralStone;

    public IsoObject CreateProjection(ProjectionType projectionType, Vector3 genPosition, Transform parent)
    {
        IsoObject projectionPrefab = GetProjection(projectionType);
        if (projectionPrefab != null)
        {
            IsoObject projection = Instantiate(projectionPrefab, genPosition, projectionPrefab.transform.rotation, parent);
            return projection;
        }

        Debug.LogError($"{projectionType} 타입의 투사체 프리팹이 존재하지 않습니다.");
        return null;
    }

    private IsoObject GetProjection(ProjectionType projectionType)
    {
        return projectionType switch
        {
            ProjectionType.SkeletonArrow => skeletonArrow,
            ProjectionType.RatGeneralStone => ratGeneralStone,
            _ => null
        };
    }
}
