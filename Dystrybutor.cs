using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using ScottPlot;

namespace StacjaBenzynowa
{
    public class Dystrybutor
    {
        private List<Transakcja> _baza = new();
        private Dictionary<RodzajPaliwa, decimal> _ceny;
        private Transakcja _aktywna;

        private const string PlikBazy = "baza.json";
        private const string PlikCennik = "ceny.json";

        public event Action<Transakcja> Kradziez;
        public bool CzyJestAktywna => _aktywna != null && !_aktywna.Oplacona;

        public Dystrybutor() => InicjujCennik();

        // --- OBSŁUGA DANYCH ---
        private void InicjujCennik()
        {
            var json = File.ReadAllText(PlikCennik);
            var opcje = new JsonSerializerOptions 
            { 
                Converters = { new JsonStringEnumConverter() },
                PropertyNameCaseInsensitive = true
            };
            _ceny = JsonSerializer.Deserialize<Dictionary<RodzajPaliwa, decimal>>(json, opcje);
        }

        public void WczytajBaze()
        {
            var json = File.ReadAllText(PlikBazy);
            if (string.IsNullOrWhiteSpace(json)) return;

            var opcje = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };
            _baza = JsonSerializer.Deserialize<List<Transakcja>>(json, opcje) ?? new List<Transakcja>();
        }

        private void Zapisz()
        {
            var opcje = new JsonSerializerOptions { WriteIndented = true, Converters = { new JsonStringEnumConverter() } };
            var json = JsonSerializer.Serialize(_baza, opcje);
            File.WriteAllText(PlikBazy, json);
        }

        // --- LOGIKA ---
        public void Tankuj(RodzajPaliwa typ, double litry)
        {
            if (_aktywna != null && !_aktywna.Oplacona) Odjazd();

            var cena = _ceny[typ];
            var suma = cena * (decimal)litry;

            _aktywna = new Transakcja
            {
                Id = Guid.NewGuid(), Data = DateTime.Now, Paliwo = typ,
                Litry = litry, Cena = cena, Kwota = suma, Oplacona = false,
                CzyFaktura = false
            };

            Console.WriteLine($"\n[Dystrybutor] {litry}L {typ} (x{cena:C}). Razem: {suma:C}");
        }

        public void Zaplac(string nabywca = null, string adres = null, string nip = null)
        {
            if (_aktywna == null || _aktywna.Oplacona) return;

            if (!string.IsNullOrEmpty(nip))
            {
                _aktywna.CzyFaktura = true;
                _aktywna.Nabywca = nabywca;
                _aktywna.Adres = adres;
                _aktywna.Nip = nip;
                Console.WriteLine($"[Dokument] Wystawiono FAKTURĘ dla: {nabywca}");
            }
            else Console.WriteLine("[Dokument] Wystawiono PARAGON.");

            _aktywna.Oplacona = true;
            _baza.Add(_aktywna);
            Zapisz();
            _aktywna = null;
        }

        public void Odjazd()
        {
            if (_aktywna == null) return;
            if (_aktywna.Oplacona) _aktywna = null;
            else
            {
                _baza.Add(_aktywna);
                Zapisz();
                Kradziez?.Invoke(_aktywna);
                _aktywna = null;
            }
        }

        // --- RAPORTY ---
        public void RaportIlosciowy()
        {
            Console.WriteLine("\n--- RAPORT ILOŚCIOWY ---");
            
            var daneLista = _baza.Where(x => x.Oplacona)
                                 .GroupBy(x => x.Paliwo)
                                 .Select(g => new 
                                 { 
                                     Typ = g.Key, 
                                     Nazwa = g.Key.ToString(),
                                     Litry = g.Sum(x => x.Litry), 
                                     Kasa = g.Sum(x => x.Kwota) 
                                 })
                                 .ToList();

            if (daneLista.Count == 0)
            {
                Console.WriteLine("Brak danych.");
                return;
            }

            foreach (var d in daneLista)
                Console.WriteLine($"{d.Typ,-6} | {d.Litry,8:F2} L | {d.Kasa,8:C}");

            //Generowanie wykresu
            try 
            {
                double[] wartosci = daneLista.Select(x => x.Litry).ToArray();
                string[] etykiety = daneLista.Select(x => x.Nazwa).ToArray();
                double[] pozycje = DataGen.Consecutive(wartosci.Length);

                var plt = new ScottPlot.Plot(600, 400);
                plt.AddBar(wartosci, pozycje);
                plt.XTicks(pozycje, etykiety);
                plt.Title("Sprzedaż paliwa (Litry)");
                plt.YLabel("Ilość [L]");
                plt.XLabel("Rodzaj paliwa");

                string dataStr = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string nazwaPliku = $"wykres_sprzedazy_{dataStr}.png";
                
                plt.SaveFig(nazwaPliku);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n[WYKRES] Wygenerowano plik: {nazwaPliku}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OSTRZEŻENIE] Nie udało się stworzyć wykresu: {ex.Message}");
            }
        }

        public void RaportKwotowy(decimal minKwota)
        {
            Console.WriteLine($"\n--- RAPORT > {minKwota:C} ---");
            var lista = _baza.Where(x => x.Kwota > minKwota).OrderByDescending(x => x.Data).ToList();

            Console.WriteLine($"{"DATA",-16} | {"DOKUMENT",-10} | {"KWOTA",-9} | STATUS");
            Console.WriteLine(new string('-', 55));

            foreach (var t in lista)
            {
                if (!t.Oplacona) Console.ForegroundColor = ConsoleColor.Red;
                string dok = t.CzyFaktura ? "FAKTURA" : "PARAGON";
                Console.WriteLine($"{t.Data,-16:yyyy-MM-dd HH:mm} | {dok,-10} | {t.Kwota,9:C} | {(t.Oplacona ? "OK" : "! KRADZIEŻ !")}");
                if (t.CzyFaktura)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"   -> Klient: {t.Nabywca} (NIP: {t.Nip})");
                }
                Console.ResetColor();
            }
        }

        public void RaportTopKlientow()
        {
            Console.WriteLine("\n--- TOP 3 KLIENTÓW (WG LITRÓW) ---");
            var ranking = _baza
                .Where(t => t.CzyFaktura && t.Oplacona && !string.IsNullOrEmpty(t.Nabywca))
                .GroupBy(t => t.Nabywca)
                .Select(g => new { Klient = g.Key, SumaLitrow = g.Sum(x => x.Litry), SumaWydatkow = g.Sum(x => x.Kwota) })
                .OrderByDescending(x => x.SumaLitrow)
                .Take(3)
                .ToList();

            if (ranking.Count == 0)
            {
                Console.WriteLine("Brak danych o klientach.");
                return;
            }

            int m = 1;
            foreach (var r in ranking)
            {
                Console.WriteLine($"{m}. {r.Klient}");
                Console.WriteLine($"   Zatankowano: {r.SumaLitrow:F2} L | Wydano: {r.SumaWydatkow:C}");
                m++;
            }
        }
    }
}
