using System;

public class Transakcja
{
    public int IdTransakcji { get; set; }
    public DateTime DataCzas { get; set; }
    public int IdPaliwa { get; set; }
    public decimal Litry { get; set; }
    public decimal Kwota { get; set; }
    public bool CzyZaplacono { get; set; }
    // W JSON nie potrzebujemy metod FromString/ToString
}
