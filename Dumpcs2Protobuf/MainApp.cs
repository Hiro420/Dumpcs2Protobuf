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
                            outputFile = new StreamWriter($"{outputFolder}/{className}.cs");
                            continue;
                        }

                        if (classStarted)
                        {
                            if (line.Contains("// Properties"))
                            {
                                classStarted = false;
                                outputFile.WriteLine("}");
                                outputFile.Close();
                            }
                            else
                            {
                                outputFile.WriteLine(line);
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
