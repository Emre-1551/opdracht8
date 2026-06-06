namespace WinkelDomein;

public class Kassa
{
    private readonly Dictionary<string, Artikel> _artikelen;
    private readonly IBetaalTerminal _betaalTerminal;
    private readonly Stack<Kassaticket> _history = new();
    private Kassaticket _huidigTicket = null!;
    private int _ticketNummer = 1;

    public string Winkel { get; }
    public string Adres { get; }
    public string Telefoonnummer { get; }
    public string BTW { get; }
    public decimal KassaSaldo { get; private set; } = 0;

    public Kassaticket HuidigTicket => _huidigTicket;

    public Kassa(Dictionary<string, Artikel> artikelen, IBetaalTerminal betaalTerminal,
        string winkel, string adres, string telefoonnummer, string btw)
    {
        _artikelen = artikelen;
        _betaalTerminal = betaalTerminal;
        Winkel = winkel;
        Adres = adres;
        Telefoonnummer = telefoonnummer;
        BTW = btw;
        NieuwTicket();
    }

    public void NieuwTicket()
    {
        _huidigTicket = new Kassaticket(
            GenereerTicketnummer(),
            Winkel,
            Adres,
            Telefoonnummer,
            BTW
        );
    }

    public bool VoegArtikelToe(string barcode, int aantal = 1)
    {
        if (!_artikelen.TryGetValue(barcode, out var artikel))
            return false;

        _huidigTicket.VoegArtikelToe(artikel, aantal);
        return true;
    }

    public void VerwijderLaatsteArtikel()
    {
        if (_huidigTicket.Items.Count > 0)
        {
            var (artikel, _) = _huidigTicket.Items.Last();
            _huidigTicket.VerwijderArtikel(artikel.Barcode);
        }
    }

    public void MaakTicketLeeg()
    {
        _huidigTicket.Wis();
    }

    public void MaakTicketOngedaan()
    {
        if (_history.Count > 0)
            _huidigTicket = _history.Pop();
    }

    public BetaalDetails? BetalenMetKaart()
    {
        _history.Push(_huidigTicket);
        var result = _betaalTerminal.VerzoekBetaling(_huidigTicket.Totaal, Winkel);
        if (result != null)
        {
            KassaSaldo += _huidigTicket.Totaal;
            NieuwTicket();
        }
        return result;
    }

    public void BetalenMetContant(decimal bedrag)
    {
        _history.Push(_huidigTicket);
        if (bedrag >= _huidigTicket.Totaal)
        {
            KassaSaldo += _huidigTicket.Totaal;
            decimal wisselgeld = bedrag - _huidigTicket.Totaal;
            NieuwTicket();
        }
    }

    public void VoegParkeerenToe(decimal bedrag)
    {
        var parkeerartikel = new Artikel("PARKEER", "Parkeren", bedrag);
        _huidigTicket.VoegArtikelToe(parkeerartikel);
    }

    private string GenereerTicketnummer()
    {
        var nu = DateTime.Now;
        return $"{nu:yyyy.MM.dd.HH.mm.ss}.{_ticketNummer++:D3}";
    }
}
