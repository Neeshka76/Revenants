using System.Collections.Generic;
using ThunderRoad;
using ThunderRoad.Skill;
using ThunderRoad.Skill.Spell;
using UnityEngine;

namespace Revenants.NPC;

public class SkillGravityDecapitateNPC : SpellSkillData
{
    protected static Collider[] colliders = new Collider[200];
    public float minCharge = 0.95f;
    public LayerMask layerMask;
    public float maxDistance = 0.2f;
    public float pushedPartVelocityMult = 0.4f;
    public string effectId = "SpellGravityPushDecapitate";
    protected EffectData effectData;
    public float pushForce = 3f;
    public ForceMode pushForceMode = ForceMode.VelocityChange;

    public override void OnCatalogRefresh()
    {
        base.OnCatalogRefresh();
        effectData = Catalog.GetData<EffectData>(effectId);
    }

    public override void OnSpellLoad(SpellData spell, SpellCaster caster = null)
    {
        base.OnSpellLoad(spell, caster);
        if (!(spell is SpellCastGravity spellCastGravity))
            return;
        spellCastGravity.OnPushEvent -= OnPushEvent;
        spellCastGravity.OnPushEvent += OnPushEvent;
    }

    public override void OnSpellUnload(SpellData spell, SpellCaster caster = null)
    {
        base.OnSpellUnload(spell, caster);
        if (!(spell is SpellCastGravity spellCastGravity))
            return;
        spellCastGravity.OnPushEvent -= OnPushEvent;
    }

    public void OnPushEvent(
        SpellCastGravity spell,
        Vector3 velocity,
        Collider[] colliders,
        int collidersCount)
    {
        if (spell.currentCharge < minCharge || !TryDismember(spell.spellCaster.Orb.position, velocity, maxDistance, layerMask,
                pushForce, pushForceMode, pushedPartVelocityMult, spell))
            return;
        effectData?.Spawn(spell.spellCaster.magicSource.position + velocity.normalized * 0.3f, Quaternion.LookRotation(velocity), null, null,
            true, null, spell.spellCaster.mana.creature.isPlayer)?.Play();
    }

    public static bool TryDismember(
        Vector3 position,
        Vector3 velocity,
        float maxDistance,
        LayerMask layerMask,
        float pushForce,
        ForceMode pushForceMode,
        float pushedPartVelocityMult,
        SpellCastGravity gravity = null)
    {
        if (!gravity.spellCaster.ragdollHand.creature.isPlayer) maxDistance *= 4f;
        int num = Physics.OverlapSphereNonAlloc(
            position + velocity.normalized * maxDistance / 2f,
            maxDistance,
            colliders,
            layerMask, QueryTriggerInteraction.Collide);
        HashSet<Creature> creatureSet = new HashSet<Creature>();
        bool flag = false;
        for (int index = 0; index < num; ++index)
        {
            Collider collider = colliders[index];
            SimpleBreakable componentInParent = collider.GetComponentInParent<SimpleBreakable>();
            if (componentInParent != null &&
                (componentInParent is not GolemCrystal golemCrystal
                 || !golemCrystal.shieldActive))
            {
                componentInParent.Break();
            }

            if (collider.attachedRigidbody == null) continue;
            // Only target RagdollParts
            if (!collider.attachedRigidbody.TryGetComponent<RagdollPart>(out RagdollPart part)) continue;
            Creature targetCreature = part.ragdoll.creature;
            Creature casterCreature = gravity.spellCaster.mana.creature;
            // Skip if the target is the caster itself
            if (targetCreature == casterCreature) continue;

            // Skip if already sliced or slicing not allowed
            if (part.isSliced || !part.sliceAllowed) continue;

            // Mark that we sliced something
            flag = true;

            // If dismemberment content is disabled, just push creature
            if (!GameManager.CheckContentActive(BuildSettings.ContentFlag.Dismemberment) && creatureSet.Add(targetCreature))
            {
                targetCreature.MaxPush(Creature.PushType.Magic, velocity);
                part.physicBody.AddForce(velocity * pushForce, pushForceMode);
                break;
            }

            // Safe slice
            part.SafeSlice();
            part.physicBody.AddForce(velocity * pushedPartVelocityMult, ForceMode.VelocityChange);

            // Kill the creature if not already dead
            if (!targetCreature.isKilled)
            {
                targetCreature.Kill(new CollisionInstance(new DamageStruct(DamageType.Energy, 10000f))
                {
                    casterHand = gravity?.spellCaster
                });
            }

            break;
        }

        return flag;
    }
}