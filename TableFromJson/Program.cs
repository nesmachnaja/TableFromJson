using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace TableFromJson
{
    class Program
    {
        static void Main(string[] args)
        {
            string filePath = @"\\ocs.ru\adm\UFR$\lnesmachnaya\Downloads\response_1730817393219.json";

            try
            {
                string jsonContent = File.ReadAllText(filePath);

                JsonDocument doc = JsonDocument.Parse(jsonContent);

                PrintJsonObject(doc.RootElement);

                //var jsonData = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonContent);

                //Regex regex = new Regex(@"^[\t\s]*""\w+[^"",\s]", RegexOptions.Multiline);
                ////Regex regex = new Regex(@"(?<=^|\{|\}|\n)\s*""(\w+)""\s*:", RegexOptions.Multiline);
                //MatchCollection matches = regex.Matches(jsonContent);

                //HashSet<string> uniqueMatches = new HashSet<string>();

                //foreach (Match match in matches)
                //{
                //    string field = match.Value.Replace("\"", "");
                //    field = Regex.Replace(field, @"[\t\s]+", "");
                //    uniqueMatches.Add(char.ToUpper(field[0]) + field.Substring(1)); 
                //}

                //foreach (string match in uniqueMatches)
                //{
                //    Console.WriteLine(match);
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при чтении файла: " + ex.Message);
            }
        }

        static void PrintJsonObject(JsonElement element)
        {
            if (element[0].ValueKind == JsonValueKind.Object)
            {
                string[] toExclude = { "array", "object" };
                Console.WriteLine("create table (");
                foreach (var property in element[0].EnumerateObject())
                {
                    string propertyName = property.Name;
                    string propertyType = GetJsonElementType(property.Value);

                    if (!toExclude.Contains(propertyType))
                        Console.WriteLine($"{char.ToUpper(propertyName[0]) + propertyName.Substring(1)} {propertyType} null,");
                }
                Console.WriteLine(")");
            }
        }

        static string GetJsonElementType(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    return "nvarchar()";
                case JsonValueKind.Number:
                    {
                        if (element.TryGetInt32(out int a))
                            return "int";
                        else return "decimal(28,12)";
                    }
                case JsonValueKind.True:
                case JsonValueKind.False:
                    return "bit";
                case JsonValueKind.Null:
                    return "null";
                case JsonValueKind.Array:
                    return "array";
                case JsonValueKind.Object:
                    return "object";
                default:
                    return "unknown";
            }
        }
    }
}
