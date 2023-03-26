    using System;
    using System.IO;
    using System.Text;

    namespace DumpFileParser
    {
        class Program
        {
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
            }
        }
    }
