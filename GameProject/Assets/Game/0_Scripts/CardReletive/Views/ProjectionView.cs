using IsoTools;
using UnityEngine;

public class ProjectionView : MonoBehaviour
{
    public IsoObject IsoObject
    {
        get
        {
            if (isoObject == null)
                Initialized();
            return isoObject;
        }
    }

    private SpriteRenderer spriteRenderer;
    private IsoObject isoObject;
    private GameObject flyVFX;

    private void Start()
    {
        gameObject.SetActive(false);
        Initialized();
    }

    private void Update()
    {
        if (flyVFX != null)
        {
            flyVFX.transform.position = spriteRenderer.gameObject.transform.position;
        }
    }

    public void StartFly(Sprite sprite, GameObject flyVFX, float scaleRate = 1)
    {
        gameObject.SetActive(true);
        this.flyVFX = HeroVisualEffectSystem.Instance.PlayVFX(flyVFX, gameObject.transform.position);
        flyVFX.transform.localScale *= scaleRate;

        if (sprite != null)
        {
            if (spriteRenderer == null)
                Initialized();
            spriteRenderer.sprite = sprite;
        }
    }

    public Vector3 EndFly()
    {
        gameObject.SetActive(false);
        Destroy(flyVFX.gameObject);
        flyVFX = null;
        return spriteRenderer.gameObject.transform.position;
    }

    private void Initialized()
    {
        spriteRenderer = gameObject.GetComponentInChildren<SpriteRenderer>();
        isoObject = gameObject.GetComponent<IsoObject>();
    }
}
