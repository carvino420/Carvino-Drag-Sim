using UnityEngine;

namespace Carvino
{
    public sealed class RaceEvent
    {
        public string name;
        public string description;
        public float opponentEtSeconds;
        public float opponentReactionSeconds;
        public int winPayout;
        public int lossPayout;
        public AiDriverSpec opponent;
    }

    /// <summary>AI drivers use the same engine and tire simulation as the player; only their inputs differ.</summary>
    public sealed class AiDriverSpec
    {
        public string displayName;
        public string vehicleId;
        public string engineId;
        public int upgradeMask;
        public float reactionSeconds;
        public float launchThrottle;
        public float launchRpm;
        public float shiftRpm;
        public float shiftVariationRpm;
        public float airFuelRatio = 12.8f;
        public float ignitionTiming = 18f;
    }

    /// <summary>One selected event supplies the opponent and purse to the shared race simulation.</summary>
    public static class RaceEventSession
    {
        public static readonly RaceEvent[] Events =
        {
            new RaceEvent { name = "LOCAL GRUDGE", description = "A forgiving first-money race for fresh builds.", opponentEtSeconds = 14.20f, opponentReactionSeconds = 0.260f, winPayout = 700, lossPayout = 150, opponent = new AiDriverSpec { displayName = "MAYA — STREET HATCH", vehicleId = "hatch", engineId = "b20", upgradeMask = 11, reactionSeconds = 0.260f, launchThrottle = .74f, launchRpm = 3900f, shiftRpm = 6500f, shiftVariationRpm = 220f, airFuelRatio = 12.9f, ignitionTiming = 17f } },
            new RaceEvent { name = "TRACK NIGHT", description = "The regular bracket at Carvino Dragway.", opponentEtSeconds = 12.80f, opponentReactionSeconds = 0.185f, winPayout = 1500, lossPayout = 350, opponent = new AiDriverSpec { displayName = "DREW — K24 TURBO", vehicleId = "hatch", engineId = "k24", upgradeMask = 235, reactionSeconds = 0.185f, launchThrottle = .88f, launchRpm = 4800f, shiftRpm = 7200f, shiftVariationRpm = 115f, airFuelRatio = 12.5f, ignitionTiming = 19f } },
            new RaceEvent { name = "MONEY RUN", description = "Fast rival, bigger purse. Bring a serious setup.", opponentEtSeconds = 11.40f, opponentReactionSeconds = 0.145f, winPayout = 3000, lossPayout = 500, opponent = new AiDriverSpec { displayName = "RICO — LS PICKUP", vehicleId = "pickup", engineId = "ls_53", upgradeMask = 235, reactionSeconds = 0.145f, launchThrottle = .94f, launchRpm = 3600f, shiftRpm = 5700f, shiftVariationRpm = 70f, airFuelRatio = 12.3f, ignitionTiming = 20f } }
        };

        public static int SelectedIndex { get; private set; } = 1;
        public static RaceEvent Selected => Events[Mathf.Clamp(SelectedIndex, 0, Events.Length - 1)];
        public static void Select(int index) => SelectedIndex = Mathf.Clamp(index, 0, Events.Length - 1);
    }
}
