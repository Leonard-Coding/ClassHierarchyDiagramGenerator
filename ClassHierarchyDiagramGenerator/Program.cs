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
        List<Class> collectedClasses = ExtractClassesFromFiles(files);
        
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
    private static string GenerateDiagramFileContent(List<Class> collectedClasses)
    {
        StringBuilder s = new StringBuilder();

        s.AppendLine(TextBlocks.FileBeginFormat);

        int x = 0;
        int y = 0;
        int itemsInRow = 0;
        const int maxItemsInRow = 20;

        foreach (Class classObject in collectedClasses)
        {
            int longestLineCharacterCount = 0;
            int lineCount = 0;

            int classElementStartIndex = s.Length - 1;

            string sanitizedClassName = Sanitize(classObject.Name);

            UpdateToLongest(ref longestLineCharacterCount, sanitizedClassName);

            s.AppendLine($"*{sanitizedClassName}*");
            lineCount++;

            s.AppendLine("--");
            lineCount++;
            
            

            if (classObject.Fields.Count > 0)
            {
                int maxFieldTypeLength = classObject.Fields.Max(f => Sanitize(f.Type)
                                                           .Length);
                foreach (Field field in classObject.Fields)
                {
                    string fieldLine = $"  {Sanitize(field.Type).PadRight(maxFieldTypeLength)} {field.Name}";
                    s.AppendLine(fieldLine);
                    lineCount++;

                    UpdateToLongest(ref longestLineCharacterCount, fieldLine);
                }

                s.AppendLine("--");
                lineCount++;
            }

            if (classObject.Properties.Count > 0)
            {
                int maxPropertyTypeLength = classObject.Properties.Max(p => p.Type
                                                                  .Length);
                foreach (Property prop in classObject.Properties)
                {
                    var paddedType = prop.Type.PadRight(maxPropertyTypeLength);
                    string propertyLine = $"  {Sanitize(paddedType)} {prop.Name}";
                    s.AppendLine(propertyLine);
                    lineCount++;

                    UpdateToLongest(ref longestLineCharacterCount, propertyLine);
                }

                s.AppendLine("--");
                lineCount++;
            }

            if (classObject.Events.Count > 0)
            {
                foreach (Event ev in classObject.Events)
                {
                    string parameterTypes = "";
                    if (ev.ParameterTypes.Count > 0)
                    {
                        parameterTypes = " " + string.Join(", ", ev.ParameterTypes.Select(Sanitize));
                    }

                    string eventLine = $"  {ev.Name}{parameterTypes}!";
                    s.AppendLine(eventLine);
                    lineCount++;

                    UpdateToLongest(ref longestLineCharacterCount, eventLine);
                }
                //kann die letzte Linie weg?
                s.AppendLine("--");
                lineCount++;
            }

            foreach (Method method in classObject.Methods)
            {
                string name = Sanitize(method.Name);
                string parameters = string.Join(", ", method.Parameters);
                string returnType = Sanitize(method.ReturnType);
                string methodLine = $"  {name}({parameters})";
                if (returnType != "void")
                {
                    methodLine += $"->{returnType}";
                }

                s.AppendLine(methodLine);
                lineCount++;

                UpdateToLongest(ref longestLineCharacterCount, methodLine);
            }

            s.Append(TextBlocks.ClassEnd);

            // now we know how wide the class element should become

            int width = longestLineCharacterCount * 6;
            int height = lineCount * 10;

            string classHeader = string.Format(TextBlocks.ClassBeginFormat, x, y, width, height);

            itemsInRow++;

            if (itemsInRow == maxItemsInRow)
            {
                x = 0;
                y += 50;
                itemsInRow = 0;
            }

            s.Insert(classElementStartIndex, classHeader);

            x += width + 20;
        }

        s.AppendLine(TextBlocks.FileEnd);
        return s.ToString();
    }

    private static void UpdateToLongest(ref int longestLineCharacterCount, string sanitizedClassName)
    {
        longestLineCharacterCount = sanitizedClassName.Length > longestLineCharacterCount
                                        ? sanitizedClassName.Length
                                        : longestLineCharacterCount;
    }

    private static List<Class> ExtractClassesFromFiles(string[] files)
    {
        List<Class> collectedClasses = new List<Class>();

        foreach (string file in files)
        {
            try
            {
                string code = File.ReadAllText(file);
                SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
                CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

                IEnumerable<ClassDeclarationSyntax> classes = root.DescendantNodes()
                                                                  .OfType<ClassDeclarationSyntax>();

                foreach (ClassDeclarationSyntax classDeclaration in classes)
                {
                    string className = classDeclaration.Identifier.Text;
                    List<Field> fields = new List<Field>();
                    List<Property> properties = new List<Property>();
                    List<Event> events = new List<Event>();
                    List<Method> methods = new List<Method>();

                    foreach (FieldDeclarationSyntax fieldDecl in classDeclaration.Members.OfType<FieldDeclarationSyntax>())
                    {
                        string type = fieldDecl.Declaration.Type.ToString();
                        foreach (VariableDeclaratorSyntax variable in fieldDecl.Declaration.Variables)
                        {
                            fields.Add(new Field { Name = variable.Identifier.Text, Type = type });
                        }
                    }

                    foreach (PropertyDeclarationSyntax propDecl in classDeclaration.Members.OfType<PropertyDeclarationSyntax>())
                    {
                        properties.Add(new Property { Name = propDecl.Identifier.Text, Type = propDecl.Type.ToString() });
                    }

                    foreach (EventFieldDeclarationSyntax eventDecl in classDeclaration.Members.OfType<EventFieldDeclarationSyntax>())
                    {
                        List<string> parameterTypes = new List<string>();
                        if (eventDecl.Declaration.Type is GenericNameSyntax genericName)
                        {
                            parameterTypes.AddRange(genericName.TypeArgumentList.Arguments.Select(a => a.ToString()));
                        }

                        foreach (VariableDeclaratorSyntax variable in eventDecl.Declaration.Variables)
                        {
                            events.Add(new Event { Name = variable.Identifier.Text, ParameterTypes = parameterTypes });
                        }
                    }

                    foreach (MethodDeclarationSyntax methodDecl in classDeclaration.Members.OfType<MethodDeclarationSyntax>())
                    {
                        methods.Add(new Method
                                    {
                                        Name = methodDecl.Identifier.Text,
                                        ReturnType = methodDecl.ReturnType.ToString(),
                                        Parameters = methodDecl.ParameterList.Parameters.Select(p => p.Identifier.Text)
                                                               .ToList()
                                    });
                    }

                    collectedClasses.Add(new Class
                                         {
                                             Name = className,
                                             Fields = fields,
                                             Properties = properties,
                                             Events = events,
                                             Methods = methods
                                         });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file {file}: {ex.Message}");
            }
        }

        return collectedClasses;
    }
}
