
namespace MeVersusMany.DataModel
{
    public interface IErg
    {
        double Distance { get; set; }
        double ExerciseTime { get; set; }
        uint Cadence { get; set; }
        double PaceInSecs { get; set; }
        uint Calories { get; set; }
        uint Power { get; set; }
        uint Heartrate { get; set; }

        bool IsWorkoutStarted();
        void Update(double timestamp);
    }
}
