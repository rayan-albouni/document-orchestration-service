using Newtonsoft.Json;

public record DocumentExtractedMessage
{
    public Guid DocumentId { get; set; }
    public required string TenantId { get; set; }
    
    [JsonConverter(typeof(ParsedDataConverter))]
    public required string ParsedData { get; set; } 
}

// Custom converter to handle both object and string ParsedData
public class ParsedDataConverter : JsonConverter<string>
{
    public override void WriteJson(JsonWriter writer, string? value, Newtonsoft.Json.JsonSerializer serializer)
    {
        writer.WriteValue(value);
    }

    public override string ReadJson(JsonReader reader, Type objectType, string? existingValue, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.String)
        {
            return reader.Value?.ToString() ?? string.Empty;
        }
        else if (reader.TokenType == JsonToken.StartObject || reader.TokenType == JsonToken.StartArray)
        {
            // Read the entire object/array and serialize it to string
            var jsonObject = serializer.Deserialize(reader);
            return JsonConvert.SerializeObject(jsonObject);
        }
        
        return string.Empty;
    }
}