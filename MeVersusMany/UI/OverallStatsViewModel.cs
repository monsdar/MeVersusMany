using Caliburn.Micro;
using MeVersusMany.DataModel;
using MeVersusMany.Storage;
using System;
using System.Collections.Generic;

namespace MeVersusMany.UI
{
    class OverallStatsViewModel : Screen
    {
        private double recordedTotalDistance = 0.0;
        private double recordedTotalExTime = 0.0;
        
        public OverallStatsViewModel(List<IErg> recordErgs)
        {
            foreach (SqliteErg erg in recordErgs)
            {
                recordedTotalDistance += erg.TotalDistance;
                recordedTotalExTime += erg.TotalExerciseTime;
            }
            double avgPace = GetPace(recordedTotalDistance, recordedTotalExTime);
            
            TotalDistanceStr = recordedTotalDistance.ToString("#.") + " m";
            TotalExTimeStr = TimeSpan.FromSeconds(recordedTotalExTime).ToString(@"hh\:mm\:ss");
            TotalAvgPaceStr = TimeSpan.FromSeconds(avgPace).ToString(@"mm\:ss\.ff");
            PositionStr = (recordErgs.Count+1) + "/" + (recordErgs.Count+1);
        }

        public string TotalDistanceStr { get; set; }
        public string TotalExTimeStr { get; set; }
        public string TotalAvgPaceStr { get; set; }
        public string PositionStr { get; set;  }

        private double GetPace(double distance, double time)
        {
            return 500.0 / (distance / time);
        }

        internal void PerformUpdate(IErg playerErg, List<IErg> recordedErgs)
        {
            UpdatePosition(playerErg, recordedErgs);

            double totalDistanceDouble = recordedTotalDistance + playerErg.Distance;
            double totalExTimeDouble = recordedTotalExTime + playerErg.ExerciseTime;
            double avgPaceDouble = 500.0 / (totalDistanceDouble / totalExTimeDouble);
            TotalDistanceStr = totalDistanceDouble.ToString("#.") + " m";
            TotalExTimeStr = TimeSpan.FromSeconds(totalExTimeDouble).ToString(@"hh\:mm\:ss");
            TotalAvgPaceStr = TimeSpan.FromSeconds(avgPaceDouble).ToString(@"mm\:ss\.fff");

            NotifyOfPropertyChange(() => PositionStr);
            NotifyOfPropertyChange(() => TotalDistanceStr);
            NotifyOfPropertyChange(() => TotalExTimeStr);
            NotifyOfPropertyChange(() => TotalAvgPaceStr);
        }

        private void UpdatePosition(IErg playerErg, List<IErg> recordedErgs)
        {
            int numOfBetterErgs = 1; //start at 1, we need to put ourself in the ranking too ;)
            foreach (IErg erg in recordedErgs)
            {
                if (playerErg.Distance < erg.Distance)
                {
                    numOfBetterErgs++;
                }
            }
            PositionStr = numOfBetterErgs + "/" + (recordedErgs.Count + 1);
        }
    }
}
