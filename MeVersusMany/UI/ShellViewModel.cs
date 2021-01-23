using Caliburn.Micro;
using System.Windows.Media;
using System;
using MeVersusMany.C2Connector;
using MeVersusMany.Storage;
using System.Collections.Generic;
using System.IO;
using MeVersusMany.DataModel;
using System.Diagnostics;

namespace MeVersusMany.UI
{
    class ShellViewModel : Screen
    {
        //activate this for testing purposes. When activated no actual Erg-connection will be made, instead a Ghost will be used a a player
        bool dryRun = true;



        IErg c2erg = null;
        List<IErg> recordedErgs = new List<IErg>();
        SqliteWriter storage = null;
        PlayerStatsViewModel playerStats = null;
        OverallStatsViewModel overallStats = null;
        RankingViewModel ranking = null;
        IEventAggregator eventAggregator = null;

        public ShellViewModel()
        {
            DisplayName = "MeVersusMany";

            //init the EventAggregator
            eventAggregator = new EventAggregator();
            eventAggregator.Subscribe(this);

            //get a connection to the C2 ergometer

            if(dryRun)
            {
                c2erg = new SqliteErg("session_21-1-19_11-52-27.Kickstarter.db"); //NOTE: use a ghost as primary erg for testing purposes
                c2erg.IsPlayer = true;
            }
            else
            {
                c2erg = new C2Erg(0); //always work with the first connected erg (address == 0)
                c2erg.IsPlayer = true;
            }
            storage = new SqliteWriter(dryRun);

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
            ranking = new RankingViewModel(recordedErgs);

            //subscribe to the WPF Rendering event to get some sort of GameLoop
            CompositionTarget.Rendering += PerformUpdate;
        }

        private void PerformUpdate(object sender, EventArgs e)
        {
            if(c2erg.IsWorkoutStarted())
            {
                //TODO: We should update the values in a thread somewhere else... do not hog the UI-Thread with this
                //NOTE: C2Erg ignores the timestamp, Ghost-Ergs need some kind of continuous timer. Calculating the timestamp anyways is for dryRun
                var timeElapsed = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();
                c2erg.Update(timeElapsed.TotalSeconds + 120);
                foreach (IErg recordErg in recordedErgs)
                {
                    recordErg.Update(c2erg.ExerciseTime);
                }

                playerStats.PerformUpdate(c2erg);
                overallStats.PerformUpdate(c2erg, recordedErgs);
                ranking.PerformUpdate(c2erg, recordedErgs);
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

        public RankingViewModel Ranking
        {
            get
            {
                return ranking;
            }
        }
    }
}
