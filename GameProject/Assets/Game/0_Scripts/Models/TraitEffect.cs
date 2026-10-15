[System.Serializable]
public abstract class TraitEffect
{
    public abstract void PerformReaction(GameAction action, CombatantView owner);
}
