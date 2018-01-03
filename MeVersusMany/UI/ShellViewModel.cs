using Caliburn.Micro;
using System.Windows.Media;
using System;
using MeVersusMany.C2Connector;
using MeVersusMany.Storage;
using System.Collections.Generic;
using System.IO;
using MeVersusMany.DataModel;

namespace MeVersusMany.UI
{
    class ShellViewModel : Screen
    {
        IErg c2erg = null;
        List<IErg> recordedErgs = new List<IErg>();
        SqliteWriter storage = null;
        PlayerStatsViewModel playerStats = null;
        OverallStatsViewModel overallStats = null;
        IEventAggregator eventAggregator = null;

        public ShellViewModel()
        {
            DisplayName = "MeVersusMany";

            //init the EventAggregator
            eventAggregator = new EventAggregator();
            eventAggregator.Subscribe(this);

            //get a connection to the C2 ergometer
            c2erg = new C2Erg(0); //always work with the first connected erg (address == 0)
            storage = new SqliteWriter();

            //get all recorded sessions
            string[] databaseFiles = Directory.GetFiles(".", "*.db");
            foreach (var file in databaseFiles)
            {
                var sqliteErg = new SqliteErg(file);
                recordedErgs.Add(sqliteErg);
            }
            
            //init the UI components
            playerStats = new PlayerStatsViewModel();
            overallStats = new OverallStatsViewModel(recordedErgs);

            //subscribe to the WPF Rendering event to get some sort of GameLoop
            CompositionTarget.Rendering += PerformUpdate;
        }

        private void PerformUpdate(object sender, EventArgs e)
        {
            if(c2erg.IsWorkoutStarted())
            {
                //TODO: We should update the values in a thread somewhere else... do not hog the UI-Thread with this
                c2erg.Update(0.0);
                foreach (SqliteErg recordErg in recordedErgs)
                {
                    recordErg.Update(c2erg.ExerciseTime);
                }

                playerStats.PerformUpdate(c2erg);
                overallStats.PerformUpdate(c2erg, recordedErgs);
                storage.PerformUpdate(c2erg);
            }
        }

        public void ExitApplication()
        {
            TryClose();
        }
        
        public PlayerStatsViewModel PlayerStats
        {
            get
            {
                return playerStats;
            }
        }
        
        public OverallStatsViewModel OverallStats
        {
            get
            {
                return overallStats;
            }
        }
    }
}
