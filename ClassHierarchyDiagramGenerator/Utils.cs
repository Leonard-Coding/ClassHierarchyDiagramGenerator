namespace ClassHierarchyDiagramGenerator;

public static class Utils
{
    /// <summary>
    /// Sanitizes the text by replacing 'less than' and 'greater than' with their HTML entities.
    /// </summary>
    public static string Sanitize(string text)
    {
        return text.Replace("<", "&lt;")
                   .Replace(">", "&gt;");
    }
    
    public static double CeilToMultiple(double value, double multiple)
    {
        if (multiple == 0)
            throw new ArgumentException("Multiple darf nicht 0 sein.");

        return Math.Ceiling(value / multiple) * multiple;
    }
}
