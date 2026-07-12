using System;
using Newtonsoft.Json;

namespace Revenants.Networking;

public class RevenantDto
{
    public string Name { get; set; }
    public string LevelIdOfDeath { get; set; }
    public DateTime TimeOfDeath { get; set; }
    public string Platform { get; set; }
    public string GameMode { get; set; }

    [JsonProperty("data")] // because it's using the data of the revenantEntity to do the database column
    public string DataJson { get; set; }
}