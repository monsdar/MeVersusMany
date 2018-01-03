﻿
using SQLite;

namespace MeVersusMany
{
    public class SqliteWriter
    {
        string filename = "";
        SQLiteConnection db = null;

        public SqliteWriter()
        {
            filename = "session_" + System.DateTime.Now.ToString("%y-%M-%d_%H-%m-%s") + ".db";
        }

        ~SqliteWriter()
        {
            if (db != null)
            {
                db.Close();
                db.Dispose();
            }
        }

        public void PerformUpdate(DataModel.IErg givenErg)
        {
            if(db == null)
            {
                db = new SQLiteConnection(filename);
                db.CreateTable<rowdata>();
            }

            var s = db.Insert(new rowdata()
            {
                avgpace = 0.0,
                calhr = 0.0,
                calories = givenErg.Calories,
                distance = givenErg.Distance,
                heartrate = givenErg.Heartrate,
                pace = givenErg.PaceInSecs,
                power = givenErg.Power,
                spm = givenErg.Cadence,
                timestamp = givenErg.ExerciseTime
            });

        }
    }
}
