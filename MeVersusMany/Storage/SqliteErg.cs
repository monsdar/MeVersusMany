using SQLite;
using System;
using MeVersusMany.DataModel;
using System.Windows.Media;

namespace MeVersusMany.Storage
{
    class SqliteErg : IErg
    {
        double lastTimestamp = -1.0; //start at a low value so the first update goes through in every case
        double timestampDelta = 0.01; //how often should we update?
        private Random rnd;

        SQLiteConnection db = null;

        public SqliteErg(string filepath)
        {
            db = new SQLiteConnection(filepath);

            TotalDistance = GetTotalDistance(db);
            TotalExerciseTime = GetTotalExerciseTime(db);

            Update(0.0);
            Distance = TotalDistance; //Put in the total distance, this will be displayed in the rankings initially (bit hacky, I know...)
            Name = "Recording";

            //get a random-seed out of the filename. Set the color of this erg with this seed
            int seed = 0;
            foreach (char letter in filepath)
            {
                seed += letter;
            }
            rnd = new Random(seed);
            Byte[] b = new Byte[3];
            rnd.NextBytes(b);
            ErgColor = Color.FromRgb(b[0], b[1], b[2]);
        }

        private double GetTotalExerciseTime(SQLiteConnection db)
        {
            return GetMaxDouble("timestamp", db);
        }

        private double GetTotalDistance(SQLiteConnection db)
        {
            return GetMaxDouble("distance", db);
        }

        private double GetMaxDouble(string fieldName, SQLiteConnection db)
        {
            string selectMaxDouble = "select MAX(" + fieldName + ") from rowdata";
            SQLiteCommand selectMaxCmd = new SQLiteCommand(db);
            selectMaxCmd.CommandText = selectMaxDouble;

            try
            {
                return selectMaxCmd.ExecuteScalar<double>();
            }
            catch (NullReferenceException)
            {
                //No MAX in DB, return 0.0 then (good enough for our case)
                return 0.0;
            }
        }

        public bool IsWorkoutStarted()
        {
            //a recorded workout is always started...
            return true;
        }

        public void Update(double timestamp)
        {
            //do not update if nothing changed (or simply not enough has changed)...
            if(Math.Abs(lastTimestamp - timestamp) < timestampDelta)
            {
                return;
            }
            lastTimestamp = timestamp;

            //update the values to reflect what's been stored at the given timestamp
            var results = db.Query<rowdata>("SELECT * FROM rowdata WHERE timestamp >= ? LIMIT 1;", (timestamp));
            if(results.Count > 0)
            {
                Distance = results[0].distance;
                ExerciseTime = results[0].timestamp;
                Cadence = results[0].spm;
                PaceInSecs = results[0].pace;
                Calories = results[0].calories;
                Power = results[0].power;
                Heartrate = results[0].heartrate;
            }
        }

        public string Name { get; set; }
        public double Distance { get; set; }
        public double ExerciseTime { get; set; }
        public uint Cadence { get; set; }
        public double PaceInSecs { get; set; }
        public uint Calories { get; set; }
        public uint Power { get; set; }
        public uint Heartrate { get; set; }
        public Color ErgColor { get; set; }

        public double TotalDistance { get; set; }
        public double TotalExerciseTime { get; set; }
        public double TotalAvgPace
        {
            get
            {
                return 500.0 / (TotalDistance / TotalExerciseTime);
            }
        }
    }
}
