// Put path to folder with .conf files -> changes IP in [Peer] Endpoint string
// optional - add date to filename
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace WGConfigEndpointIPChanger
{
    class Program
    {
        static void changeEndpointIP(string filePath, string newIp)
        {
            string[] lines = File.ReadAllLines(filePath);
            bool isPeerSection = false;
            bool isModified = false;

            for (int i = 0; i < lines.Length; i++)
            {
                string trimmedLine = lines[i].Trim();

                if (trimmedLine.StartsWith("[", StringComparison.Ordinal) && trimmedLine.EndsWith("]", StringComparison.Ordinal))
                {
                    isPeerSection = trimmedLine.Equals("[Peer]", StringComparison.OrdinalIgnoreCase);
                }

                if (isPeerSection && trimmedLine.StartsWith("Endpoint", StringComparison.OrdinalIgnoreCase))
                {
                    string[] parts = trimmedLine.Split(new[] { '=' }, 2);
                    if (parts.Length == 2)
                    {
                        string endpointValue = parts[1].Trim();
                        int colonIndex = endpointValue.LastIndexOf(':');

                        if (colonIndex != -1)
                        {
                            string port = endpointValue.Substring(colonIndex).Trim();

                            lines[i] = $"Endpoint = {newIp.Trim()}{port}";
                            isModified = true;
                            break;
                        }
                    }
                }
            }

            if (isModified)
            {
                File.WriteAllLines(filePath, lines);
                Console.WriteLine($"   -> file updated successfully");
            }
            else
            {
                Console.WriteLine($"   -> Endpoint IP not found");
            }
        }

        static string GetFilePathWithUpdatedDate(string currentFilePath)
        {
            if (string.IsNullOrWhiteSpace(currentFilePath)) 
                return currentFilePath;

            string dateStamp = DateTime.Now.ToString("ddMMyy"); 
            string directory = Path.GetDirectoryName(currentFilePath) ?? "";
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(currentFilePath);
            string extension = Path.GetExtension(currentFilePath);

            string pattern = @"_\d{6}$";

            if (Regex.IsMatch(fileNameWithoutExt, pattern))
            {
                fileNameWithoutExt = Regex.Replace(fileNameWithoutExt, pattern, $"_{dateStamp}");
            }
            else
            {
                fileNameWithoutExt = $"{fileNameWithoutExt}_{dateStamp}";
            }

            return Path.Combine(directory, $"{fileNameWithoutExt}{extension}");
        }

        static void Main(string[] args)
        {
            string folderPath = "";
            string endpointIP = "";

            // test if args from exe (cmd input)
            if (args.Length >= 2)
            {
                folderPath = args[0];
                endpointIP = args[1];
                Console.WriteLine($"Got folder: {folderPath}, Endpoint IP {endpointIP}");
            }

            else
            {
                Console.WriteLine("Select folder with .conf files: ");
                folderPath = Console.ReadLine();
                Console.WriteLine("Enter endoint Ipv4 (example - 8.8.8.8): ");
                endpointIP = Console.ReadLine();
            }

            // deletes invalid characters
            folderPath = folderPath.Trim('"');
            endpointIP = endpointIP?.Trim();

            if (string.IsNullOrWhiteSpace(folderPath) || string.IsNullOrWhiteSpace(endpointIP))
            {
                Console.WriteLine("\nError: Folder path or IP cannot be empty!");
                Console.ReadKey();
                return;
            }

            try
            {
                // search only .conf files
                string[] files = Directory.GetFiles(folderPath, "*.conf");
                if (files.Length == 0)
                    Console.WriteLine("No files in folder");

                foreach (string file in files)
                {
                    Console.WriteLine($"Changing file: {Path.GetFileName(file)}");
                    changeEndpointIP(file, endpointIP);

                    try
                    {
                        string newFilePath = GetFilePathWithUpdatedDate(file);
                        if (file != newFilePath)
                        {
                            if (File.Exists(newFilePath))
                            {
                                File.Delete(newFilePath);
                            }

                            File.Move(file, newFilePath);
                            Console.WriteLine($"   -> renamed to: {Path.GetFileName(newFilePath)}");
                        }
                    }
                    catch (Exception renameEx)
                    {
                        Console.WriteLine($"   -> [Warning] Failed to rename file: {renameEx.Message}");
                    }
                }

                Console.WriteLine("\nDone! Press any key to exit");
                Console.ReadKey();
            }

            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}