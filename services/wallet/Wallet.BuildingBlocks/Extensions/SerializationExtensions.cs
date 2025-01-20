using System.Text.Json;

namespace Wallet.BuildingBlocks.Extensions;

public static class SerializationExtensions
{
    public static string ToJson(this object value,
        JsonSerializerOptions? serializerOptions = null!)
    {
        var options = serializerOptions ?? new JsonSerializerOptions
        {
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };

        return System.Text.Json.JsonSerializer.Serialize(value, options);
    }
}