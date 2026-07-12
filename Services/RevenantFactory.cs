using System;
using System.Collections.Generic;
using Revenants.Helpers;
using Revenants.Properties;
using ThunderRoad;
using UnityEngine;

namespace Revenants.Services;

public class RevenantFactory
{
    public RevenantData Create(Creature creature, string playerName, List<ContainerContent> playerContainerContent)
    {
        // Get creature colors
        Color hairColorPrimary = creature.GetColor(Creature.ColorModifier.Hair);
        Color hairColorSecondary = creature.GetColor(Creature.ColorModifier.HairSecondary);
        Color hairColorSpecular = creature.GetColor(Creature.ColorModifier.HairSpecular);
        Color eyesColorIris = creature.GetColor(Creature.ColorModifier.EyesIris);
        Color eyesColorSclera = creature.GetColor(Creature.ColorModifier.EyesSclera);
        Color skinColor = creature.GetColor(Creature.ColorModifier.Skin);

        // Get location and level info
        Vector3 locationOfDeath = LevelLocationHelper.LocationInLevelToSave();
        string levelID = LevelLocationHelper.LevelID();

        return new RevenantData
        {
            PlayerName = playerName,
            LevelIdOfDeath = levelID,
            TimeOfDeath = DateTime.UtcNow,
            Platform = Common.GetQualityLevel().ToString(),
            GameMode = GameModeManager.instance.currentGameMode.id,
            PositionOfDeathX = locationOfDeath.x,
            PositionOfDeathY = locationOfDeath.y,
            PositionOfDeathZ = locationOfDeath.z,
            CreatureId = creature.creatureId,
            EthnicityId = creature.currentEthnicGroup.id,
            ListContainerContent = playerContainerContent,
            HairColorPrimaryR = hairColorPrimary.r,
            HairColorSecondaryR = hairColorSecondary.r,
            HairColorSpecularR = hairColorSpecular.r,
            EyesColorIrisR = eyesColorIris.r,
            EyesColorScleraR = eyesColorSclera.r,
            SkinColorR = skinColor.r,
            HairColorPrimaryG = hairColorPrimary.g,
            HairColorSecondaryG = hairColorSecondary.g,
            HairColorSpecularG = hairColorSpecular.g,
            EyesColorIrisG = eyesColorIris.g,
            EyesColorScleraG = eyesColorSclera.g,
            SkinColorG = skinColor.g,
            HairColorPrimaryB = hairColorPrimary.b,
            HairColorSecondaryB = hairColorSecondary.b,
            HairColorSpecularB = hairColorSpecular.b,
            EyesColorIrisB = eyesColorIris.b,
            EyesColorScleraB = eyesColorSclera.b,
            SkinColorB = skinColor.b,
            HairColorPrimaryA = hairColorPrimary.a,
            HairColorSecondaryA = hairColorSecondary.a,
            HairColorSpecularA = hairColorSpecular.a,
            EyesColorIrisA = eyesColorIris.a,
            EyesColorScleraA = eyesColorSclera.a,
            SkinColorA = skinColor.a
        };
    }
}