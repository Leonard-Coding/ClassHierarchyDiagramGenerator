using System.Text;
using static ClassHierarchyDiagramGenerator.Utils;

namespace ClassHierarchyDiagramGenerator;

internal static class DiagramGeneration
{
    private const int ClassSpace = 100;
    private const int LineSpace = 50;
    private const int ItemSpace = 10;
    private const int MaxItemsInRow = -1; // indicates unlimited block counts per row
    private const bool ClassInterfaceArrows = true; //if true arrows are activated and generated
    private const bool BaseClassArrows = true;
    private const bool InterfaceArrows = true;
    private const bool AddPathBlock = true; //ich komme aktuell nicht an den Path dran, weil ich die variable path hier nicht verwenden kann
    private const bool MoveInterfacesToRelatedClasses = true;

    public static GenerationResult GenerateDiagramFileContent(List<Class> classes, List<Interface> interfaces, List<Enum> enums)
    {
        StringBuilder stringBuilder = new StringBuilder();

        stringBuilder.AppendLine(TextBlocks.FileBeginFormat);

        int maxHeight = 0;
        int currentX = 0;
        int currentY = 0;
        int itemsInRow = 0;
        int blockCount = 0;
        int arrowCount = 0;

        foreach (Class item in classes)
        {
            InsertClass(stringBuilder, item, ref blockCount, ref maxHeight, ref itemsInRow, currentX, ref currentY, out double width);
            InsertBlockLineBreakOrMoveHorizontally(width, ref currentX, ref currentY, ref maxHeight, ref itemsInRow);
        }

        AddBlockLineBreakAfterBlockType(ref currentY, ref itemsInRow, ref maxHeight);
        currentX = 0;
        
        foreach (Interface item in interfaces)
        {
            InsertInterface(stringBuilder, item, ref blockCount, ref maxHeight, ref itemsInRow, ref currentX, ref currentY, MoveInterfacesToRelatedClasses, classes, interfaces, out double width);
            InsertBlockLineBreakOrMoveHorizontally(width, ref currentX, ref currentY, ref maxHeight, ref itemsInRow);
        }

        int interfaceMaxHeight = 0;
        foreach (var interfaceHeight in interfaces)
        {
            if (interfaceHeight.LayoutData.Height > interfaceMaxHeight)
            {
                interfaceMaxHeight = interfaceHeight.LayoutData.Height;
            }
        }
        AddBlockLineBreakAfterBlockType(ref currentY, ref itemsInRow, ref interfaceMaxHeight);
        currentX = 0;
            
        maxHeight = 0;
        
        foreach (Enum item in enums)
        {
            InsertEnum(stringBuilder, item, ref blockCount, ref maxHeight, ref itemsInRow, ref currentX, ref currentY, out double width);
            InsertBlockLineBreakOrMoveHorizontally(width, ref currentX, ref currentY, ref maxHeight, ref itemsInRow);
        }

        if (ClassInterfaceArrows)
        {    
            foreach (Class item in classes)
            {
                foreach (var interfaceName in item.Interfaces)
                {
                    foreach (var @interface in interfaces)
                    {
                        if (interfaceName == @interface.Name)
                        {
                            const string arrowType = "lt=&lt;&lt;-";
                            const string layer = "layer=0";
                            var classData = item.LayoutData;
                            var interfaceData = @interface.LayoutData;
                            var xClass = classData.X;
                            var widthClass = classData.Width;
                            var yClass = classData.Y;
                            var heightClass = classData.Height;
                            var xArrowClass = xClass + widthClass / 2;
                            var yArrowClass = yClass + heightClass;
                            var xInterface = interfaceData.X;
                            var widthInterface = interfaceData.Width;
                            var yInterface = interfaceData.Y;
                            var xArrowInterface = xInterface + widthInterface / 2;
                            var xDifference = -(xArrowClass - xArrowInterface);
                            var yDifference = -(yArrowClass - yInterface);
                            stringBuilder.AppendLine("  <element>");
                            stringBuilder.AppendLine("    <id>Relation</id>");
                            stringBuilder.AppendLine("    <coordinates>");
                            stringBuilder.AppendLine($"     <x>{xArrowClass}</x>");
                            stringBuilder.AppendLine($"     <y>{yArrowClass}</y>");
                            stringBuilder.AppendLine("     <w>0</w>");
                            stringBuilder.AppendLine("     <h>0</h>");
                            stringBuilder.AppendLine("    </coordinates>");
                            stringBuilder.AppendLine($"    <panel_attributes>{arrowType}");
                            stringBuilder.AppendLine($"{layer}</panel_attributes>");
                            stringBuilder.AppendLine($"    <additional_attributes>{xDifference}.0;{yDifference}.0;0.0;0.0</additional_attributes>");
                            stringBuilder.AppendLine("   </element>");
                            arrowCount++;
                        }
                    }
                }
            }
        }
    
        if (BaseClassArrows)
        {
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
                                const string layer = "layer=0";
                                var classData = item.LayoutData;
                                var interfaceData = @class.LayoutData;
                                var xClass = classData.X;
                                var widthClass = classData.Width;
                                var yClass = classData.Y;
                                var xArrowClass = xClass + widthClass / 2;
                                var xInterfaceData = interfaceData.X;
                                var widthInterfaceData = interfaceData.Width;
                                var yInterfaceData = interfaceData.Y;
                                var xArrowInterfaceData = xInterfaceData + widthInterfaceData / 2;
                                var xDifference = -(xArrowClass - xArrowInterfaceData);
                                var yDifference = -(yClass - yInterfaceData);
                                stringBuilder.AppendLine("  <element>");
                                stringBuilder.AppendLine("    <id>Relation</id>");
                                stringBuilder.AppendLine("    <coordinates>");
                                stringBuilder.AppendLine($"     <x>{xArrowClass}</x>");
                                stringBuilder.AppendLine($"     <y>{yClass - heightDifference + yDifference}</y>");
                                stringBuilder.AppendLine("     <w>0</w>");
                                stringBuilder.AppendLine("     <h>0</h>");
                                stringBuilder.AppendLine("    </coordinates>");
                                stringBuilder.AppendLine($"    <panel_attributes>{arrowType}");
                                stringBuilder.AppendLine($"{layer}</panel_attributes>");
                                stringBuilder
                                   .AppendLine($"    <additional_attributes>0.0;{-yDifference + heightDifference};0.0;0.0;{xDifference};0.0;{xDifference};{heightDifference}</additional_attributes>");
                                stringBuilder.AppendLine("   </element>");
                                heightDifference += 10;
                                arrowCount++;
                            }
                        }
                    }
                }
            }
        }

        if (InterfaceArrows)
        {
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
                                const string arrowType = "lt=-&gt;&gt;";
                                const string layer = "layer=0";
                                var classData = item.LayoutData;
                                var interfaceData = names.LayoutData;
                                var xInterface2 = classData.X;
                                var widthInterface2 = classData.Width;
                                var heightInterface2 = classData.Height;
                                var yInterface2 = classData.Y;
                                var xArrowInterface2 = xInterface2 + widthInterface2 / 2;
                                var xInterface = interfaceData.X;
                                var widthInterface = interfaceData.Width;
                                var heightInterface = interfaceData.Height;
                                var yInterface = interfaceData.Y;
                                var xArrowInterface = xInterface + widthInterface / 2;
                                var xDifference = -(xArrowInterface2 - xArrowInterface);
                                var yDifference = -(yInterface2 - yInterface);
                                stringBuilder.AppendLine("  <element>");
                                stringBuilder.AppendLine("    <id>Relation</id>");
                                stringBuilder.AppendLine("    <coordinates>");
                                stringBuilder.AppendLine($"     <x>{xArrowInterface2}</x>");
                                stringBuilder.AppendLine($"     <y>{yInterface + heightDifferenceInterfaces + heightInterface + heightInterface}</y>");
                                stringBuilder.AppendLine("     <w>0</w>");
                                stringBuilder.AppendLine("     <h>0</h>");
                                stringBuilder.AppendLine("    </coordinates>");
                                stringBuilder.AppendLine($"    <panel_attributes>{arrowType}");
                                stringBuilder.AppendLine($"{layer}</panel_attributes>");
                                stringBuilder.AppendLine($"    <additional_attributes>0.0;{-yDifference - heightDifferenceInterfaces - (interfaceMaxHeight - heightInterface) - 10};0.0;0.0;{xDifference};0.0;{xDifference};{yDifference - heightDifferenceInterfaces - (interfaceMaxHeight - heightInterface2) - 10}</additional_attributes>");
                                stringBuilder.AppendLine("   </element>");
                                heightDifferenceInterfaces += 10;
                                arrowCount++;
                            }
                        }
                    }
                }
            }
        }

        if (AddPathBlock)
        {
            stringBuilder.AppendLine(TextBlocks.PathBlock);
            stringBuilder.AppendLine(@"CC:\Repos\ClassHierarchyDiagramGenerator\ClassHierarchyDiagramGenerator");
            stringBuilder.AppendLine(TextBlocks.PathBlockafterPath);
        }

        stringBuilder.AppendLine(TextBlocks.FileEnd);

        return new GenerationResult { ArrowCount = arrowCount, BlockCount = blockCount, DiagramFileContent = stringBuilder.ToString() };
    }

    private static void AddBlockLineBreakAfterBlockType(ref int currentY, ref int itemsInRow, ref int interfaceMaxHeight)
    {
        currentY += ClassSpace - LineSpace;
        
        if (itemsInRow != 0)
        {
            currentY += LineSpace + interfaceMaxHeight;
        }
        
        itemsInRow = 0;
    }

    private static void InsertBlockLineBreakOrMoveHorizontally(double width,
                                                               ref int currentX,
                                                               ref int currentY,
                                                               ref int maxHeight,
                                                               ref int itemsInRow)
    {
        if (itemsInRow == MaxItemsInRow)
        {
            currentX = 0;
            currentY += LineSpace + maxHeight;
            itemsInRow = 0;
            maxHeight = 0;
        }
        else
        {
            currentX += (int) width + ItemSpace;
        }
    }
    
    
    private static void InsertClass(StringBuilder s,
                                    Class currentClass,
                                    ref int blockCount,
                                    ref int maxHeight,
                                    ref int itemsInRow,
                                    int x,
                                    ref int y,
                                    out double width)
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

        width = RoundUpToMultiple(longestLineCharacterCount * 8.5 + 10, 10);
        currentClass.LayoutData.Width = (int) width;
        double height = RoundUpToMultiple((lineCount - divideLineCount) * 13 + divideLineCount * 8 + 20, 10);
        currentClass.LayoutData.Height = (int) height;

        if (height >= maxHeight)
        {
            maxHeight = (int) height;
        }

        string classHeader = string.Format(TextBlocks.ClassBeginFormat, x, y, width, height);

        itemsInRow++;

        s.Insert(classElementStartIndex, classHeader);
    }

    private static void InsertInterface(StringBuilder s,
                                        Interface currentInterface,
                                        ref int blockCount,
                                        ref int maxHeight,
                                        ref int itemsInRow,
                                        ref int x,
                                        ref int y,
                                        bool MoveInterfacesToRelatedClasses,
                                        List<Class> classes,
                                        List<Interface> interfaces,
                                        out double width)
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
            int maxPropertyTypeLength = currentInterface.Properties.Max(p => p.Type.Length);

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
        s.AppendLine("transparency=0");
        s.Append(TextBlocks.ClassEnd);
        blockCount++;

        width = RoundUpToMultiple(longestLineCharacterCount * 8.5 + 10, 10);
        currentInterface.LayoutData.Width = (int) width;

        double height = RoundUpToMultiple((lineCount - divideLineCount) * 13 + divideLineCount * 8 + 20, 10);
        currentInterface.LayoutData.Height = (int) height;

        if (height >= maxHeight)
        {
            maxHeight = (int) height;
        }

        double xSave = x;
        
        if (MoveInterfacesToRelatedClasses)
        {
            foreach (var classItem in classes)
            {
                foreach (var classItemInterface in classItem.Interfaces)
                {
                    if (classItemInterface == currentInterface.Name)
                    {
                        x = classItem.LayoutData.X;
                        foreach (var interfaceItem in interfaces)
                        {
                            if (x == interfaceItem.LayoutData.X)
                            {
                                x += interfaceItem.LayoutData.Height + LineSpace;
                            }
                        }
                    }
                }
            }
        }
        
        currentInterface.LayoutData.X = x;
        string classHeader = string.Format(TextBlocks.InterfaceBeginFormat, x, y, width, height);
        s.Insert(classElementStartIndex, classHeader);
        x = (int)xSave;
        itemsInRow++;
    }

    private static void InsertEnum(StringBuilder s,
                                   Enum currentEnum,
                                   ref int blockCount,
                                   ref int maxHeight,
                                   ref int itemsInRow,
                                   ref int x,
                                   ref int y,
                                   out double width)
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

        width = RoundUpToMultiple(longestLineCharacterCount * 8.5 + 10, 10);
        currentEnum.LayoutData.Width = (int) width;

        double height = RoundUpToMultiple((lineCount - divideLineCount) * 13 + divideLineCount * 8 + 20, 10);
        currentEnum.LayoutData.Height = (int) height;

        if (height >= maxHeight)
        {
            maxHeight = (int) height;
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

    public sealed class GenerationResult
    {
        public required string DiagramFileContent { get; init; }
        public required int BlockCount { get; init; }
        public required int ArrowCount { get; init; }
    }
}
