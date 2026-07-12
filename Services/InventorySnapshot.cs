using System.Collections.Generic;
using ThunderRoad;

namespace Revenants.Services;

public class InventorySnapshot
{
    public List<ContainerContent> Capture(Creature creature)
    {
        List<ContainerContent> result = new List<ContainerContent>();
        Container _playerContainer = creature.container;
        //Snippet.DebugLog($"Saved Nb content : {playerCrContainer.contents.Count}", "magenta");
        string channelFound = "";
        foreach (ContainerContent content in _playerContainer.contents)
        {
            //Snippet.DebugLog($"Saved Content : {content.referenceID}", "red");
            switch (content)
            {
                case SkillContent skillContent:
                    //Snippet.DebugLog($"SkillContent : {skillContent.data.id}; {skillContent.data.combatSkill}");
                    result.Add(skillContent.Clone());
                    break;
                case SpellContent spellContent:
                    //Snippet.DebugLog($"SpellContent : {spellContent.data.id}; {spellContent.data.combatSkill}");
                    result.Add(spellContent.Clone());
                    break;
                case ItemContent itemContent:
                    //Snippet.DebugLog($"Itemcontent : {itemContent.data.id}; {itemContent.data.type}");
                    switch (itemContent.state)
                    {
                        // Get itemContent from the wardrobe of the creature
                        case ContentStateWorn wornState:
                            if (!itemContent.data.TryGetModule(out ItemModuleWardrobe module1))
                            {
                                Snippet.DebugLog($"Cannot wear {itemContent.referenceID} because it doesn't have an itemModuleWardrobe", "red", Snippet.DebugType.Error);
                            }
                            else
                            {
                                if (!module1.TryGetWardrobe(creature, out ItemModuleWardrobe.CreatureWardrobe wardrobe) || wardrobe.manikinWardrobeData == null)
                                    continue;
                                //Snippet.DebugLog($"Wardrobe data ({itemContent.referenceID}) nb Channels {wardrobe.manikinWardrobeData.channels.Length}", "cyan");
                                foreach (string channel in wardrobe.manikinWardrobeData.channels)
                                {
                                    //Snippet.DebugLog($"Wardrobe data ({itemContent.referenceID}) channel : {channel}", "yellow");
                                    channelFound = channel;
                                }

                                ItemContent contentToAdd = itemContent.Clone() as ItemContent;
                                contentToAdd.customDataList ??= new List<ContentCustomData>();
                                contentToAdd.customDataList.Add(new RevenantWardRobeData
                                {
                                    Id = itemContent.data.id,
                                    Tier = itemContent.data.tier,
                                    Channel = channelFound
                                });
                                //Snippet.DebugLog($"Wardrobe data ({itemContent.referenceID}) nb Layers {wardrobe.manikinWardrobeData.layers.Length}", "cyan");
                                foreach (int layer in wardrobe.manikinWardrobeData.layers)
                                {
                                    //Snippet.DebugLog($"Wardrobe data ({itemContent.referenceID}) layer : {layer}", "orange");
                                }

                                //Snippet.DebugLog($"Adding contentToAdd  : {contentToAdd.data.id}", "lime");
                                result.Add(contentToAdd);
                            }

                            continue;
                        // Get itemContent from the holster of the creature
                        case ContentStateHolder holderState:
                            ItemContent contentToAddHolstered = itemContent.Clone() as ItemContent;
                            contentToAddHolstered.customDataList ??= new List<ContentCustomData>();
                            ItemData holsteredItemData = contentToAddHolstered.data;
                            contentToAddHolstered.customDataList.Add(new RevenantItemData
                            {
                                Id = holsteredItemData.id,
                                Tier = holsteredItemData.tier,
                                Slot = holsteredItemData.slot,
                                Type = holsteredItemData.type,
                                Category = holsteredItemData.category
                            });
                            //Snippet.DebugLog($"Adding contentToAddHolstered  : {contentToAddHolstered.data.id}", "lime");
                            result.Add(contentToAddHolstered);
                            continue;
                    }

                    break;
                default:
                    //Snippet.DebugLog($"Adding : {content.referenceID}");
                    result.Add(content.Clone());
                    break;
            }
        }

        return result;
    }
}