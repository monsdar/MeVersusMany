
using System;
using System.Windows.Media;
using Concept2;

namespace MeVersusMany.C2Connector
{
    public class C2Erg : DataModel.IErg
    {
        PerformanceMonitor pm;

        public bool IsPlayer { get; set; } = true;
        public string Name { get; set; }
        public double Distance { get; set; }
        public double ExerciseTime { get; set; }
        public uint Cadence { get; set; }
        public double PaceInSecs { get; set; }
        public double RecentPace { get; set; } = 0.0;
        public uint Calories { get; set; }
        public uint Power { get; set; }
        public uint Heartrate { get; set; }
        public Color ErgColor { get; set; }
        public DateTime WorkoutDate { get; set; }

        public C2Erg()
        {
            //Init our properties...
            Cadence = 0;
            Calories = 0;
            Distance = 0.0;
            ExerciseTime = 0.0;
            Heartrate = 0;
            PaceInSecs = 0;
            Power = 0;
            Name = "Player"; //TODO: Let the user set the name
            ErgColor = Color.FromRgb(0, 0, 0);
            WorkoutDate = DateTime.Now;
        }

        public PerformanceMonitor InitErg(ushort ergAddress)
        {
            //init the C2 interface
            //TODO: What if there are multiple Ergs connected, is it a good idea to init the interface multiple times?
            PMUSBInterface.Initialize();
            PMUSBInterface.InitializeProtocol(999);

            ushort numErgs = PMUSBInterface.DiscoverPMs(PMUSBInterface.PMtype.PM3TESTER_PRODUCT_NAME);
            numErgs += PMUSBInterface.DiscoverPMs(PMUSBInterface.PMtype.PM3_PRODUCT_NAME);
            numErgs += PMUSBInterface.DiscoverPMs(PMUSBInterface.PMtype.PM3_PRODUCT_NAME2);
            numErgs += PMUSBInterface.DiscoverPMs(PMUSBInterface.PMtype.PM4_PRODUCT_NAME);
            numErgs += PMUSBInterface.DiscoverPMs(PMUSBInterface.PMtype.PM5_PRODUCT_NAME);
            if (numErgs == 0)
            {
                //do not init the PM before there's an actual PM connected
                throw new PMUSBInterface.PMUSBException("No erg found", -1);
            }
            return new PerformanceMonitor(ergAddress);
        }

        public bool IsErgConnected()
        {
            if(pm != null)
            {
                return true;
            }

            try
            {
                //this has to be done initially to get a connection to the PM
                pm = InitErg(0); //always work with the first connected erg (address == 0)
                return true;
            }
            catch (PMUSBInterface.PMUSBException ex)
            {
                //No erg connected yet, so workout is obviously not started...
                return false;
            }

            
        }

        public bool IsWorkoutStarted()
        {
            try
            {
                //this has to be done initially to get a connection to the PM
                //fails with PMUSBInterface.PMUSBException if there's no success in connecting the Erg
                if (pm == null)
                {
                    pm = InitErg(0); //always work with the first connected erg (address == 0)
                }

                //Try to get the erg status inited first...
                if (!pm.IsStatusInited)
                {
                    pm.StatusUpdate();
                }

                //high res update to get the current workout state
                pm.HighResolutionUpdate();
            }
            catch(PMUSBInterface.PMUSBException ex)
            {
                //No erg connected yet, so workout is obviously not started...
                return false;
            }

            var state = pm.WorkoutState;
            if (state != 0)
            {
                return true;
            }

            return false;
        }

        public void Update(double timestamp)
        {
            //NOTE: Timestamp will be ignored...

            pm.LowResolutionUpdate(); //throws PMUSBInterface.PMUSBException
            
            Cadence = pm.SPM;
            Calories = pm.Calories;
            Distance = pm.Distance;
            ExerciseTime = pm.Worktime;
            Heartrate = pm.Heartrate;
            PaceInSecs = (uint)pm.Pace;
            Power = pm.Power;
            RecentPace = 500.0 / (Distance / ExerciseTime); //this is not recent, but total. It's the easiest way to calc something usable with what we've got.
        }
    }
}