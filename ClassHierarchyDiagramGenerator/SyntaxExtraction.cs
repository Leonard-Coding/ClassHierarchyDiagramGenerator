using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ClassHierarchyDiagramGenerator;

internal static class SyntaxExtractionFromFiles
{
    public static List<Class> ExtractClasses(string[] files)
    {
        List<Class> sortedDescending = new List<Class>();

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
                    List<string> interfaces = new List<string>();

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

                    string baseClass = "";
                    if (classDeclaration.BaseList != null)
                    {
                        var baseTypes = classDeclaration.BaseList.Types.Select(t => t.ToString()).ToArray();

                        if (!baseTypes[0]
                               .StartsWith("I"))
                        {
                            baseClass = baseTypes[0];
                            interfaces.AddRange(baseTypes.Skip(1));
                        }
                        else
                        {
                            interfaces.AddRange(baseTypes);
                        }
                    }

                    sortedDescending.Add(new Class
                                         {
                                             Name = className,
                                             Fields = fields,
                                             Properties = properties,
                                             Events = events,
                                             Methods = methods,
                                             BaseClass = baseClass,
                                             Interfaces = interfaces
                                         });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file {file}: {ex.Message}");
            }
        }

        return sortedDescending;
    }

    public static List<Interface> ExtractInterfaces(string[] files)
    {
        List<Interface> sortedDescending = new List<Interface>();

        foreach (string file in files)
        {
            try
            {
                string code = File.ReadAllText(file);
                SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
                CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

                IEnumerable<InterfaceDeclarationSyntax> classes = root.DescendantNodes()
                                                                      .OfType<InterfaceDeclarationSyntax>();

                foreach (InterfaceDeclarationSyntax classDeclaration in classes)
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

                    sortedDescending.Add(new Interface
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

        return sortedDescending;
    }

    public static List<Enum> ExtractEnums(string[] files)
    {
        List<Enum> sortedDescending = new List<Enum>();

        foreach (string file in files)
        {
            try
            {
                string code = File.ReadAllText(file);
                SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
                CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

                IEnumerable<EnumDeclarationSyntax> enums = root.DescendantNodes()
                                                               .OfType<EnumDeclarationSyntax>();

                foreach (EnumDeclarationSyntax enumDeclaration in enums)
                {
                    string className = enumDeclaration.Identifier.Text;
                    List<string> members = new List<string>();

                    foreach (EnumMemberDeclarationSyntax enumMember in enumDeclaration.Members)
                    {
                        string identifier = enumMember.Identifier.Text;
                        members.Add(identifier);
                    }

                    sortedDescending.Add(new Enum { Name = className, Members = members });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file {file}: {ex.Message}");
            }
        }

        return sortedDescending;
    }
}
