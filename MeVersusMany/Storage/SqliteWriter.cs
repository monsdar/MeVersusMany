
using SQLite;
using System.Globalization;

namespace MeVersusMany
{
    public class SqliteWriter
    {
        bool dryRun = false;
        string filename = "";
        SQLiteConnection db = null;

        public SqliteWriter(bool dryRun = false)
        {
            filename = "recordings/session_" + System.DateTime.Now.ToString("yy-MM-dd_HH-mm-ss") + ".db";
            this.dryRun = dryRun;
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
            if(dryRun)
            {
                return;
            }

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
