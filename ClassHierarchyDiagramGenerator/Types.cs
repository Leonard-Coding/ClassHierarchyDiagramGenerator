namespace ClassHierarchyDiagramGenerator;

public class Class
{
    public string Name { get; init; }
    public string Type { get; init; }
    public List<Class> BaseTypes { get; init; }
    public List<Interface> Interfaces { get; init; }
    public List<Field> Fields { get; init; }
    public List<Property> Properties { get; init; }
    public List<Event> Events { get; init; }
    public List<Method> Methods { get; init; }

    public NodeData Data { get; init; }
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

    public NodeData Data { get; init; }
}

public class Enum
{
    public string Name { get; init; }
    public string Type { get; init; }
    public NodeData Data { get; init; }
}

public abstract class NodeData
{
    public int X { get; init; }
    public int Y { get; init; }

    public int Height { get; init; }
    public int Width { get; init; }
}