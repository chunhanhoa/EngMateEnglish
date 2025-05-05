using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using TiengAnh.Models;

namespace TiengAnh.Converters
{
    public class TestModelConverter : JsonConverter<TestModel>
    {
        public override TestModel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected start of object");
            }

            var test = new TestModel();
            
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return test;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException("Expected property name");
                }

                string propertyName = reader.GetString();
                reader.Read();

                switch (propertyName.ToLower())
                {
                    case "_id":
                        test.TestIdentifier = reader.GetString();
                        break;
                    case "title":
                        test.Title = reader.GetString();
                        break;
                    case "description":
                        test.Description = reader.GetString();
                        break;
                    case "duration":
                        test.Duration = reader.GetInt32();
                        break;
                    case "level":
                        test.Level = reader.GetString();
                        break;
                    case "category":
                        test.Category = reader.GetString();
                        break;
                    case "imageurl":
                        test.ImageUrl = reader.GetString();
                        break;
                    case "createdat":
                        if (reader.TokenType == JsonTokenType.StartObject)
                        {
                            // Handle MongoDB date format
                            while (reader.Read())
                            {
                                if (reader.TokenType == JsonTokenType.EndObject)
                                    break;
                                if (reader.TokenType == JsonTokenType.PropertyName && reader.GetString() == "$date")
                                {
                                    reader.Read();
                                    test.CreatedDate = DateTime.Parse(reader.GetString());
                                }
                            }
                        }
                        else
                        {
                            test.CreatedDate = DateTime.Parse(reader.GetString());
                        }
                        break;
                    case "updatedat":
                        if (reader.TokenType == JsonTokenType.StartObject)
                        {
                            // Handle MongoDB date format
                            while (reader.Read())
                            {
                                if (reader.TokenType == JsonTokenType.EndObject)
                                    break;
                                if (reader.TokenType == JsonTokenType.PropertyName && reader.GetString() == "$date")
                                {
                                    reader.Read();
                                    test.UpdatedDate = DateTime.Parse(reader.GetString());
                                }
                            }
                        }
                        else
                        {
                            test.UpdatedDate = DateTime.Parse(reader.GetString());
                        }
                        break;
                    case "questions":
                        if (reader.TokenType == JsonTokenType.StartArray)
                        {
                            test.Questions = JsonSerializer.Deserialize<List<TestQuestionModel>>(ref reader, options);
                        }
                        break;
                    default:
                        // Skip unknown properties
                        JsonDocument.ParseValue(ref reader);
                        break;
                }
            }

            throw new JsonException("Expected end of object");
        }

        public override void Write(Utf8JsonWriter writer, TestModel value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            
            writer.WriteString("_id", value.TestIdentifier);
            writer.WriteString("title", value.Title);
            writer.WriteString("description", value.Description);
            writer.WriteNumber("duration", value.Duration);
            writer.WriteString("level", value.Level);
            writer.WriteString("category", value.Category);
            writer.WriteString("imageUrl", value.ImageUrl);
            writer.WriteString("createdAt", value.CreatedDate.ToString("o"));
            writer.WriteString("updatedAt", value.UpdatedDate.ToString("o"));
            
            writer.WritePropertyName("questions");
            JsonSerializer.Serialize(writer, value.Questions, options);
            
            writer.WriteEndObject();
        }
    }
}
