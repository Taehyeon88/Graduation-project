using UnityEngine;

public class CardArrowView : MonoBehaviour
{
    [SerializeField] private GameObject arrowHead;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float headOffset = 0.5f;   //선 끝을 화살촉 앞에서 멈추는 거리

    private Vector3 startPosition;

    private void Update()
    {
        Vector3 endPosition = GetMouseWorldPosition();
        Vector3 direction = (endPosition - startPosition).normalized;

        lineRenderer.SetPosition(1, endPosition - direction * headOffset);
        arrowHead.transform.position = endPosition;
        arrowHead.transform.right = direction;
    }

    //Publics
    public void SetUp(Vector3 startPosition)
    {
        this.startPosition = startPosition;
        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, GetMouseWorldPosition());
    }

    //Privates
    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = startPosition.z;
        return mouseWorld;
    }
}
