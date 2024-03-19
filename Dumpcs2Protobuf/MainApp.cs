using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace DumpFileParser
{
    public class Dumper
    {
        static Regex readonly_extractor = new Regex("<(.*?)>");

        static List<string> global_imports = new List<string>();

        static string parseType(string type, List<string> importList)
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
                case "bool":
                    return "bool";
                case "string":
                    return "string";
                case "double":
                    return "double";
                case "float":
                    return "float";
                case "ByteString": // Google.Protobuf.ByteString beebyte
                    return "bytes";
                case "MapField":
                    return "map";
                default:
                    if (type.Contains("<"))
                    {
                        var t = type.IndexOf('<');
                        var lst = new List<string>();
                        foreach (var genParam in type.Substring(t + 1, type.Length - t - 2).Split(new char[]{',', ' '}, StringSplitOptions.RemoveEmptyEntries))
                        {
                            lst.Add(parseType(genParam, importList));
                        }

                        return $"{parseType(type.Substring(0, t), importList)}<{string.Join(", ", lst)}>";
                    }
                    if (!importList.Contains($"import \"{type}.proto\";"))
                    {
                        if (!global_imports.Contains(type))
                        {
                            global_imports.Add(type);
                        }
                        importList.Add($"import \"{type}.proto\";");
                    }
                    return type;
            }
        }
        static void Polish(string folderPath)
        {

            string[] files = Directory.GetFiles(folderPath);

            foreach (string file in files)
            {
                string contents = File.ReadAllText(file);

                contents = contents.Replace("\r\n\r\n", "\r\n").Replace("\n\n", "\n");
                contents = contents.Replace(";\r\nmessage", ";\n\nmessage");

                File.WriteAllText(file, contents);

                Console.WriteLine($"Done {file}!");
            }
        }

        static HashSet<string> dump(string folder, string output_dir)
        {
            var result = new HashSet<string>();
            
            foreach (string file in Directory.GetFiles(folder))
            {
                try
                {
                    Console.WriteLine($"parsing {file} to proto...");
                    string proto_name = Path.GetFileNameWithoutExtension(file);
                    string header = "syntax = \"proto3\";\n";
                    List<string> final = new List<string>();
                    int fn_counter = 0;
                    int f_counter = 0;
                    List<string> imports = new List<string>();
                    List<string> field_nums = new List<string>();
                    string[] lines = File.ReadAllLines(file);

                    foreach (string line in lines)
                    {
                        if (line.StartsWith("public const int "))
                        {
                            field_nums.Add(line.Replace("public const int ", "").TrimEnd(';').Split('=')[1].Trim());
                        }
                        else if (line.Contains("MapField<"))
                        {
                            string mapFieldPattern = @"MapField<([^,]+),\s*([^>]+)>";
                            Match mapMatch = Regex.Match(line, mapFieldPattern);
                            if (mapMatch.Success)
                            {
                                string keyType = parseType(mapMatch.Groups[1].Value.Trim(), imports);
                                string valueType = parseType(mapMatch.Groups[2].Value.Trim(), imports);

                                // Assuming the variable name follows the MapField declaration, extract it
                                string variableName = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Last().Trim(';');

                                // Constructing the map field declaration for Protocol Buffers
                                string mapFieldDeclaration = $"\tmap<{keyType}, {valueType}> {variableName} = {field_nums[f_counter].Trim(' ')};";
                                final.Add(mapFieldDeclaration);
                                f_counter++;
                            }
                        }
                        else if (line.StartsWith("public") && !line.Contains("const"))
                        {
                            string[] fullstr = line.TrimEnd(';').Split(' ');
                            fullstr = new string[]
                            {
                                fullstr.First(),
                                string.Join(" ", fullstr.Skip(1).Reverse().Skip(1).Reverse()),
                                fullstr.Last()
                            };
                            if (fullstr[1].Contains("readonly") || fullstr[1].Contains("RepeatedField")) //readonly
                            {
                                Match match = readonly_extractor.Match(line.TrimEnd(';'));
                                string field = match.Groups[1].Value;
                                if (field.Contains(","))
                                {
                                    string[] mapData = field.Split(',');
                                    try
                                    {
                                        string s = (mapData[0]);
                                        string se = (mapData[1]);
                                        Regex r = new Regex("int|uint|long|ulong|bool|string|double|float|ByteString");
                                        if (r.IsMatch(s) && (r.IsMatch(se)))
                                        {
                                            //Console.WriteLine($"DEBUG: \tmap<{parseType(s, imports)}, {parseType(se, imports)}> {fullstr[3]} = {field_nums[f_counter].Trim(' ')}; ");
                                            final.Add($"\tmap<{parseType(s, imports)}, {parseType(mapData[1], imports)}> {fullstr[3]} = {field_nums[f_counter].Trim(' ')};");
                                        }
                                        else if (r.IsMatch(s) && !(r.IsMatch(se)))
                                        {
                                            final.Add($"\tmap<{parseType(s, imports)}, {parseType(se, imports)}> {fullstr[3]} = {field_nums[f_counter].Trim(' ')};");
                                        }
                                        else
                                        {
                                            final.Add($"\tmap<{parseType(mapData[0], imports)}, {parseType(mapData[1], imports)}> {fullstr[4]} = {field_nums[f_counter].Trim(' ')};");
                                        }
                                        //final.Add($"\tmap<{parseType(mapData[0], imports)}, {parseType(mapData[1], imports)}> {fullstr[4]} = {field_nums[f_counter]};");
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e.Message);
                                        continue;
                                    }
                                }
                                else
                                {
                                    final.Add($"\trepeated {parseType(field, imports)} {fullstr[2]} = {field_nums[f_counter].Trim(' ')};");
                                }
                            }
                            else
                            {
                                final.Add($"\t{parseType(fullstr[1], imports)} {fullstr[2]} = {field_nums[f_counter].Trim(' ')};");
                            }
                            f_counter++;
                        }
                    }
                    foreach (string i in imports)
                    {
                        final.Insert(0, i);
                    }
                    final.Insert(0, header);
                    final.Insert(imports.Count + 1, "");
                    final.Insert(imports.Count + 2, $"message {proto_name}" + " {");
                    final.Add("}");

                    File.WriteAllLines($"{output_dir}\\{proto_name}.proto", final);

                    result.UnionWith(imports.Select(x => x.Substring(8, x.Length - 16)));
                }
                catch (Exception E)
                {
                    Console.WriteLine($"Error while parsing {file} ({E})");
                }
            }

            return result;
        }

        static void Main(string[] args)
        {
            string fileName = "dump.cs";
            string searchString = "IMessage";
            string outputFolder = "output";

            if (!File.Exists(fileName))
            {
                Console.WriteLine($"File '{fileName}' not found.");
                return;
            }

            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            string[] lines = File.ReadAllLines(fileName);
            bool classStarted = false;
            StreamWriter outputFile = null;

            try
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();

                    if (line.Contains(searchString) && line.Contains("public sealed class"))
                    {
                        if (classStarted)
                        {
                            outputFile.WriteLine("}");
                            outputFile.Close();
                        }

                        classStarted = true;
                        string[] parts = line.Split(' ');
                        string className = parts[3];
                        Console.WriteLine($"Dumping {className}");
                        outputFile = new StreamWriter($"{outputFolder}/{className}.cs");
                        continue;
                    }

                    if (classStarted)
                    {
                        if (line.Contains("// Constructors") || (line.Contains("// Methods")))
                        {
                            classStarted = false;
                            outputFile.WriteLine("}");
                            outputFile.Close();
                        }
                        else
                        {
                            if (line.Contains("DebuggerNonUserCodeAttribute") || (line.StartsWith("public override") || (line.StartsWith("public static")) || (line.StartsWith("private")) || (line.Contains("// Properties")) || (line.Contains("."))))
                            {
                                continue;
                            }
                            else
                            {
                                if (line.Contains(" { get; set; }"))
                                {
                                    line = line.Replace(" { get; set; }", "");
                                    outputFile.WriteLine(line);
                                }
                                else if (line.Contains(" { get; }"))
                                {
                                    line = line.Replace(" { get; }", "");
                                    outputFile.WriteLine(line);
                                }
                                else
                                {
                                    outputFile.WriteLine(line);
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                if (outputFile != null)
                {
                    outputFile.Close();
                }
            }


            var enums = new Dictionary<string, string>();

            var sb = new StringBuilder();

            var memberDefined = new HashSet<string>();


            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();

                if (!line.StartsWith("public enum ")) continue;

                sb.Clear();
                
                var name = line.Split(' ')[2];

                sb.Append($@"syntax = ""proto3"";

enum {name}
{{
");
                var searcher = $"\tpublic const {name} ";

                Console.WriteLine($"Dumping enum {name}");
                
                while (i < lines.Length && lines[i] != "}")
                {
                    if (lines[i].StartsWith(searcher))
                    {
                        var val = lines[i].Substring(searcher.Length);
                        var t = val.Split(' ')[0];
                        var t0 = t;
                        while (memberDefined.Contains(t)) t = $"{name}_{t}";
                        val = val.Replace(t0, t);
                        memberDefined.Add(t);
                        sb.AppendLine(val);
                    }
                    ++i;
                }

                sb.AppendLine("}");

                enums[name] = sb.ToString();
            }


            Console.WriteLine("File parsing completed.");

            string outputDir = ".\\output";
            string defsDir = ".\\defs";

            if (!Directory.Exists(defsDir))
            {
                Directory.CreateDirectory(defsDir);
            }

            var usedTypes = dump(outputDir, defsDir);

            foreach (var @enum in usedTypes)
                if (enums.TryGetValue(@enum, out var val))
                    File.WriteAllText($"{defsDir}/{@enum}.proto", val);

            Polish(defsDir);

            //Console.WriteLine(string.Join(", ", global_imports));
        }
    }
}
