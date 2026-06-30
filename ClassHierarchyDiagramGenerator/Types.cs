namespace ClassHierarchyDiagramGenerator;

public sealed class Class
{
    public required string Name { get; init; }
    public string Type { get; init; } = "";
    public required string BaseClass { get; init; }
    public required List<string> Interfaces { get; init; }
    public required List<Field> Fields { get; init; }
    public required List<Property> Properties { get; init; }
    public required List<Event> Events { get; init; }
    public required List<Method> Methods { get; init; }

    public NodeLayoutData LayoutData { get; } = new();

    public int MemberCount
    {
        get { return Fields.Count + Properties.Count + Events.Count + Methods.Count; }
    }
}

public sealed class Method
{
    public required string Name { get; init; }
    public required string ReturnType { get; init; }
    public required List<string> Parameters { get; init; }
}

public sealed class Field
{
    public required string Name { get; init; }
    public required string Type { get; init; }
}

public sealed class Property
{
    public required string Name { get; init; }
    public required string Type { get; init; }
}

public sealed class Event
{
    public required string Name { get; init; }
    public required List<string> ParameterTypes { get; init; }
}

public sealed class Interface
{
    public required string Name { get; init; }
    public string Type { get; init; } = "";

    public required List<Interface> Interfaces { get; init; }
    public required List<Field> Fields { get; init; }
    public required List<Property> Properties { get; init; }
    public required List<Event> Events { get; init; }
    public required List<Method> Methods { get; init; }

    public NodeLayoutData LayoutData { get; } = new();

    public int MemberCount
    {
        get { return Fields.Count + Properties.Count + Events.Count + Methods.Count; }
    }
}

public sealed class Enum
{
    public required string Name { get; init; }
    public required List<string> Members { get; init; }
    public NodeLayoutData LayoutData { get; } = new();

    public int MemberCount
    {
        get { return Members.Count; }
    }
}

public sealed class NodeLayoutData
{
    public int X { get; set; }
    public int Y { get; set; }

    public int Height { get; set; }
    public int Width { get; set; }
}
