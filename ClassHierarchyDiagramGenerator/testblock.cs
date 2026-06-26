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


public interface IFahrzeug
{
    // Eigenschaften
    string Marke { get; set; }
    int Baujahr { get; set; }

    // Methoden
    void Starten();
    void Stoppen();
}

// Klasse Auto implementiert das Interface
public class Auto : IFahrzeug
{
    public string Marke { get; set; }
    public int Baujahr { get; set; }

    public void Starten()
    {
        Console.WriteLine($"Das Auto {Marke} startet.");
    }

    public void Stoppen()
    {
        Console.WriteLine($"Das Auto {Marke} stoppt.");
    }
}

// Klasse Fahrrad implementiert das Interface
public class Fahrrad : IFahrzeug
{
    public string Marke { get; set; }
    public int Baujahr { get; set; }

    public void Starten()
    {
        Console.WriteLine($"Das Fahrrad {Marke} wird losgefahren.");
    }

    public void Stoppen()
    {
        Console.WriteLine($"Das Fahrrad {Marke} wird angehalten.");
    }
}

public class Programmm
{
    public static void Mainnn()
    {
        // Interface-Referenzen auf konkrete Objekte
        IFahrzeug meinAuto = new Auto { Marke = "BMW", Baujahr = 2020 };
        IFahrzeug meinFahrrad = new Fahrrad { Marke = "Cube", Baujahr = 2022 };

        // Beide Objekte über das Interface ansprechen
        meinAuto.Starten();
        meinAuto.Stoppen();

        meinFahrrad.Starten();
        meinFahrrad.Stoppen();
    }
}

public interface IElektronischesGeraet
{
    string Modell { get; set; }
    bool IstEingeschaltet { get; }

    void Einschalten();
    void Ausschalten();
}

// Klasse Fernseher implementiert das Interface
public class Fernseher : IElektronischesGeraet
{
    public string Modell { get; set; }
    public bool IstEingeschaltet { get; private set; }

    public void Einschalten()
    {
        IstEingeschaltet = true;
        Console.WriteLine($"Der Fernseher {Modell} ist jetzt an.");
    }

    public void Ausschalten()
    {
        IstEingeschaltet = false;
        Console.WriteLine($"Der Fernseher {Modell} ist jetzt aus.");
    }
}

// Klasse Laptop implementiert das Interface
public class Laptop : IElektronischesGeraet
{
    public string Modell { get; set; }
    public bool IstEingeschaltet { get; private set; }

    public void Einschalten()
    {
        IstEingeschaltet = true;
        Console.WriteLine($"Der Laptop {Modell} wurde gestartet.");
    }

    public void Ausschalten()
    {
        IstEingeschaltet = false;
        Console.WriteLine($"Der Laptop {Modell} wurde heruntergefahren.");
    }
}

public class Programmmmm
{
    public static void Mainnnnnn()
    {
        IElektronischesGeraet tv = new Fernseher { Modell = "Samsung QLED" };
        IElektronischesGeraet notebook = new Laptop { Modell = "Dell XPS" };

        tv.Einschalten();
        notebook.Einschalten();

        tv.Ausschalten();
        notebook.Ausschalten();
    }
}

public interface ITier
{
    string Name { get; set; }
    int Alter { get; set; }

    void LautGeben();
    void Bewegen();
}

// Klasse Hund implementiert das Interface
public class Hund : ITier
{
    public string Name { get; set; }
    public int Alter { get; set; }

    public void LautGeben()
    {
        Console.WriteLine($"{Name} bellt: Wuff Wuff!");
    }

    public void Bewegen()
    {
        Console.WriteLine($"{Name} rennt fröhlich herum.");
    }
}

// Klasse Katze implementiert das Interface
public class Katze : ITier
{
    public string Name { get; set; }
    public int Alter { get; set; }

    public void LautGeben()
    {
        Console.WriteLine($"{Name} miaut: Miau!");
    }

    public void Bewegen()
    {
        Console.WriteLine($"{Name} schleicht leise durch den Raum.");
    }
}

public class Programn
{
    public static void Mainm()
    {
        ITier hund = new Hund { Name = "Bello", Alter = 5 };
        ITier katze = new Katze { Name = "Minka", Alter = 3 };

        hund.LautGeben();
        hund.Bewegen();

        katze.LautGeben();
        katze.Bewegen();
    }
}

public enum Wetter{sonnig, regnerisch, nebelig}