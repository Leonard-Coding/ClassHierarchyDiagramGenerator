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
}
