using Caliburn.Micro;
using System.Windows.Media;
using System;
using MeVersusMany.C2Connector;
using MeVersusMany.Storage;
using System.Collections.Generic;
using System.IO;
using MeVersusMany.DataModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

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



        //this is for disabling sleep and screesnaver for the applications runtime
        //from https://stackoverflow.com/a/2284720
        public const uint ES_CONTINUOUS = 0x80000000;
        public const uint ES_SYSTEM_REQUIRED = 0x00000001;
        public const uint ES_DISPLAY_REQUIRED = 0x00000002;
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint SetThreadExecutionState([In] uint esFlags);

        public ShellViewModel()
        {
            DisplayName = "MeVersusMany";

            SetThreadExecutionState(ES_CONTINUOUS | ES_DISPLAY_REQUIRED | ES_SYSTEM_REQUIRED);

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
            SetThreadExecutionState(ES_CONTINUOUS);
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
