using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Class that handles the cost function for the DTW algorithm (note that this isnt a unity gameobject and cant be attached to one)
/// </summary>
public class AngleCostFunction : DTWCalculator.DTWCostfunction
{

    private FBTRecorder fbtRecorder;

    /// <summary>
    /// Constructor that initialize the function
    /// </summary>
    /// <param name="fbtRecorder">Reference to the FBTRecorder-Class</param>
    public AngleCostFunction(FBTRecorder fbtRecorder)
    {
        this.fbtRecorder = fbtRecorder;
    }

    /// <summary>
    /// Extracts the Y-Axis rotation from a given quaternion
    /// </summary>
    /// <param name="q">Quaternion q where the Y-Axis rotation should be extracted</param>
    /// <returns>Quaternion with Y-Axis only</returns>
    private Quaternion GetYAxis(Quaternion q)
    {
        float theta = Mathf.Atan2(q.y, q.w);
        return new Quaternion(0.0f, Mathf.Sin(theta), 0.0f, Mathf.Cos(theta));
    }

    /// <summary>
    /// Returns the overall angle difference between the current playerFrame and recorderFrame
    /// </summary>
    /// <param name="playerFrame">FBTRecorder.Frame current player frame</param>
    /// <param name="recordingFrame">FBTRecorder.Frame current recorder frame</param>
    /// <returns>float of the angle difference (but summarized so not the averages)</returns>
    public float GetDTWCosts(FBTRecorder.Frame playerFrame, FBTRecorder.Frame recordingFrame)
    {
        int rootIndex = this.fbtRecorder.GetRootIndex();
        float totalAngleDiff = 0.0f;

        Quaternion invPlayerHipY = Quaternion.Inverse(this.GetYAxis(playerFrame.keypoints[rootIndex].rotation) );
        Quaternion invRecorderHipY = Quaternion.Inverse(this.GetYAxis(recordingFrame.keypoints[rootIndex].rotation) );

        for (int i = 0; i < MotionMatchingDBManager.CONNECTIONS.Length; i += 2)
        {
            int from = MotionMatchingDBManager.CONNECTIONS[i];
            int to = MotionMatchingDBManager.CONNECTIONS[i + 1];

            if (recordingFrame.keypoints[from].ignore == true && recordingFrame.keypoints[to].ignore == true)
            {
                continue;
            }
            else if (recordingFrame.keypoints[from].empty == true || recordingFrame.keypoints[to].empty == true)
            {
                continue;
            }
            else
            {
                Vector3 cDirection = invPlayerHipY * (playerFrame.keypoints[from].position - playerFrame.keypoints[to].position);
                Vector3 nDirection = invRecorderHipY * (recordingFrame.keypoints[from].position - recordingFrame.keypoints[to].position);

                float dotAngle = Mathf.Clamp(Vector3.Dot(cDirection.normalized, nDirection.normalized), -1.0f, 1.0f);
                float angleDif = Mathf.Abs(Mathf.Acos(dotAngle) * Mathf.Rad2Deg);

                if (angleDif >= MotionMatchingSystem.MAX_ANGLE)
                {
                    angleDif = MotionMatchingSystem.MAX_ANGLE;
                }

                totalAngleDiff += angleDif;
            }
        }

        return totalAngleDiff;
    }

    /// <summary>
    /// Returns the angle difference but as an array so that the values are readable per bone we are checking
    /// </summary>
    /// <param name="playerFrame">FBTRecorder.Frame current player frame</param>
    /// <param name="recordingFrame">FBTRecorder.Frame current recorder frame</param>
    /// <returns>float array of the angle differences for every bone we are interested in (-1 for the bones we dont care about)</returns>
    public float[] GetDTWCostsArray(FBTRecorder.Frame playerFrame, FBTRecorder.Frame recordingFrame)
    {
        int rootIndex = this.fbtRecorder.GetRootIndex();
        float [] totalAngleDiff = new float[recordingFrame.keypoints.Count];

        Quaternion invPlayerHipY = Quaternion.Inverse(this.GetYAxis(playerFrame.keypoints[rootIndex].rotation));
        Quaternion invRecorderHipY = Quaternion.Inverse(this.GetYAxis(recordingFrame.keypoints[rootIndex].rotation));

        for (int i = 0; i < MotionMatchingDBManager.CONNECTIONS.Length; i += 2)
        {
            int from = MotionMatchingDBManager.CONNECTIONS[i];
            int to = MotionMatchingDBManager.CONNECTIONS[i + 1];

            int idx = from;

            if (recordingFrame.keypoints[from].ignore == true && recordingFrame.keypoints[to].ignore == true)
            {
                totalAngleDiff[idx] = -1.0f;
            }
            else if (recordingFrame.keypoints[from].empty == true || recordingFrame.keypoints[to].empty == true)
            {
                totalAngleDiff[idx] = -1.0f;
            }
            else
            {
                Vector3 cDirection = invPlayerHipY * (playerFrame.keypoints[from].position - playerFrame.keypoints[to].position);
                Vector3 nDirection = invRecorderHipY * (recordingFrame.keypoints[from].position - recordingFrame.keypoints[to].position);

                float dotAngle = Mathf.Clamp(Vector3.Dot(cDirection.normalized, nDirection.normalized), -1.0f, 1.0f);
                float angleDif = Mathf.Abs(Mathf.Acos(dotAngle) * Mathf.Rad2Deg);

                if (angleDif >= MotionMatchingSystem.MAX_ANGLE)
                {
                    angleDif = MotionMatchingSystem.MAX_ANGLE;
                }

                totalAngleDiff[idx] = angleDif;
            }
        }

        return totalAngleDiff;
    }
}