using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// Typ wyliczeniowy dla rodzajów paliwa
public enum TypPaliwa
{
    Pb95,
    Pb98,
    ON, // Diesel
    LPG
}

// Klasa reprezentująca pojedynczą transakcję
public class Transakcja
{
    public DateTime Data { get; set; }
    public TypPaliwa Paliwo { get; set; }
    public double IloscLitrow { get; set; }
    public decimal CenaZaLitr { get; set; }
    public decimal KwotaCalkowita { get; set; }
    public bool CzyOplacona { get; set; }

    // Konstruktor ułatwiający tworzenie
    public Transakcja(TypPaliwa paliwo, double litry, decimal cena)
    {
        Data = DateTime.Now;
        Paliwo = paliwo;
        IloscLitrow = litry;
        CenaZaLitr = cena;
        KwotaCalkowita = (decimal)litry * cena;
        CzyOplacona = false;
    }

    // Pusty konstruktor dla serializacji/odczytu
    public Transakcja() { }

    // Format zapisu do pliku (jedna linia)
    public override string ToString()
    {
        return $"{Data}|{Paliwo}|{IloscLitrow}|{CenaZaLitr}|{KwotaCalkowita}|{CzyOplacona}";
    }

    // Metoda statyczna do odczytu z linii tekstu
    public static Transakcja FromString(string linia)
    {
        var dane = linia.Split('|');
        return new Transakcja
        {
            Data = DateTime.Parse(dane[0]),
            Paliwo = (TypPaliwa)Enum.Parse(typeof(TypPaliwa), dane[1]),
            IloscLitrow = double.Parse(dane[2]),
            CenaZaLitr = decimal.Parse(dane[3]),
            KwotaCalkowita = decimal.Parse(dane[4]),
            CzyOplacona = bool.Parse(dane[5])
        };
    }
}
