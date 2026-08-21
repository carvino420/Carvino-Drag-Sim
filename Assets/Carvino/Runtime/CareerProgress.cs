namespace Carvino
{
    /// <summary>Small local career gate for the prototype. Public competitive progression moves server-side later.</summary>
    public static class CareerProgress
    {
        public static int Wins => RaceHistory.CareerWins;
        public static string RankName => Wins >= 9 ? "DRAGWAY FINALIST" : Wins >= 6 ? "TRACK VETERAN" : Wins >= 3 ? "UP-AND-COMER" : Wins >= 1 ? "LOCAL RACER" : "ROOKIE";

        public static bool IsEventUnlocked(int eventIndex) => Wins >= RequiredWins(eventIndex);

        public static string UnlockText(int eventIndex)
        {
            int required = RequiredWins(eventIndex);
            return required == 0 ? "OPEN NOW" : "WIN " + required + " CAREER RACE" + (required == 1 ? string.Empty : "S");
        }

        private static int RequiredWins(int eventIndex)
        {
            switch (eventIndex)
            {
                case 0: return 0;
                case 1: return 1;
                case 2: return 3;
                case 3: return 4;
                case 4: return 6;
                default: return 9;
            }
        }
    }
}
