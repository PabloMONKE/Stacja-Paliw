public class Paliwo
{
    public int IdPaliwa { get; set; }
    public string Nazwa { get; set; }
    public decimal CenaZaLitr { get; set; }
    // W JSON nie potrzebujemy metod FromString/ToString
}
