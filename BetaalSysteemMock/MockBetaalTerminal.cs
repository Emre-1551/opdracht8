using WinkelDomein;

namespace BetaalSysteemMock;

/// <summary>
/// Gesimuleerde betaalterminal. Genereert willekeurig kaartgegevens.
/// De verwerkingstijd is configureerbaar via de constructor.
/// Geen console-output: de UI-laag is verantwoordelijk voor weergave.
/// </summary>
public class MockBetaalTerminal : IBetaalTerminal
{
    private readonly Random _random = new();
    private readonly int _minWachtMs;
    private readonly int _maxWachtMs;

    private static readonly (string Type, string Variant)[] Kaarten =
    {
        ("Mastercard", "Debit"),
        ("Mastercard", "Credit"),
        ("Visa", "Debit"),
        ("Visa", "Credit"),
        ("Bancontact", "Debit"),
        ("Maestro", "Debit"),
    };

    private static readonly string[] Methodes =
    {
        "Contactloos",
        "Chip + PIN",
        "NFC (telefoon)",
    };

    /// <summary>
    /// Maakt een gesimuleerde betaalterminal aan.
    /// </summary>
    /// <param name="minWachtMs">Minimale wachttijd in milliseconden (standaard 3000).</param>
    /// <param name="maxWachtMs">Maximale wachttijd in milliseconden (standaard 30000).</param>
    public MockBetaalTerminal(int minWachtMs = 3000, int maxWachtMs = 30000)
    {
        _minWachtMs = minWachtMs;
        _maxWachtMs = maxWachtMs;
    }

    public BetaalDetails? VerzoekBetaling(decimal bedrag, string boodschap)
    {
        int wachtMs = _minWachtMs + _random.Next(_maxWachtMs - _minWachtMs);
        Thread.Sleep(wachtMs);

        // ~10% kans op mislukking.
        if (_random.Next(10) == 0)
            return null;

        var (kaartType, kaartVariant) = Kaarten[_random.Next(Kaarten.Length)];
        string methode = Methodes[_random.Next(Methodes.Length)];
        string gemaskerd = $"****{_random.Next(1000, 9999)}";
        string referentie = $"TXN-{DateTime.Now:yyyyMMdd}-{_random.Next(100000, 999999)}";

        return new BetaalDetails(kaartType, kaartVariant, methode, gemaskerd, referentie, bedrag);
    }
}
