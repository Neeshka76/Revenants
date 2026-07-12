using System.Linq;
using ThunderRoad;

namespace Revenants.Options;

public static class ModOptions
{
    #region Fields
    
    public enum LevelTypeOptions
    {
        Any,
        Dungeon,
        Arena
    }
    
    private static ModOptionBool[] booleanOption = new ModOptionBool[2]
    {
        new ModOptionBool("Disabled", false),
        new ModOptionBool("Enabled", true)
    };
    
    private static ModOptionString[] gameModeOptions = gameModeOptions =
        new[]
                { new ModOptionString("Any", "Any") }
            .Concat(Catalog.GetDataList(Category.GameMode)
                .Select(x => new ModOptionString(x.id, x.id)))
            .ToArray();
    
    private static ModOptionInt[] arr_NbRevenantsPerRoom = ModOptionInt.CreateArray(0, 10, 1);
    
    #endregion
    
    #region Misc Settings
    
    [ModOptionCategory("Misc Settings", 1)]
    [ModOption("Mod Activation", "Enable/Disable the mod functionality", nameof(booleanOption), order = 1, defaultValueIndex = 1)]
    public static bool EnableMod = true;
    
    [ModOptionCategory("Misc Settings", 1)]
    [ModOption("Display Revenants Name", "Display the revenants name", nameof(booleanOption), order = 2, defaultValueIndex = 1)]
    public static bool DisplayName = true;
    
    [ModOptionCategory("Misc Settings", 1)]
    [ModOption("Revenants Fights Everyone", "Revenants fights every factions", nameof(booleanOption), order = 3, defaultValueIndex = 0)]
    public static bool RevenantsFightAgainstEveryone = false;
    
    #endregion
    
    #region Filter Settings
    
    [ModOptionCategory("Filter Settings", 2)]
    [ModOptionSlider]
    [ModOption("Level Type Usable", "Allow Revenants in Any/Arena/Dungeons", order = 1, defaultValueIndex = 0)]
    public static LevelTypeOptions LevelTypeOption;
    
    [ModOptionCategory("Filter Settings", 2)]
    [ModOptionSlider]
    [ModOption("Game Mode Usable", "Allow Revenants in Any/Sandbox/Crystal Hunt", nameof(gameModeOptions), order = 2, defaultValueIndex = 0)]
    public static string GameModeOption;
    [ModOptionCategory("Filter Settings", 2)]
    [ModOption("Allow Revenants At Home", "Allow Revenants at the home level", nameof(booleanOption), order = 3, defaultValueIndex = 0)]
    public static bool AllowAtHome = false;
    
    #endregion
    
    #region Spawn Settings
    
    [ModOptionCategory("Spawn Settings", 3)]
    [ModOptionSlider]
    [ModOption("Min number of Revenants per Room", "Minimal number of revenants per room", nameof(arr_NbRevenantsPerRoom), order = 0, defaultValueIndex = 2)]
    public static int MinNumberOfRevenantsPerRoom = 1;
    
    [ModOptionCategory("Spawn Settings", 3)]
    [ModOptionSlider]
    [ModOption("Max number of Revenants per Room", "Maximal number of revenants per room", nameof(arr_NbRevenantsPerRoom), order = 1, defaultValueIndex = 5)]
    public static int MaxNumberOfRevenantsPerRoom = 4;
    
    #endregion
}