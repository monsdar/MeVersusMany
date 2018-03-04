/**
The MIT License (MIT)

Copyright (c) 2016 Marek GAMELASTER Kraus

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using System.Collections.Generic;

namespace Concept2
{
    public enum StrokePhase
    {
        Idle = 0,
        Catch = 1,
        Drive = 2,
        Dwell = 3,
        Recovery = 4
    }

    public class PerformanceMonitor
    {
        //Needed to calc the avg spm
        private uint nSPM = 0;
        private int nSPMReads = 0;

        public ushort DeviceNumber { get; set; }

        public PerformanceMonitor(ushort DeviceNumber)
        {
            this.DeviceNumber = DeviceNumber;
            IsStatusInited = false;

            Calories = 0;
            DeviceNumber = 0;
            Distance = 0.0f;
            DragFactor = 0;
            Heartrate = 0;
            Pace = 0.0f;
            Power = 0;
            Serial = "";
            SPM = 0;
            SPMAvg = 0.0f;
            StrokePhase = StrokePhase.Idle;
            Worktime = 0.0f;
            WorkoutState = 0;
        }

        public PMUSBInterface.CSAFECommand CreateCommand()
        {
            return new PMUSBInterface.CSAFECommand(this.DeviceNumber);
        }

        public uint DragFactor
        {
            get; set;
        }

        public float Distance
        {
            get; set;
        }

        public float Worktime
        {
            get; set;
        }
        public uint WorkoutState
        {
            get; set;
        }

        public float Pace
        {
            get; set;
        }

        public uint Power
        {
            get; set;
        }

        public uint SPM
        {
            get; set;
        }

        public float SPMAvg
        {
            get; set;
        }
        public StrokePhase StrokePhase
        {
            get; set;
        }
        public uint Heartrate
        {
            get; set;
        }
        public uint Calories
        {
            get; set;
        }
        public string Serial
        {
            get; set;
        }
        public bool IsStatusInited
        {
            get; private set;
        }

        public void StatusUpdate()
        {
            PMUSBInterface.CSAFECommand cmd = CreateCommand();
            cmd.CommandsBytes.Add((uint)PMUSBInterface.CSAFECommands.CSAFE_GETSERIAL_CMD);
            
            List<uint> data = cmd.Execute();

            uint currentbyte = 0;
            uint datalength = 0;

            if (data[(int)currentbyte] == (uint)PMUSBInterface.CSAFECommands.CSAFE_GETSERIAL_CMD)
            {
                currentbyte++;
                datalength = data[(int)currentbyte];
                currentbyte++;

                Serial += (char)data[(int)currentbyte];
                Serial += (char)data[(int)currentbyte + 1];
                Serial += (char)data[(int)currentbyte + 2];
                Serial += (char)data[(int)currentbyte + 3];
                Serial += (char)data[(int)currentbyte + 4];
                Serial += (char)data[(int)currentbyte + 5];
                Serial += (char)data[(int)currentbyte + 6];
                Serial += (char)data[(int)currentbyte + 7];
                Serial += (char)data[(int)currentbyte + 8];

                currentbyte += datalength;
            }

            IsStatusInited = true;
        }

        public void HighResolutionUpdate()
        {
            // Get the stroke state
            PMUSBInterface.CSAFECommand cmd = CreateCommand();
            cmd.CommandsBytes.Add((uint)PMUSBInterface.CSAFECommands.CSAFE_SETUSERCFG1_CMD);
            cmd.CommandsBytes.Add(0x02);
            cmd.CommandsBytes.Add((uint)PMUSBInterface.CSAFECommands.CSAFE_PM_GET_STROKESTATE);
            cmd.CommandsBytes.Add((uint)PMUSBInterface.CSAFECommands.CSAFE_PM_GET_WORKOUTSTATE);
            
            List<uint> data = cmd.Execute();
            uint currentbyte = 0;
            uint datalength = 0;

            if (data[(int)currentbyte] == (uint)PMUSBInterface.CSAFECommands.CSAFE_SETUSERCFG1_CMD)
            {
                currentbyte += 2;
            }

            if (data[(int)currentbyte] == (uint)PMUSBInterface.CSAFECommands.CSAFE_PM_GET_STROKESTATE)
            {
                currentbyte++;
                datalength = data[(int)currentbyte];
                currentbyte++;

                switch (data[(int)currentbyte])
                {
                    case 0:
                    case 1:
                        StrokePhase = StrokePhase.Catch;
                        break;
                    case 2:
                        StrokePhase = StrokePhase.Drive;
                        break;
                    case 3:
                        StrokePhase = StrokePhase.Dwell;
                        break;
                    case 4:
                        StrokePhase = StrokePhase.Recovery;
                        break;
                }
                currentbyte++;
            }
            if (data[(int)currentbyte] == (uint)PMUSBInterface.CSAFECommands.CSAFE_PM_GET_WORKOUTSTATE)
            {
                currentbyte++;
                datalength = data[(int)currentbyte];
                currentbyte++;

                WorkoutState = data[(int)currentbyte];

                currentbyte += datalength;
            }
        }

        public void LowResolutionUpdate()
        {
            PMUSBInterface.CSAFECommand cmd = CreateCommand();

            // Header and number of extension commands.
            cmd.CommandsBytes.Add((uint)PMUSBInterface.CSAFECommands.CSAFE_SETUSERCFG1_CMD);
            cmd.CommandsBytes.Add(0x03);

            // Three PM3 extension commands.
            cmd.CommandsBytes.Add((uint)PMUSBInterface.CSAFECommands.CSAFE_PM_GET_DRAGFACTOR);
            cmd.CommandsBytes.Add((uint)PMUSBInterface.CSAFECommands.CSAFE_PM_GET_WORKDISTANCE);
            cmd.CommandsBytes.Add((uint)PMUSBInterface.CSAFECommands.CSAFE_PM_GET_WORKTIME);

            // Standard commands.
            cmd.CommandsBytes.Add((uint)PMUSBInterface.CSAFECommands.CSAFE_GETPACE_CMD);
            cmd.CommandsBytes.Add((uint)PMUSBInterface.CSAFECommands.CSAFE_GETPOWER_CMD);
            cmd.CommandsBytes.Add((uint)PMUSBInterface.CSAFECommands.CSAFE_GETCADENCE_CMD);
            cmd.CommandsBytes.Add((uint)PMUSBInterface.CSAFECommands.CSAFE_GETHRCUR_CMD);
            cmd.CommandsBytes.Add((uint)PMUSBInterface.CSAFECommands.CSAFE_GETCALORIES_CMD);
            
            List<uint> data = cmd.Execute();

            uint currentbyte = 0;
            uint datalength = 0;

            if (data[(int)currentbyte] == (uint)PMUSBInterface.CSAFECommands.CSAFE_SETUSERCFG1_CMD)
            {
                currentbyte += 2;
            }
            if (data[(int)currentbyte] == (uint)PMUSBInterface.CSAFECommands.CSAFE_PM_GET_DRAGFACTOR)
            {
                currentbyte++;
                datalength = data[(int)currentbyte];
                currentbyte++;

                DragFactor = data[(int)currentbyte];

                currentbyte += datalength;
            }
            if (data[(int)currentbyte] == (uint)PMUSBInterface.CSAFECommands.CSAFE_PM_GET_WORKDISTANCE)
            {
                currentbyte++;
                datalength = data[(int)currentbyte];
                currentbyte++;

                uint distanceTemp = (data[(int)currentbyte] + (data[(int)currentbyte + 1] << 8) + (data[(int)currentbyte + 2] << 16) + (data[(int)currentbyte + 3] << 24)) / 10;
                uint fractionTemp = data[(int)currentbyte + 4];

                Distance = (float)distanceTemp + ((float)fractionTemp / 10.0f);

                currentbyte += datalength;
            }
            if (data[(int)currentbyte] == (uint)PMUSBInterface.CSAFECommands.CSAFE_PM_GET_WORKTIME)
            {
                currentbyte++;
                datalength = data[(int)currentbyte];
                currentbyte++;

                if (datalength == 5)
                {
                    uint timeInSeconds = (data[(int)currentbyte] + (data[(int)currentbyte + 1] << 8) + (data[(int)currentbyte + 2] << 16) + (data[(int)currentbyte + 3] << 24)) / 100;
                    uint fraction = data[(int)currentbyte + 4];

                    Worktime = (float)timeInSeconds + ((float)fraction / 100.0f);
                }
                currentbyte += datalength;
            }
            if (data[(int)currentbyte] == (uint)PMUSBInterface.CSAFECommands.CSAFE_GETPACE_CMD)
            {
                currentbyte++;
                datalength = data[(int)currentbyte];
                currentbyte++;

                // Pace is in seconds/Km
                uint pace = data[(int)currentbyte] + (data[(int)currentbyte + 1] << 8);
                // get pace in seconds / 500m
                double fPace = pace / 2.0;
                Pace = (float)fPace;

                currentbyte += datalength;
            }
            if (data[(int)currentbyte] == (uint)PMUSBInterface.CSAFECommands.CSAFE_GETPOWER_CMD)
            {
                currentbyte++;
                datalength = data[(int)currentbyte];
                currentbyte++;

                Power = data[(int)currentbyte] + (data[(int)currentbyte + 1] << 8);

                currentbyte += datalength;
            }
            if (data[(int)currentbyte] == (uint)PMUSBInterface.CSAFECommands.CSAFE_GETCADENCE_CMD)
            {
                currentbyte++;
                datalength = data[(int)currentbyte];
                currentbyte++;

                uint currentSPM = data[(int)currentbyte];

                if (0 < currentSPM)
                {
                    nSPM += currentSPM;
                    nSPMReads++;

                    SPM = currentSPM;
                    SPMAvg = ((float)nSPM * 1.0f) / ((float)nSPMReads * 1.0f);
                }

                currentbyte += datalength;
            }
            if (data[(int)currentbyte] == (uint)PMUSBInterface.CSAFECommands.CSAFE_GETHRCUR_CMD)
            {
                currentbyte++;
                datalength = data[(int)currentbyte];
                currentbyte++;

                uint currentHeartBeat = data[(int)currentbyte];
                Heartrate = currentHeartBeat;

                currentbyte += datalength;
            }
            if (data[(int)currentbyte] == (uint)PMUSBInterface.CSAFECommands.CSAFE_GETCALORIES_CMD)
            {
                currentbyte++;
                datalength = data[(int)currentbyte];
                currentbyte++;

                uint currentCalories = data[(int)currentbyte] + (data[(int)currentbyte + 1] << 8);
                Calories = currentCalories;

                currentbyte += datalength;
            }
        }
    }
}