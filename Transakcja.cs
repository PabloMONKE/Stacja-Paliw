using System;

namespace StacjaBenzynowa
{
    public class Transakcja
    {
        public Guid Id { get; set; }
        public DateTime Data { get; set; }
        public RodzajPaliwa Paliwo { get; set; }
        public double Litry { get; set; }
        public decimal Cena { get; set; }
        public decimal Kwota { get; set; }
        public bool Oplacona { get; set; }
        //Do faktury
        public bool CzyFaktura { get; set; }
        public string Nabywca { get; set; } //Null jeśli paragon
        public string Adres { get; set; }
        public string Nip { get; set; }

        public override string ToString()
        {
            string dok = CzyFaktura ? $"FAKTURA ({Nip})" : "PARAGON";
            string status = Oplacona ? "OK" : "BRAK WPŁATY";
            return $"{Data} | {Paliwo} | {Litry:F2}L | {Kwota:C} | {dok} | {status}";
        }
    }
}
