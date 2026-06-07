namespace WinkelDomein;

public class Kassa
{
    private readonly Dictionary<string, Artikel> _artikelen;
    private readonly IBetaalTerminal _betaalTerminal;
    private readonly Stack<Kassaticket> _history = new();
    private readonly List<Kassaticket> _parkeerdeTickets = new();
    private Kassaticket _huidigTicket = null!;
    private int _ticketNummer = 1;

    public string Winkel { get; }
    public string Adres { get; }
    public string Telefoonnummer { get; }
    public string BTW { get; }
    public decimal KassaSaldo { get; private set; } = 0;

    public Kassaticket HuidigTicket => _huidigTicket;
    public IReadOnlyList<Kassaticket> ParkeerdeTickets => _parkeerdeTickets.AsReadOnly();
    public int AantalGeparkeerd => _parkeerdeTickets.Count;

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

        _history.Push(_huidigTicket.MaakKopie());
        _huidigTicket.VoegArtikelToe(artikel, aantal);
        return true;
    }

    public void VerwijderLaatsteArtikel()
    {
        if (_huidigTicket.Items.Count > 0)
        {
            _history.Push(_huidigTicket.MaakKopie());
            var (artikel, _) = _huidigTicket.Items.Last();
            _huidigTicket.VerwijderArtikel(artikel.Barcode);
        }
    }

    public bool VerminderArtikelMet1(string barcode)
    {
        if (_artikelen.ContainsKey(barcode))
        {
            _history.Push(_huidigTicket.MaakKopie());
            return _huidigTicket.VerminderArtikelMet1(barcode);
        }
        return false;
    }

    public void MaakTicketLeeg()
    {
        if (_huidigTicket.Items.Count > 0)
        {
            _history.Push(_huidigTicket.MaakKopie());
            _huidigTicket.Wis();
        }
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

    public void ParkeerTicket()
    {
        if (_huidigTicket.Items.Count > 0)
        {
            _parkeerdeTickets.Add(_huidigTicket);
            NieuwTicket();
        }
    }

    public Kassaticket? HaalGeparkeerdeTicketOp(int index)
    {
        if (index >= 0 && index < _parkeerdeTickets.Count)
        {
            var ticket = _parkeerdeTickets[index];
            _parkeerdeTickets.RemoveAt(index);
            _huidigTicket = ticket;
            return ticket;
        }
        return null;
    }

    private string GenereerTicketnummer()
    {
        var nu = DateTime.Now;
        return $"{nu:yyyy.MM.dd.HH.mm.ss}.{_ticketNummer++:D3}";
    }
}
