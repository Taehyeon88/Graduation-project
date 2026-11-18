using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//화면 전체를 어둡게 덮고 대상만 뚫어 보여 주는 층. 구멍 밖 입력은 막고 구멍 안 입력은 아래 UI로 통과시킴
public class TutorialSpotlightGraphic : RawImage, ICanvasRaycastFilter, IPointerClickHandler
{
    private const int MaxHoles = 8;

    private static readonly int HolesId = Shader.PropertyToID("_Holes");
    private static readonly int HoleCountId = Shader.PropertyToID("_HoleCount");
    private static readonly int RimCountId = Shader.PropertyToID("_RimCount");
    private static readonly int SizeId = Shader.PropertyToID("_Size");
    private static readonly int DimAmountId = Shader.PropertyToID("_DimAmount");
    private static readonly int PulseId = Shader.PropertyToID("_Pulse");

    private readonly List<Rect> screenHoles = new();
    private readonly Vector4[] shaderHoles = new Vector4[MaxHoles];
    private Material runtimeMaterial;

    public bool BlockInput { get; set; }   //true면 구멍 밖 입력을 막음

    protected override void Awake()
    {
        base.Awake();
        if (!Application.isPlaying || material == null) return;

        runtimeMaterial = new Material(material);   //에셋 머티리얼을 건드리지 않도록 복사본 사용
        material = runtimeMaterial;
    }

    //targets: 빛나는 대상 구멍 (화면 좌표)
    public void Apply(List<Rect> targets, float dimAmount, float pulse)
    {
        if (runtimeMaterial == null) return;

        screenHoles.Clear();
        int rimCount = Mathf.Min(targets.Count, MaxHoles);
        for (int i = 0; i < rimCount; i++)
            screenHoles.Add(targets[i]);

        Rect canvasRect = rectTransform.rect;
        for (int i = 0; i < screenHoles.Count; i++)
        {
            Vector2 min = ScreenToCanvas(screenHoles[i].min, canvasRect);
            Vector2 max = ScreenToCanvas(screenHoles[i].max, canvasRect);
            shaderHoles[i] = new Vector4(min.x, min.y, max.x, max.y);
        }

        runtimeMaterial.SetVectorArray(HolesId, shaderHoles);
        runtimeMaterial.SetFloat(HoleCountId, screenHoles.Count);
        runtimeMaterial.SetFloat(RimCountId, rimCount);
        runtimeMaterial.SetVector(SizeId, canvasRect.size);
        runtimeMaterial.SetFloat(DimAmountId, dimAmount);
        runtimeMaterial.SetFloat(PulseId, pulse);
    }

    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        if (!BlockInput) return false;

        foreach (Rect hole in screenHoles)
        {
            if (hole.Contains(screenPoint)) return false;   //구멍 안은 통과
        }
        return true;
    }

    //설명 단계는 화면 클릭으로도 넘어감 (행동 단계는 Next가 무시함)
    public void OnPointerClick(PointerEventData eventData)
    {
        if (TutorialSystem.Instance != null)
            TutorialSystem.Instance.Next();
    }

    private Vector2 ScreenToCanvas(Vector2 screenPoint, Rect canvasRect)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, null, out Vector2 local);
        return local - canvasRect.min;
    }
}
