using ThunderRoad;

namespace Revenants.NPC;
public class SkillSecondWindNPC : SkillData
{
    public int chargesPerLevel = 1;
    public float healRatio = 0.5f;

    public string startEffectId;
    public string loopEffectId;

    public float knockbackUpwardsForce = 1f;
    public float knockbackRadius = 4f;
    public float knockbackForce = 10f;

    public float rageDamageReduction = 0.1f;
    public float rageDuration = 10f;
    public float rageStrengthMultiplier = 10f;
    public float rageSpeedMultiplier = 1.3f;

    public EffectData startEffectData;
    //public EffectData loopEffectData;

    public override void OnCatalogRefresh()
    {
        base.OnCatalogRefresh();
        startEffectData = Catalog.GetData<EffectData>(startEffectId);
        //loopEffectData = Catalog.GetData<EffectData>(loopEffectId);
    }

    public override void OnSkillLoaded(SkillData skillData, Creature creature)
    {
        base.OnSkillLoaded(skillData, creature);

        SeconWindNPCBehaviour behaviour = creature.gameObject.GetComponent<SeconWindNPCBehaviour>();
        if (!behaviour)
            behaviour = creature.gameObject.AddComponent<SeconWindNPCBehaviour>();

        behaviour.Setup(this, creature);
    }
}