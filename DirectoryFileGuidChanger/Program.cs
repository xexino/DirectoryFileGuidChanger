using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        string directoryPath = "C:\\Users\\ayman.elhamouss\\OneDrive - AIZOON CONSULTING SRL\\Desktop\\PIMaterial_MS.MSModel\\PIMaterial_MS.MSModel\\Model"; // Update this to the path of your directory

        await Task.WhenAll(
            ReplaceGuidAsync(Path.Combine(directoryPath, "Type")),
            ReplaceGuidAsync(Path.Combine(directoryPath, "Command"))
        );

        Console.WriteLine("All GUIDs replaced successfully.");
    }

    static async Task ReplaceGuidAsync(string directoryPath)
    {
        try
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);
            FileInfo[] files = directoryInfo.GetFiles("*.ul", SearchOption.TopDirectoryOnly);

            foreach (FileInfo file in files)
            {
                ReplaceGuid(file.FullName);
                await CheckAndReplaceGuidRefAsync(file.FullName + ".new");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred in directory {directoryPath}: {ex.Message}");
        }
    }

    static void ReplaceGuid(string filePath)
    {
        try
        {
            FileInfo fileInfo = new FileInfo(filePath);

            // Read the content of the file
            string content = File.ReadAllText(filePath);

             
            string pattern = @"@Guid\((.*?)\)";

            // Replace all occurrences of @Guid( followed by a GUID with the new GUID
            MatchEvaluator evaluator = new MatchEvaluator((Match m) => { return "@Guid(" + Guid.NewGuid().ToString() + ")"; });
            string newContent = Regex.Replace(content, pattern, evaluator);

            // Write the modified content back to a new file
            string newFilePath = Path.Combine(fileInfo.Directory.FullName, fileInfo.Name + ".new");
            File.WriteAllText(newFilePath, newContent);

            Console.WriteLine("GUIDs replaced successfully in file: " + filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred while replacing GUIDs in file " + filePath + ": " + ex.Message);
        }
    }

    static async Task CheckAndReplaceGuidRefAsync(string filePath)
    {
        try
        {
            string content = await File.ReadAllTextAsync(filePath);

          
            string pattern = @"@GuidRef\((.*?)\)";

            MatchCollection matches = Regex.Matches(content, pattern);

            foreach (Match match in matches)
            {
                string[] parts = match.Groups[1].Value.Split(',').Select(s => s.Trim()).ToArray();

                
                if (parts.Length != 2)
                    continue;

                // Check if the first parameter is not the desired GUID
                if (parts[0] == "05cc2347-6936-41c2-a184-411d44294525")
                {
                    // Replace the first parameter with the desired GUID
                    parts[0] = "d9c194b4-8ab7-41e1-a0ae-893ecb9ec9e8";

                    // Get the GUID for PARAMETERTYPE PropertyAttributeParameterType from the Type file
                    string parameterTypeGuid = await GetParameterTypeGuidAsync();

                    // Replace the second parameter with the GUID from the Type file
                    if (!string.IsNullOrEmpty(parameterTypeGuid))
                        parts[1] = parameterTypeGuid;
                }

                // Construct the new GuidRef string
                string newGuidRef = "@GuidRef(" + string.Join(", ", parts) + ")";

                // Replace the old GuidRef with the new one in the content
                content = content.Replace(match.Value, newGuidRef);
            }

            // Write the modified content back to the file
            await File.WriteAllTextAsync(filePath, content);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while checking and replacing GuidRef in file {filePath}: {ex.Message}");
        }
    }

    public static async Task<string> GetParameterTypeGuidAsync()
    {
        string filePath = @"C:\Users\ayman.elhamouss\OneDrive - AIZOON CONSULTING SRL\Desktop\PIMaterial_MS.MSModel\PIMaterial_MS.MSModel\Model\Type\Type.ul.new";

        try
        {
            // Read the content of the file
            string content = await File.ReadAllTextAsync(filePath);

            // Define regex pattern to match the GUID of the parameter type
            string pattern = @"\[@Guid\((.*?)\)\]\s*PARAMETERTYPE\s+PropertyAttributeParameterType";

            // Find the first match using the pattern
            Match match = Regex.Match(content, pattern);

            // If a match is found
            if (match.Success)
            {
                // Extract and return the GUID value
                string guid = match.Groups[1].Value;
                return guid;
            }
            else
            {
              
                return null; 
            }
        }
        catch (Exception ex)
        {
            // Handle any exceptions
            Console.WriteLine("An error occurred: " + ex.Message);
            return null; 
        }
    }



    static string GetNewlyGeneratedGuid()
    {
        // Simulate generating a new GUID
        return Guid.NewGuid().ToString();
    }
}
