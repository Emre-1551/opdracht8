namespace WinkelDomein;

public interface IBetaalTerminal
{
    BetaalDetails? VerzoekBetaling(decimal bedrag, string boodschap);
}
