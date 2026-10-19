using System.Collections;

public class StatSystem : Singleton<StatSystem>
{
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<ModifyStatGA>(ModifyStatGAPerformer);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<ModifyStatGA>();
    }

    //Performers
    private IEnumerator ModifyStatGAPerformer(ModifyStatGA modifyStatGA)
    {
        foreach (var target in modifyStatGA.Targets)
        {
            if (target != null)
                target.ModifyStat(modifyStatGA.StatType, modifyStatGA.Amount);
        }
        yield return null;
    }
}
