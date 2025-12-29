using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class that manages all of the drawing for the stick figure avatar
/// </summary>
public class AvatarStickVisualizer : MonoBehaviour
{

    public int rootIndex = 0;
    public Transform floorLevel;
    public Color avatarColor;
    public GameObject[] keyPoints;
    public Material assignedMaterial;

    private Quaternion startRotation;
    private List<Quaternion> rotations;

    /// <summary>
    /// Updates the connected lines for every frame to match the current posture etc.
    /// </summary>
    void Update()
    {
        this.UpdateAvatarLines();
    }

    /// <summary>
    /// Called by the FBTRecorderManager and initializes the stick figure
    /// </summary>
    public void Initialize()
    {
        for(int i = 0; i < this.keyPoints.Length; i++)
        {
            LineRenderer lineRenderer = this.keyPoints[i].GetComponent<LineRenderer>();
            lineRenderer.startColor = this.avatarColor;
            lineRenderer.endColor = this.avatarColor;
        }

        for(int i = 0; i < this.keyPoints.Length; i++)
        {
            this.keyPoints[i].transform.GetChild(0).GetComponent<Renderer>().material = this.assignedMaterial;
        }

    }

    /// <summary>
    /// Reset the current applied color from the stick figure
    /// </summary>
    public void ResetColor()
    {
        this.SetColor(this.avatarColor);
    }

    /// <summary>
    /// Sets the color for the stick figure
    /// </summary>
    /// <param name="newColor">Color which indicates the new color of the stick figure</param>
    public void SetColor(Color newColor)
    {
        for (int i = 0; i < this.keyPoints.Length; i++)
        {
            LineRenderer lineRenderer = this.keyPoints[i].GetComponent<LineRenderer>();
            lineRenderer.startColor = this.avatarColor;
            lineRenderer.endColor = this.avatarColor;
        }
    }

    /// <summary>
    /// Calculates the bone center position and returns all perpendicular vectors for that center to allow placements and offset which we can apply for that bone
    /// </summary>
    /// <param name="boneNumber">The bone we are intereset in</param>
    /// <returns></returns>
    public (Vector3, Vector3, Vector3) GetBoneCenterPosition(int boneNumber)
    {
        if (boneNumber < 0 || boneNumber >= this.keyPoints.Length)
        {
            return (Vector3.zero, Vector3.zero, Vector3.zero);
        }


        int fromBone = boneNumber;
        int toBone = -1;

        for (int i = 0; i < MotionMatchingDBManager.CONNECTIONS.Length; i += 2)
        {
            if (MotionMatchingDBManager.CONNECTIONS[i] == boneNumber)
            {
                toBone = MotionMatchingDBManager.CONNECTIONS[i + 1];
                break;
            }
        }

        if (toBone == -1)
        {
            return (Vector3.zero, Vector3.zero, Vector3.zero);
        }

        Vector3 targetPos = (this.keyPoints[fromBone].transform.position + this.keyPoints[toBone].transform.position) * 0.5f;

        Vector3 zDirection = Vector3.Normalize(this.keyPoints[toBone].transform.position - this.keyPoints[fromBone].transform.position);
        Vector3 xDirection = Vector3.Normalize(Vector3.Cross(Vector3.up, zDirection));
        Vector3 yDirection = Vector3.Cross(zDirection, xDirection);

        if (fromBone > 8)//so that every bone faces the same side (small trick to avoid the ordering problem here)
        {
            yDirection = -yDirection;
        }


        return (targetPos, targetPos - this.keyPoints[this.rootIndex].transform.position, yDirection);
    }

    /// <summary>
    /// Allows to specificly define a color of a single bone
    /// </summary>
    /// <param name="boneId">int of the bone id</param>
    /// <param name="color">Color for that bone</param>
    public void SetBoneColor(int boneId, Color color)
    {
        if (boneId < 0 || boneId >= this.keyPoints.Length)
        {
            return;
        }

        LineRenderer lineRenderer = this.keyPoints[boneId].GetComponent<LineRenderer>();
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }

    /// <summary>
    /// Sets the bone color for two conencted bones at once
    /// </summary>
    /// <param name="boneId">int of bone which should be colourized(including its neighbour)</param>
    /// <param name="color">Color for the bones</param>
    public void SetConnectedBoneColor(int boneId, Color color)
    {
        List<int> bone = new List<int>();
        bone.Add(boneId);

        List<int> connectedBones = MotionMatchingDBManager.GetConnectedBones(bone);

        for(int i = 0; i < connectedBones.Count; i++)
        {
            this.SetBoneColor(connectedBones[i], color);
        }
    }

    /// <summary>
    /// Sets the visibility of all of the boens (if they should be displayed or not)
    /// </summary>
    /// <param name="visible">bool if they are visible or not</param>
    public void SetAllBonesVisibility(bool visible)
    {
        for (int i = 0; i < keyPoints.Length; i++)
        {
            LineRenderer lineRenderer = this.keyPoints[i].GetComponent<LineRenderer>();
            lineRenderer.enabled = visible;

            this.keyPoints[i].transform.GetChild(0).gameObject.SetActive(visible);
        }
    }

    /// <summary>
    /// Set the visibility for specific bones only
    /// </summary>
    /// <param name="boneIds"></param>
    /// <param name="visible"></param>
    public void SetBonesVisibility(List<int> boneIds, bool visible)
    {
        for(int i = 0; i<boneIds.Count; i++)
        {
            LineRenderer lineRenderer = this.keyPoints[boneIds[i]].GetComponent<LineRenderer>();
            lineRenderer.enabled = visible;
            this.keyPoints[boneIds[i]].transform.GetChild(0).gameObject.SetActive(visible);
        }
    }

    /// <summary>
    /// Hides all of the bones that are not given in this list
    /// </summary>
    /// <param name="boneIds">List of int of the bones we want to show only</param>
    public void ShowOnlyFocusBones(List<int> boneIds)
    {
        this.SetAllBonesVisibility(false);
        this.SetBonesVisibility(MotionMatchingDBManager.GetConnectedBones(boneIds), true);
    }

    /// <summary>
    /// Updates the rotation data for the stick figure (will be update/applied in the next update frame)
    /// </summary>
    /// <param name="transforms">List of quaternions indicating the directions</param>
    public void UpdateRotations(List<Quaternion> transforms)
    {
        this.rotations = transforms;
    }

    /// <summary>
    /// Updates the Avatar Lines based on the rotations data we are storing right now
    /// </summary>
    private void UpdateAvatarLines()
    {

        if(this.rotations == null || this.rotations.Count <= 0)
        {
            return;
        }

        float lowestY = 1000000.0f;

        for (int i = 0; i < this.rotations.Count; i++)
        {
            this.keyPoints[i].transform.localRotation = this.rotations[i];
            lowestY = Mathf.Min(lowestY, this.keyPoints[i].transform.position.y);
        }

        float differenceY = this.floorLevel.position.y - lowestY;
        this.keyPoints[this.rootIndex].transform.position += (new Vector3(0.0f, differenceY, 0.0f));


        for (int i = 0; i < MotionMatchingDBManager.CONNECTIONS.Length; i+=2)
        {
            int from = MotionMatchingDBManager.CONNECTIONS[i];
            int to = MotionMatchingDBManager.CONNECTIONS[i + 1];

            GameObject gFrom = this.keyPoints[from];
            GameObject gTo = this.keyPoints[to];

            LineRenderer lineRenderer = gFrom.GetComponent<LineRenderer>();

            lineRenderer.SetPosition(0, gFrom.transform.position);
            lineRenderer.SetPosition(1, gTo.transform.position);
        }


        this.rotations.Clear();//to force a new update in the next frame
    }
}
