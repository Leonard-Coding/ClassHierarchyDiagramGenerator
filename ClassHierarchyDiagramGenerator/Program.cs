using System.Text;
using static ClassHierarchyDiagramGenerator.Utils;

namespace ClassHierarchyDiagramGenerator;

internal static class Program
{
    private const int LineSpace = 50;
    private const int ItemSpace = 10;

    public static void Main(string[] args)
    {
        // TODO: Hard-coded output for now. Better let the user decide how to call and where to save the output.
        const string outputPath = "output.uxf";
        
        // Wenn die Länge vom eingegebenen Path 0 ist, also kein Path eingegeben wurde
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
        
        //er sucht nach cs files und fügt sie zur Liste files hinzu
        string[] files = Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories);

        // wenn die Anzahl an files in der Liste files 0 ist dann
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

        //er schreibt in das output document alles rein, aber was genau ist catch? output path ist, wohin das output dokument muss?
        try
        {
            File.WriteAllText(outputPath, fileContent);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    

    // hier wird dann berechnet, wo und wie das in umlet muss?
    private static string GenerateDiagramFileContent(List<Class> classes, List<Interface> interfaces, List<Enum> enums)
    {
        StringBuilder s = new StringBuilder();

        s.AppendLine(TextBlocks.FileBeginFormat);
        
        int maximumheight = 0;
        int x = 0;
        int y = 0;
        int itemsInRow = 0;
        int anzahlBlöcke = 0;
        const int MaxItemsInRow = -1;
        

        foreach (Class item in classes)
        {
            InsertClass(classes, s, item, ref anzahlBlöcke, ref maximumheight, ref x, ref y, out int width, ref itemsInRow);
            
            if (itemsInRow == MaxItemsInRow)
            {
                x = 0;
                y += LineSpace + maximumheight;
                itemsInRow = 0;
                maximumheight = 0;
            }
            else
            {
                x += width + ItemSpace;
            }
        }
        
        x = 0;
        if (itemsInRow != 0)
        {
            y += LineSpace + maximumheight;
        }
        maximumheight = 0;
        itemsInRow = 0;
        
        foreach (Interface currentInterface in interfaces)
        {
            InsertInterface(interfaces, s, currentInterface, ref anzahlBlöcke, ref maximumheight, ref x, ref y, out int width, ref itemsInRow);
            
            if (itemsInRow == MaxItemsInRow)
            {
                x = 0;
                y += LineSpace + maximumheight;
                itemsInRow = 0;
                maximumheight = 0;
            }
            else
            {
                x += width + ItemSpace;
            }
        }
        
        y += LineSpace + maximumheight;
        x = 0;
        if (itemsInRow != 0)
        {
            y += LineSpace + maximumheight;
        }
        maximumheight = 0;
        itemsInRow = 0;
        
        foreach (Enum currentEnum in enums)
        {
            InsertEnum(enums, s, currentEnum, ref anzahlBlöcke, ref maximumheight, ref x, ref y, out int width, ref itemsInRow);
            
            if (itemsInRow == MaxItemsInRow)
            {
                x = 0;
                y += LineSpace + maximumheight;
                itemsInRow = 0;
                maximumheight = 0;
            }
            else
            {
                x += width + ItemSpace;
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
                        var classData = item.LayoutData;
                        var interfaceData = @interface.LayoutData;
                        string pfeilart = "lt=&lt;&lt;-";
                        double xgegeben = classData.X;    //x
                        double widthgegeben = classData.Width; //x
                        double ygegeben = classData.Y;
                        double heightgegeben = classData.Height;
                        double xpfeil = xgegeben + (widthgegeben / 2); //x
                        double ypfeil = ygegeben + heightgegeben;
                        int xzielgegeben = interfaceData.X;       //x
                        int widthzielgegeben = interfaceData.Width; //x
                        int yzielgegeben = interfaceData.Y;
                        double xende = xzielgegeben + (widthzielgegeben / 2); //x
                        double yende = yzielgegeben;
                        double xveränderungpfeil = -(xpfeil - xende); //x
                        double yveränderungpfeil = -(ypfeil - yende);
                        //for each verbindung
                        s.AppendLine("  <element>");                                            //fest
                        s.AppendLine("    <id>Relation</id>");                                  //fest
                        s.AppendLine("    <coordinates>");                                      //fest
                        s.AppendLine($"     <x>{xpfeil}</x>");                                  //vom startblock x+1/2*width (ceiltomultiple 10)
                        s.AppendLine($"     <y>{ypfeil}</y>");                                  //vom startblock y+height
                        s.AppendLine("     <w>0</w>");                                          //fest erstmal
                        s.AppendLine("     <h>0</h>");                                          //fest erstmal
                        s.AppendLine("    </coordinates>");                                     //fest
                        s.AppendLine($"    <panel_attributes>{pfeilart}</panel_attributes>"); //pfeilart verändern erstmal fest
                        s.AppendLine($"    <additional_attributes>{xveränderungpfeil}.0;{yveränderungpfeil}.0;0.0;0.0</additional_attributes>"); 
                        //1=xbewegung vom Pfeil, vom Ziel x+1/2*width und Differenz zu start x
                        //2=ybewegung vom Pfeil, vom Ziel y und Differenz zu start y
                        //3 und 4 fest erstmal
                        s.AppendLine("   </element>"); //fest
                    }
                }
            }
        }

        int unterschied = 0;
        foreach (Class item in classes)
        {
            if (item.BaseClass.Length > 0)
            {
                foreach (var baseclassName in item.BaseClass)
                {
                    foreach (Class name in classes)
                    {
                        if (baseclassName.ToString() == name.Name)
                        {    
                            var classData = item.LayoutData;
                            unterschied += 20;    
                            var interfaceData = name.LayoutData;
                            string pfeilart = "lt=-&gt;&gt;";
                            double xgegeben = classData.X;    //x
                            double widthgegeben = classData.Width; //x
                            double ygegeben = classData.Y;
                            double xpfeil = xgegeben + (widthgegeben / 2); //x
                            double ypfeil = ygegeben;
                            int xzielgegeben = interfaceData.X;       //x
                            int widthzielgegeben = interfaceData.Width; //x
                            int yzielgegeben = interfaceData.Y;
                            double xende = xzielgegeben + (widthzielgegeben / 2); //x
                            double yende = yzielgegeben;
                            double xveränderungpfeil = -(xpfeil - xende); //x
                            double yveränderungpfeil = -(ypfeil - yende);
                            //for each verbindung
                            s.AppendLine("  <element>");                                            //fest
                            s.AppendLine("    <id>Relation</id>");                                  //fest
                            s.AppendLine("    <coordinates>");                                      //fest
                            s.AppendLine($"     <x>{xpfeil}</x>");                                  //vom startblock x+1/2*width (ceiltomultiple 10)
                            s.AppendLine($"     <y>{ypfeil-unterschied+yveränderungpfeil}</y>");                                  //vom startblock y+height
                            s.AppendLine("     <w>0</w>");                                          //fest erstmal
                            s.AppendLine("     <h>0</h>");                                          //fest erstmal
                            s.AppendLine("    </coordinates>");                                     //fest
                            s.AppendLine($"    <panel_attributes>{pfeilart}</panel_attributes>"); //pfeilart verändern erstmal fest
                            s.AppendLine($"    <additional_attributes>0.0;{-yveränderungpfeil+unterschied};0.0;0.0;{xveränderungpfeil};0.0;{xveränderungpfeil};{unterschied}</additional_attributes>"); 
                            //1=xbewegung vom Pfeil, vom Ziel x+1/2*width und Differenz zu start x
                            //2=ybewegung vom Pfeil, vom Ziel y und Differenz zu start y
                            //3 und 4 fest erstmal
                            s.AppendLine("   </element>"); //fest
                            unterschied += 10;
                        }
                    }
                }
            }
        }
        
        unterschied = 0;
        foreach (Interface item in interfaces)
        {
            if (item.Interfaces.Count > 0)
            {
                foreach (var interfacename in item.Interfaces)
                {
                    foreach (Interface names in interfaces)
                    {
                        if (interfacename == names.Name)
                        {    
                            var classData = item.LayoutData;
                            unterschied -= 20;    
                            var interfaceData = names.LayoutData;
                            string pfeilart = "lt=-&gt;&gt;";
                            double xgegeben = classData.X;    //x
                            double widthgegeben = classData.Width; //x
                            double ygegeben = classData.Y;
                            double heightgegeben = classData.Height;
                            double xpfeil = xgegeben + (widthgegeben / 2); //x
                            double ypfeil = ygegeben;
                            int xzielgegeben = interfaceData.X;       //x
                            int widthzielgegeben = interfaceData.Width; //x
                            int yzielgegeben = interfaceData.Y;
                            double xende = xzielgegeben + (widthzielgegeben / 2); //x
                            double yende = yzielgegeben;
                            double xveränderungpfeil = -(xpfeil - xende); //x
                            double yveränderungpfeil = -(ypfeil - yende);
                            //for each verbindung
                            s.AppendLine("  <element>");                                            //fest
                            s.AppendLine("    <id>Relation</id>");                                  //fest
                            s.AppendLine("    <coordinates>");                                      //fest
                            s.AppendLine($"     <x>{xpfeil}</x>");                                  //vom startblock x+1/2*width (ceiltomultiple 10)
                            s.AppendLine($"     <y>{ygegeben+heightgegeben-unterschied}</y>");                                  //vom startblock y+height
                            s.AppendLine("     <w>0</w>");                                          //fest erstmal
                            s.AppendLine("     <h>0</h>");                                          //fest erstmal
                            s.AppendLine("    </coordinates>");                                     //fest
                            s.AppendLine($"    <panel_attributes>{pfeilart}</panel_attributes>"); //pfeilart verändern erstmal fest
                            s.AppendLine($"    <additional_attributes>0.0;{-yveränderungpfeil+unterschied};0.0;0.0;{xveränderungpfeil};0.0;{xveränderungpfeil};{unterschied}</additional_attributes>"); 
                            //1=xbewegung vom Pfeil, vom Ziel x+1/2*width und Differenz zu start x
                            //2=ybewegung vom Pfeil, vom Ziel y und Differenz zu start y
                            //3 und 4 fest erstmal
                            s.AppendLine("   </element>"); //fest
                            unterschied += 10;
                        }
                    }
                }
            }
        }
        
        s.AppendLine(TextBlocks.FileEnd);
        Console.WriteLine("Dein Dokument ist fertig mit " + anzahlBlöcke + " Blöcken"); //schöner schreiben?
        return s.ToString();
    }

    private static void InsertClass(List<Class> classes,
                                   StringBuilder s,
                                   Class currentClass,
                                   ref int anzahlBlöcke,
                                   ref int maximumheight,
                                   ref int x,
                                   ref int y,
                                   out int width,
                                   ref int itemsInRow)
    {
        int longestLineCharacterCount = 0;
        int lineCount = 0;
        int strichlineCount = 0;
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
            strichlineCount++;
                
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
            int maxPropertyTypeLength = currentClass.Properties.Max(p => p.Type
                                                                  .Length);
                
            s.AppendLine("--");
            lineCount++;
            strichlineCount++;
                
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
            strichlineCount++;
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
            strichlineCount++;
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
        anzahlBlöcke++;
            
        // now we know how wide the class element should become
        static double CeilToMultiple(double value, double multiple)
        {
            if (multiple == 0)
                throw new ArgumentException("Multiple darf nicht 0 sein.");

            return Math.Ceiling(value / multiple) * multiple;
        }

        double widthnn = CeilToMultiple(longestLineCharacterCount * 8.5 + 10, 10);
        width = (int)widthnn;
        currentClass.LayoutData.Width = width;
        double heightnn = CeilToMultiple((lineCount - strichlineCount) * 13 + strichlineCount * 8 + 20, 10);
        int height = (int)heightnn;
        currentClass.LayoutData.Height = height;
        
        if (height >= maximumheight)
        {
            maximumheight = height; 
        }
            
        string classHeader = string.Format(TextBlocks.ClassBeginFormat, x, y, width, height);

        itemsInRow++;

       

        s.Insert(classElementStartIndex, classHeader);
    }
    
    private static void InsertInterface(List<Interface> interfaces,
                                       StringBuilder s,
                                       Interface currentInterface,
                                       ref int anzahlBlöcke,
                                       ref int maximumheight,
                                       ref int x,
                                       ref int y,
                                       out int width,
                                       ref int itemsInRow)
    {
        int longestLineCharacterCount = 0;
        int lineCount = 0;
        int strichlineCount = 0;
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
            strichlineCount++;
                
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
            strichlineCount++;
                
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
            strichlineCount++;
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
            strichlineCount++;
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
        anzahlBlöcke++;
            
        // now we know how wide the class element should become
        static double CeilToMultiple(double value, double multiple)
        {
            if (multiple == 0)
                throw new ArgumentException("Multiple darf nicht 0 sein.");

            return Math.Ceiling(value / multiple) * multiple;
        }

        double widthnn = CeilToMultiple(longestLineCharacterCount * 8.5 + 10, 10);
        width = (int)widthnn;
        currentInterface.LayoutData.Width = width;
        double heightnn = CeilToMultiple((lineCount - strichlineCount) * 13 + strichlineCount * 8 + 20, 10);
        int height = (int)heightnn;
        currentInterface.LayoutData.Height = height;
        
        if (height >= maximumheight)
        {
            maximumheight = height; 
        }
            
        string classHeader = string.Format(TextBlocks.ClassBeginFormat, x, y, width, height);

        s.Insert(classElementStartIndex, classHeader);
    }
    
    private static void InsertEnum(List<Enum> enums,
                                   StringBuilder s,
                                   Enum currentEnum,
                                   ref int anzahlBlöcke,
                                   ref int maximumheight,
                                   ref int x,
                                   ref int y,
                                   out int width,
                                   ref int itemsInRow)
    {
        int longestLineCharacterCount = 0;
        int lineCount = 0;
        int strichlineCount = 0;
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
            strichlineCount++;
                
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
        anzahlBlöcke++;
            
        // now we know how wide the class element should become
        static double CeilToMultiple(double value, double multiple)
        {
            if (multiple == 0)
                throw new ArgumentException("Multiple darf nicht 0 sein.");

            return Math.Ceiling(value / multiple) * multiple;
        }

        double widthnn = CeilToMultiple(longestLineCharacterCount * 8.5 + 10, 10);
        width = (int)widthnn;
        currentEnum.LayoutData.Width = width;
        double heightnn = CeilToMultiple((lineCount - strichlineCount) * 13 + strichlineCount * 8 + 20, 10);
        int height = (int)heightnn;
        currentEnum.LayoutData.Height = height;
        
        if (height >= maximumheight)
        {
            maximumheight = height; 
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
