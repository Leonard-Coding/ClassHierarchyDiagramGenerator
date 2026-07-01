using System.Text;
using static ClassHierarchyDiagramGenerator.Utils;

namespace ClassHierarchyDiagramGenerator;

internal static class Program
{
    private const int ClassSpace = 100;
    private const int LineSpace = 50;
    private const int ItemSpace = 10;

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

        List<Class> collectedClasses = SyntaxExtractionFromFiles.ExtractClasses(files).OrderByDescending(c => c.MemberCount).ToList();
        List<Interface> collectedInterfaces = SyntaxExtractionFromFiles.ExtractInterfaces(files).OrderByDescending(i => i.MemberCount).ToList();
        List<Enum> collectedEnums = SyntaxExtractionFromFiles.ExtractEnums(files).OrderByDescending(e => e.MemberCount).ToList();
        
        string fileContent = GenerateDiagramFileContent(collectedClasses, collectedInterfaces, collectedEnums);
        
        try
        {
            File.WriteAllText(outputPath, fileContent);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    
    private static string GenerateDiagramFileContent(List<Class> classes, List<Interface> interfaces, List<Enum> enums)
    {
        StringBuilder s = new StringBuilder();

        s.AppendLine(TextBlocks.FileBeginFormat);
        
        int maxHeight = 0;
        int x = 0;
        int y = 0;
        int itemsInRow = 0;
        int blockCount = 0;
        int arrowCount = 0;
        const int maxItemsInRow = -1;
        
        foreach (Class item in classes)
        {
            InsertClass(classes, s, item, ref blockCount, ref maxHeight, ref x, ref y, out double width, ref itemsInRow);
            
            if (itemsInRow == maxItemsInRow)
            {
                x = 0;
                y += LineSpace + maxHeight;
                itemsInRow = 0;
                maxHeight = 0;
            }
            else
            {
                x += (int)width + ItemSpace;
            }
        }
        
        y += ClassSpace - LineSpace;
        x = 0;
        if (itemsInRow != 0)
        {
            y += LineSpace + maxHeight;
        }
        maxHeight = 0;
        itemsInRow = 0;
        
        foreach (Interface currentInterface in interfaces)
        {
            InsertInterface(interfaces, s, currentInterface, ref blockCount, ref maxHeight, ref x, ref y, out double width, ref itemsInRow);
            
            if (itemsInRow == maxItemsInRow)
            {
                x = 0;
                y += LineSpace + maxHeight;
                itemsInRow = 0;
                maxHeight = 0;
            }
            else
            {
                x += (int)width + ItemSpace;
            }
        }

        y += ClassSpace - LineSpace;
        x = 0;
        if (itemsInRow != 0)
        {
            y += LineSpace + maxHeight;
        }
        maxHeight = 0;
        itemsInRow = 0;
        
        foreach (Enum currentEnum in enums)
        {
            InsertEnum(enums, s, currentEnum, ref blockCount, ref maxHeight, ref x, ref y, out double width, ref itemsInRow);
            
            if (itemsInRow == maxItemsInRow)
            {
                x = 0;
                y += LineSpace + maxHeight;
                itemsInRow = 0;
                maxHeight = 0;
            }
            else
            {
                x += (int)width + ItemSpace;
            }
        }

        foreach (Class item in classes)
        {
            foreach (var interfaceName in item.Interfaces)
            {
                foreach (var @interface in interfaces)
                {
                    if (interfaceName == @interface.Name)
                    {
                        const string arrowType = "lt=&lt;&lt;-";
                        var classData = item.LayoutData;
                        var interfaceData = @interface.LayoutData;
                        var xClass = classData.X;
                        var widthClass = classData.Width;
                        var yClass = classData.Y;
                        var heightClass = classData.Height;
                        var xArrowClass = xClass + (widthClass / 2);
                        var yArrowClass = yClass + heightClass;
                        var xInterface = interfaceData.X;
                        var widthInterface = interfaceData.Width;
                        var yInterface = interfaceData.Y;
                        var xArrowInterface = xInterface + (widthInterface / 2);
                        var xDifference = -(xArrowClass - xArrowInterface);
                        var yDifference = -(yArrowClass - yInterface);
                        s.AppendLine("  <element>");
                        s.AppendLine("    <id>Relation</id>");
                        s.AppendLine("    <coordinates>");
                        s.AppendLine($"     <x>{xArrowClass}</x>");
                        s.AppendLine($"     <y>{yArrowClass}</y>");
                        s.AppendLine("     <w>0</w>");
                        s.AppendLine("     <h>0</h>");
                        s.AppendLine("    </coordinates>");
                        s.AppendLine($"    <panel_attributes>{arrowType}</panel_attributes>");
                        s.AppendLine($"    <additional_attributes>{xDifference}.0;{yDifference}.0;0.0;0.0</additional_attributes>"); 
                        s.AppendLine("   </element>");
                        arrowCount++;
                    }
                }
            }
        }

        int heightDifference = 0;
        foreach (Class item in classes)
        {
            if (item.BaseClass.Length > 0)
            {
                foreach (var baseClassName in item.BaseClass)
                {
                    foreach (Class @class in classes)
                    {
                        if (baseClassName.ToString() == @class.Name)
                        {    
                            heightDifference += 20;    
                            const string arrowType = "lt=-&gt;&gt;";
                            var classData = item.LayoutData;
                            var interfaceData = @class.LayoutData;
                            var xClass = classData.X;
                            var widthClass = classData.Width;
                            var yClass = classData.Y;
                            var xArrowClass = xClass + (widthClass / 2);
                            var xInterfaceData = interfaceData.X; 
                            var widthInterfaceData = interfaceData.Width;
                            var yInterfaceData = interfaceData.Y;
                            var xArrowInterfaceData = xInterfaceData + (widthInterfaceData / 2);
                            var xDifference = -(xArrowClass - xArrowInterfaceData);
                            var yDifference = -(yClass - yInterfaceData);
                            s.AppendLine("  <element>");
                            s.AppendLine("    <id>Relation</id>");
                            s.AppendLine("    <coordinates>");
                            s.AppendLine($"     <x>{xArrowClass}</x>");
                            s.AppendLine($"     <y>{yClass-heightDifference+yDifference}</y>");
                            s.AppendLine("     <w>0</w>");
                            s.AppendLine("     <h>0</h>");
                            s.AppendLine("    </coordinates>");
                            s.AppendLine($"    <panel_attributes>{arrowType}</panel_attributes>");
                            s.AppendLine($"    <additional_attributes>0.0;{-yDifference+heightDifference};0.0;0.0;{xDifference};0.0;{xDifference};{heightDifference}</additional_attributes>"); 
                            s.AppendLine("   </element>");
                            heightDifference += 10;
                            arrowCount++;
                        }
                    }
                }
            }
        }
        
        int heightDifferenceInterfaces = 0;
        foreach (Interface item in interfaces)
        {
            if (item.Interfaces.Count > 0)
            {
                foreach (var interfaceName in item.Interfaces)
                {
                    foreach (Interface names in interfaces)
                    {
                        if (interfaceName == names.Name)
                        {    
                            heightDifferenceInterfaces -= 20;    
                            const string arrowType = "lt=-&gt;&gt;";
                            var classData = item.LayoutData;
                            var interfaceData = names.LayoutData;
                            var xInterface2 = classData.X;
                            var widthInterface2 = classData.Width;
                            var yInterface2 = classData.Y;
                            var heightInterface2 = classData.Height;
                            var xArrowInterface2 = xInterface2 + (widthInterface2 / 2);
                            var xInterface = interfaceData.X;
                            var widthInterface = interfaceData.Width;
                            var yInterface = interfaceData.Y;
                            var xArrowInterface = xInterface + (widthInterface / 2);
                            var xDifference = -(xArrowInterface2 - xArrowInterface);
                            var yDifference = -(yInterface2 - yInterface);
                            s.AppendLine("  <element>");
                            s.AppendLine("    <id>Relation</id>");
                            s.AppendLine("    <coordinates>");
                            s.AppendLine($"     <x>{xArrowInterface2}</x>");
                            s.AppendLine($"     <y>{yInterface2+heightInterface2-heightDifferenceInterfaces}</y>");
                            s.AppendLine("     <w>0</w>");
                            s.AppendLine("     <h>0</h>");
                            s.AppendLine("    </coordinates>");
                            s.AppendLine($"    <panel_attributes>{arrowType}</panel_attributes>");
                            s.AppendLine($"    <additional_attributes>0.0;{-yDifference+heightDifferenceInterfaces};0.0;0.0;{xDifference};0.0;{xDifference};{heightDifferenceInterfaces}</additional_attributes>");
                            s.AppendLine("   </element>");
                            heightDifferenceInterfaces += 10;
                            arrowCount++;
                        }
                    }
                }
            }
        }
        
        s.AppendLine(TextBlocks.FileEnd);
        Console.WriteLine($"Your document finished generating and includes {blockCount} blocks and {arrowCount} connections.");
        return s.ToString();
    }

    private static void InsertClass(List<Class> classes,
                                   StringBuilder s,
                                   Class currentClass,
                                   ref int blockCount,
                                   ref int maxHeight,
                                   ref int x,
                                   ref int y,
                                   out double width,
                                   ref int itemsInRow)
    {
        int longestLineCharacterCount = 0;
        int lineCount = 0;
        int divideLineCount = 0;
        int classElementStartIndex = s.Length - 1;
        currentClass.LayoutData.X = x;
        currentClass.LayoutData.Y = y;
        
        UpdateToLongest(ref longestLineCharacterCount, currentClass.Name);

        string sanitizedClassName = Sanitize(currentClass.Name);
        s.AppendLine($"*{sanitizedClassName}*");
        lineCount++;

        if (currentClass.Fields.Count > 0)
        {
            int maxFieldTypeLength = currentClass.Fields.Max(f => f.Type.Length);
                
            s.AppendLine("--");
            lineCount++;
            divideLineCount++;
                
            foreach (Field field in currentClass.Fields)
            {
                string fieldLine = $"  {field.Type.PadRight(maxFieldTypeLength)} {field.Name}";
                UpdateToLongest(ref longestLineCharacterCount, fieldLine);
                    
                s.AppendLine(Sanitize(fieldLine));
                lineCount++;
            }
        }

        if (currentClass.Properties.Count > 0)
        {
            int maxPropertyTypeLength = currentClass.Properties.Max(p => p.Type.Length);
                
            s.AppendLine("--");
            lineCount++;
            divideLineCount++;
                
            foreach (Property prop in currentClass.Properties)
            {
                var paddedType = prop.Type.PadRight(maxPropertyTypeLength);
                string propertyLine = $"  {paddedType} {prop.Name}";
                    
                UpdateToLongest(ref longestLineCharacterCount, propertyLine);
                    
                s.AppendLine(Sanitize(propertyLine));
                lineCount++;
            }
        }

        if (currentClass.Events.Count > 0)
        {
            s.AppendLine("--");
            lineCount++;
            divideLineCount++;
        }

        if (currentClass.Events.Count > 0)
        {
            foreach (Event ev in currentClass.Events)
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

        if (currentClass.Methods.Count > 0)
        {
            s.AppendLine("--");
            lineCount++;
            divideLineCount++;
        }

        foreach (Method method in currentClass.Methods)
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

        s.AppendLine($"bg={currentClass.LayoutData.Color}");
        s.AppendLine("transparency=0");
        s.Append(TextBlocks.ClassEnd);
        blockCount++;
        
        static double CeilToMultiple(double value, double multiple)
        {
            if (multiple == 0)
                throw new ArgumentException("Multiple darf nicht 0 sein.");

            return Math.Ceiling(value / multiple) * multiple;
        }

        width = CeilToMultiple(longestLineCharacterCount * 8.5 + 10, 10);
        currentClass.LayoutData.Width = (int)width;
        double height = CeilToMultiple((lineCount - divideLineCount) * 13 + divideLineCount * 8 + 20, 10);
        currentClass.LayoutData.Height = (int)height;
        
        if (height >= maxHeight)
        {
            maxHeight = (int)height; 
        }
            
        string classHeader = string.Format(TextBlocks.ClassBeginFormat, x, y, width, height);

        itemsInRow++;

       

        s.Insert(classElementStartIndex, classHeader);
    }
    
    private static void InsertInterface(List<Interface> interfaces,
                                       StringBuilder s,
                                       Interface currentInterface,
                                       ref int blockCount,
                                       ref int maxHeight,
                                       ref int x,
                                       ref int y,
                                       out double width,
                                       ref int itemsInRow)
    {
        int longestLineCharacterCount = 0;
        int lineCount = 0;
        int divideLineCount = 0;
        currentInterface.LayoutData.X = x;
        currentInterface.LayoutData.Y = y;
        
        int classElementStartIndex = s.Length - 1;

        UpdateToLongest(ref longestLineCharacterCount, currentInterface.Name);

        string sanitizedClassName = Sanitize(currentInterface.Name);
        s.AppendLine($"*{sanitizedClassName}*");
        lineCount++;

        if (currentInterface.Fields.Count > 0)
        {
            int maxFieldTypeLength = currentInterface.Fields.Max(f => f.Type.Length);
                
            s.AppendLine("--");
            lineCount++;
            divideLineCount++;
                
            foreach (Field field in currentInterface.Fields)
            {
                string fieldLine = $"  {field.Type.PadRight(maxFieldTypeLength)} {field.Name}";
                UpdateToLongest(ref longestLineCharacterCount, fieldLine);
                    
                s.AppendLine(Sanitize(fieldLine));
                lineCount++;
            }
        }

        if (currentInterface.Properties.Count > 0)
        {
            int maxPropertyTypeLength = currentInterface.Properties.Max(p => p.Type
                                                                  .Length);
                
            s.AppendLine("--");
            lineCount++;
            divideLineCount++;
                
            foreach (Property prop in currentInterface.Properties)
            {
                var paddedType = prop.Type.PadRight(maxPropertyTypeLength);
                string propertyLine = $"  {paddedType} {prop.Name}";
                    
                UpdateToLongest(ref longestLineCharacterCount, propertyLine);
                    
                s.AppendLine(Sanitize(propertyLine));
                lineCount++;
            }
        }

        if (currentInterface.Events.Count > 0)
        {
            s.AppendLine("--");
            lineCount++;
            divideLineCount++;
        }

        if (currentInterface.Events.Count > 0)
        {
            foreach (Event ev in currentInterface.Events)
            {
                string parameterTypes = "";
                if (ev.ParameterTypes.Count > 0)
                {
                    parameterTypes = " " + string.Join(", ", ev.ParameterTypes);
                }

                string eventLine = $"  {ev.Name}i{parameterTypes}!";
                UpdateToLongest(ref longestLineCharacterCount, eventLine);
                    
                s.AppendLine(Sanitize(eventLine));
                lineCount++;
            }
                
        }

        if (currentInterface.Methods.Count > 0)
        {
            s.AppendLine("--");
            lineCount++;
            divideLineCount++;
        }

        foreach (Method method in currentInterface.Methods)
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
        
        
        s.AppendLine($"bg={currentInterface.LayoutData.Color}");
        s.AppendLine($"transparency=0");
        s.Append(TextBlocks.ClassEnd);
        blockCount++;
        
        static double CeilToMultiple(double value, double multiple)
        {
            if (multiple == 0)
                throw new ArgumentException("Multiple darf nicht 0 sein.");

            return Math.Ceiling(value / multiple) * multiple;
        }

        width = CeilToMultiple(longestLineCharacterCount * 8.5 + 10, 10);
        currentInterface.LayoutData.Width = (int)width;
        
        double height = CeilToMultiple((lineCount - divideLineCount) * 13 + divideLineCount * 8 + 20, 10);
        currentInterface.LayoutData.Height = (int)height;
        
        if (height >= maxHeight)
        {
            maxHeight = (int)height; 
        }
            
        string classHeader = string.Format(TextBlocks.ClassBeginFormat, x, y, width, height);

        s.Insert(classElementStartIndex, classHeader);
        
        itemsInRow++;
    }
    
    private static void InsertEnum(List<Enum> enums,
                                   StringBuilder s,
                                   Enum currentEnum,
                                   ref int blockCount,
                                   ref int maxHeight,
                                   ref int x,
                                   ref int y,
                                   out double width,
                                   ref int itemsInRow)
    {
        int longestLineCharacterCount = 0;
        int lineCount = 0;
        int divideLineCount = 0;
        currentEnum.LayoutData.X = x;
        currentEnum.LayoutData.Y = y;
        
        int classElementStartIndex = s.Length - 1;

        UpdateToLongest(ref longestLineCharacterCount, currentEnum.Name);
        
        string sanitizedClassName = Sanitize(currentEnum.Name);
        s.AppendLine($"*{sanitizedClassName}*");
        lineCount++;

        if (currentEnum.Members.Count > 0)
        {
            s.AppendLine("--");
            lineCount++;
            divideLineCount++;
                
            foreach (string members in currentEnum.Members)
            {
                string membersLine = $"  {members}";
                UpdateToLongest(ref longestLineCharacterCount, membersLine);
                s.AppendLine(membersLine);
                lineCount++;
            }
        }
        
        
        s.AppendLine($"bg={currentEnum.LayoutData.Color}");
        s.AppendLine("transparency=0");
        s.Append(TextBlocks.ClassEnd);
        blockCount++;
        
        static double CeilToMultiple(double value, double multiple)
        {
            if (multiple == 0)
                throw new ArgumentException("Multiple darf nicht 0 sein.");

            return Math.Ceiling(value / multiple) * multiple;
        }

        width = CeilToMultiple(longestLineCharacterCount * 8.5 + 10, 10);
        currentEnum.LayoutData.Width = (int)width;
        
        double height = CeilToMultiple((lineCount - divideLineCount) * 13 + divideLineCount * 8 + 20, 10);
        currentEnum.LayoutData.Height = (int)height;
        
        if (height >= maxHeight)
        {
            maxHeight = (int)height; 
        }
            
        string classHeader = string.Format(TextBlocks.ClassBeginFormat, x, y, width, height);

        itemsInRow++;
        
        s.Insert(classElementStartIndex, classHeader);
    }
    private static void UpdateToLongest(ref int longestLineCharacterCount, string sanitizedClassName)
    {
        longestLineCharacterCount = sanitizedClassName.Length > longestLineCharacterCount
                                        ? sanitizedClassName.Length
                                        : longestLineCharacterCount;
    }
}
