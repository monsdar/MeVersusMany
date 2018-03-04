using Caliburn.Micro;
using System;

namespace MeVersusMany.UI
{
    class PlayerStatsViewModel : Screen
    {   
        public PlayerStatsViewModel()
        {
            Cadence = "0 SPM";
            Calories = "0 cal";
            Distance = "0 m";
            ExerciseTime = "00:00:00";
            Pace = "00:00.0";
            Power = "0 Watt";
            AvgPace = "00:00.000";
            Forecast = "0 m";
        }

        public string Cadence { get; private set; }
        public string Calories { get; private set; }
        public string Distance { get; private set; }
        public string ExerciseTime { get; private set; }
        public string Pace { get; private set; }
        public string Power { get; private set; }

        public string AvgPace { get; private set; }
        public string Forecast { get; private set; }

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
