namespace ClassHierarchyDiagramGenerator;

public class Class
{
    public string Name { get; init; }
    public string Type { get; init; }
    public string BaseClass { get; init; }
    public List<string> Interfaces { get; init; }
    public List<Field> Fields { get; init; }
    public List<Property> Properties { get; init; }
    public List<Event> Events { get; init; }
    public List<Method> Methods { get; init; }

    public NodeData Data { get; } = new();
    public int MemberCount
    {
        get { return Fields.Count + Properties.Count + Events.Count + Methods.Count; }
    }
}

public class Method
{
    public string Name { get; init; }
    public string ReturnType { get; init; }
    public List<string> Parameters { get; init; }
}

public class Field
{
    public string Name { get; init; }
    public string Type { get; init; }
}

public class Property
{
    public string Name { get; init; }
    public string Type { get; init; }
}

public class Event
{
    public string Name { get; init; }
    public List<string> ParameterTypes { get; init; }
}

public class Interface
{
    public string Name { get; init; }
    public string Type { get; init; }

    public List<Interface> Interfaces { get; init; }
    public List<Field> Fields { get; init; }
    public List<Property> Properties { get; init; }
    public List<Event> Events { get; init; }
    public List<Method> Methods { get; init; }

    public NodeData Data { get; } = new();

    public int MemberCount
    {
        get { return Fields.Count + Properties.Count + Events.Count + Methods.Count; }
    }
}

public class Enum
{
    public string Name { get; init; }
    public List<string> Members { get; init; }
    public NodeData Data { get; } = new();

    public int MemberCount
    {
        get { return Members.Count; }
    }
}

public class NodeData
{
    public int X { get; set; }
    public int Y { get; set; }

    public int Height { get; set; }
    public int Width { get; set; }
}
