using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.Json; // Dodajemy obsługę JSON

public class Dystrybutor
{
    public int IdDystrybutora { get; set; }
    public List<Paliwo> PaliwaDostepne { get; private set; }
    public List<Transakcja> Transakcje { get; private set; } = new List<Transakcja>();

    private const string SciezkaPlikuTransakcji = "transakcje.json"; // Zmiana na .json
    private const string SciezkaPlikuPaliw = "paliwa.json";          // Zmiana na .json

    // Definicja delegatów i eventów
    public delegate void AwariaHandler(string komunikat);
    public delegate void BrakPlatnosciHandler(Transakcja transakcja);

    public event AwariaHandler BladProgramu;
    public event BrakPlatnosciHandler KlientOdjechalBezPlatnosci;

    public Dystrybutor(int id)
    {
        IdDystrybutora = id;
        WczytajPaliwa();
        WczytajTransakcje();
    }

    // -----------------------------------------------------------------
    // --- NOWE METODY OBSŁUGI PLIKÓW JSON ---
    // -----------------------------------------------------------------

    private void WczytajPaliwa()
    {
        try
        {
            if (File.Exists(SciezkaPlikuPaliw))
            {
                string jsonString = File.ReadAllText(SciezkaPlikuPaliw);
                // Deserializacja List<Paliwo>
                PaliwaDostepne = JsonSerializer.Deserialize<List<Paliwo>>(jsonString);
            }
            else
            {
                // Ustawienie domyślnych paliw i zapisanie ich do pliku JSON
                PaliwaDostepne = new List<Paliwo>
                {
                    new Paliwo { IdPaliwa = 1, Nazwa = "Benzyna 95", CenaZaLitr = 6.55m },
                    new Paliwo { IdPaliwa = 2, Nazwa = "Diesel", CenaZaLitr = 6.80m }
                };
                ZapiszPaliwa();
            }
        }
        catch (Exception ex)
        {
            BladProgramu?.Invoke($"Błąd odczytu paliw JSON: {ex.Message}");
            PaliwaDostepne = new List<Paliwo>();
        }
    }
    
    private void ZapiszPaliwa()
    {
        // Serializacja List<Paliwo> do stringu JSON
        var jsonString = JsonSerializer.Serialize(PaliwaDostepne, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(SciezkaPlikuPaliw, jsonString);
    }

    public void WczytajTransakcje()
    {
        try
        {
            if (File.Exists(SciezkaPlikuTransakcji))
            {
                string jsonString = File.ReadAllText(SciezkaPlikuTransakcji);
                // Deserializacja List<Transakcja>
                Transakcje = JsonSerializer.Deserialize<List<Transakcja>>(jsonString);
            }
        }
        catch (Exception ex)
        {
            BladProgramu?.Invoke($"Błąd podczas wczytywania transakcji JSON: {ex.Message}");
            Transakcje = new List<Transakcja>();
        }
    }

    public void ZapiszTransakcje()
    {
        try
        {
            // Serializacja List<Transakcja> do stringu JSON
            var jsonString = JsonSerializer.Serialize(Transakcje, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SciezkaPlikuTransakcji, jsonString);
        }
        catch (Exception ex)
        {
            BladProgramu?.Invoke($"Błąd podczas zapisu transakcji JSON: {ex.Message}");
        }
    }
    
    // -----------------------------------------------------------------
    // --- Metody funkcjonalne (niezmienione) ---
    // -----------------------------------------------------------------
    
    public Transakcja TankujPaliwo(int idWybranegoPaliwa, decimal iloscLitrow)
    {
        var paliwo = PaliwaDostepne.FirstOrDefault(p => p.IdPaliwa == idWybranegoPaliwa);

        if (paliwo == null || iloscLitrow <= 0)
        {
            BladProgramu?.Invoke("Nieprawidłowy wybór paliwa lub ilość.");
            return null;
        }

        var nowaTransakcja = new Transakcja
        {
            IdTransakcji = Transakcje.Any() ? Transakcje.Max(t => t.IdTransakcji) + 1 : 1,
            DataCzas = DateTime.Now,
            IdPaliwa = idWybranegoPaliwa,
            Litry = iloscLitrow,
            Kwota = iloscLitrow * paliwo.CenaZaLitr,
            CzyZaplacono = false
        };

        Transakcje.Add(nowaTransakcja);
        Console.WriteLine($"\n[Dystrybutor {IdDystrybutora}] Rozpoczęto tankowanie. Kwota: {nowaTransakcja.Kwota:C}");
        return nowaTransakcja;
    }

    public void PrzyjmijPlatnosc(Transakcja transakcja)
    {
        if (transakcja != null && !transakcja.CzyZaplacono)
        {
            transakcja.CzyZaplacono = true;
            ZapiszTransakcje();
            Console.WriteLine($"[Dystrybutor {IdDystrybutora}] Płatność za transakcję #{transakcja.IdTransakcji} zaakceptowana.");
        }
    }

    public void SymulujOdjazdBezPlatnosci(Transakcja transakcja)
    {
        if (transakcja != null && !transakcja.CzyZaplacono)
        {
            Console.WriteLine($"\n!!! [Dystrybutor {IdDystrybutora}] Klient odjechał bez zapłaty za transakcję #{transakcja.IdTransakcji}!");
            KlientOdjechalBezPlatnosci?.Invoke(transakcja);
            ZapiszTransakcje();
        }
    }
}
