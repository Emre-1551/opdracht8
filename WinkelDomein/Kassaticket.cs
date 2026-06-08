namespace WinkelDomein;

public class Kassaticket
{
    private readonly List<(Artikel artikel, int aantal)> _items = new();
    public IReadOnlyList<(Artikel artikel, int aantal)> Items => _items.AsReadOnly();

    public string Ticketnummer { get; set; }
    public DateTime Datum { get; }
    public string Winkel { get; }
    public string Adres { get; }
    public string Telefoonnummer { get; }
    public string BTW { get; }
    public string? Klantnaam { get; set; }

    private const decimal KlantKortingPercentage = 5m;

    public decimal Kortingsbedrag => Klantnaam != null ? BerekenSubtotaal() * KlantKortingPercentage / 100m : 0m;

    public decimal Totaal => BerekenSubtotaal() - Kortingsbedrag;

    public Kassaticket(string ticketnummer, string winkel, string adres, string telefoonnummer, string btw)
    {
        Ticketnummer = ticketnummer;
        Datum = DateTime.Now;
        Winkel = winkel;
        Adres = adres;
        Telefoonnummer = telefoonnummer;
        BTW = btw;
    }

    public void VoegArtikelToe(Artikel artikel, int aantal = 1)
    {
        var bestaande = _items.FirstOrDefault(x => x.artikel.Barcode == artikel.Barcode);
        if (bestaande != default)
        {
            _items.Remove(bestaande);
            _items.Add((bestaande.artikel, bestaande.aantal + aantal));
        }
        else
        {
            _items.Add((artikel, aantal));
        }
    }

    public void VerwijderArtikel(string barcode)
    {
        var item = _items.FirstOrDefault(x => x.artikel.Barcode == barcode);
        if (item != default)
            _items.Remove(item);
    }

    public bool VerminderArtikelMet1(string barcode)
    {
        var item = _items.FirstOrDefault(x => x.artikel.Barcode == barcode);
        if (item != default)
        {
            _items.Remove(item);
            if (item.aantal > 1)
            {
                _items.Add((item.artikel, item.aantal - 1));
            }
            return true;
        }
        return false;
    }

    public void Wis()
    {
        _items.Clear();
    }

    public Kassaticket MaakKopie()
    {
        var kopie = new Kassaticket(Ticketnummer, Winkel, Adres, Telefoonnummer, BTW);
        kopie.Klantnaam = Klantnaam;
        foreach (var (artikel, aantal) in _items)
        {
            kopie.VoegArtikelToe(artikel, aantal);
        }
        return kopie;
    }

    public string GenereerTicketTekst()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("═════════════════════════════════════════════════════");
        sb.AppendLine(Winkel.PadCenter(53));
        sb.AppendLine(Adres.PadCenter(53));
        sb.AppendLine($"Tel: {Telefoonnummer}".PadCenter(53));
        sb.AppendLine($"BTW: {BTW}".PadCenter(53));
        sb.AppendLine("═════════════════════════════════════════════════════");
        sb.AppendLine($"Ticket: {Ticketnummer}");
        sb.AppendLine($"Datum:  {Datum:yyyy-MM-dd HH:mm}");
        if (Klantnaam != null)
            sb.AppendLine($"Klant:  {Klantnaam}");
        sb.AppendLine("─────────────────────────────────────────────────────");

        if (_items.Count == 0)
        {
            sb.AppendLine("(leeg)");
            sb.AppendLine("─────────────────────────────────────────────────────");
        }
        else
        {
            sb.AppendLine();
            var btwGroepen = _items.GroupBy(x => x.artikel.BtwPercentage).ToList();

            foreach (var (artikel, aantal) in _items)
            {
                var regelPrice = artikel.Prijs * aantal;
                sb.AppendLine($"{aantal}x {artikel.Naam} € {regelPrice:F2} {artikel.BtwPercentage:F0}%");
            }

            sb.AppendLine();
            sb.AppendLine("─────────────────────────────────────────────────────");

            var subtotal = BerekenSubtotaal();
            var btwBedrag = BerekenBTWBedrag();

            sb.AppendLine($"Subtotaal excl. BTW: €{subtotal,30:F2}");
            if (Klantnaam != null)
                sb.AppendLine($"Klantenkorting 5%:   €{-Kortingsbedrag,30:F2}");
            sb.AppendLine($"BTW 21%:             €{btwBedrag,30:F2}");
            sb.AppendLine("─────────────────────────────────────────────────────");
            sb.AppendLine($"TOTAAL: €{Totaal,42:F2}");
        }

        sb.AppendLine("═════════════════════════════════════════════════════");

        return sb.ToString();
    }

    private decimal BerekenSubtotaal()
    {
        return _items.Sum(x => x.artikel.Prijs * x.aantal);
    }

    private decimal BerekenBTWBedrag()
    {
        return _items.Sum(x => (x.artikel.Prijs * x.aantal * x.artikel.BtwPercentage) / 100m);
    }
}

public static class StringExtensions
{
    public static string PadCenter(this string text, int width)
    {
        int totalPadding = width - text.Length;
        int leftPadding = totalPadding / 2;
        return text.PadLeft(text.Length + leftPadding).PadRight(width);
    }
}
