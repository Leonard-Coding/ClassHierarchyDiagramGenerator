using System.Text;

namespace ClassHierarchyDiagramGenerator;
//Fields, Properties, Events, Methods

internal sealed class testblock
{
    public int uhrzeitinsec = 10 * 60 * 60 + 37 * 60 + 12;
    public string Uhrzeit = "10:37:12";
    private int eins = 1;
    private int zwei = 2;
    private int drei = 4;
    private int vier = 3;
    public bool über30grad = false;
    public string Uhrzeit1
    {
        get => Uhrzeit;
        set => Uhrzeit = value ?? throw new ArgumentNullException(nameof(value));

    }

    public string[] Favoriterats = ["rat 1", "rat 2", "rat 3"];
    
    
    // Privates Feld
    private string name;

    // Property mit get und set
    public string Name
    {
        get { return name; } // Wert zurückgeben
        set
        {
            // Validierung
            if (!string.IsNullOrWhiteSpace(value))
                name = value;
            else
                throw new ArgumentException("Name darf nicht leer sein.");
        }
        
        // Methode ohne Rückgabewert
       
    
    }
    public event EventHandler AlarmAusgeloest;
    public event EventHandler AlarmAusgeloest2;
    public event EventHandler<EventArgs> AlarmAusgeloest3;
    // Methode mit Rückgabewert
    static int Addiere(int x, int y)
    {
        return x + y;
    }

    static void nichtmain()
    {
        // Aufruf der Methode ohne Rückgabewert
       

        // Aufruf der Methode mit Rückgabewert
        int summe = Addiere(5, 7);
        Console.WriteLine($"Summe: {summe}");
    }
}
    