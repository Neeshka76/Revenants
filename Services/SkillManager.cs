using ThunderRoad.Skill.Spell;
using ThunderRoad.Skill;
using ThunderRoad;
using System.Collections.Generic;

namespace Revenants.Services;

public class SkillManager
{
    private readonly Creature _creature;
    private List<SkillData> _skillsToGive = new List<SkillData>();
    private List<SpellData> _spellsToGive = new List<SpellData>();

    public SkillManager(Creature creature)
    {
        _creature = creature;
        ManageSkill();
    }

    private void ManageSkill()
    {
        _skillsToGive = SkillsToGive();
        foreach (SkillData skill in _skillsToGive)
        {
            //Snippet.DebugLog($"Adding : {skill.id} to {_creature}", "lime");
            _creature.container.AddSkillContent(skill);
        }
        foreach (SpellData spell in _spellsToGive)
        {
            //Snippet.DebugLog($"Adding : {spell.id} to {_creature}", "lime");
            _creature.container.AddSpellContent(spell);
        }
    }

    private bool HasABow(ItemContent itemContent)
    {
        if (itemContent.state is not ContentStateHolder holderState)
            return false;
        if (itemContent.data == null) return false;
        itemContent.data.TryGetModule(out ItemModuleBow bowModule);
        return bowModule != null;
    }

    private List<SkillData> SkillsToGive()
    {
        List<SpellContent> spellContents = new List<SpellContent>();
        List<SpellCastCharge> spells = new List<SpellCastCharge>();
        bool foundABow = false;
        foreach (ContainerContent content in _creature.container.contents)
        {
            switch (content)
            {
                case SpellContent spellContent:
                    spellContents.Add(spellContent);
                    //Snippet.DebugLog($"Finding spell {spellContent.referenceID} for {_creature.name}");
                    if (Catalog.GetData<SpellCastCharge>(spellContent.referenceID, false) is SpellCastCharge spellCastCharge)
                    {
                        //Snippet.DebugLog($"Spell {spellContent.referenceID} is {spellCastCharge.GetType()} for {_creature.name}");
                        spells.Add(spellCastCharge);
                    }
                    if (Catalog.GetData<SkillThunderbolt>(spellContent.referenceID, false) is SkillThunderbolt skillThunderbolt)
                    {
                        //Snippet.DebugLog($"Adding {skillThunderbolt.id} to {_creature}");
                        SpellData spellData = Catalog.GetData<SpellCastLightning>("ThunderboltAILightning");
                        //Snippet.DebugLog($"Trying to add {spellData.id} to {_creature}");
                        _spellsToGive.Add(spellData);
                    }
                    break;
                case SkillContent skillContent:

                    break;
                case ItemContent itemContent:
                    if (!foundABow)
                        foundABow = HasABow(itemContent);
                    break;
            }
        }

        switch (spells.Count)
        {
            case 0:
                //Snippet.DebugLog($"No spell found of SpellCastCharge", "red");
                break;
            case > 0:
                //Snippet.DebugLog($"Adding ChargeSpeedBoost", "cyan");
                _skillsToGive.Add(Catalog.GetData<AISkillData>("ChargeSpeedBoost"));
                break;
        }

        switch (spells.Count)
        {
            case 1:
            {
                //Snippet.DebugLog($"Adding MeleeImbue{spells[0].id}", "cyan");
                _skillsToGive.Add(Catalog.GetData<SkillData>($"MeleeImbue{spells[0].id}"));
                if (foundABow)
                {
                    //Snippet.DebugLog($"Adding ArrowImbue{spells[0].id}", "cyan");
                    _skillsToGive.Add(Catalog.GetData<SkillData>($"ArrowImbue{spells[0].id}"));
                }

                break;
            }
            case > 1:
            {
                //Snippet.DebugLog($"Adding MeleeImbueRandom", "cyan");
                _skillsToGive.Add(Catalog.GetData<SkillData>($"MeleeImbueRandom"));
                if (foundABow)
                {
                    //Snippet.DebugLog($"Adding ArrowImbueRandom", "cyan");
                    _skillsToGive.Add(Catalog.GetData<SkillData>($"ArrowImbueRandom"));
                }

                break;
            }
        }

        return _skillsToGive;
    }
}