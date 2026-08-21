using UnityEngine;

namespace Carvino
{
    /// <summary>Local prototype time-slip records. Production competitive records will move to a server authority.</summary>
    public static class RaceHistory
    {
        public static string BuildKey(DragBuild build, TrackSurfaceSpec surface, RaceDistanceSpec distance)
        {
            return build.vehicle.id + "." + build.engine.id + "." + GarageSession.UpgradeMask + "." + surface.type + "." + distance.type;
        }

        public static float BestEt(DragBuild build, TrackSurfaceSpec surface, RaceDistanceSpec distance) => PlayerPrefs.GetFloat("carvino.pb.et." + BuildKey(build, surface, distance), 0f);
        public static float BestTrapMph(DragBuild build, TrackSurfaceSpec surface, RaceDistanceSpec distance) => PlayerPrefs.GetFloat("carvino.pb.mph." + BuildKey(build, surface, distance), 0f);
        public static int TotalPasses => PlayerPrefs.GetInt("carvino.history.passes", 0);
        public static int TotalWins => PlayerPrefs.GetInt("carvino.history.wins", 0);
        public static int CareerWins => PlayerPrefs.GetInt("carvino.history.careerWins", 0);

        public static bool RecordCompletedPass(DragBuild build, DragSimulation simulation, TrackSurfaceSpec surface, RaceDistanceSpec distance, bool won, bool isCareerEvent)
        {
            string key = BuildKey(build, surface, distance);
            float previousBest = BestEt(build, surface, distance);
            bool personalBest = previousBest <= 0f || simulation.ElapsedSeconds < previousBest;
            if (personalBest) PlayerPrefs.SetFloat("carvino.pb.et." + key, simulation.ElapsedSeconds);
            PlayerPrefs.SetFloat("carvino.pb.mph." + key, Mathf.Max(BestTrapMph(build, surface, distance), simulation.FinishTrapMph));
            PlayerPrefs.SetInt("carvino.history.passes", TotalPasses + 1);
            if (won) PlayerPrefs.SetInt("carvino.history.wins", TotalWins + 1);
            if (won && isCareerEvent) PlayerPrefs.SetInt("carvino.history.careerWins", CareerWins + 1);
            PlayerPrefs.Save();
            return personalBest;
        }

        public static void RecordFailure()
        {
            PlayerPrefs.SetInt("carvino.history.passes", TotalPasses + 1);
            PlayerPrefs.Save();
        }
    }
}
