
using System;
using System.Windows.Media;

namespace MeVersusMany.DataModel
{
    public interface IErg
    {
        bool IsPlayer { get; set; }
        string Name { get; set; }
        double Distance { get; set; }
        double ExerciseTime { get; set; }
        uint Cadence { get; set; }
        double PaceInSecs { get; set; }
        double RecentPace { get; set; }
        uint Calories { get; set; }
        uint Power { get; set; }
        uint Heartrate { get; set; }
        Color ErgColor { get; set; }
        DateTime WorkoutDate { get; set; }

        bool IsWorkoutStarted();
        bool IsErgConnected();
        void Update(double timestamp);
    }
}
