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
            Console.WriteLine(" /\\_/\\ \n( o.o )\n > ^ <");
            string filePath = string.Empty;

            while (filePath == string.Empty || !File.Exists(filePath))
            {
                filePath = GetDirectory();
                if (!File.Exists(filePath)) Console.WriteLine("Указанного файла не существует");
            }

            //string filePath = @"\\ocs.ru\adm\UFR$\lnesmachnaya\Downloads\response_1730817393219.json";

            try
            {
                string jsonContent = File.ReadAllText(filePath);

                JsonDocument doc = JsonDocument.Parse(jsonContent);

                PrintJsonObject(doc.RootElement);

                //var jsonData = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonContent);

                //Regex regex = new Regex(@"^[\t\s]*""\w+[^"",\s]", RegexOptions.Multiline);Ошибка при чтении файла: Specified argument was out of the range of valid values. (Parameter 'startIndex')
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

            Console.WriteLine();
            Console.WriteLine(" /\\_/\\ \n( -.- )\n > ^ <\n");
            Console.WriteLine("Нажмите любую клавишу для выхода");
            Console.ReadKey();
        }

        private static string GetDirectory()
        {
            Console.WriteLine("Введите путь к файлу для парсинга:");
            string filePath = Console.ReadLine();
            Console.WriteLine();
            return filePath;
        }

        static void PrintJsonObject(JsonElement element)
        {
            if (element[0].ValueKind == JsonValueKind.Object)
            {
                string result = $"create table ( \n";

                //Console.WriteLine("create table (");
                string body;
                CreateQueryBody(element, "", out body);
                result = result.Insert(result.Length, body);
                result = result.Insert(result.Length, ")");

                Console.WriteLine(result);
            }
        }

        private static void CreateQueryBody(JsonElement element, string currentLevel, out string result)
        {
            string[] toExclude = { "array", "object" };
            result = currentLevel;

            if (element.GetArrayLength() > 0)
            {
                foreach (var property in element[0].EnumerateObject())
                {
                    string propertyName = property.Name;
                    string propertyType = GetJsonElementType(property.Value);

                    //JsonElement child;
                    //if (propertyType == "array") child = property.Value[0];

                    if (!toExclude.Contains(propertyType) 
                        //|| propertyType == "array" && GetJsonElementType(property.Value[0]) != "object") 
                        && !result.Contains($"{char.ToUpper(propertyName[0]) + propertyName.Substring(1)} {propertyType} null"))
                        result = result.Insert(result.Length, $"{char.ToUpper(propertyName[0]) + propertyName.Substring(1)} {propertyType} null,\n");
                    else if (propertyType == "array" && GetJsonElementType(property.Value[0]) != "object")
                        result = result.Insert(result.Length, $"{char.ToUpper(propertyName[0]) + propertyName.Substring(1)} {GetJsonElementType(property.Value[0])} null,\n");
                    else if (propertyType == "array")
                    {
                        Console.WriteLine($"Выполнить парсинг вложенного узла {propertyName}? Введите Y для подтверждения, N - для отказа");
                        string reply = Console.ReadLine();
                        if (reply.ToUpper() == "Y")
                            CreateQueryBody(property.Value, result, out result);
                    }
                }
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
