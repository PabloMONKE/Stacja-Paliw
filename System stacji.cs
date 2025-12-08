using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

public class SystemStacji
{
    public Dystrybutor Dystrybutor1 { get; private set; }
    private const string SCIEZKA_RAPORTU = "raport_linq.txt";

    public SystemStacji()
    {
        Dystrybutor1 = new Dystrybutor(1);
        // 4. Podpięcie metod obsługi eventów
        Dystrybutor1.BladProgramu += ObsluzBlad;
        Dystrybutor1.KlientOdjechalBezPlatnosci += ObsluzBrakPlatnosci;
        Console.WriteLine($"SystemStacji: Wczytano {Dystrybutor1.Transakcje.Count} historycznych transakcji.");
    }

    // Metoda obsługująca zdarzenie błędu (Exception)
    private void ObsluzBlad(string komunikat)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\n[AWARYJNY ALERT SYSTEMU]: {komunikat}");
        Console.ResetColor();
    }

    // Metoda obsługująca zdarzenie braku płatności
    private void ObsluzBrakPlatnosci(Transakcja transakcja)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[ALERT BEZPIECZEŃSTWA]: Transakcja #{transakcja.IdTransakcji} - Odjazd bez zapłaty ({transakcja.Kwota:C}). Należy podjąć działania.");
        Console.ResetColor();
    }

    // 2. Metoda wyszukiwania informacji przy pomocy zapytań LINQ
    public void GenerujZestawieniaLINQ(decimal kwotaMin = 100.00m)
    {
        Console.WriteLine("\n--- Generowanie Raportów LINQ ---");
        
        // Bierzemy pod uwagę tylko opłacone transakcje do zestawień sprzedaży
        var transakcje = Dystrybutor1.Transakcje
                                     .Where(t => t.CzyZaplacono)
                                     .ToList();

        var raport = new List<string> { $"--- RAPORT ZESTAWIEŃ LINQ ({DateTime.Now}) ---" };

        // Zestawienie A: Ile litrów sprzedano poszczególnych rodzajów paliwa
        var sprzedaneLitry = transakcje
            .GroupBy(t => t.IdPaliwa)
            .Select(g => new
            {
                Paliwo = Dystrybutor1.PaliwaDostepne.FirstOrDefault(p => p.IdPaliwa == g.Key)?.Nazwa ?? "Nieznane",
                SumaLitrow = g.Sum(t => t.Litry)
            })
            .OrderByDescending(x => x.SumaLitrow)
            .ToList();

        raport.Add("\n1. Sprzedaż Litrów wg. Rodzaju Paliwa:");
        foreach (var item in sprzedaneLitry)
        {
            raport.Add($"- {item.Paliwo}: {item.SumaLitrow:F2} litrów");
            Console.WriteLine($"- {item.Paliwo}: {item.SumaLitrow:F2} litrów");
        }

        // Zestawienie B: Ile było transakcji na kwotę większą od kwotaMin dla poszczególnych paliw
        var transakcjePowyzejX = transakcje
            .Where(t => t.Kwota > kwotaMin)
            .GroupBy(t => t.IdPaliwa)
            .Select(g => new
            {
                Paliwo = Dystrybutor1.PaliwaDostepne.FirstOrDefault(p => p.IdPaliwa == g.Key)?.Nazwa ?? "Nieznane",
                LiczbaTransakcji = g.Count()
            })
            .OrderByDescending(x => x.LiczbaTransakcji)
            .ToList();

        raport.Add($"\n2. Liczba Transakcji powyżej {kwotaMin:C} wg. Rodzaju Paliwa:");
        Console.WriteLine($"\n2. Liczba Transakcji powyżej {kwotaMin:C} wg. Rodzaju Paliwa:");

        foreach (var item in transakcjePowyzejX)
        {
            raport.Add($"- {item.Paliwo}: {item.LiczbaTransakcji} transakcji");
            Console.WriteLine($"- {item.Paliwo}: {item.LiczbaTransakcji} transakcji");
        }

        // Zapis raportu do pliku
        try
        {
            File.WriteAllLines(SCIEZKA_RAPORTU, raport);
            Console.WriteLine($"Raport zapisano do pliku: {SCIEZKA_RAPORTU}");
        }
        catch (Exception ex)
        {
            ObsluzBlad($"Błąd zapisu raportu LINQ: {ex.Message}");
        }
    }
}
