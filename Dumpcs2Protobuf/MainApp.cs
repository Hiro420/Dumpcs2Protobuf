using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace DumpFileParser
    {
    public class MyClass
    {
        static Regex readonly_extractor = new Regex("<(.*?)>");

        static List<string> global_imports = new List<string>();

        static string parseType(string type, List<string> importList)
        {
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
                case "ADGGCNMABEF": // Google.Protobuf.ByteString beebyte
                    return "bytes";
                default:
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

        static void dump(string folder, string output_dir)
        {
            foreach (string file in Directory.GetFiles(folder))
            {
                try
                {
                    Console.WriteLine($"parsing {file} to proto...");
                    string proto_name = Path.GetFileNameWithoutExtension(file);
                    string header = "syntax = \"proto3\";\n\noption java_package = \"emu.grasscutter.net.proto\";\n";
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
                            field_nums.Add(line.Replace("public const int ", "").TrimEnd(';').Split('=')[1]);
                            fn_counter++;
                        }
                        else if (line.StartsWith("public") && !line.Contains("const"))
                        {
                            string[] fullstr = line.TrimEnd(';').Split(' ');
                            if (fullstr[1].Contains("HGEDNJFDKFL") || (fullstr[1].Contains("NBBGAPIFGPL"))) //readonly
                            {
                                Match match = readonly_extractor.Match(line.TrimEnd(';'));
                                string field = match.Groups[1].Value;
                                if (field.Contains(","))
                                {
                                    string[] mapData = field.Split(',');
                                    final.Add($"\tmap<{parseType(mapData[0], imports)}, {parseType(mapData[1], imports)}> {fullstr[4]} = {field_nums[f_counter]};");
                                }
                                else
                                {
                                    final.Add($"\trepeated {parseType(field, imports)} {fullstr[2]} = {field_nums[f_counter]};");
                                }
                            }
                            else
                            {
                                final.Add($"\t{parseType(fullstr[1], imports)} {fullstr[2]} = {field_nums[f_counter]};");
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
                }
                catch (Exception E)
                {
                    Console.WriteLine($"Error while parsing {file} ({E})");
                }
            }
        }

        static void Main(string[] args)
        {
            string fileName = "dump.cs";
            string searchString = "CKGMNELNBGB";
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
                            if (line.Contains("DebuggerNonUserCodeAttribute") || (line.StartsWith("public override") || (line.StartsWith("public static")) || (line.StartsWith("private")) || (line.Contains("// Properties"))))
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

            Console.WriteLine("File parsing completed.");

            string outputDir = ".\\output";
            string defsDir = ".\\defs";

            if (!Directory.Exists(defsDir))
            {
                Directory.CreateDirectory(defsDir);
            }

            dump(outputDir, defsDir);

            Console.WriteLine(string.Join(", ", global_imports));
        }
    }

}
