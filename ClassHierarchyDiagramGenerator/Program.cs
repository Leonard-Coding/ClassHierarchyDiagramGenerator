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
        string fileContent = GenerateDiagramFileContent(collectedClasses, collectedInterfaces, collectedEnums);

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
    private static string GenerateDiagramFileContent(List<Class> classes, List<Interface> interfaces, List<Enum> enums)
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

        foreach (Class item in classes)
        {
            transparency = InsertClass(classes, s, item, transparency, bgcolor, maxItemsInRow, ref anzahlBlöcke, ref maximumheight, ref x, ref y, ref itemsInRow);
        }

        x = 0;
        y = maximumheight + 50;
        maximumheight = 0;
        transparency = 0;
        
        foreach (Interface item in interfaces)
        {
            transparency = InsertClass(interfaces, s, item, transparency, bgcolor, maxItemsInRow, ref anzahlBlöcke, ref maximumheight, ref x, ref y, ref itemsInRow);
        }
        
        x = 0;
        y = maximumheight + 50;
        maximumheight = 0;
        transparency = 0;
        
        /*foreach (Enum item in enums)
        {
            transparency = InsertClass(enums, s, item, transparency, bgcolor, maxItemsInRow, ref anzahlBlöcke, ref maximumheight, ref x, ref y, ref itemsInRow);
        }
        */
        s.AppendLine(TextBlocks.FileEnd);
        Console.WriteLine("Dein Dokument ist fertig mit " + anzahlBlöcke + " Blöcken"); //schöner schreiben
        return s.ToString();
    }

    private static int InsertClass(List<Class> classes,
                                   StringBuilder s,
                                   Class item,
                                   int transparency,
                                   string bgcolor,
                                   int maxItemsInRow,
                                   ref int anzahlBlöcke,
                                   ref int maximumheight,
                                   ref int x,
                                   ref int y,
                                   ref int itemsInRow)
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
        
        s.AppendLine($"bg={bgcolor}");
        s.AppendLine($"transparency={transparency}");
        var stepsize = 100 / (classes.Count - 1);
        transparency = transparency + stepsize;
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
        return transparency;
    }
    
    private static int InsertClass(List<Interface> interfaces,
                                   StringBuilder s,
                                   Interface item,
                                   int itransparency,
                                   string ibgcolor,
                                   int imaxItemsInRow,
                                   ref int ianzahlBlöcke,
                                   ref int imaximumheight,
                                   ref int ix,
                                   ref int iy,
                                   ref int iitemsInRow)
    {
        int ilongestLineCharacterCount = 0;
        int ilineCount = 0;
        int istrichlineCount = 0;
        ibgcolor = "red";

        int iclassElementStartIndex = s.Length - 1;

        UpdateToLongest(ref ilongestLineCharacterCount, item.Name);

        string isanitizedClassName = Sanitize(item.Name);
        s.AppendLine($"*{isanitizedClassName}*");
        ilineCount++;

        if (item.Fields.Count > 0)
        {
            int imaxFieldTypeLength = item.Fields.Max(f => f.Type.Length);
                
            s.AppendLine("--");
            ilineCount++;
            istrichlineCount++;
                
            foreach (Field field in item.Fields)
            {
                string ifieldLine = $"  {field.Type.PadRight(imaxFieldTypeLength)} {field.Name}";
                UpdateToLongest(ref ilongestLineCharacterCount, ifieldLine);
                    
                s.AppendLine(Sanitize(ifieldLine));
                ilineCount++;
            }
        }

        if (item.Properties.Count > 0)
        {
            int imaxPropertyTypeLength = item.Properties.Max(p => p.Type
                                                                  .Length);
                
            s.AppendLine("--");
            ilineCount++;
            istrichlineCount++;
                
            foreach (Property prop in item.Properties)
            {
                var ipaddedType = prop.Type.PadRight(imaxPropertyTypeLength);
                string ipropertyLine = $"  {ipaddedType} {prop.Name}";
                    
                UpdateToLongest(ref ilongestLineCharacterCount, ipropertyLine);
                    
                s.AppendLine(Sanitize(ipropertyLine));
                ilineCount++;
            }
        }

        if (item.Events.Count > 0)
        {
            s.AppendLine("--");
            ilineCount++;
            istrichlineCount++;
        }

        if (item.Events.Count > 0)
        {
            foreach (Event ev in item.Events)
            {
                string iparameterTypes = "";
                if (ev.ParameterTypes.Count > 0)
                {
                    iparameterTypes = " " + string.Join(", ", ev.ParameterTypes);
                }

                string ieventLine = $"  {ev.Name}i{iparameterTypes}!";
                UpdateToLongest(ref ilongestLineCharacterCount, ieventLine);
                    
                s.AppendLine(Sanitize(ieventLine));
                ilineCount++;
            }
                
        }

        if (item.Methods.Count > 0)
        {
            s.AppendLine("--");
            ilineCount++;
            istrichlineCount++;
        }

        foreach (Method method in item.Methods)
        {
            string iparameters = string.Join(", ", method.Parameters);
            string ireturnType = method.ReturnType;
            string imethodLine = $"  {method.Name}({iparameters})";
            if (ireturnType != "void")
            {
                imethodLine += $"->{ireturnType}";
            }

            UpdateToLongest(ref ilongestLineCharacterCount, imethodLine);
                
            s.AppendLine(Sanitize(imethodLine));
            ilineCount++;
        }
        
        
        s.AppendLine($"bg={ibgcolor}");
        s.AppendLine($"transparency={itransparency}");
        var stepSize = 100 / (interfaces.Count - 1);
        itransparency += stepSize;
        s.Append(TextBlocks.ClassEnd);
        ianzahlBlöcke++;
            
        // now we know how wide the class element should become
        static double CeilToMultiple(double value, double multiple)
        {
            if (multiple == 0)
                throw new ArgumentException("Multiple darf nicht 0 sein.");

            return Math.Ceiling(value / multiple) * multiple;
        }

        double widthnn = CeilToMultiple(ilongestLineCharacterCount * 6 + 8, 10);
        int iwidth = (int)widthnn;
        double heightnn = CeilToMultiple((ilineCount - istrichlineCount) * 10 + istrichlineCount * 6 + 14, 10);
        int iheight = (int)heightnn;
        if (iheight >= imaximumheight)
        {
            imaximumheight = iheight; 
        }
            
        string iclassHeader = string.Format(TextBlocks.ClassBeginFormat, ix, iy, iwidth, iheight);

        iitemsInRow++;

        if (iitemsInRow == imaxItemsInRow)
        {
            ix = 0;
            iy += 0 + imaximumheight;
            imaximumheight = 0;
            iitemsInRow = 0;
            iwidth = 0;
        }

        s.Insert(iclassElementStartIndex, iclassHeader);

        ix += iwidth + 0;
        return itransparency;
    }
    
    private static int InsertClass(List<Enum> enums,
                                   StringBuilder s,
                                   Enum item,
                                   int etransparency,
                                   string ebgcolor,
                                   int emaxItemsInRow,
                                   ref int eanzahlBlöcke,
                                   ref int emaximumheight,
                                   ref int ex,
                                   ref int ey,
                                   ref int eitemsInRow)
    {
        int elongestLineCharacterCount = 0;
        int elineCount = 0;
        int estrichlineCount = 0;

        int eclassElementStartIndex = s.Length - 1;

        UpdateToLongest(ref elongestLineCharacterCount, item.Name);

        string esanitizedClassName = Sanitize(item.Name);
        s.AppendLine($"*{esanitizedClassName}*");
        elineCount++;

        if (item.Fields.Count > 0)
        {
            int emaxFieldTypeLength = item.Fields.Max(f => f.Type.Length);
                
            s.AppendLine("--");
            elineCount++;
            estrichlineCount++;
                
            foreach (Field field in item.Fields)
            {
                string efieldLine = $"  {field.Type.PadRight(emaxFieldTypeLength)} {field.Name}";
                UpdateToLongest(ref elongestLineCharacterCount, efieldLine);
                    
                s.AppendLine(Sanitize(efieldLine));
                elineCount++;
            }
        }

        if (item.Properties.Count > 0)
        {
            int emaxPropertyTypeLength = item.Properties.Max(p => p.Type
                                                                  .Length);
                
            s.AppendLine("--");
            elineCount++;
            estrichlineCount++;
                
            foreach (Property prop in item.Properties)
            {
                var epaddedType = prop.Type.PadRight(emaxPropertyTypeLength);
                string epropertyLine = $"  {epaddedType} {prop.Name}";
                    
                UpdateToLongest(ref elongestLineCharacterCount, epropertyLine);
                    
                s.AppendLine(Sanitize(epropertyLine));
                elineCount++;
            }
        }

        if (item.Events.Count > 0)
        {
            s.AppendLine("--");
            elineCount++;
            estrichlineCount++;
        }

        if (item.Events.Count > 0)
        {
            foreach (Event ev in item.Events)
            {
                string eparameterTypes = "";
                if (ev.ParameterTypes.Count > 0)
                {
                    eparameterTypes = " " + string.Join(", ", ev.ParameterTypes);
                }

                string eeventLine = $"  {ev.Name}{eparameterTypes}!";
                UpdateToLongest(ref elongestLineCharacterCount, eeventLine);
                    
                s.AppendLine(Sanitize(eeventLine));
                elineCount++;
            }
                
        }

        if (item.Methods.Count > 0)
        {
            s.AppendLine("--");
            elineCount++;
            estrichlineCount++;
        }

        foreach (Method method in item.Methods)
        {
            string eparameters = string.Join(", ", method.Parameters);
            string ereturnType = method.ReturnType;
            string emethodLine = $"  {method.Name}({eparameters})";
            if (ereturnType != "void")
            {
                emethodLine += $"->{ereturnType}";
            }

            UpdateToLongest(ref elongestLineCharacterCount, emethodLine);
                
            s.AppendLine(Sanitize(emethodLine));
            elineCount++;
        }

        etransparency += 100 / enums.Count;
        s.AppendLine($"bg={ebgcolor}");
        s.AppendLine($"transparency={etransparency}");
        s.Append(TextBlocks.ClassEnd);
        eanzahlBlöcke++;
            
        // now we know how wide the class element should become
        static double CeilToMultiple(double value, double multiple)
        {
            if (multiple == 0)
                throw new ArgumentException("Multiple darf nicht 0 sein.");

            return Math.Ceiling(value / multiple) * multiple;
        }

        double ewidthnn = CeilToMultiple(elongestLineCharacterCount * 6 + 8, 10);
        int ewidth = (int)ewidthnn;
        double eheightnn = CeilToMultiple((elineCount - estrichlineCount) * 10 + estrichlineCount * 6 + 14, 10);
        int eheight = (int)eheightnn;
        if (eheight >= emaximumheight)
        {
            emaximumheight = eheight; 
        }
            
        string classHeader = string.Format(TextBlocks.ClassBeginFormat, ex, ey, ewidth, eheight);

        eitemsInRow++;

        if (eitemsInRow == emaxItemsInRow)
        {
            ex = 0;
            ey += 0 + emaximumheight;
            emaximumheight = 0;
            eitemsInRow = 0;
            ewidth = 0;
        }

        s.Insert(eclassElementStartIndex, classHeader);

        ex += ewidth + 0;
        return etransparency;
    }
    private static void UpdateToLongest(ref int longestLineCharacterCount, string sanitizedClassName)
    {
        longestLineCharacterCount = sanitizedClassName.Length > longestLineCharacterCount
                                        ? sanitizedClassName.Length
                                        : longestLineCharacterCount;
    }
}
