﻿using SQLite;
using System;
using MeVersusMany.DataModel;
using System.Windows.Media;
using MeVersusMany.Util;
using System.Text.RegularExpressions;
using System.Linq;

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
            InitWorkoutDate(filepath);

            Update(0.0);
            Distance = GetDistanceAt(db, 60.0 * 30.0); //60 seconds * 30 minutes

            //get a random-seed out of the filename. Set the color and name of this erg with this seed
            int seed = 0;
            foreach (char letter in filepath)
            {
                seed += letter;
            }
            rnd = new Random(seed);
            Byte[] b = new Byte[3];
            rnd.NextBytes(b);
            ErgColor = Color.FromRgb(b[0], b[1], b[2]);

            RandomNameProvider.Seed = seed;
            InitName(filepath);
        }

        private void InitName(string filepath)
        {
            //Regex: session_([0-9-_]*)\.*(.*).db

            //Input: .\\session_17-10-18_18-26-14.Fred.db
            //Group 1: 17-10-18_18-26-14
            //Group 2: Fred

            //Input: .\\session_17-10-18_18-26-14.db
            //Group 1: 17-10-18_18-26-14
            //Group 2: <empty>

            var pattern = "session_([0-9-_]*)\\.*(.*).db";
            var match = Regex.Match(filepath, pattern);
            if (match.Groups.Count >= 3 && !string.IsNullOrEmpty(match.Groups[2].Value))
            {
                Name = "Ghost " + match.Groups[2].Value;
            }
            else
            {
                Name = "Ghost " + RandomNameProvider.RandomName;
            }
        }

        private void InitWorkoutDate(string filepath)
        {
            //Input: .\\session_17-10-18_18-26-14.db
            //Regex: session_([0-9-_]*).db
            //Group: 17-10-18_18-26-14

            var pattern = "session_([0-9-_]*)\\.*(.*).db";
            var match = Regex.Match(filepath, pattern);
            if(match.Groups.Count >= 2)
            { 
                WorkoutDate = DateTime.ParseExact(match.Groups[1].Value, "yy-MM-dd_HH-mm-ss", null);
            }
        }

        private double GetTotalExerciseTime(SQLiteConnection db)
        {
            return GetMaxDouble("timestamp", db);
        }

        private double GetTotalDistance(SQLiteConnection db)
        {
            return GetMaxDouble("distance", db);
        }

        private double GetDistanceAt(SQLiteConnection db, double timestamp)
        {
            return GetDoubleAt("distance", db, timestamp);
        }

        private double GetDoubleAt(string fieldName, SQLiteConnection db, double timestamp)
        {
            string selectDoubleAt = "select MAX(" + fieldName + ") from rowdata WHERE timestamp <= " + timestamp;
            SQLiteCommand selectDoubleCmd = new SQLiteCommand(db);
            selectDoubleCmd.CommandText = selectDoubleAt;

            try
            {
                return selectDoubleCmd.ExecuteScalar<double>();
            }
            catch (NullReferenceException)
            {
                //No MAX in DB, return 0.0 then (good enough for our case)
                return 0.0;
            }
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
            if (Math.Abs(lastTimestamp - timestamp) < timestampDelta)
            {
                return;
            }
            lastTimestamp = timestamp;

            //When the given timestamp exceeds our timestamp we should start again from the beginning. This way all bots will continue to row, even when the recording is already over
            int numCompleted = (int)(timestamp / TotalExerciseTime);
            if (timestamp > TotalExerciseTime)
            {
                timestamp = (timestamp % TotalExerciseTime);
            }

            //update the values to reflect what's been stored at the given timestamp
            var results = db.Query<rowdata>($"SELECT * FROM rowdata WHERE timestamp >= ? LIMIT 1;", timestamp);
            if (results.Count > 0)
            {
                Distance = results[0].distance + (numCompleted * TotalDistance);
                ExerciseTime = results[0].timestamp;
                Cadence = results[0].spm;
                PaceInSecs = results[0].pace;
                Calories = results[0].calories;
                Power = results[0].power;
                Heartrate = results[0].heartrate;

                //only query for future values when there's at least one value we can work with
                try
                {
                    var resultAvg = db.ExecuteScalar<float>($"SELECT AVG(pace) FROM rowdata WHERE timestamp <= ? AND timestamp > ?; ", timestamp, timestamp - 30.0);
                    RecentPace = resultAvg;
                }
                catch (Exception)
                {
                    //do nothing in here... FuturePaceInSecs can stay as it is
                }
            }
        }

        public bool IsErgConnected()
        {
            //A SqliteErg is always connected...
            return true;
        }

        public bool IsPlayer { get; set; } = false;
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
