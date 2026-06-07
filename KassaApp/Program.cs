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

        ToonTicket(kassa.HuidigTicket, kassa);

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
            {
                try { Console.Clear(); } catch { }
                ToonTicket(kassa.HuidigTicket, kassa);
                continue;
            }

            if (invoer.Length >= 1 && invoer.Length <= 2 && int.TryParse(invoer, out int extraAantal) && extraAantal > 0 && lastBarcode != null)
            {
                if (kassa.VoegArtikelToe(lastBarcode, extraAantal))
                {
                    try { Console.Clear(); } catch { }
                    ToonTicket(kassa.HuidigTicket, kassa);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"⚠ Kan geen {extraAantal} stuks meer toevoegen van {lastBarcode}.");
                    Console.ResetColor();
                    System.Threading.Thread.Sleep(1500);
                    try { Console.Clear(); } catch { }
                    ToonTicket(kassa.HuidigTicket, kassa);
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

                ToonTicket(kassa.HuidigTicket, kassa);

                continue;

            }



            if (invoer.Equals("H", StringComparison.OrdinalIgnoreCase))

            {
                if (kassa.AantalGeparkeerd == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("⚠ Geen geparkeerde tickets.");
                    Console.ResetColor();
                    System.Threading.Thread.Sleep(1500);
                    try { Console.Clear(); } catch { }
                    ToonTicket(kassa.HuidigTicket, kassa);
                }
                else if (kassa.HuidigTicket.Items.Count > 0)
                {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("⚠ Huidig ticket heeft items.");
                    Console.WriteLine("[P]arkeren | [K] betalen | [A]nnuleren (wissen): ");
                    Console.ResetColor();
                    string? keuze = Console.ReadLine()?.Trim().ToUpper() ?? "";

                    if (keuze == "P")
                    {
                        kassa.ParkeerTicket();
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("✓ Ticket geparkeerd.");
                        Console.ResetColor();
                        System.Threading.Thread.Sleep(1000);
                        try { Console.Clear(); } catch { }
                        ToonGeparkeerdeTickets(kassa);
                    }
                    else if (keuze == "K")
                    {
                        try { Console.Clear(); } catch { }
                        ToonTicket(kassa.HuidigTicket, kassa);
                        // Fall through to let main loop handle payment
                        continue;
                    }
                    else if (keuze == "A")
                    {
                        kassa.MaakTicketLeeg();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("✓ Ticket gewist.");
                        Console.ResetColor();
                        System.Threading.Thread.Sleep(1000);
                        try { Console.Clear(); } catch { }
                        ToonGeparkeerdeTickets(kassa);
                    }
                    else
                    {
                        try { Console.Clear(); } catch { }
                        ToonTicket(kassa.HuidigTicket, kassa);
                    }
                }
                else
                {
                    try { Console.Clear(); } catch { }
                    ToonGeparkeerdeTickets(kassa);
                }

                continue;

            }



            if (invoer.Equals("Z", StringComparison.OrdinalIgnoreCase))

            {

                kassa.MaakTicketOngedaan();

                try { Console.Clear(); } catch { }

                ToonTicket(kassa.HuidigTicket, kassa);

                continue;

            }



            if (invoer.Equals("D", StringComparison.OrdinalIgnoreCase))

            {

                kassa.VerwijderLaatsteArtikel();

                try { Console.Clear(); } catch { }

                ToonTicket(kassa.HuidigTicket, kassa);

                continue;

            }



            if (invoer.Equals("K", StringComparison.OrdinalIgnoreCase))

            {

                Console.WriteLine("\n⏳ Betaling met kaart wordt verwerkt...");

                var result = kassa.BetalenMetKaart();

                if (result != null)

                {
                    ToonTicket(kassa.HuidigTicket, kassa);
                    Console.WriteLine();
                    Console.WriteLine("═════════════════════════════════════════════════════");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(result.KaartType.PadCenter(53));
                    Console.WriteLine(result.KaartVariant.PadCenter(53));
                    Console.WriteLine(result.GemaskerdKaartnummer.PadCenter(53));
                    Console.WriteLine(result.Methode.PadCenter(53));
                    Console.ResetColor();
                    Console.WriteLine("─────────────────────────────────────────────────────");
                    Console.WriteLine($"Bedrag:     €{result.Bedrag,40:F2}");
                    Console.WriteLine($"Ref:        {result.TransactieReferentie}");
                    Console.WriteLine("─────────────────────────────────────────────────────");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"✓ Betaling ontvangen - €{result.Bedrag:F2}".PadCenter(53));
                    Console.ResetColor();
                    Console.WriteLine("═════════════════════════════════════════════════════");
                    Console.WriteLine();

                    System.Threading.Thread.Sleep(3000);

                }

                else

                {

                    Console.ForegroundColor = ConsoleColor.Red;

                    Console.WriteLine("✗ Betaling geweigerd.");

                    Console.ResetColor();

                    System.Threading.Thread.Sleep(2000);

                }

                try { Console.Clear(); } catch { }

                ToonTicket(kassa.HuidigTicket, kassa);

                continue;

            }



            if (invoer.Equals("C", StringComparison.OrdinalIgnoreCase))

            {

                Console.Write("Contant bedrag: €");

                if (decimal.TryParse(Console.ReadLine(), out decimal bedrag) && bedrag >= kassa.HuidigTicket.Totaal)

                {

                    decimal wisselgeld = bedrag - kassa.HuidigTicket.Totaal;

                    var totaal = kassa.HuidigTicket.Totaal;
                    kassa.BetalenMetContant(bedrag);

                    ToonTicket(kassa.HuidigTicket, kassa);
                    Console.WriteLine();
                    Console.WriteLine("═════════════════════════════════════════════════════");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Contante betaling".PadCenter(53));
                    Console.ResetColor();
                    Console.WriteLine("─────────────────────────────────────────────────────");
                    Console.WriteLine($"Bedrag:     €{totaal,40:F2}");
                    Console.WriteLine($"Betaald:    €{bedrag,40:F2}");
                    Console.WriteLine($"Wisselgeld: €{wisselgeld,40:F2}");
                    Console.WriteLine("─────────────────────────────────────────────────────");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"✓ Betaling ontvangen - €{totaal:F2}".PadCenter(53));
                    Console.ResetColor();
                    Console.WriteLine("═════════════════════════════════════════════════════");
                    Console.WriteLine();

                    System.Threading.Thread.Sleep(3000);

                }

                else

                {

                    Console.ForegroundColor = ConsoleColor.Red;

                    Console.WriteLine("✗ Onvoldoende bedrag.");

                    Console.ResetColor();

                    System.Threading.Thread.Sleep(1500);

                }

                try { Console.Clear(); } catch { }

                ToonTicket(kassa.HuidigTicket, kassa);

                continue;

            }



            if (invoer.Equals("P", StringComparison.OrdinalIgnoreCase))

            {

                if (kassa.HuidigTicket.Items.Count == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("⚠ Kan geen lege ticket parkeren.");
                    Console.ResetColor();
                    System.Threading.Thread.Sleep(1500);
                }
                else
                {
                    kassa.ParkeerTicket();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("✓ Ticket geparkeerd. Nieuw ticket gereed.");
                    Console.ResetColor();
                    System.Threading.Thread.Sleep(1500);
                }

                try { Console.Clear(); } catch { }

                ToonTicket(kassa.HuidigTicket, kassa);

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

                            ToonTicket(kassa.HuidigTicket, kassa);

                        }

                        else

                        {

                            Console.ForegroundColor = ConsoleColor.Yellow;

                            Console.WriteLine($"⚠ Artikel met barcode '{invoer}' niet gevonden.");

                            Console.ResetColor();

                            System.Threading.Thread.Sleep(1500);

                            try { Console.Clear(); } catch { }

                            ToonTicket(kassa.HuidigTicket, kassa);

                        }

                    }

                }

                continue;

            }



            if (kassa.VoegArtikelToe(invoer))

            {
                lastBarcode = invoer;

                try { Console.Clear(); } catch { }

                ToonTicket(kassa.HuidigTicket, kassa);

            }

            else

            {

                Console.ForegroundColor = ConsoleColor.Yellow;

                Console.WriteLine($"⚠ Artikel met barcode '{invoer}' niet gevonden.");

                Console.ResetColor();

                System.Threading.Thread.Sleep(1500);

                try { Console.Clear(); } catch { }

                ToonTicket(kassa.HuidigTicket, kassa);

            }

        }

    }



    private static void ToonTicket(Kassaticket ticket, Kassa kassa)

    {
        Console.WriteLine(ticket.GenereerTicketTekst());

    }



    private static void ToonHelpTekst()

    {

        Console.WriteLine();

        Console.WriteLine("<scan barcode> | <1-2 digit aantal><Enter> voor extra van laatst gescande");

        Console.WriteLine("[D]<Enter> = verwijderen | [Z]<Enter> = undo-laatste");

        Console.WriteLine("[K]<Enter> = betalen met Kaart | [C]<Enter> = betaald met Cash");

        Console.WriteLine("[P]<Enter> = parkeren | [H]<Enter> = herstellen ticket | [A]<Enter> = afbreken");

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

        Console.WriteLine("  • Voor extra stuks: 3<Enter> of 03<Enter> (voegt meer van laatst gescande)");
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

    private static void ToonGeparkeerdeTickets(Kassa kassa)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Geparkeerde tickets:");
        Console.ResetColor();

        for (int i = 0; i < kassa.ParkeerdeTickets.Count; i++)
        {
            var ticket = kassa.ParkeerdeTickets[i];
            var aantalProducten = ticket.Items.Count;
            var productLabel = aantalProducten == 1 ? "product" : "producten";
            Console.WriteLine($"{i + 1}. #{ticket.Ticketnummer} ({aantalProducten} {productLabel})");
        }

        Console.WriteLine();
        Console.Write("Ticket nummer kiezen (1-{0}) of [A] annuleren: ", kassa.AantalGeparkeerd);

        string? invoer = Console.ReadLine()?.Trim() ?? "";

        if (invoer.Equals("A", StringComparison.OrdinalIgnoreCase))
        {
            try { Console.Clear(); } catch { }
            return;
        }

        if (int.TryParse(invoer, out int keuze) && keuze >= 1 && keuze <= kassa.AantalGeparkeerd)
        {
            var opgehaald = kassa.HaalGeparkeerdeTicketOp(keuze - 1);
            if (opgehaald != null)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✓ Ticket {opgehaald.Ticketnummer} hersteld.");
                Console.ResetColor();
                System.Threading.Thread.Sleep(1500);
            }
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("✗ Ongeldige keuze.");
            Console.ResetColor();
            System.Threading.Thread.Sleep(1500);
        }

        try { Console.Clear(); } catch { }
        ToonTicket(kassa.HuidigTicket, kassa);
    }

}

 