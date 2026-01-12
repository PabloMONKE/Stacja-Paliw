using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StacjaBenzynowa
{
    public class Dystrybutor
    {
        private List<Transakcja> _baza = new();
        private Dictionary<RodzajPaliwa, decimal> _ceny;
        private Transakcja _aktywna; //Bieżące, jeszcze nie zakończone tankowanie

        private const string PlikBazy = "baza.json";
        private const string PlikCennik = "ceny.json";

        public event Action<Transakcja> Kradziez;

        public Dystrybutor() => InicjujCennik();

        //Obsługa danych
        private void InicjujCennik()
        {
            var json = File.ReadAllText(PlikCennik);
            
            var opcje = new JsonSerializerOptions 
            { 
                Converters = { new JsonStringEnumConverter() },
                PropertyNameCaseInsensitive = true
            };
            //Zamiana tekstu JSON na obiekt Dictionary (Słownik)
            _ceny = JsonSerializer.Deserialize<Dictionary<RodzajPaliwa, decimal>>(json, opcje);
        }

        public void WczytajBaze()
        {
            var json = File.ReadAllText(PlikBazy);
            
            var opcje = new JsonSerializerOptions 
            { 
                Converters = { new JsonStringEnumConverter() } 
            };

            _baza = JsonSerializer.Deserialize<List<Transakcja>>(json, opcje) ?? new List<Transakcja>();
        }

        private void Zapisz()
        {
            var opcje = new JsonSerializerOptions 
            { 
                WriteIndented = true, 
                Converters = { new JsonStringEnumConverter() } 
            };
            var json = JsonSerializer.Serialize(_baza, opcje);
            File.WriteAllText(PlikBazy, json);
        }

        // --- LOGIKA ---
        public void Tankuj(RodzajPaliwa typ, double litry)
        {
            // Jeśli ktoś zaczął nowe tankowanie, a poprzednie nie jest opłacone -> kradzież
            if (_aktywna != null && !_aktywna.Oplacona) Odjazd();

            var cena = _ceny[typ];
            var suma = cena * (decimal)litry;

            //Tworzenie nowej transakcji
            _aktywna = new Transakcja
            {
                Id = Guid.NewGuid(), Data = DateTime.Now, Paliwo = typ,
                Litry = litry, Cena = cena, Kwota = suma, Oplacona = false
            };

            Console.WriteLine($"\n[Dystrybutor] {litry}L {typ} (x{cena:C}). Razem: {suma:C}");
        }

        public void Zaplac()
        {
            if (_aktywna == null || _aktywna.Oplacona) return;

            Console.WriteLine("[Terminal] Płatność OK.");
            _aktywna.Oplacona = true;
            
            _baza.Add(_aktywna);
            Zapisz();
            _aktywna = null;
        }

        public void Odjazd()
        {
            if (_aktywna == null) return;

            if (_aktywna.Oplacona)
            {
                _aktywna = null;
            }
            else
            {
                _baza.Add(_aktywna);
                Zapisz();
                Kradziez?.Invoke(_aktywna); //Wywołania zdarzenia kradzież
                _aktywna = null;
            }
        }

        // --- RAPORTY (LINQ) ---
        public void RaportIlosciowy()
        {
            Console.WriteLine("\n--- RAPORT ILOŚCIOWY ---");
            // LINQ: Filtrujemy tylko opłacone -> Grupujemy po paliwie -> Sumujemy litry i kwoty
            var dane = _baza.Where(x => x.Oplacona)
                            .GroupBy(x => x.Paliwo)
                            .Select(g => new { Typ = g.Key, Litry = g.Sum(x => x.Litry), Kasa = g.Sum(x => x.Kwota) });

            foreach (var d in dane)
                Console.WriteLine($"{d.Typ,-6} | {d.Litry,8:F2} L | {d.Kasa,8:C}");
        }

        public void RaportKwotowy(decimal minKwota)
        {
            Console.WriteLine($"\n--- RAPORT > {minKwota:C} ---");
            // LINQ: Filtrujemy kwoty większe niż X -> Sortujemy od najnowszych
            var lista = _baza.Where(x => x.Kwota > minKwota)
                             .OrderByDescending(x => x.Data)
                             .ToList();

            Console.WriteLine($"{"DATA",-16} | {"TYP",-6} | {"KWOTA",-9} | STATUS");
            Console.WriteLine(new string('-', 50));

            foreach (var t in lista)
            {
                // Kolorujemy na czerwono, jeśli to kradzież
                if (!t.Oplacona) Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{t.Data,-16:yyyy-MM-dd HH:mm} | {t.Paliwo,-6} | {t.Kwota,9:C} | {(t.Oplacona ? "OK" : "! KRADZIEŻ !")}");
                Console.ResetColor();
            }
        }
    }
}