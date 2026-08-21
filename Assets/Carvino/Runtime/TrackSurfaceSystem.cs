namespace Carvino
{
    public enum TrackSurfaceType { PreppedStrip, Street, DampStreet }

    public sealed class TrackSurfaceSpec
    {
        public TrackSurfaceType type;
        public string displayName;
        public string description;
        public float gripMultiplier;
        public float rollingResistance;
    }

    public static class TrackSurfaceCatalog
    {
        public static readonly TrackSurfaceSpec PreppedStrip = new TrackSurfaceSpec { type = TrackSurfaceType.PreppedStrip, displayName = "PREPPED STRIP", description = "Rubbered-in launch surface with the best repeatability.", gripMultiplier = 1.20f, rollingResistance = .015f };
        public static readonly TrackSurfaceSpec Street = new TrackSurfaceSpec { type = TrackSurfaceType.Street, displayName = "STREET", description = "Uneven, dusty pavement. Tire setup and throttle control matter more.", gripMultiplier = .85f, rollingResistance = .025f };
        public static readonly TrackSurfaceSpec DampStreet = new TrackSurfaceSpec { type = TrackSurfaceType.DampStreet, displayName = "DAMP STREET", description = "Low-grip surface for risky test passes.", gripMultiplier = .60f, rollingResistance = .035f };

        public static TrackSurfaceSpec Get(TrackSurfaceType type)
        {
            switch (type)
            {
                case TrackSurfaceType.Street: return Street;
                case TrackSurfaceType.DampStreet: return DampStreet;
                default: return PreppedStrip;
            }
        }
    }

    /// <summary>Temporary race-day selection; both player and AI read this same value.</summary>
    public static class RaceSurfaceSession
    {
        public static TrackSurfaceType SelectedType { get; private set; } = TrackSurfaceType.PreppedStrip;
        public static TrackSurfaceSpec Selected => TrackSurfaceCatalog.Get(SelectedType);
        public static void Select(TrackSurfaceType type) => SelectedType = type;
    }
}
