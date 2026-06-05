namespace WinkelDomein;

public class BetaalDetails
{
    public string KaartType { get; }
    public string KaartVariant { get; }
    public string Methode { get; }
    public string GemaskerdKaartnummer { get; }
    public string TransactieReferentie { get; }
    public decimal Bedrag { get; }
    public DateTime Tijdstip { get; }

    public BetaalDetails(string kaartType, string kaartVariant, string methode,
        string gemaskerdKaartnummer, string transactieReferentie, decimal bedrag)
    {
        KaartType = kaartType;
        KaartVariant = kaartVariant;
        Methode = methode;
        GemaskerdKaartnummer = gemaskerdKaartnummer;
        TransactieReferentie = transactieReferentie;
        Bedrag = bedrag;
        Tijdstip = DateTime.Now;
    }
}
