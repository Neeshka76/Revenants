using System.Collections;
using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;

namespace Revenants.NPC;

public class SeconWindNPCBehaviour : ThunderBehaviour
{
    private Creature _creature;
    private SkillSecondWindNPC _data;
    
    private int _charges;
    private bool _raging;
    private Coroutine _rageRoutine;
    
    public void Setup(SkillSecondWindNPC skillData, Creature owner)
    {
        _data = skillData;
        _creature = owner;
        _charges = _data.chargesPerLevel;
        _raging = false;
        _creature.OnKillEvent -= OnKill;
        _creature.OnKillEvent += OnKill;
    }
    
    private void OnDestroy()
    {
        if (_creature != null)
            _creature.OnKillEvent -= OnKill;
    }
    
    private void OnKill(CollisionInstance collisionInstance, EventTime eventTime)
    {
        if (eventTime == EventTime.OnStart) return;
        TryConsume(collisionInstance);
    }
    
    private void TryConsume(CollisionInstance collision)
    {
        if (_creature == null || collision == null) return;
        if (_raging) return;
        if (_charges <= 0) return;
        DamageStruct dmg = collision.damageStruct;
        if (dmg.damageType == DamageType.UnBlockable ||
            dmg.damageType == DamageType.Energy &&
            Mathf.Approximately(dmg.damage, 99999f))
            return;
        _charges--;
        DoKnockback(_creature);
        if (_rageRoutine != null)
            StopCoroutine(_rageRoutine);
        _rageRoutine = StartCoroutine(RageRoutine());
    }
    
    private void DoKnockback(Creature creature)
    {
        List<ThunderEntity> entities = ThunderEntity.InRadiusNaive(
            creature.Center,
            _data.knockbackRadius,
            Filter.LiveCreaturesExcept(creature)
        );
        float knockBackForce = 1f;
        float knockBackUpwardsForce = 1f;
        for (int i = 0; i < entities.Count; i++)
        {
            if (entities[i] is not Creature target) continue;
            //Snippet.DebugLog($"Found Player entity : {target.isPlayer}; {target.name}", "magenta");
            target.MaxPush(
                Creature.PushType.Magic,
                target.Center - creature.Center);
            if (target.isPlayer)
            {
                knockBackForce = _data.knockbackForce * 2.5f;
                knockBackUpwardsForce = _data.knockbackUpwardsForce * 2.5f;
            }
            else
            {
                knockBackForce = _data.knockbackForce;
                knockBackUpwardsForce = _data.knockbackUpwardsForce;
            }
            
            target.AddExplosionForce(
                _data.knockbackForce,
                creature.Center,
                knockBackForce,
                knockBackUpwardsForce,
                ForceMode.VelocityChange,
                target.ragdoll.targetPart.collisionHandler);
        }
    }
    
    private IEnumerator RageRoutine()
    {
        _raging = true;
        _creature.ResurrectMaxHealth();
        _creature.ragdoll.SetState(Ragdoll.State.Standing, true, true);
        _creature.brain.SetState(Brain.State.Combat);
        _creature.Heal(_creature.maxHealth * _data.healRatio);
        EffectInstance startFx = _data.startEffectData?.Spawn(_creature.transform);
        startFx?.Play();
        _creature.SetDamageMultiplier(_data, _data.rageDamageReduction);
        _creature.AddJointForceMultiplier(_data, _data.rageStrengthMultiplier, _data.rageStrengthMultiplier);
        _creature.currentLocomotion.SetAllSpeedModifiers(_data, _data.rageSpeedMultiplier);
        //EffectInstance loopFx = data.loopEffectData?.Spawn(creature.transform);
        //loopFx?.Play();
        yield return new WaitForSecondsRealtime(_data.rageDuration);
        //loopFx?.End();
        _creature.RemoveDamageMultiplier(_data);
        _creature.RemoveJointForceMultiplier(_data);
        _creature.currentLocomotion.RemoveSpeedModifier(_data);
        startFx?.End();
        _raging = false;
    }
}