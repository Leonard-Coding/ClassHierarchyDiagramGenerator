using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static ClassHierarchyDiagramGenerator.Utils;

namespace ClassHierarchyDiagramGenerator;

internal static class Program
{
    public static void Main(string[] args)
    {
        // TODO: Hard-coded output for now. Better let the user decide how to call and where to save the output.
        const string outputPath = "output.uxf";
        
        // Wenn die Länge vom eingegebenen Path 0 ist also kein Path eingegeben wurde
        if (args.Length == 0)
        {
            Console.WriteLine("Please provide a path to a directory as a command line argument.");
            return;
            
        }

        int anzahlBlöcke = 0;
        
        // ändert das die Schriftart zu Monospaced? 
        string path = args[0];
        
        // Wenn er nicht(!) existiert der Path dann
        if (!Directory.Exists(path))
        {
            Console.WriteLine($"The directory '{path}' does not exist.");
            return;
        }
        
        //er sucht nach cs files und fügt sie zur liste files hinzu
        string[] files = Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories);

        // wenn die anzahl an files in der liste files 0 ist dann
        if (files.Length == 0)
        {
            Console.WriteLine("No C# files found in the directory.");
            return;
        }

        // 
        List<Class> collectedClasses = SyntaxExtractionFromFiles.ExtractClasses(files).OrderByDescending(c => c.MemberCount).ToList();
        List<Interface> collectedInterfaces = SyntaxExtractionFromFiles.ExtractInterfaces(files).OrderByDescending(i => i.MemberCount).ToList();
        List<Enum> collectedEnums = SyntaxExtractionFromFiles.ExtractEnums(files).OrderByDescending(e => e.MemberCount).ToList();
        
        //
        string fileContent = GenerateDiagramFileContent(collectedClasses);

        //er schreibt in das output document alles rein aber was genau ist catch? output path ist wohin das output dokument muss?
        try
        {
            File.WriteAllText(outputPath, fileContent);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    

    // hier wird dann berechnet wo und wie das in umlet muss?
    private static string GenerateDiagramFileContent(List<Class> sortedDescending)
    {
        StringBuilder s = new StringBuilder();

        s.AppendLine(TextBlocks.FileBeginFormat);

        string bgcolor = "gray";
        int transparency = 0;
        int maximumheight = 0;
        int x = 0;
        int y = 0;
        int itemsInRow = 0;
        int anzahlBlöcke = 0;
        const int maxItemsInRow = -1;

        foreach (Class item in sortedDescending)
        {
            int longestLineCharacterCount = 0;
            int lineCount = 0;
            int strichlineCount = 0;

            int classElementStartIndex = s.Length - 1;

            UpdateToLongest(ref longestLineCharacterCount, item.Name);

            string sanitizedClassName = Sanitize(item.Name);
            s.AppendLine($"*{sanitizedClassName}*");
            lineCount++;

            if (item.Fields.Count > 0)
            {
                int maxFieldTypeLength = item.Fields.Max(f => f.Type.Length);
                
                s.AppendLine("--");
                lineCount++;
                strichlineCount++;
                
                foreach (Field field in item.Fields)
                {
                    string fieldLine = $"  {field.Type.PadRight(maxFieldTypeLength)} {field.Name}";
                    UpdateToLongest(ref longestLineCharacterCount, fieldLine);
                    
                    s.AppendLine(Sanitize(fieldLine));
                    lineCount++;
                }
            }

            if (item.Properties.Count > 0)
            {
                int maxPropertyTypeLength = item.Properties.Max(p => p.Type
                                                                  .Length);
                
                s.AppendLine("--");
                lineCount++;
                strichlineCount++;
                
                foreach (Property prop in item.Properties)
                {
                    var paddedType = prop.Type.PadRight(maxPropertyTypeLength);
                    string propertyLine = $"  {paddedType} {prop.Name}";
                    
                    UpdateToLongest(ref longestLineCharacterCount, propertyLine);
                    
                    s.AppendLine(Sanitize(propertyLine));
                    lineCount++;
                }
            }

            if (item.Events.Count > 0)
            {
                s.AppendLine("--");
                lineCount++;
                strichlineCount++;
            }

            if (item.Events.Count > 0)
            {
                foreach (Event ev in item.Events)
                {
                    string parameterTypes = "";
                    if (ev.ParameterTypes.Count > 0)
                    {
                        parameterTypes = " " + string.Join(", ", ev.ParameterTypes);
                    }

                    string eventLine = $"  {ev.Name}{parameterTypes}!";
                    UpdateToLongest(ref longestLineCharacterCount, eventLine);
                    
                    s.AppendLine(Sanitize(eventLine));
                    lineCount++;
                }
                
            }

            if (item.Methods.Count > 0)
            {
                s.AppendLine("--");
                lineCount++;
                strichlineCount++;
            }

            foreach (Method method in item.Methods)
            {
                string parameters = string.Join(", ", method.Parameters);
                string returnType = method.ReturnType;
                string methodLine = $"  {method.Name}({parameters})";
                if (returnType != "void")
                {
                    methodLine += $"->{returnType}";
                }

                UpdateToLongest(ref longestLineCharacterCount, methodLine);
                
                s.AppendLine(Sanitize(methodLine));
                lineCount++;
            }

            transparency += 100 / sortedDescending.Count;
            s.AppendLine($"bg={bgcolor}");
            s.AppendLine($"transparency={transparency}");
            s.Append(TextBlocks.ClassEnd);
            anzahlBlöcke++;
            
            // now we know how wide the class element should become
            static double CeilToMultiple(double value, double multiple)
            {
                if (multiple == 0)
                    throw new ArgumentException("Multiple darf nicht 0 sein.");

                return Math.Ceiling(value / multiple) * multiple;
            }

            double widthnn = CeilToMultiple(longestLineCharacterCount * 6 + 8, 10);
            int width = (int)widthnn;
            double heightnn = CeilToMultiple((lineCount - strichlineCount) * 10 + strichlineCount * 6 + 14, 10);
            int height = (int)heightnn;
            if (height >= maximumheight)
            {
                maximumheight = height; 
            }
            
            string classHeader = string.Format(TextBlocks.ClassBeginFormat, x, y, width, height);

            itemsInRow++;

            if (itemsInRow == maxItemsInRow)
            {
                x = 0;
                y += 0 + maximumheight;
                maximumheight = 0;
                itemsInRow = 0;
                width = 0;
            }

            s.Insert(classElementStartIndex, classHeader);

            x += width + 0;
        }

        s.AppendLine(TextBlocks.FileEnd);
        Console.WriteLine("Dein Dokument ist fertig mit " + anzahlBlöcke + " Blöcken"); //schöner schreiben
        return s.ToString();
    }

    private static void UpdateToLongest(ref int longestLineCharacterCount, string sanitizedClassName)
    {
        longestLineCharacterCount = sanitizedClassName.Length > longestLineCharacterCount
                                        ? sanitizedClassName.Length
                                        : longestLineCharacterCount;
    }
}
