using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class that manages the stick figures
/// </summary>
public class AvatarVisualizerManager : MonoBehaviour
{
    public AvatarStickVisualizer playerVisualizer;
    public AvatarStickVisualizer trainerVisualizer;

    private FBTRecorder recorder = null;
    private FullBodyTrackingManager fbtManager = null;

    private bool customFigureUpdate = false;


    /// <summary>
    /// Is called by the FBTRecorerManager and initalize this class and the stick figures
    /// </summary>
    /// <param name="recorder">FBTRecorder reference</param>
    public void Initialize(FBTRecorder recorder)
    {
        this.recorder = recorder;
        this.fbtManager = this.recorder.GetFBTManager();

        this.playerVisualizer.Initialize();
        this.trainerVisualizer.Initialize();

        this.trainerVisualizer.gameObject.SetActive(false);
    }

    /// <summary>
    /// Returns the AvatarStickVisualizer of the trainer stick figure
    /// </summary>
    /// <returns>AvatarStickVisualizer</returns>
    public AvatarStickVisualizer GetTrainerVisualizer()
    {
        return this.trainerVisualizer;
    }

    /// <summary>
    /// Returns the AvatarStickVisualizer of the player stick figure
    /// </summary>
    /// <returns>AvatarStickVisualizer</returns>
    public AvatarStickVisualizer GetPlayerVisualizer()
    {
        return this.playerVisualizer;
    }

    /// <summary>
    /// Defines if the manager should be in controll of the stickfigure or some custom display should be used (mainly used by the graph)
    /// </summary>
    /// <param name="enabled">bool if enabled or disabled</param>
    public void SetCustomStickfigureUpdate(bool enabled)
    {
        this.customFigureUpdate = enabled;
    }

    /// <summary>
    /// Updates the stick figures by updateing the rotation data for them
    /// </summary>
    public void Update()
    {
        if(this.customFigureUpdate == true || this.fbtManager == null)
        {
            return;
        }

        Transform[] playerTransforms = this.fbtManager.GetUsedAvatarTransforms();
        List<Transform> recorderTransforms = this.recorder.GetCurrentFrameTransform();

        List<Quaternion> playerQuaternions = new List<Quaternion>();
        List<Quaternion> recorderQuaternions = new List<Quaternion>();

        for (int i = 0; i < playerTransforms.Length; i++)
        {
            playerQuaternions.Add(playerTransforms[i].localRotation);
        }

        for (int i = 0; i < recorderTransforms.Count; i++)
        {
            recorderQuaternions.Add(recorderTransforms[i].localRotation);
        }


        this.playerVisualizer.UpdateRotations(playerQuaternions);
        this.trainerVisualizer.UpdateRotations(recorderQuaternions);
    }
}
