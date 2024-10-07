using System.Text.RegularExpressions;
namespace DumpFileParser;

public class Dumper
{
    static string parseType(string type)
    {
        type = type.Trim(' ');
        switch (type)
        {
            case "int":
                return "int32";
            case "uint":
                return "uint32";
            case "long":
                return "int64";
            case "ulong":
                return "uint64";
            case "string":
                return "string";
            case "ByteString": // Google.Protobuf.ByteString beebyte
                return "bytes";
            default:
                // Console.WriteLine($"returning unknown field type {type}");
                if (type.Contains(".")) {
                    type = type.Split(".").Last();
                }
                return type;
        }
    }

    static void Main(string[] args)
    {
        string fileName = "dump.cs";
        string searchString = "IMessage";
        string asmName = "RPG.Network.Proto.dll";
        string outputFolder = "output";
        List<string> blackListEnumNames = new();

        if (!File.Exists(fileName))
        {
            Console.WriteLine($"File '{fileName}' not found.");
            return;
        }

        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }


        string fileText = File.ReadAllText(fileName);
        StreamWriter outputFile = new(Path.Combine(outputFolder, "output.proto"));
        outputFile.WriteLine($"// Extracted from {fileName} using Dumpcs2Protobuf");
        outputFile.WriteLine("syntax = \"proto3\";\n");
        string[] segments = fileText.Split(new string[] { "// AssemblyIndex:" }, StringSplitOptions.None);
        Console.WriteLine($"Parsing {segments.Length} segments...");

        foreach (string segment in segments) 
        {
            if (!segment.Split("\n")[2].Trim().EndsWith(asmName)) {
                continue;
            }
            string classNameStuff = segment.Split("\n")[5].Trim();
            // public sealed class ChessRogueSkipTeachingLevelScRsp : IMessage, IMessage<ChessRogueSkipTeachingLevelScRsp>, IEquatable<ChessRogueSkipTeachingLevelScRsp>, IDeepCloneable<ChessRogueSkipTeachingLevelScRsp>
            if (classNameStuff.StartsWith("public sealed class") && classNameStuff.Contains($": {searchString}"))
            {
                string className = classNameStuff.Split("public sealed class ")[1].Split(" :")[0].Trim();
            
                // if (className != "SceneEntityInfo") continue;

                string[] lines = segment.Split("\n");
                List<uint> fieldNums = GetFieldNums(lines);
                List<string> fieldStrings = GetFieldStrings(lines);
                ProtoEnum? oneofEnum = null;
                ProtoMessage protoMessage = new();
                string? oneOfEnumName = null; 
                string? oneOfName = null;
                if (fieldNums.Count != fieldStrings.Count) {
                    string trimmed = fieldStrings.Last().Replace(" { get; }", "").Replace("public ", "");
                    oneOfName = trimmed.Split(" ")[1];
                    oneOfEnumName = trimmed.Split(" ")[0];
                    blackListEnumNames.Add(oneOfEnumName.Split(".").Last());
                    oneofEnum = FindAndParseEnum(oneOfEnumName, segments);
                }
                
                for (int i = 0; i < fieldNums.Count; i++) {
                    ProtoFIeld field = new();
                    string fieldstr = fieldStrings[i]
                        .Replace("public ", "")
                        .Replace(" { get; set; }", "")
                        .Replace(" { get; }", "")
                        .TrimEnd();
                    if (fieldstr.StartsWith("RepeatedField<")) {
                        fieldstr = fieldstr.Substring("RepeatedField<".Length).Replace(">", "");
                        string fieldType = parseType(fieldstr.Split(" ")[0]);
                        string fieldName = fieldstr.Split(" ")[1];
                        field.isRepeated = true;
                        field.fieldName = fieldName;
                        field.fieldType = fieldType;
                    } else if (fieldstr.StartsWith("MapField<")) {
                        fieldstr = fieldstr.Substring("MapField<".Length).Replace(">", "");
                        string[] types = fieldstr.Replace(",", "").Split(" ");
                        string key = parseType(types[0]);
                        string val = parseType(types[1]);
                        field.mapKey = key;
                        field.mapValue = val;
                        field.fieldName = types[2];
                    } else {
                        string fieldType = parseType(fieldstr.Split(" ")[0]);
                        string fieldName = fieldstr.Split(" ")[1];
                        field.fieldType = fieldType;
                        field.fieldName = fieldName;
                    }
                    field.val = fieldNums[i];

                    if (oneofEnum != null && oneofEnum.valDict.ContainsValue((int)field.val)) {
                        if (protoMessage.oneOfList.Any(oneOfField => oneOfField.oneOfName == oneOfName)) {
                            OneOf oneOf = protoMessage.oneOfList.Find(oneOfField => oneOfField.oneOfName == oneOfName)!;
                            oneOf.fieldList.Add(field);
                        } else {
                            OneOf oneOf = new();
                            oneOf.oneOfName = oneOfName!;
                            oneOf.fieldList.Add(field);
                            protoMessage.oneOfList.Add(oneOf);
                        }
                    } else {
                        protoMessage.fieldList.Add(field);
                    }
                }
                protoMessage.protoName = className;
                WriteMessageToFile(protoMessage, outputFile);
            } else if (classNameStuff.StartsWith("public enum ")) {
                string className = classNameStuff.Split("public enum ").Last().Trim();
                string[] lines = segment.Split("\n");
                ProtoEnum protoEnum = parseEnum(className, lines);
                WriteEnumToFile(protoEnum, outputFile, blackListEnumNames);
            }
            
        }
        Console.WriteLine("File parsing completed.");
        outputFile.Close();
    }

    static string CamelToSnake(string camelStr) {
        bool isAllUppercase = camelStr.All(char.IsUpper); // Beebyte
        if (string.IsNullOrEmpty(camelStr) || isAllUppercase)
            return camelStr;
        return Regex.Replace(camelStr, @"(([a-z])(?=[A-Z][a-zA-Z])|([A-Z])(?=[A-Z][a-z]))", "$1_").ToLower();
    }

    static void WriteMessageToFile(ProtoMessage msg, StreamWriter writer) {
        writer.WriteLine($"message {msg.protoName} {{");
        foreach (ProtoFIeld field in msg.fieldList) {
            if (field.isRepeated) {
                writer.WriteLine($"\trepeated {field.fieldType} {CamelToSnake(field.fieldName)} = {field.val};");
            } else if (field.mapKey != null) {
                writer.WriteLine($"\tmap<{field.mapKey},{field.mapValue}> {CamelToSnake(field.fieldName)} = {field.val};");
            } else {
                writer.WriteLine($"\t{field.fieldType} {CamelToSnake(field.fieldName)} = {field.val};");
            }
        }
        foreach (OneOf oneOf in msg.oneOfList) {
            writer.WriteLine($"\toneof {oneOf.oneOfName} {{");
            foreach (ProtoFIeld field in oneOf.fieldList) {
                if (field.isRepeated) {
                    writer.WriteLine($"\t\trepeated {field.fieldType} {CamelToSnake(field.fieldName)} = {field.val};");
                } else if (field.mapKey != null) {
                    writer.WriteLine($"\t\tmap<{field.mapKey},{field.mapValue}> {CamelToSnake(field.fieldName)} = {field.val};");
                } else {
                    writer.WriteLine($"\t\t{field.fieldType} {CamelToSnake(field.fieldName)} = {field.val};");
                }
            }
            writer.WriteLine($"\t}}");
        }
        writer.WriteLine("}\n");
    }

    static void WriteEnumToFile(ProtoEnum msg, StreamWriter writer, List<string> blackListEnumNames) {
        if (blackListEnumNames.Contains(msg.enumName)) {
            return;
        }
        writer.WriteLine($"enum {msg.enumName} {{");
        foreach (KeyValuePair<string, int> entry in msg.valDict) {
            writer.WriteLine($"\t{entry.Key} = {entry.Value};");
        }
        writer.WriteLine("}\n");
    }

    static ProtoEnum FindAndParseEnum(string enumName, string[] segments) {
        var segment = segments.FirstOrDefault(s => {
            var lines = s.Split('\n');
            return lines.Length > 5 && lines[5].Trim().Contains($"enum {enumName}");
        });

        if (segment != null) {
            var lines = segment.Split('\n');
            return parseEnum(enumName, lines);
        }

        throw new ArgumentException($"Enum {enumName} not found in the provided segments.");
    }

    static ProtoEnum parseEnum(string enumName, string[] lines) {
        ProtoEnum result = new();
        for (int i=0; i<lines.Length; i++) {
            string line = lines[i];
            if (!line.Contains("const ") || !line.Contains(" = ")) continue;
            int val = Int32.Parse(line.Split(";").First().Split(" ").Last());
            string name;
            if (lines[i-1].Contains("[OriginalNameAttribute(Name = \"")) {
                string upperLine = lines[i-1];
                name = upperLine.Split("[OriginalNameAttribute(Name = \"").Last().Split("\"").First();
            } else {
                name = line.Split($"{enumName} ").Last().Split("=").First().Trim();
            }
            if (enumName.Contains(".")) {
                enumName = enumName.Split(".").Last();
            }
            result.enumName = enumName;
            result.valDict[name] = val;
        }
        return result;
    }

    static List<uint> GetFieldNums(string[] lines) {
        List<uint> result = new();

        int fieldsStartIndex = -1;
        int propertiesStartIndex = -1;

        // Find the indices of the "// Fields" and "// Properties" lines
        for (int i = 0; i < lines.Length; i++) {
            if (lines[i].Trim().StartsWith("// Fields")) {
                fieldsStartIndex = i + 1; // Start reading from the next line
            } else if (lines[i].Trim().StartsWith("// Properties")) {
                propertiesStartIndex = i; // Stop reading just before this line
                break; // No need to search further
            }
        }

        // If both indices were found, parse the field definitions
        if (fieldsStartIndex >= 0 && propertiesStartIndex > fieldsStartIndex) {
            for (int i = fieldsStartIndex; i < propertiesStartIndex; i++) {
                string line = lines[i].Trim();
                // Skip empty lines or comments
                if (string.IsNullOrEmpty(line) || line.StartsWith("//")) {
                    continue;
                }

                // Extract field definition: get the part that represents the field number
                // This assumes that the field number is defined as `public const int SomeFieldName = 15;`
                var match = Regex.Match(line, @"public\s+const\s+int\s+(\w+)\s*=\s*(\d+);");

                if (match.Success) {
                    // The field number is the value assigned to the constant
                    if (uint.TryParse(match.Groups[2].Value, out uint fieldNum)) {
                        result.Add(fieldNum);
                    }
                }
            }
        }

        return result;
    }

    static List<string> GetFieldStrings(string[] lines) {
        List<string> properties = new();

        foreach (var line in lines) {
            string trimmedLine = line.Trim();

            // Check for public properties that are not static
            if (trimmedLine.StartsWith("public ") && !trimmedLine.Contains("static")) {
                // Match for property pattern: public <Type> <Name> { get; set; }
                var match = Regex.Match(trimmedLine, 
                    @"public\s+([\w\.]+(\s*<.*?>)?)\s+(\w+)\s*\{( get;(\s+set;)?| set;(\s+get;)?| get;| set;) \}");


                if (match.Success) {
                    properties.Add(trimmedLine);
                }
            }
        }

        return properties;
    }

}