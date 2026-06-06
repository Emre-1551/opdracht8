using WinkelDomein;

using BetaalSysteemMock;



namespace KassaApp;



public class Program

{

    public static void Main(string[] args)

    {

        Console.OutputEncoding = System.Text.Encoding.UTF8;



        var artikelen = new Dictionary<string, Artikel>

        {

            { "123456", new Artikel("123456", "Appels (1kg)", 2.50m) },

            { "234567", new Artikel("234567", "Brood", 1.80m) },

            { "345678", new Artikel("345678", "Melk (1L)", 1.20m) },

            { "456789", new Artikel("456789", "Kaas (500g)", 4.50m) },

            { "567890", new Artikel("567890", "Chocolade", 3.00m) },

        };



        var betaalTerminal = new MockBetaalTerminal(minWachtMs: 1000, maxWachtMs: 3000);

        var kassa = new Kassa(

            artikelen,

            betaalTerminal,

            "WARENHUIS OVERFLOW",

            "Stapelplein 1, 9000 Gent",

            "09 234 56 78",

            "BE 0123.456.789"

        );



        ToonWelkomScherm(kassa);

        HoofdlusBedieningspaneel(kassa, artikelen);

    }



    private static void ToonWelkomScherm(Kassa kassa)

    {

        try { Console.Clear(); } catch { }

        ToonTicket(kassa.HuidigTicket);

    }



    private static void HoofdlusBedieningspaneel(Kassa kassa, Dictionary<string, Artikel> artikelen)

    {
        string? lastBarcode = null;

        while (true)

        {

            ToonHelpTekst();

            Console.Write("> ");

            string? invoer = Console.ReadLine()?.Trim() ?? "";



            if (string.IsNullOrWhiteSpace(invoer))

                continue;

            if (invoer.Length == 2 && int.TryParse(invoer, out int extraAantal) && extraAantal > 0 && lastBarcode != null)
            {
                if (kassa.VoegArtikelToe(lastBarcode, extraAantal))
                {
                    try { Console.Clear(); } catch { }
                    ToonTicket(kassa.HuidigTicket);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"⚠ Kan geen {extraAantal} stuks meer toevoegen van {lastBarcode}.");
                    Console.ResetColor();
                    System.Threading.Thread.Sleep(1500);
                    try { Console.Clear(); } catch { }
                    ToonTicket(kassa.HuidigTicket);
                }
                continue;
            }

            if (invoer.Equals("A", StringComparison.OrdinalIgnoreCase))

            {

                Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine("Transactie afgebroken.");

                Console.ResetColor();

                kassa.MaakTicketLeeg();

                try { Console.Clear(); } catch { }

                ToonTicket(kassa.HuidigTicket);

                continue;

            }



            if (invoer.Equals("H", StringComparison.OrdinalIgnoreCase))

            {

                ToonHulp();

                continue;

            }



            if (invoer.Equals("Z", StringComparison.OrdinalIgnoreCase))

            {

                kassa.MaakTicketOngedaan();

                try { Console.Clear(); } catch { }

                ToonTicket(kassa.HuidigTicket);

                continue;

            }



            if (invoer.Equals("D", StringComparison.OrdinalIgnoreCase))

            {

                kassa.VerwijderLaatsteArtikel();

                try { Console.Clear(); } catch { }

                ToonTicket(kassa.HuidigTicket);

                continue;

            }



            if (invoer.Equals("K", StringComparison.OrdinalIgnoreCase))

            {

                Console.WriteLine("\n⏳ Betaling met kaart wordt verwerkt...");

                var result = kassa.BetalenMetKaart();

                if (result != null)

                {

                    Console.ForegroundColor = ConsoleColor.Green;

                    Console.WriteLine("✓ Betaling geslaagd!");

                    Console.ResetColor();

                    Console.WriteLine($"  Kaart:      {result.KaartType} {result.KaartVariant}");

                    Console.WriteLine($"  Nummer:     {result.GemaskerdKaartnummer}");

                    Console.WriteLine($"  Methode:    {result.Methode}");

                    Console.WriteLine($"  Referentie: {result.TransactieReferentie}");

                    Console.WriteLine($"  Bedrag:     €{result.Bedrag:F2}");

                    Console.WriteLine($"  Kasasaldo:  €{kassa.KassaSaldo:F2}");

                    Console.WriteLine();

                    System.Threading.Thread.Sleep(2000);

                }

                else

                {

                    Console.ForegroundColor = ConsoleColor.Red;

                    Console.WriteLine("✗ Betaling geweigerd.");

                    Console.ResetColor();

                    System.Threading.Thread.Sleep(1500);

                }

                try { Console.Clear(); } catch { }

                ToonTicket(kassa.HuidigTicket);

                continue;

            }



            if (invoer.Equals("C", StringComparison.OrdinalIgnoreCase))

            {

                Console.Write("Contant bedrag: €");

                if (decimal.TryParse(Console.ReadLine(), out decimal bedrag) && bedrag >= kassa.HuidigTicket.Totaal)

                {

                    decimal wisselgeld = bedrag - kassa.HuidigTicket.Totaal;

                    kassa.BetalenMetContant(bedrag);

                    Console.ForegroundColor = ConsoleColor.Green;

                    Console.WriteLine($"✓ Betaald met contant. Wisselgeld: €{wisselgeld:F2}");

                    Console.WriteLine($"  Kasasaldo:  €{kassa.KassaSaldo:F2}");

                    Console.ResetColor();

                    System.Threading.Thread.Sleep(2000);

                }

                else

                {

                    Console.ForegroundColor = ConsoleColor.Red;

                    Console.WriteLine("✗ Onvoldoende bedrag.");

                    Console.ResetColor();

                    System.Threading.Thread.Sleep(1500);

                }

                try { Console.Clear(); } catch { }

                ToonTicket(kassa.HuidigTicket);

                continue;

            }



            if (invoer.Equals("P", StringComparison.OrdinalIgnoreCase))

            {

                Console.Write("Parkeerdebet (€): ");

                if (decimal.TryParse(Console.ReadLine(), out decimal parkeer) && parkeer > 0)

                {

                    kassa.VoegParkeerenToe(parkeer);

                    try { Console.Clear(); } catch { }

                    ToonTicket(kassa.HuidigTicket);

                }

                continue;

            }



            if (invoer.StartsWith("[") && invoer.EndsWith("]"))

            {

                var hoeveelheid = invoer.Substring(1, invoer.Length - 2);

                if (int.TryParse(hoeveelheid, out int aantal) && aantal > 0)

                {

                    Console.Write("<scan barcode> of [barcode]<Enter>");

                    Console.Write(": ");

                    invoer = Console.ReadLine()?.Trim() ?? "";

                    if (!string.IsNullOrWhiteSpace(invoer))

                    {

                        if (kassa.VoegArtikelToe(invoer, aantal))

                        {
                            lastBarcode = invoer;

                            try { Console.Clear(); } catch { }

                            ToonTicket(kassa.HuidigTicket);

                        }

                        else

                        {

                            Console.ForegroundColor = ConsoleColor.Yellow;

                            Console.WriteLine($"⚠ Artikel met barcode '{invoer}' niet gevonden.");

                            Console.ResetColor();

                            System.Threading.Thread.Sleep(1500);

                            try { Console.Clear(); } catch { }

                            ToonTicket(kassa.HuidigTicket);

                        }

                    }

                }

                continue;

            }



            if (kassa.VoegArtikelToe(invoer))

            {
                lastBarcode = invoer;

                try { Console.Clear(); } catch { }

                ToonTicket(kassa.HuidigTicket);

            }

            else

            {

                Console.ForegroundColor = ConsoleColor.Yellow;

                Console.WriteLine($"⚠ Artikel met barcode '{invoer}' niet gevonden.");

                Console.ResetColor();

                System.Threading.Thread.Sleep(1500);

                try { Console.Clear(); } catch { }

                ToonTicket(kassa.HuidigTicket);

            }

        }

    }



    private static void ToonTicket(Kassaticket ticket)

    {

        Console.WriteLine(ticket.GenereerTicketTekst());

    }



    private static void ToonHelpTekst()

    {

        Console.WriteLine();

        Console.WriteLine("<scan barcode> | <2-digit aantal><Enter> voor extra van laatst gescande");

        Console.WriteLine("[D]<Enter> = verwijderen | [Z]<Enter> = undo-laatste");

        Console.WriteLine("[K]<Enter> = betalen met Kaart | [C]<Enter> = betaald met Cash");

        Console.WriteLine("[P]<Enter> = parkeren | [H]<Enter> = hulp | [A]<Enter> = afbreken");

    }



    private static void ToonHulp()

    {

        Console.Clear();

        Console.WriteLine("═══════════════════════════════════════════════════════════");

        Console.WriteLine("HELP - Bedieningshandleiding");

        Console.WriteLine("═══════════════════════════════════════════════════════════");

        Console.WriteLine();

        Console.WriteLine("ARTIKELEN SCANNEN:");

        Console.WriteLine("  • Voer een barcode in: 123456<Enter>");

        Console.WriteLine("  • Voor extra stuks: 03<Enter> (voegt 3 meer van laatst gescande)");
        Console.WriteLine("  • Of gebruik: [aantal]<Enter> voor een ander artikel");

        Console.WriteLine();

        Console.WriteLine("TRANSACTIE BEHEREN:");

        Console.WriteLine("  [D] - Verwijder het laatste artikel");

        Console.WriteLine("  [Z] - Maak de laatste actie ongedaan (undo)");

        Console.WriteLine("  [A] - Breek de transactie af");

        Console.WriteLine();

        Console.WriteLine("BETALING:");

        Console.WriteLine("  [K] - Betaal met PIN-kaart");

        Console.WriteLine("  [C] - Betaal met contant geld");

        Console.WriteLine("  [P] - Voeg parkeerdebet toe");

        Console.WriteLine();

        Console.WriteLine("BESCHIKBARE ARTIKELEN:");

        Console.WriteLine("  • 123456: Appels (1kg) - €2,50");

        Console.WriteLine("  • 234567: Brood - €1,80");

        Console.WriteLine("  • 345678: Melk (1L) - €1,20");

        Console.WriteLine("  • 456789: Kaas (500g) - €4,50");

        Console.WriteLine("  • 567890: Chocolade - €3,00");

        Console.WriteLine();

        Console.WriteLine("═══════════════════════════════════════════════════════════");

        Console.WriteLine("Druk op een toets om terug te gaan...");

        Console.ReadKey();

        Console.Clear();

    }

}

 