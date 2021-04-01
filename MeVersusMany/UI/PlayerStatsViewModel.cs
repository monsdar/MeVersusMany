using Caliburn.Micro;
using System;

namespace MeVersusMany.UI
{
    class PlayerStatsViewModel : Screen
    {   
        public PlayerStatsViewModel()
        {}

        public string Cadence { get; private set; } = "0 SPM";
        public string Calories { get; private set; } = "0 cal";
        public string Distance { get; private set; } = "0 m";
        public string ExerciseTime { get; private set; } = "00:00:00";
        public string Pace { get; private set; } = "00:00.0";
        public string Power { get; private set; } = "0 Watt";
        public string AvgPace { get; private set; } = "00:00.000";
        public string Forecast { get; private set; } = "0 m";

        public void PerformUpdate(DataModel.IErg givenErg)
        {
            Cadence = givenErg.Cadence + " SPM";
            Calories = givenErg.Calories.ToString() + " cal";
            Distance = givenErg.Distance.ToString("#.") + " m";
            ExerciseTime = TimeSpan.FromSeconds(givenErg.ExerciseTime).ToString(@"hh\:mm\:ss");
            Pace = TimeSpan.FromSeconds(givenErg.PaceInSecs).ToString(@"mm\:ss");
            Power = givenErg.Power.ToString() + " Watt";

            if (givenErg.ExerciseTime > 5.0)
            {
                double avgPaceDouble = 500.0 / (givenErg.Distance / givenErg.ExerciseTime);
                AvgPace = TimeSpan.FromSeconds(avgPaceDouble).ToString(@"mm\:ss\.fff");
            }

            if(givenErg.ExerciseTime > 5.0)
            {
                double timeLeft = 1800.0 - givenErg.ExerciseTime; //TODO: Do not assume 30min, make this configurable
                double forecastDouble = givenErg.Distance + (timeLeft * (500.0 / givenErg.PaceInSecs));
                Forecast = forecastDouble.ToString("#.") + " m";
            }

            NotifyOfPropertyChange(() => AvgPace);
            NotifyOfPropertyChange(() => Cadence);
            NotifyOfPropertyChange(() => Calories);
            NotifyOfPropertyChange(() => Distance);
            NotifyOfPropertyChange(() => ExerciseTime);
            NotifyOfPropertyChange(() => Forecast);
            NotifyOfPropertyChange(() => Pace);
            NotifyOfPropertyChange(() => Power);
        }
    }
}
