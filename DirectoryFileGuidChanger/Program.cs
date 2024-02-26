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

    private void mymethod() { }

    static async Task Main(string[] args)
    {
        var program = new Program();

        Console.Write("Enter the Directory Path: ");
        //  
        string? directoryPath = "C:\\Users\\ayman.elhamouss\\OneDrive - AIZOON CONSULTING SRL\\Desktop\\PIMaterial_MS.MSModel\\PIMaterial_MS.MSModel\\Model";

        await Task.WhenAll(
            ReplaceGuidAsync(directoryPath)

        );

        Console.WriteLine("All GUIDs replaced successfully.");
    }

    static async Task ReplaceGuidAsync(string directoryPath)
    {
        try
        {
            string commandFileNAme = Path.Combine(directoryPath, "Command") + "\\Command.ul";
            string typeFileName = Path.Combine(directoryPath, "Type") + "\\Type.ul";
            ReplaceGuid(typeFileName);
            ReplaceGuid(commandFileNAme);

            string commandFileNameNew = Path.Combine(directoryPath, "Command") + "\\Command.ul.new";
            string typeFileNameNew = Path.Combine(directoryPath, "Type") + "\\Type.ul.new";
            CheckAndReplaceGuidRefInTypeFile(typeFileNameNew);
            CheckAndReplaceGuidRefInCommandFile(commandFileNameNew);

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



    static void CheckAndReplaceGuidRefInCommandFile(string filePath)

    {
        List<string> nextLinesAfterGuidRef = ExtractNextLinesAfterGuidRef(filePath).Select(x => x).ToList();
        foreach (string line in nextLinesAfterGuidRef)
        {
            Console.WriteLine(line);
        }
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

                    //handle the command file changing GuidRef here  :)  

                    List<(string ParameterType, string Guid)> parameterTypesAndGuidsAsync = GetParameterTypesAndGuidsAsync();

                    List<string> parameterTypes = parameterTypesAndGuidsAsync.Select(pair => pair.ParameterType).ToList();


                    //foreach(string parameterType in parameterTypes)
                    //{
                    //    Console.WriteLine($"{parameterType}");
                    //}




                    //foreach (var result in matchingResults)
                    //{
                    //    parts[1] = result.Guid;
                    //}
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

    //Function that return the parameter types and guids 
    static List<(string ParameterType, string Guid)> GetParameterTypesAndGuidsAsync()
    {
        string filePath = @"C:\Users\ayman.elhamouss\OneDrive - AIZOON CONSULTING SRL\Desktop\PIMaterial_MS.MSModel\PIMaterial_MS.MSModel\Model\Type\Type.ul.new";
        List<(string ParameterType, string Guid)> parameterTypesAndGuids = new List<(string ParameterType, string Guid)>();

        try
        {
            // Read the content of the file
            string content = File.ReadAllText(filePath);

            // Define regex pattern to match the PARAMETERTYPE
            string pattern = @"\[@Guid\((.*?)\)\]\s*PARAMETERTYPE\s+(\S+)";
            string guidPattern = @"\[@Guid\((.*?)\)\]";

            // Find all matches using the pattern
            MatchCollection matches = Regex.Matches(content, pattern);
            MatchCollection guidMatches = Regex.Matches(content, guidPattern);

            foreach (Match match in matches)
            {
                string matchValue = match.Groups[2].Value;

                // Find the GUID match that precedes the current parameter type match
                Match guidMatch = guidMatches.LastOrDefault(m => m.Index == match.Index);
                if (guidMatch != null)
                {
                    string guidMatchValue = guidMatch.Groups[1].Value;
                    parameterTypesAndGuids.Add((matchValue, guidMatchValue));
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
    //Function that extract the Proprety type from the command file
    static List<string> ExtractNextLinesAfterGuidRef(string filePath)
    {
        List<string> extractedContentList = new List<string>();

        if (File.Exists(filePath))
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath);
                bool foundGuidRef = false;

                foreach (string line in lines)
                {
                    if (foundGuidRef)
                    {
                        Match match = Regex.Match(line, @"\.[^.]+\s*$");
                        if (match.Success)
                        {
                            string extractedString = match.Value.TrimStart('.').Trim();
                            extractedContentList.Add(extractedString);
                        }
                        foundGuidRef = false;
                    }

                    if (line.Contains("[@GuidRef("))
                    {
                        foundGuidRef = true;
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

        return extractedContentList;
    }


    //in this function we check the matching results 
    static List<(string MatchedLine, string Guid)> FindMatchingResults(List<string> list1, List<(string ParameterType, string Guid)> list2)
    {
        List<(string MatchedLine, string Guid)> matchingResults = new List<(string MatchedLine, string Guid)>();


        foreach (string line in list1)
        {

            foreach (var parameterTypeAndGuid in list2)
            {
                if (line.Contains(parameterTypeAndGuid.ParameterType))
                {
                    matchingResults.Add((line, parameterTypeAndGuid.Guid));
                    break;
                }
            }
        }

        return matchingResults;
    }

    static string GetFirstGuidAsync()
    {
        string filePath = @"C:\Users\ayman.elhamouss\OneDrive - AIZOON CONSULTING SRL\Desktop\PIMaterial_MS.MSModel\PIMaterial_MS.MSModel\Model\Type\Type.ul.new";
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