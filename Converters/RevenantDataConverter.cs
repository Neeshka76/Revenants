using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ThunderRoad;

namespace Revenants;

public class RevenantDataConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        // Only handle ContentCustomData and its subclasses
        return objectType == typeof(ContentCustomData);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JObject obj = JObject.Load(reader);

        string typeName = obj["$type"]?.ToString();

        if (typeName != null)
        {
            if (typeName.Contains(nameof(RevenantItemData)))
            {
                return obj.ToObject<RevenantItemData>(serializer);
            }

            if (typeName.Contains(nameof(RevenantWardRobeData)))
            {
                return obj.ToObject<RevenantWardRobeData>(serializer);
            }
        }

        return null; // or ignore unknown types
    }

    public override bool CanWrite => false;

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        return;
    }
}