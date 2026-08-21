namespace Carvino
{
    public enum RaceDistanceType { EighthMile, QuarterMile }

    public sealed class RaceDistanceSpec
    {
        public RaceDistanceType type;
        public string displayName;
        public float meters;
    }

    public static class RaceDistanceCatalog
    {
        public static readonly RaceDistanceSpec EighthMile = new RaceDistanceSpec { type = RaceDistanceType.EighthMile, displayName = "1/8 MILE", meters = DragSimulation.EighthMileMeters };
        public static readonly RaceDistanceSpec QuarterMile = new RaceDistanceSpec { type = RaceDistanceType.QuarterMile, displayName = "1/4 MILE", meters = DragSimulation.QuarterMileMeters };
        public static RaceDistanceSpec Get(RaceDistanceType type) => type == RaceDistanceType.EighthMile ? EighthMile : QuarterMile;
    }

    public static class RaceDistanceSession
    {
        public static RaceDistanceType SelectedType { get; private set; } = RaceDistanceType.QuarterMile;
        public static RaceDistanceSpec Selected => RaceDistanceCatalog.Get(SelectedType);
        public static void Select(RaceDistanceType type) => SelectedType = type;
    }
}
