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

        public override string ToString() => 
            $"{Data} | {Paliwo} | {Litry:F2}L | {Kwota:C} | {(Oplacona ? "OK" : "BRAK WPŁATY")}";
    }
}