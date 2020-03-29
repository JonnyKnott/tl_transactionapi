using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TrueLayer.TransactionData.Models.ApiModels.JsonConverters
{
    public class TransactionAmountJsonConverter : JsonConverter<double>
    {
        public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.GetDouble();
        }

        public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
        {
            var rounded = Math.Abs(Math.Round(value, 2));
            writer.WriteNumberValue(rounded);
        }
    }
}