using System;
using System.Collections.Generic;
using ThunderRoad;

namespace Revenants.Properties;

[Serializable]
public class RevenantData
{
    public string PlayerName { get; set; }
    public string LevelIdOfDeath { get; set; }
    public DateTime TimeOfDeath { get; set; }
    public string Platform { get; set; }
    public string GameMode { get; set; }
    public float PositionOfDeathX { get; set; }
    public float PositionOfDeathY { get; set; }
    public float PositionOfDeathZ { get; set; }
    public string CreatureId { get; set; }
    public string EthnicityId { get; set; }
    public List<ContainerContent> ListContainerContent { get; set; }
    public float HairColorPrimaryR { get; set; }
    public float HairColorPrimaryG { get; set; }
    public float HairColorPrimaryB { get; set; }
    public float HairColorPrimaryA { get; set; }
    public float HairColorSecondaryR { get; set; }
    public float HairColorSecondaryG { get; set; }
    public float HairColorSecondaryB { get; set; }
    public float HairColorSecondaryA { get; set; }
    public float HairColorSpecularR { get; set; }
    public float HairColorSpecularG { get; set; }
    public float HairColorSpecularB { get; set; }
    public float HairColorSpecularA { get; set; }
    public float EyesColorIrisR { get; set; }
    public float EyesColorIrisG { get; set; }
    public float EyesColorIrisB { get; set; }
    public float EyesColorIrisA { get; set; }
    public float EyesColorScleraR { get; set; }
    public float EyesColorScleraG { get; set; }
    public float EyesColorScleraB { get; set; }
    public float EyesColorScleraA { get; set; }
    public float SkinColorR { get; set; }
    public float SkinColorG { get; set; }
    public float SkinColorB { get; set; }
    public float SkinColorA { get; set; }
}