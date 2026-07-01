namespace ClassHierarchyDiagramGenerator;

internal static class Program
{
    public static void Main(string[] args)
    {
        const string outputPath = "output.uxf";

        if (args.Length == 0)
        {
            Console.WriteLine("Please provide a path to a directory as a command line argument.");
            return;
        }

        string path = args[0];

        if (!Directory.Exists(path))
        {
            Console.WriteLine($"The directory '{path}' does not exist.");
            return;
        }

        string[] files = Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories);

        if (files.Length == 0)
        {
            Console.WriteLine("No C# files found in the directory.");
            return;
        }

        List<Class> collectedClasses = SyntaxExtractionFromFiles.ExtractClasses(files)
                                                                .OrderByDescending(c => c.MemberCount)
                                                                .ToList();
        
        List<Interface> collectedInterfaces = SyntaxExtractionFromFiles.ExtractInterfaces(files)
                                                                       .OrderByDescending(i => i.MemberCount)
                                                                       .ToList();
        
        List<Enum> collectedEnums = SyntaxExtractionFromFiles.ExtractEnums(files)
                                                             .OrderByDescending(e => e.MemberCount)
                                                             .ToList();

        var  result = DiagramGeneration.GenerateDiagramFileContent(collectedClasses, collectedInterfaces, collectedEnums);
        
        
        var fileContent = result.DiagramFileContent;

        try
        {
            File.WriteAllText(outputPath, fileContent);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        
        Console.WriteLine($"Your document finished generating and includes {result.BlockCount} blocks and {result.ArrowCount} connections.");
    }
}
