
using Concept2;

namespace MeVersusMany.C2Connector
{
    public class C2Erg : DataModel.IErg
    {
        PerformanceMonitor pm;

        public double Distance { get; set; }
        public double ExerciseTime { get; set; }
        public uint Cadence { get; set; }
        public double PaceInSecs { get; set; }
        public uint Calories { get; set; }
        public uint Power { get; set; }
        public uint Heartrate { get; set; }

        public C2Erg(ushort givenErgAdress = 0)
        {
            //init the C2 interface
            //TODO: What if there are multiple Ergs are connected, is it a good idea to init the interface multiple times?
            PMUSBInterface.Initialize();
            PMUSBInterface.InitializeProtocol(999);

            ushort numErgs = PMUSBInterface.DiscoverPMs(PMUSBInterface.PMtype.PM3TESTER_PRODUCT_NAME);
            numErgs += PMUSBInterface.DiscoverPMs(PMUSBInterface.PMtype.PM3_PRODUCT_NAME);
            numErgs += PMUSBInterface.DiscoverPMs(PMUSBInterface.PMtype.PM3_PRODUCT_NAME2);
            numErgs += PMUSBInterface.DiscoverPMs(PMUSBInterface.PMtype.PM4_PRODUCT_NAME);
            numErgs += PMUSBInterface.DiscoverPMs(PMUSBInterface.PMtype.PM5_PRODUCT_NAME);
            if (numErgs == 0)
            {
                //TODO: This means that no erg has been found... what to do?
            }

            pm = new PerformanceMonitor(givenErgAdress)
            {
                //init everything by hand, this avoids us crashing if no connection is made to the erg
                Calories = 0,
                DeviceNumber = 0,
                Distance = 0.0f,
                DragFactor = 0,
                Heartrate = 0,
                Pace = 0.0f,
                Power = 0,
                Serial = "",
                SPM = 0,
                SPMAvg = 0.0f,
                StrokePhase = StrokePhase.Idle,
                Worktime = 0.0f,
                WorkoutState = 0
            };

            try
            {
                pm.StatusUpdate();
            }
            catch (PMUSBInterface.PMUSBException)
            {
                //No Erg found, USB exception...
            }

            //Init our properties...
            Cadence = 0;
            Calories = 0;
            Distance = 0.0;
            ExerciseTime = 0.0;
            Heartrate = 0;
            PaceInSecs = 0;
            Power = 0;
        }

        public bool IsWorkoutStarted()
        {
            pm.HighResolutionUpdate();
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

            try
            {
                pm.LowResolutionUpdate();
            }
            catch (PMUSBInterface.PMUSBException)
            {
                //No Erg found, USB exception...
            }
            
            Cadence = pm.SPM;
            Calories = pm.Calories;
            Distance = pm.Distance;
            ExerciseTime = pm.Worktime;
            Heartrate = pm.Heartrate;
            PaceInSecs = (uint)pm.Pace;
            Power = pm.Power;
        }
    }
}