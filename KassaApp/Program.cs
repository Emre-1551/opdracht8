using WinkelDomein;

using BetaalSysteemMock;



namespace KassaApp;



public class Program

{

    public static void Main(string[] args)

    {

        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Logger.LogSystem("KassaApp opgestart.");

        var artikelen = new Dictionary<string, Artikel>

        {

            { "123456", new Artikel("123456", "Appels (1kg)", 2.50m) },

            { "234567", new Artikel("234567", "Brood", 1.80m) },

            { "345678", new Artikel("345678", "Melk (1L)", 1.20m) },

            { "456789", new Artikel("456789", "Kaas (500g)", 4.50m) },

            { "567890", new Artikel("567890", "Chocolade", 3.00m) },

        };

        Logger.LogSystem($"{artikelen.Count} producten ingeladen.");

        var betaalTerminal = new MockBetaalTerminal(minWachtMs: 1000, maxWachtMs: 3000);

        var kassa = new Kassa(

            artikelen,

            betaalTerminal,

            "WARENHUIS OVERFLOW",

            "Stapelplein 1, 9000 Gent",

            "09 234 56 78",

            "BE 0123.456.789"

        );

        Logger.LogKassa($"NIEUW TICKET {kassa.HuidigTicket.Ticketnummer}");

        ToonWelkomScherm(kassa);

        HoofdlusBedieningspaneel(kassa, artikelen);

    }



    private static void ToonWelkomScherm(Kassa kassa)

    {

        try { Console.Clear(); } catch { }

    }



    private static void HoofdlusBedieningspaneel(Kassa kassa, Dictionary<string, Artikel> artikelen)

    {

        string? lastBarcode = null;

        Kassaticket? paidTicketDisplay = null;



        while (true)

        {

            if (paidTicketDisplay != null)

            {

                ToonTicket(paidTicketDisplay, kassa);

                Console.WriteLine();

                ToonTicket(kassa.HuidigTicket, kassa);

                Console.WriteLine();

            }

            else

            {

                ToonTicket(kassa.HuidigTicket, kassa);

            }



            ToonHelpTekst(kassa);



            Console.Write("> ");



            string? invoer = Console.ReadLine()?.Trim() ?? "";



            if (string.IsNullOrWhiteSpace(invoer))

            {

                try { Console.Clear(); } catch { }

                continue;

            }



            if (invoer.Length >= 1 && invoer.Length <= 2 && int.TryParse(invoer, out int extraAantal) && extraAantal > 0 && lastBarcode != null)

            {

                if (kassa.VoegArtikelToe(lastBarcode, extraAantal))

                {

                    paidTicketDisplay = null;

                    var artikel = artikelen[lastBarcode];

                    var ticket = kassa.HuidigTicket;

                    var itemCount = ticket.Items.FirstOrDefault(x => x.artikel.Barcode == lastBarcode).aantal;

                    Logger.LogKassa($"SCAN {itemCount}x {artikel.Naam} ({lastBarcode}) op ticket {ticket.Ticketnummer}");

                    try { Console.Clear(); } catch { }

                }

                else

                {

                    Console.ForegroundColor = ConsoleColor.Yellow;

                    Console.WriteLine($"⚠ Kan geen {extraAantal} stuks meer toevoegen van {lastBarcode}.");

                    Console.ResetColor();

                    System.Threading.Thread.Sleep(1500);

                    try { Console.Clear(); } catch { }

                }

                continue;

            }



            if (invoer.Equals("A", StringComparison.OrdinalIgnoreCase))

            {

                Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine("Transactie afgebroken.");

                Console.ResetColor();



                paidTicketDisplay = null;

                var ticketToSave = kassa.HuidigTicket.MaakKopie();

                var ticketNum = kassa.HuidigTicket.Ticketnummer;

                kassa.MaakTicketLeeg();

                Logger.LogKassa($"ANNULERING ticket {ticketNum}");

                SaveAbortedTicket(ticketToSave);



                try { Console.Clear(); } catch { }



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

                        // Fall through to let main loop handle payment

                        continue;

                    }

                    else if (keuze == "A")

                    {

                        paidTicketDisplay = null;

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



                continue;

            }



            if (invoer.Equals("D", StringComparison.OrdinalIgnoreCase))

            {

                Console.Write("<scan barcode> of verwijderen: ");



                string barcode = Console.ReadLine()?.Trim() ?? "";



                if (!string.IsNullOrWhiteSpace(barcode))

                {

                    if (kassa.VerminderArtikelMet1(barcode))

                    {

                        paidTicketDisplay = null;

                        var artikel = artikelen[barcode];

                        Logger.LogKassa($"VERWIJDER 1x {artikel.Naam} ({barcode}) van ticket {kassa.HuidigTicket.Ticketnummer}");



                        try { Console.Clear(); } catch { }

                    }

                    else

                    {

                        Console.ForegroundColor = ConsoleColor.Yellow;

                        Console.WriteLine($"⚠ Artikel met barcode '{barcode}' niet op ticket.");

                        Console.ResetColor();

                        System.Threading.Thread.Sleep(1500);

                        try { Console.Clear(); } catch { }

                    }

                }

                else

                {

                    try { Console.Clear(); } catch { }

                }

                continue;

            }



            if (invoer.Equals("K", StringComparison.OrdinalIgnoreCase))

            {

                try { Console.Clear(); } catch { }

                var ticketBeforPayment = kassa.HuidigTicket.MaakKopie();



                Console.WriteLine();

                Console.WriteLine("┌─────────────────────────────────┐");

                Console.WriteLine("│      BETAALTERMINAL             │");

                Console.WriteLine("├─────────────────────────────────┤");

                Console.WriteLine($"│ Bedrag: €{ticketBeforPayment.Totaal,22:F2} │");

                Console.WriteLine("│ Bied uw kaart aan...            │");

                Console.WriteLine("└─────────────────────────────────┘");

                Console.WriteLine();



                var result = kassa.BetalenMetKaart();



                if (result != null)

                {

                    Logger.LogKassa($"BETALING ticket {ticketBeforPayment.Ticketnummer}: €{result.Bedrag:F2} ({result.KaartType})");

                    try { Console.Clear(); } catch { }

                    ToonTicket(ticketBeforPayment, kassa);

                    Console.WriteLine(result.KaartType);

                    Console.WriteLine(result.GemaskerdKaartnummer);

                    Console.WriteLine(result.Methode);

                    Console.WriteLine($"Bedrag:     €{result.Bedrag,40:F2}");

                    Console.WriteLine($"Ref:        {result.TransactieReferentie}");

                    Console.WriteLine("─────────────────────────────────────────────────────");

                    Console.ForegroundColor = ConsoleColor.Green;

                    Console.WriteLine($"✓ Betaling ontvangen - €{result.Bedrag:F2}".PadCenter(53));

                    Console.ResetColor();

                    Console.WriteLine();

                    Console.WriteLine();



                    SavePaidTicket(ticketBeforPayment, result);

                }

                else

                {

                    Console.ForegroundColor = ConsoleColor.Red;

                    Console.WriteLine("✗ Betaling geweigerd.");

                    Console.ResetColor();

                    System.Threading.Thread.Sleep(2000);

                    try { Console.Clear(); } catch { }

                }



                continue;

            }



            if (invoer.Equals("C", StringComparison.OrdinalIgnoreCase))

            {

                try { Console.Clear(); } catch { }

                var totaal = kassa.HuidigTicket.Totaal;

                var ticketBeforePayment = kassa.HuidigTicket.MaakKopie();

                var reference = $"CASH-{DateTime.Now:yyyyMMdd-HHmmss}";



                kassa.BetalenMetContant(totaal);

                Logger.LogKassa($"BETALING ticket {ticketBeforePayment.Ticketnummer}: €{totaal:F2} (Cash)");



                try { Console.Clear(); } catch { }

                ToonTicket(ticketBeforePayment, kassa);

                Console.WriteLine("═════════════════════════════════════════════════════");

                Console.WriteLine("─────────────────────────────────────────────────────");

                Console.WriteLine($"Bedrag:     €{totaal,40:F2}");

                Console.WriteLine($"Ref:        {reference}");

                Console.WriteLine("─────────────────────────────────────────────────────");

                Console.ForegroundColor = ConsoleColor.Green;

                Console.WriteLine($"✓ Betaling ontvangen - €{totaal:F2}".PadCenter(53));

                Console.ResetColor();

                Console.WriteLine("═════════════════════════════════════════════════════");

                Console.WriteLine();



                var cashPaymentDetails = new BetaalDetails("Cash", "-", "Cash", "-", reference, totaal);

                SavePaidTicket(ticketBeforePayment, cashPaymentDetails, isCardPayment: false);



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

                    var oldTicket = kassa.HuidigTicket.Ticketnummer;

                    kassa.ParkeerTicket();

                    Logger.LogKassa($"PARKEREN ticket {oldTicket}");

                    Logger.LogKassa($"NIEUW TICKET {kassa.HuidigTicket.Ticketnummer}");

                    Console.ForegroundColor = ConsoleColor.Green;

                    Console.WriteLine("✓ Ticket geparkeerd. Nieuw ticket gereed.");

                    Console.ResetColor();

                    System.Threading.Thread.Sleep(1500);

                }



                try { Console.Clear(); } catch { }



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

                        }

                        else

                        {

                            Console.ForegroundColor = ConsoleColor.Yellow;

                            Console.WriteLine($"⚠ Artikel met barcode '{invoer}' niet gevonden.");

                            Console.ResetColor();

                            System.Threading.Thread.Sleep(1500);

                            try { Console.Clear(); } catch { }

                        }

                    }

                }

                continue;

            }



            if (kassa.VoegArtikelToe(invoer))

            {

                paidTicketDisplay = null;

                var artikel = artikelen[invoer];

                var ticket = kassa.HuidigTicket;

                var itemCount = ticket.Items.FirstOrDefault(x => x.artikel.Barcode == invoer).aantal;

                Logger.LogKassa($"SCAN {itemCount}x {artikel.Naam} ({invoer}) op ticket {ticket.Ticketnummer}");

                lastBarcode = invoer;



                try { Console.Clear(); } catch { }

            }

            else

            {

                Console.ForegroundColor = ConsoleColor.Yellow;

                Console.WriteLine($"⚠ Artikel met barcode '{invoer}' niet gevonden.");

                Console.ResetColor();

                System.Threading.Thread.Sleep(1500);

                try { Console.Clear(); } catch { }

            }

        }

    }



    private static void ToonTicket(Kassaticket ticket, Kassa kassa)

    {

        Console.WriteLine(ticket.GenereerTicketTekst());

    }



    private static void ToonHelpTekst(Kassa kassa)

    {

        Console.WriteLine();

        if (kassa.AantalGeparkeerd > 0)
        {
            var label = kassa.AantalGeparkeerd == 1 ? "ticket" : "tickets";
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"[{kassa.AantalGeparkeerd} geparkeerd {label}]");
            Console.ResetColor();
        }

        Console.WriteLine();

        Console.WriteLine("<scan barcode> of [barcode]<Enter> | [aantal extra]<Enter>");

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
                Logger.LogKassa($"HERVATTEN ticket {opgehaald.Ticketnummer}");
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
    }

    private static void SavePaidTicket(Kassaticket ticket, BetaalDetails paymentDetails, bool isCardPayment = true)
    {
        try
        {
            // Navigate up from KassaApp to solution root, then into data/tickets
            var appDir = Directory.GetCurrentDirectory();
            var solutionRoot = Directory.GetParent(appDir)?.FullName ?? appDir;
            var ticketDir = Path.Combine(solutionRoot, "data", "tickets");
            if (!Directory.Exists(ticketDir))
                Directory.CreateDirectory(ticketDir);

            // Use ticket number with kassaticket- prefix
            var filename = Path.Combine(ticketDir, $"kassaticket-{ticket.Ticketnummer}.txt");

            // Build content with ticket only
            var sb = new System.Text.StringBuilder();
            sb.Append(ticket.GenereerTicketTekst());

            // Payment details - no borders, just the info
            if (isCardPayment)
            {
                sb.AppendLine(paymentDetails.KaartType);
                sb.AppendLine(paymentDetails.GemaskerdKaartnummer);
                sb.AppendLine(paymentDetails.Methode);
                sb.AppendLine($"Bedrag:     €{paymentDetails.Bedrag,40:F2}");
                sb.AppendLine($"Ref:        {paymentDetails.TransactieReferentie}");
            }
            else
            {
                sb.AppendLine("Contante betaling");
                sb.AppendLine($"Bedrag:     €{paymentDetails.Bedrag,40:F2}");
                sb.AppendLine($"Ref:        {paymentDetails.TransactieReferentie}");
            }

            sb.AppendLine("═════════════════════════════════════════════════════");
            sb.AppendLine($"✓ Betaling ontvangen - €{paymentDetails.Bedrag:F2}".PadCenter(53));
            sb.AppendLine("═════════════════════════════════════════════════════");

            File.WriteAllText(filename, sb.ToString());
            Logger.LogSystem($"Ticket opgeslagen: kassaticket-{ticket.Ticketnummer}.txt");
        }
        catch
        {
            // Silent failure for ticket saving
        }
    }

    private static void SaveAbortedTicket(Kassaticket ticket)
    {
        try
        {
            // Navigate up from KassaApp to solution root, then into data/tickets
            var appDir = Directory.GetCurrentDirectory();
            var solutionRoot = Directory.GetParent(appDir)?.FullName ?? appDir;
            var ticketDir = Path.Combine(solutionRoot, "data", "tickets");
            if (!Directory.Exists(ticketDir))
                Directory.CreateDirectory(ticketDir);

            // Use ticket number with kassaticket- prefix
            var filename = Path.Combine(ticketDir, $"kassaticket-{ticket.Ticketnummer}.txt");

            // Build content with ticket + cancellation marker
            var sb = new System.Text.StringBuilder();
            sb.Append(ticket.GenereerTicketTekst());
            sb.AppendLine("** GEANNULEERD **");
            sb.AppendLine("═════════════════════════════════════════════════════");

            File.WriteAllText(filename, sb.ToString());
            Logger.LogSystem($"Ticket opgeslagen: kassaticket-{ticket.Ticketnummer}.txt (GEANNULEERD)");
        }
        catch
        {
            // Silent failure for ticket saving
        }
    }
}