namespace WinkelDomein;

public class Artikel
{
    public string Barcode { get; }
    public string Naam { get; }
    public decimal Prijs { get; }
    public decimal BtwPercentage { get; }

    public Artikel(string barcode, string naam, decimal prijs, decimal btwPercentage = 21m)
    {
        Barcode = barcode;
        Naam = naam;
        Prijs = prijs;
        BtwPercentage = btwPercentage;
    }
}
