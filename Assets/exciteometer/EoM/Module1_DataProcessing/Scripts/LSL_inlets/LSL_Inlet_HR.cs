using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExciteOMeter
{
    public class LSL_Inlet_HR : InletShortSamples
    {
        public Cube c;
        public static BotAnimationStateController bas;
        public static Visualizer dmm;
        public static TimerScript gametime;
        void Start()
        {
            c = new Cube();
            bas = new BotAnimationStateController();
            dmm = new Visualizer();
            gametime = new TimerScript();
            // [optional] call this only, if your gameobject hosting this component
            // got instantiated during runtime

            // registerAndLookUpStream();
        }

        /// <summary>
            /// Override this method to implement whatever should happen with the samples...
            /// IMPORTANT: Avoid heavy processing logic within this method, update a state and use
            /// coroutines for more complexe processing tasks to distribute processing time over
            /// several frames
            /// </summary>
            /// <param name="newSample"></param>
            /// <param name="timeStamp"></param>
        protected override void Process(short[] newSample, double timeStamp)
        {
            //TODO: The event only sends float[], all samples need to be parsed to float
            EoM_Events.Send_OnDataReceived(VariableType, ExciteOMeterManager.GetTimestamp(), (float)newSample[0]);
            print("value of float new sample is"+newSample[0]);
           // if (!newSample[0].Equals(null))
           // {
               
                bas.Get_HR_Value((float)newSample[0]);
                dmm.updateMusic((float)newSample[0]);
                gametime.Get_HR_Value((float)newSample[0]);
            //if(c!=null)
            //c.gethrvalues((float)newSample[0]);

            // }
            //LoggerController.instance.WriteLine(LogName.VariableHeartRate, ExciteOMeterManager.GetTimestampString() + "," + ExciteOMeterManager.ConvertFloatToString(newSample[0],0));
        }
    }
}
