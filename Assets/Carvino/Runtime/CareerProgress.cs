namespace Carvino
{
    /// <summary>Small local career gate for the prototype. Public competitive progression moves server-side later.</summary>
    public static class CareerProgress
    {
        public static int Wins => RaceHistory.TotalWins;
        public static string RankName => Wins >= 5 ? "DRAGWAY REGULAR" : Wins >= 2 ? "UP-AND-COMER" : "ROOKIE";
        public static bool IsEventUnlocked(int eventIndex) => eventIndex == 0 || (eventIndex == 1 ? Wins >= 1 : Wins >= 3);
        public static string UnlockText(int eventIndex) => eventIndex == 0 ? "OPEN NOW" : eventIndex == 1 ? "WIN 1 CAREER RACE" : "WIN 3 CAREER RACES";
    }
}
