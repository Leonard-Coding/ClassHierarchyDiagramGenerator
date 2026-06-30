using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ClassHierarchyDiagramGenerator;

internal static class SyntaxExtractionFromFiles
{
    static string[] _endungen = { "Factory", "Provider", "Files" };
    static string[] _endungenb = { "Utils", "Extensions", "block" };
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

                    List<Field> fields = GetFields(classDeclaration);
                    List<Property> properties = GetProperties(classDeclaration);
                    List<Event> events = GetEvents(classDeclaration);
                    List<Method> methods = GetMethods(classDeclaration);
                    List<string> interfaces = GetInheritedTypes(classDeclaration, out string baseClass);

                    var @class = new Class
                                 {
                                     Name = className,
                                     Fields = fields,
                                     Properties = properties,
                                     Events = events,
                                     Methods = methods,
                                     BaseClass = baseClass,
                                     Interfaces = interfaces
                                 };
                    if (_endungen.Any(endung => @class.Name.EndsWith(endung)))
                    {
                        @class.LayoutData.Color = "magenta";
                    }
                    else if (_endungenb.Any(endung => @class.Name.EndsWith(endung)))
                    {
                        @class.LayoutData.Color = "pink";
                    }
                    sortedDescending.Add(@class);
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

                IEnumerable<InterfaceDeclarationSyntax> interfaces = root.DescendantNodes()
                                                                         .OfType<InterfaceDeclarationSyntax>();

                foreach (InterfaceDeclarationSyntax interfaceDeclaration in interfaces)
                {
                    string name = interfaceDeclaration.Identifier.Text;

                    List<Field> fields = GetFields(interfaceDeclaration);
                    List<Property> properties = GetProperties(interfaceDeclaration);
                    List<Event> events = GetEvents(interfaceDeclaration);
                    List<Method> methods = GetMethods(interfaceDeclaration);
                    List<string> inherited = GetInheritedTypes(interfaceDeclaration, out string _);

                    sortedDescending.Add(new Interface
                                         {
                                             Name = name,
                                             Fields = fields,
                                             Properties = properties,
                                             Events = events,
                                             Methods = methods,
                                             Interfaces = inherited
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

    private static List<string> GetInheritedTypes(TypeDeclarationSyntax typeDeclaration, out string baseClass)
    {
        List<string> interfaces = new List<string>();
        baseClass = "";

        if (typeDeclaration.BaseList == null)
        {
            return interfaces;
        }

        var baseTypes = typeDeclaration.BaseList.Types.Select(t => t.ToString())
                                       .ToArray();

        // If the class has a base class, it is the first item in baseTypes.
        // We assume that an interface always starts with 'I'.
        if (!baseTypes[0]
               .StartsWith('I'))
        {
            baseClass = baseTypes[0];
            interfaces.AddRange(baseTypes.Skip(1));
        }
        else
        {
            interfaces.AddRange(baseTypes);
        }

        return interfaces;
    }

    private static List<Method> GetMethods(TypeDeclarationSyntax typeDeclaration)
    {
        List<Method> methods = new List<Method>();
        foreach (MethodDeclarationSyntax methodDecl in typeDeclaration.Members.OfType<MethodDeclarationSyntax>())
        {
            methods.Add(new Method
                        {
                            Name = methodDecl.Identifier.Text,
                            ReturnType = methodDecl.ReturnType.ToString(),
                            Parameters = methodDecl.ParameterList.Parameters.Select(p => p.Identifier.Text)
                                                   .ToList()
                        });
        }

        return methods;
    }

    private static List<Event> GetEvents(TypeDeclarationSyntax typeDeclaration)
    {
        List<Event> events = new List<Event>();
        foreach (EventFieldDeclarationSyntax eventDecl in typeDeclaration.Members.OfType<EventFieldDeclarationSyntax>())
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

        return events;
    }

    private static List<Property> GetProperties(TypeDeclarationSyntax typeDeclaration)
    {
        List<Property> properties = new List<Property>();
        foreach (PropertyDeclarationSyntax propDecl in typeDeclaration.Members.OfType<PropertyDeclarationSyntax>())
        {
            properties.Add(new Property { Name = propDecl.Identifier.Text, Type = propDecl.Type.ToString() });
        }

        return properties;
    }

    private static List<Field> GetFields(TypeDeclarationSyntax typeDeclaration)
    {
        List<Field> fields = new List<Field>();
        foreach (FieldDeclarationSyntax fieldDeclaration in typeDeclaration.Members.OfType<FieldDeclarationSyntax>())
        {
            string type = fieldDeclaration.Declaration.Type.ToString();
            foreach (VariableDeclaratorSyntax variable in fieldDeclaration.Declaration.Variables)
            {
                fields.Add(new Field { Name = variable.Identifier.Text, Type = type });
            }
        }

        return fields;
    }
}
