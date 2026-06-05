using WinkelDomein;
using BetaalSysteemMock;

// ════════════════════════════════════════════════════════════════════
//  Demo: Hoe de MockBetaalTerminal te gebruiken
// ════════════════════════════════════════════════════════════════════
//
//  Dit project toont hoe je de gesimuleerde betaalterminal kan
//  inzetten om betalingen te testen zonder echte hardware.
//
//  DEPENDENCY INVERSION
//  ────────────────────
//  De interface IBetaalTerminal is gedefinieerd in WinkelDomein
//  (de domeinlaag). De implementatie MockBetaalTerminal zit in
//  BetaalSysteemMock (een aparte library).
//
//  Waarom?
//  - De domeinlaag (Kassa, Kassaticket, ...) hangt NIET af van
//    de concrete terminal. Ze kent enkel de interface.
//  - De KassaApp bepaalt welke implementatie wordt gebruikt.
//    Dit heet "dependency inversion": de high-level module
//    (domein) definieert wát nodig is; de low-level module
//    (mock) bepaalt hóe het werkt.
//
//  In de praktijk:
//    // In KassaApp bij het opstarten:
//    IBetaalTerminal terminal = new MockBetaalTerminal();
//    var kassa = new Kassa(..., terminal, ...);
//
//  De verwerkingstijd is configureerbaar:
//    // Standaard: 3-30 seconden (realistisch)
//    IBetaalTerminal terminal = new MockBetaalTerminal();
//
//    // Snel voor testen: 50-200 ms
//    IBetaalTerminal snelleTerminal = new MockBetaalTerminal(50, 200);
//
//  Later kan je de mock vervangen door een echte terminal:
//    IBetaalTerminal terminal = new WorldlineTerminal(poort: "COM3");
//    var kassa = new Kassa(..., terminal, ...);
//
//  De Kassa-klasse verandert niet. Dat is het punt.
//
// ════════════════════════════════════════════════════════════════════

Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("╔══════════════════════════════════════════╗");
Console.WriteLine("║  Demo: MockBetaalTerminal                ║");
Console.WriteLine("╚══════════════════════════════════════════╝");
Console.WriteLine();

// ─── Stap 1: Maak een terminal aan met realistische timing ──────

Console.WriteLine("── Test 1: Enkele betaling (realistische timing) ──");
Console.WriteLine();

IBetaalTerminal terminal = new MockBetaalTerminal();

Console.WriteLine("Betaling van €25,50 wordt aangevraagd...");
Console.WriteLine("(De mock simuleert 3-30 seconden wachttijd)");
Console.WriteLine();

BetaalDetails? resultaat = terminal.VerzoekBetaling(25.50m, "Demo Winkel - Test");

if (resultaat == null)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Betaling mislukt of geweigerd.");
    Console.ResetColor();
}
else
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Betaling geslaagd!");
    Console.ResetColor();
    Console.WriteLine($"  Kaart:      {resultaat.KaartType} {resultaat.KaartVariant}");
    Console.WriteLine($"  Nummer:     {resultaat.GemaskerdKaartnummer}");
    Console.WriteLine($"  Methode:    {resultaat.Methode}");
    Console.WriteLine($"  Bedrag:     €{resultaat.Bedrag:F2}");
    Console.WriteLine($"  Referentie: {resultaat.TransactieReferentie}");
    Console.WriteLine($"  Tijdstip:   {resultaat.Tijdstip:yyyy-MM-dd HH:mm:ss}");
}

// ─── Stap 2: Snelle terminal voor bulktesten ────────────────────

Console.WriteLine();
Console.WriteLine("── Test 2: 20 snelle pogingen (50-200ms) ──");
Console.WriteLine("   (~10% kans op weigering per poging)");
Console.WriteLine();

// Korte tijdspannes zodat de demo niet lang duurt.
IBetaalTerminal snelleTerminal = new MockBetaalTerminal(minWachtMs: 50, maxWachtMs: 200);

int geslaagd = 0;
int mislukt = 0;

for (int i = 1; i <= 20; i++)
{
    Console.Write($"  Poging {i,2}: ");
    BetaalDetails? r = snelleTerminal.VerzoekBetaling(10.00m, "Snelle test");
    if (r == null)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("GEWEIGERD");
        Console.ResetColor();
        mislukt++;
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"OK — {r.KaartType} {r.KaartVariant} ({r.Methode})");
        Console.ResetColor();
        geslaagd++;
    }
}

Console.WriteLine();
Console.WriteLine($"  Resultaat: {geslaagd} geslaagd, {mislukt} geweigerd.");