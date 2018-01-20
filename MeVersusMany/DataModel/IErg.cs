
using System.Windows.Media;

namespace MeVersusMany.DataModel
{
    public interface IErg
    {
        string Name { get; set; }
        double Distance { get; set; }
        double ExerciseTime { get; set; }
        uint Cadence { get; set; }
        double PaceInSecs { get; set; }
        uint Calories { get; set; }
        uint Power { get; set; }
        uint Heartrate { get; set; }
        Color ErgColor { get; set; }

        bool IsWorkoutStarted();
        void Update(double timestamp);
    }
}
