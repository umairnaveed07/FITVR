using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Class that manages all of the DTW calculations. Note that all of the functions are static and therefore can be directly called
/// </summary>
public class DTWCalculator
{
    private const float INF = 1000000.0f;

    /// <summary>
    /// Small interface class for the cost function to allow customs once
    /// </summary>
    public interface DTWCostfunction
    {
        public float GetDTWCosts(FBTRecorder.Frame playerFrame, FBTRecorder.Frame recordingFrame);
    }

    public class DTWPath
    {
        public int posX;
        public int posY;
        public float sumValue;
        public float singleValue;
        public int dX;
        public int dY;
    }


    private static readonly int[] DTW_NEIGHBOURS = {
           -1,0,
           0,-1,
           -1,-1
    };

    private static readonly int[] FAST_DTW_RES_NEIGHBOURS = {
           0, 0,
           1, 0,
           0, 1,
           1, 1
    };

    private static readonly int[] FAST_DTW_CELL_NEIGHBOURS = {
           1, 0,
           -1,0,
           0, 1,
           0,-1,
           1, 1,
           -1,1,
           1,-1,
           -1,-1
    };

    /// <summary>
    /// Returns the optimal path from a cost matrix
    /// </summary>
    /// <param name="dtw">2d float-array of the cost matrix</param>
    /// <returns>List of DTWPath of the optimal path</returns>
    public static List<DTWPath> GetOptimalPath(float[,] dtw)
    {
        List<DTWPath> path = new List<DTWPath>();

        int cPosX = dtw.GetLength(0) - 1;
        int cPosY = dtw.GetLength(1) - 1;
        float cValue = dtw[cPosX,cPosY];

        DTWPath startEntry = new DTWPath();
        startEntry.sumValue = cValue;
        startEntry.posX = cPosX;
        startEntry.posY = cPosY;
        startEntry.dX = 0;
        startEntry.dY = 0;
        path.Add(startEntry);

        while (cPosX != 0 && cPosY != 0)
        {
            int nextX = cPosX;
            int nextY = cPosY;

            for (int i = 0; i < DTW_NEIGHBOURS.Length; i += 2)
            {
                int moveX = cPosX + DTW_NEIGHBOURS[i];
                int moveY = cPosY + DTW_NEIGHBOURS[i + 1];

                if (moveX < 0 || moveY < 0)
                {
                    continue;
                }

                if (dtw[moveX,moveY] <= cValue)
                {
                    cValue = dtw[moveX,moveY];

                    nextX = moveX;
                    nextY = moveY;
                }
            }

            int dX = nextX - cPosX;
            int dY = nextY - cPosY;

            cPosX = nextX;
            cPosY = nextY;

            DTWPath entry = new DTWPath();
            entry.sumValue = cValue;
            entry.posX = nextX;
            entry.posY = nextY;
            entry.dX = dX;
            entry.dY = dY;

            path.Add(entry);
        }

        return path;
    }

    /// <summary>
    /// Recalculates the costs of the optimal path
    /// </summary>
    /// <param name="path">List of DTWPath of the optimal path</param>
    /// <param name="dtw">2d float-array of the cost matrix</param>
    /// <param name="costFunction">DTWCostfunction of the used cost function</param>
    /// <param name="playerFrame">List of FBTRecorder.Frame of all the player frames</param>
    /// <param name="recordingFrame">List of FBTRecorder.Frame of all the recording frames</param>
    /// <returns></returns>
    public static List<DTWPath> RecalculatePathValues(List<DTWPath> path, float[,] dtw, DTWCostfunction costFunction, List<FBTRecorder.Frame> playerFrame, List<FBTRecorder.Frame> recordingFrame)
    {

        if(path.Count == 0)
        {
            return path;
        }

        for(int i = 0; i < path.Count; i++)
        {
            int x = path[i].posX;
            int y = path[i].posY;

            path[i].singleValue = costFunction.GetDTWCosts(playerFrame[x], recordingFrame[y]);
        }

        path[path.Count - 1].sumValue = path[path.Count - 1].singleValue;

        if (path.Count <= 1)
        {
            return path;
        }

        for (int i = path.Count-2; i >= 0; i--)
        {
            path[i].sumValue = path[i+1].sumValue + path[i].singleValue;
        }

        return path;
    }

    /// <summary>
    /// Gets the optimal path but with a slop restriction. Based on the idea of the slope restriction from this paper: https://www.researchgate.net/publication/324177668_Gesture_Recognition_Using_Dynamic_Time_Warping_and_Kinect_A_Practical_Approach/link/5c12ac6e299bf139c756bd2a/
    /// </summary>
    /// <param name="dtw">2d float-array of the cost-matrix</param>
    /// <param name="maxSlopeCount">int of the maximum slope that should be used</param>
    /// <returns></returns>
    public static List<DTWPath> GetSlopeRestrictedOptimalPath(float[,] dtw, int maxSlopeCount)
    {
        List<DTWPath> path = new List<DTWPath>();

        int slopeCountX = 0;
        int slopeCountY = 0;

        bool blockY = false;
        bool blockX = false;

        int cPosX = dtw.GetLength(0) - 1;
        int cPosY = dtw.GetLength(1) - 1;
        float cValue = dtw[cPosX, cPosY];

        DTWPath startEntry = new DTWPath();
        startEntry.sumValue = cValue;
        startEntry.posX = cPosX;
        startEntry.posY = cPosY;
        startEntry.dX = 0;
        startEntry.dY = 0;
        path.Add(startEntry);

        while (cPosX != 0 && cPosY != 0)
        {
            int nextX = cPosX;
            int nextY = cPosY;

            for (int i = 0; i < DTW_NEIGHBOURS.Length; i += 2)
            {

                if(blockX == true)
                {
                    if(DTW_NEIGHBOURS[i] != 0 && DTW_NEIGHBOURS[i + 1] == 0)
                    {
                        continue;
                    }
                }
                else if(blockY == true)
                {
                    if (DTW_NEIGHBOURS[i] == 0 && DTW_NEIGHBOURS[i + 1] != 0)
                    {
                        continue;
                    }
                }

                int moveX = cPosX + DTW_NEIGHBOURS[i];
                int moveY = cPosY + DTW_NEIGHBOURS[i + 1];

                if (moveX < 0 || moveY < 0)
                {
                    continue;
                }

                if (dtw[moveX, moveY] <= cValue)
                {
                    cValue = dtw[moveX, moveY];

                    nextX = moveX;
                    nextY = moveY;
                }
            }

            int dX = nextX - cPosX;
            int dY = nextY - cPosY;

            if(dX != 0 && dY == 0)
            {
                slopeCountX++;

                if (slopeCountX >= maxSlopeCount)
                {
                    blockX = true;
                    continue;
                }

            }
            else
            {
                blockX = false;
                slopeCountX = 0;
            }

            if(dY != 0 && dX == 0)
            {
                slopeCountY++;

                if (slopeCountY >= maxSlopeCount)
                {
                    blockY = true;
                    continue;
                }
            }
            else
            {
                blockY = false;
                slopeCountY = 0;
            }

            cPosX = nextX;
            cPosY = nextY;

            DTWPath entry = new DTWPath();
            entry.sumValue = cValue;
            entry.posX = nextX;
            entry.posY = nextY;
            entry.dX = dX;
            entry.dY = dY;

            path.Add(entry);
        }

        return path;
    }

    /// <summary>
    /// Teduces the given input array by half, by interpolating between the keypoints of the next two frames 
    /// </summary>
    /// <param name="array">List of FBTRecorder.Frame that should be reduced by half</param>
    /// <returns>List of FBTRecorder.Frame but reduced by half</returns>

    private static List<FBTRecorder.Frame> ReduceByHalf(List<FBTRecorder.Frame> array)
    {
        List<FBTRecorder.Frame> halfRes = new List<FBTRecorder.Frame>();

        int fin = array.Count;

        if (fin % 2 != 0)
        {
            fin -= 1;
        }

        for (int i = 0; i < fin; i += 2)
        {
            FBTRecorder.Frame newFrame = new FBTRecorder.Frame();
            newFrame.keypoints = new List<FBTRecorder.Keypoint>();

            for (int j = 0; j < array[i].keypoints.Count; j++)
            {
                if ( (array[i].keypoints[j].empty && array[i + 1].keypoints[j].empty) || (array[i].keypoints[j].ignore && array[i + 1].keypoints[j].ignore) )
                {
                    FBTRecorder.Keypoint key = new FBTRecorder.Keypoint();
                    key.empty = true;

                    newFrame.keypoints.Add(key);
                }
                else if (array[i].keypoints[j].empty || array[i].keypoints[j].ignore)
                {
                    newFrame.keypoints.Add(array[i + 1].keypoints[j]);
                }
                else if (array[i + 1].keypoints[j].empty || array[i + 1].keypoints[j].ignore)
                {
                    newFrame.keypoints.Add(array[i].keypoints[j]);
                }
                else
                {
                    FBTRecorder.Keypoint key = new FBTRecorder.Keypoint();
                    key.empty = false;
                    key.ignore = false;
                    key.localRotation = Quaternion.Slerp(array[i].keypoints[j].localRotation, array[i + 1].keypoints[j].localRotation, 0.5f);
                    newFrame.keypoints.Add(key);
                }
            }

            halfRes.Add(newFrame);
        }

        return halfRes;
    }

    /// <summary>
    /// Based on the low res dtw now extract the cell positions we need to go for the higher resolution dtw
    /// </summary>
    /// <param name="lowResDTW">2d float-array of the reduced cost-matrix</param>
    /// <param name="playerArray">List of playerArray of all of the player movements</param>
    /// <param name="recordingArray">List of playerArray of all of the recording movements</param>
    /// <param name="radius">The searching or expanding radius for the upscaling</param>
    /// <param name="containedNeighbours">bool 2d-array of the neighbours that we are currently considering</param>
    /// <returns></returns>
    private static bool[,] ExpandResWindow(float[,] lowResDTW, List<FBTRecorder.Frame> playerArray, List<FBTRecorder.Frame> recordingArray, int radius, bool[,] containedNeighbours)
    {
        List<DTWPath> optimalPath = GetOptimalPath(lowResDTW);
        //we need a quick lookup table to know which cells are covered by us and which arent

        for (int i = optimalPath.Count - 1; i >= 0; i--)
        {
            int posX = optimalPath[i].posX;
            int posY = optimalPath[i].posY;

            int resPosX = posX * 2;
            int resPosY = posY * 2;

            for (int j = 0; j < FAST_DTW_RES_NEIGHBOURS.Length; j += 2)
            {
                int cellX = resPosX + FAST_DTW_RES_NEIGHBOURS[j];
                int cellY = resPosY + FAST_DTW_RES_NEIGHBOURS[j + 1];

                ExpandSearchRadius(containedNeighbours, radius, cellX, cellY);
            }

            if (i - 1 > 0)
            {
                int nextPosX = optimalPath[i - 1].posX;
                int nextPosY = optimalPath[i - 1].posY;

                //checking if we move diagonal (which means we need to add the corner cells aswell)
                if ((nextPosX - posX) != 0 && (nextPosY - posY) != 0)
                {
                    int cxL = resPosX + 1;
                    int cyL = resPosY + 2;

                    int cxR = resPosX + 2;
                    int cyR = resPosY + 1;

                    if (cxL < containedNeighbours.GetLength(0) && cyL < containedNeighbours.GetLength(1))
                    {
                        ExpandSearchRadius(containedNeighbours, radius, cxL, cyL);
                    }

                    if (cxR < containedNeighbours.GetLength(0) && cyR < containedNeighbours.GetLength(1))
                    {
                        ExpandSearchRadius(containedNeighbours, radius, cxR, cyR);
                    }
                }
            }

        }

        return containedNeighbours;
    }

    /// <summary>
    /// Expand the given search path by including sorrounding neighbours (but only if they arent already contained)
    /// </summary>
    /// <param name="containedNeighbours">bool 2d-array of the already contained neighbours</param>
    /// <param name="radius">The search / expanding radius</param>
    /// <param name="posX">int of the x grid cell index</param>
    /// <param name="posY">int of the y grid cell index</param>
    /// <returns></returns>
    private static bool [,] ExpandSearchRadius(bool [,] containedNeighbours, int radius, int posX, int posY)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for(int y = -radius; y <= radius; y++)
            {
                int dX = posX + x;
                int dY = posY + y;

                if(dX < 0 || dY < 0 || dX >= containedNeighbours.GetLength(0) || dY >= containedNeighbours.GetLength(1))
                {
                    continue;
                }

                containedNeighbours[dX,dY] = true;
            }
        }


        return containedNeighbours;
    }

    /// <summary>
    /// Performs the fast dtw algorithm
    /// </summary>
    /// <param name="playerArray">List of playerArray of all of the player movements</param>
    /// <param name="recordingArray">List of playerArray of all of the recording movements</param>
    /// <param name="radius">float of the search/expanding radius</param>
    /// <param name="costFunction">DTWCostfunction of the used cost function </param>
    /// <param name="window">The current bool 2d-array window-array (defines which cells should be contained or not)</param>
    /// <returns>The float 2d-array of the cost-matrix</returns>
    public static float[,] FastDTW(List<FBTRecorder.Frame> playerArray, List<FBTRecorder.Frame> recordingArray, int radius, DTWCostfunction costFunction, bool[,] window)
    {
        int minTSsize = radius + 2;

        if (playerArray.Count <= minTSsize || recordingArray.Count <= minTSsize)
        {
            return PerformDTW(playerArray, recordingArray, costFunction);
        }

        List<FBTRecorder.Frame> reducedX = ReduceByHalf(playerArray);
        List<FBTRecorder.Frame> reducedY = ReduceByHalf(recordingArray);

        float[,] lowResDTW = FastDTW(reducedX, reducedY, radius, costFunction, window);
        window = ExpandResWindow(lowResDTW, playerArray, recordingArray, radius, window);


        return PerformRestrictedDTW(playerArray, recordingArray, window, costFunction);
    }

    /// <summary>
    /// Performs the fast dtw algorithm (and initialize the window parameter, use this method if you want to you the fast dtw algorithm)
    /// </summary>
    /// <param name="playerArray">List of playerArray of all of the player movements</param>
    /// <param name="recordingArray">List of playerArray of all of the recording movements</param>
    /// <param name="radius">The search/expanding radius for the fast dtw algorithm</param>
    /// <param name="costFunction">DTWCostfunction of the used cost function</param>
    /// <returns></returns>
    public static float[,] PerformFastDTW(List<FBTRecorder.Frame> playerArray, List<FBTRecorder.Frame> recordingArray, int radius, DTWCostfunction costFunction)
    {
        bool[,] containedNeighbours = new bool[playerArray.Count,recordingArray.Count];

        for (int i = 0; i < playerArray.Count; i++)
        {

            for (int j = 0; j < recordingArray.Count; j++)
            {
                containedNeighbours[i,j] = false;
            }
        }


        return FastDTW(playerArray, recordingArray, radius, costFunction, containedNeighbours);
    }

    /// <summary>
    /// Performs the dtw algorithm based on the search path we have given as a parameter 
    /// <param name="playerArray">List of playerArray of all of the player movements</param>
    /// <param name="recordingArray">List of playerArray of all of the recording movements</param>
    /// <param name="window">bool 2d-array containg only the grid-cell that we are interested in performing the cost calculations</param>
    /// <param name="costFunction">DTWCostfunction of the used cost function</param>
    /// <returns></returns>
    private static float[,] PerformRestrictedDTW(List<FBTRecorder.Frame> playerArray, List<FBTRecorder.Frame> recordingArray,  bool[,] window, DTWCostfunction costFunction)
    {
        float[,] dtw = new float[playerArray.Count, recordingArray.Count];

        for (int i = 0; i < playerArray.Count; i++)
        {
            for (int j = 0; j < recordingArray.Count; j++)
            {
                dtw[i,j] = INF;

            }
        }

        dtw[0,0] = 0.0f;

        for (int i = 1; i < playerArray.Count; i++)
        {
            for (int j = 1; j < recordingArray.Count; j++)
            {
                if (window[i,j] == false)
                {
                    continue; //fixme we can speed it up by a huge margin (but first we need to make it work)
                }

                float cost = costFunction.GetDTWCosts(playerArray[i], recordingArray[j]);
                dtw[i,j] = cost + Mathf.Min(dtw[i - 1,j], Mathf.Min(dtw[i,j - 1], dtw[i - 1,j - 1]));
            }
        }

        return dtw;
    }

    /// <summary>
    /// Performs the normal dtw algorithm
    /// </summary>
    /// <param name="playerArray">List of playerArray of all of the player movements</param>
    /// <param name="recordingArray">List of playerArray of all of the recording movements</param>
    /// <param name="costFunction">DTWCostfunction of the used cost function</param>
    /// <returns></returns>
    public static float[,] PerformDTW(List<FBTRecorder.Frame> playerArray, List<FBTRecorder.Frame> recordingArray, DTWCostfunction costFunction)
    {
        float[,] dtw = new float[playerArray.Count, recordingArray.Count];

        for (int i = 0; i < playerArray.Count; i++)
        {
            for (int j = 0; j < recordingArray.Count; j++)
            {
                dtw[i,j] = INF;
            }
        }

        dtw[0,0] = 0.0f;

        for (int i = 1; i < playerArray.Count; i++)
        {
            for (int j = 1; j < recordingArray.Count; j++)
            {
                float cost = costFunction.GetDTWCosts(playerArray[i], recordingArray[j]);
                dtw[i,j] = cost + Mathf.Min(dtw[i - 1,j], Mathf.Min(dtw[i,j - 1], dtw[i - 1,j - 1]));
            }
        }

        return dtw;
    }

    /// <summary>
    /// Performs the DTW algorithm but with an additional window parameter to avoid repition/overusing of the same indices 
    /// </summary>
    /// <param name="playerArray">List of playerArray of all of the player movements</param>
    /// <param name="recordingArray">List of playerArray of all of the recording movements</param>
    /// <param name="costFunction">DTWCostfunction of the used cost function</param>
    /// <param name="windowLength">int of the length of the window </param>
    /// <returns></returns>
    public static float[,] PerformWindowDTW(List<FBTRecorder.Frame> playerArray, List<FBTRecorder.Frame> recordingArray, DTWCostfunction costFunction, int windowLength)
    {
        float[,] dtw = new float[playerArray.Count, recordingArray.Count];
        int w = Mathf.Max(windowLength, Mathf.Abs(playerArray.Count - recordingArray.Count)); //to avoid crashes we must ensure that our window always contains the items that are larger than |playerArray-recordingArray|

        for (int i = 0; i < playerArray.Count; i++)
        {
            for (int j = 0; j < recordingArray.Count; j++)
            {
                dtw[i, j] = INF;
            }
        }

        dtw[0, 0] = 0.0f;

        //window specific loop
        for (int i = 1; i < playerArray.Count; i++)
        {
            int start = Mathf.Max(1, i - w);
            int fin = Mathf.Min(recordingArray.Count, i + w);

            for (int j = start; j < fin; j++)
            {
                dtw[i, j] = 0.0f;
            }
        }

        for (int i = 1; i < playerArray.Count; i++)
        {
            int start = Mathf.Max(1, i - w);
            int fin = Mathf.Min(recordingArray.Count, i + w);

            for (int j = start; j < fin; j++)
            {
                float cost = costFunction.GetDTWCosts(playerArray[i], recordingArray[j]);
                dtw[i, j] = cost + Mathf.Min(dtw[i - 1, j], Mathf.Min(dtw[i, j - 1], dtw[i - 1, j - 1]));
            }
        }

        return dtw;
    }

    /// <summary>
    /// Prints the dtw cost-matrix but can also be for any 2d float array
    /// </summary>
    /// <param name="dtw">The 2d float-array cost matrix</param>
    public static void PrintDTWMatrix(float[,] dtw)
    {
        Debug.Log("--------------------");

        string line = "";

        for (int i = 0; i < dtw.GetLength(0); i++)
        {

            for (int j = 0; j < dtw.GetLength(1); j++)
            {
                if (dtw[i,j] == INF)
                {
                    line += "[inf] ";
                }
                else
                {
                    int num = (int)dtw[i,j];
                    line += "[" + num.ToString().PadLeft(3, '0') + "] ";
                }
            }

            line += "\n";
        }

        Debug.Log(line);
        Debug.Log("--------------------");
    }

    /// <summary>
    /// Prints the neighbour matrix (used for debuggin the fast dtw algorithm mainly) but can also be used for any bool 2d-array
    /// </summary>
    /// <param name="dtw">The 2d bool-array neighbour matrix</param>
    public static void PrintNeighbourMatrix(bool[,] dtw)
    {
        Debug.Log("--------------------");

        string line = "";

        for (int i = 0; i < dtw.GetLength(0); i++)
        {

            for (int j = 0; j < dtw.GetLength(1); j++)
            {
                if (dtw[i,j] == true)
                {
                    line += "[x] ";
                }
                else
                {
                    line += "[ ] ";
                }
            }

            line += "\n";
        }

        Debug.Log(line);
        Debug.Log("--------------------");
    }
}
