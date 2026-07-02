namespace ClassHierarchyDiagramGenerator;

public static class TextBlocks
{
    public const string FileBeginFormat = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""no""?>
<diagram program=""umlet"" version=""15.1"">
  <help_text>fontfamily=Monospaced</help_text>
  <zoom_level>10</zoom_level>";
    
    public const string PathBlock = @"  <element>
    <id>UMLClass</id>
    <coordinates>
      <x>0</x>
      <y>-70</y>
      <w>0</w>
      <h>0</h>
    </coordinates>
    <panel_attributes>*Path:*";

    public const string PathBlockafterPath = @"style=autoresize
fontsize=20
halign=left</panel_attributes>
    <additional_attributes/>
  </element>";
        
    public const string FileEnd = @"</diagram>";

    public const string ClassBeginFormat =
        "  <element>\n    <id>UMLClass</id>\n    <coordinates>\n      <x>{0}</x>\n      <y>{1}</y>\n      <w>{2}</w>\n      <h>{3}</h>\n    </coordinates>\n    <panel_attributes>";
    public const string InterfaceBeginFormat =
        "  <element>\n    <id>UMLState</id>\n    <coordinates>\n      <x>{0}</x>\n      <y>{1}</y>\n      <w>{2}</w>\n      <h>{3}</h>\n    </coordinates>\n    <panel_attributes>";
    public const string ClassEnd = "\n    </panel_attributes>\n    <additional_attributes/>\n  </element>\n";
}
