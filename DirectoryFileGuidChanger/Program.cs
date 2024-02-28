using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

class Program
{

    enum ModelFile { Type, Command }



    static async Task Main(string[] args)
    {
        var program = new Program();

        Console.Write("Enter the Directory Path: ");

        string? directoryPath = @"C:\\Users\\ayman.elhamouss\\OneDrive - AIZOON CONSULTING SRL\\Desktop\\PIMaterial_MS.MSModel\\Model";




        ReplaceGuidAsync(directoryPath);
        Console.WriteLine("All GUIDs replaced successfully.");

    }

    static void ReplaceGuidAsync(string directoryPath)
    {
        Dictionary<string, string> extractedContentDict = ExtractNextLinesAfterGuidRef();
        List<(string GuidRef, string ExtractedString)> extractedContentList = extractedContentDict.Select(kv => (kv.Key, kv.Value)).ToList();
        Dictionary<string, string> parameterTypesAndGuids = GetParameterTypesAndGuids();



        try
        {
            string commandFileNAme = Path.Combine(directoryPath, "Command") + "\\Command.ul";
            string typeFileName = Path.Combine(directoryPath, "Type") + "\\Type.ul";



            bool typeFileReplaced = false;
            if (!File.Exists(typeFileName + ".new"))
            {
                ReplaceGuid(typeFileName);
                typeFileReplaced = true;
            }
            ReplaceGuid(commandFileNAme);



            string commandFileNameNew = Path.Combine(directoryPath, "Command") + "\\Command.ul.new";
            string typeFileNameNew = Path.Combine(directoryPath, "Type") + "\\Type.ul.new";
            if (typeFileReplaced)
            {
                CheckAndReplaceGuidRefInTypeFile(typeFileNameNew);
            }

            UpdateGuidRefAndWriteToCommandFile(commandFileNameNew, extractedContentList, parameterTypesAndGuids);




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

            string content = File.ReadAllText(filePath);


            string pattern = @"@Guid\((.*?)\)";

            MatchEvaluator evaluator = new MatchEvaluator((Match m) => { return "@Guid(" + Guid.NewGuid().ToString() + ")"; });
            string newContent = Regex.Replace(content, pattern, evaluator);


            string newFilePath = Path.Combine(fileInfo.Directory.FullName, fileInfo.Name + ".new");
            File.WriteAllText(newFilePath, newContent);

            Console.WriteLine("GUIDs replaced successfully in file: " + filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred while replacing GUIDs in file " + filePath + ": " + ex.Message);
        }
    }


    static void CheckAndReplaceGuidRefInTypeFile(string filePath)
    {
        try
        {
            string content = File.ReadAllText(filePath);

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

                    // Get the first GUID from the Type file
                    string firstGuid = GetFirstGuidAsync();

                    // Replace the second parameter with the first GUID from the Type file
                    if (firstGuid != null)
                        parts[1] = firstGuid;
                }

                // Construct the new GuidRef string
                string newGuidRef = "@GuidRef(" + string.Join(", ", parts) + ")";

                // Replace the old GuidRef with the new one in the content
                content = content.Replace(match.Value, newGuidRef);
            }

            // Write the modified content back to the file
            File.WriteAllText(filePath, content);

        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while checking and replacing GuidRef in file {filePath}: {ex.Message}");
        }
    }








    static void UpdateGuidRefAndWriteToCommandFile(string filePath, List<(string GuidRef, string ExtractedString)> extractedContentList,
        Dictionary<string, string> parameterTypesAndGuids)
    {
        try
        {
            string content = File.ReadAllText(filePath);

            string pattern = @"\[@GuidRef\((05cc2347-6936-41c2-a184-411d44294525,\s*.*?)\)\]";

            MatchCollection matches = Regex.Matches(content, pattern);

            string updatedContent = content; // Initialize with the original content

            foreach (Match match in matches)
            {
                for (int i = 0; i < extractedContentList.Count; i++)
                {
                    var item = extractedContentList[i];
                    string extractedString = item.ExtractedString;

                    if (parameterTypesAndGuids.ContainsKey(extractedString))
                    {
                        string[] parts = item.GuidRef.Split(", ");

                        if (parts[0] == "05cc2347-6936-41c2-a184-411d44294525")
                        {
                            parts[0] = "d9c194b4-8ab7-41e1-a0ae-893ecb9ec9e8";

                            string newGuid = parameterTypesAndGuids[extractedString];

                            string updatedGuidRef = $"{parts[0]}, {newGuid}";

                            Console.WriteLine(updatedGuidRef);
                            updatedContent = updatedContent.Replace(match.Value, $"[@GuidRef({updatedGuidRef})]");

                            // Update the item in extractedContentList
                            extractedContentList[i] = (updatedGuidRef, extractedString);
                            // Break out of the loop after updating the item
                            break;
                        }
                    }
                }
            }

            // Write the updated content back to the file
            File.WriteAllText(filePath, updatedContent);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while checking and replacing GuidRef in file {filePath}: {ex.Message}");
        }
    }



    static Dictionary<string, string> ExtractNextLinesAfterGuidRef()
    {
        string filePath = @"C:\Users\ayman.elhamouss\OneDrive - AIZOON CONSULTING SRL\Desktop\PIMaterial_MS.MSModel\Model\Command\Command.ul";

        Dictionary<string, string> extractedContentDict = new Dictionary<string, string>();

        if (File.Exists(filePath))
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath);
                string currentGuidRef = null;

                foreach (string line in lines)
                {
                    if (currentGuidRef != null)
                    {
                        Match match = Regex.Match(line, @"\.[^.]+\s*$");
                        if (match.Success)
                        {
                            string extractedString = match.Value.TrimStart('.').Trim();
                            // Only add to extractedContentDict if it matches the desired values
                            if (extractedString == "PropertyAttributeParameterType" || extractedString == "MaterialPropertyExtendedType")
                            {
                                extractedContentDict[currentGuidRef] = extractedString;
                            }
                            currentGuidRef = null;
                        }
                    }

                    if (line.Contains("[@GuidRef("))
                    {
                        Match guidMatch = Regex.Match(line, @" \[@GuidRef\(([^,]+),\s*([^)]+)\)\]");

                        if (guidMatch.Success)
                        {
                            string guidRef = guidMatch.Groups[1].Value + ", " + guidMatch.Groups[2].Value;
                            currentGuidRef = guidRef;
                        }
                    }
                }
            }
            catch (IOException e)
            {
                Console.WriteLine($"An error occurred while reading the file: {e.Message}");
            }
        }
        else
        {
            Console.WriteLine("The file does not exist.");
        }

        return extractedContentDict;
    }


    static Dictionary<string, string> GetParameterTypesAndGuids()
    {
        string filePath = @"C:\Users\ayman.elhamouss\OneDrive - AIZOON CONSULTING SRL\Desktop\PIMaterial_MS.MSModel\Model\Type\Type.ul.new";
        Dictionary<string, string> parameterTypesAndGuids = new Dictionary<string, string>();

        try
        {
            // Read the content of the file
            string content = File.ReadAllText(filePath);

            // Define regex pattern to match the PARAMETERTYPE
            string pattern = @"\[@Guid\((.*?)\)\]\s*PARAMETERTYPE\s+(\S+)";

            // Find all matches using the pattern
            MatchCollection matches = Regex.Matches(content, pattern);
            Console.WriteLine(content);


            foreach (Match match in matches)
            {

                string matchValue = match.Groups[2].Value;
                string guidPattern = @"\[@Guid\((.*?)\)\]\s*PARAMETERTYPE\s+" + matchValue;
                MatchCollection guidMatches = Regex.Matches(content, guidPattern);
                // Find the GUID match that precedes the current parameter type match
                Match guidMatch = guidMatches.LastOrDefault(m => m.Index == match.Index);

                foreach (var guid in guidMatches)
                {
                    Console.WriteLine(guid);
                }
                if (guidMatch != null)
                {
                    string guidMatchValue = guidMatch.Groups[1].Value;
                    parameterTypesAndGuids[matchValue] = guidMatchValue;
                }
            }
        }
        catch (Exception ex)
        {
            // Handle any exceptions
            Console.WriteLine("An error occurred: " + ex.Message);
        }

        return parameterTypesAndGuids;
    }


    static string GetFirstGuidAsync()
    {
        string filePath = @"C:\Users\ayman.elhamouss\OneDrive - AIZOON CONSULTING SRL\Desktop\PIMaterial_MS.MSModel\Model\Type\Type.ul.new";
        try
        {
            string content = File.ReadAllText(filePath);
            string pattern = @"\[@Guid\((.*?)\)\]";
            Match match = Regex.Match(content, pattern);

            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            return null; // Return null if no matching GUID found
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
            return null;
        }
    }

    
}