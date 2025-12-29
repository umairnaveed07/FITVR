using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System;

using UnityEngine;

/// <summary>
/// Class that manages the interpolation of bones
/// </summary>
public class BoneInterpolationManager : MonoBehaviour
{

    /// <summary>
    /// Helper class for the interpolation
    /// </summary>
    private class InterpolationData
    {
        public Vector3 startPosition;
        public Quaternion startRotation;
        public Vector3 targetPosition;
        public Quaternion targetRotation;

        public Transform objectTransform;

        public float interpolationTime ;
        public float interpolationDuration;

        private bool rotationOnly;
        private bool positionOnly;
        private bool useLocalRotation;

        /// <summary>
        /// Construction that sets all of the database to this class
        /// </summary>
        /// <param name="_ref"></param>
        /// <param name="sP"></param>
        /// <param name="sR"></param>
        /// <param name="tP"></param>
        /// <param name="tR"></param>
        /// <param name="length"></param>
        /// <param name="rotationOnly"></param>
        /// <param name="positionOnly"></param>
        /// <param name="useLocalRotation"></param>
        public InterpolationData(Transform _ref, Vector3 sP, Quaternion sR, Vector3 tP, Quaternion tR, float length, bool rotationOnly, bool positionOnly, bool useLocalRotation)
        { 
            this.objectTransform = _ref;

            this.startPosition = sP;
            this.startRotation = sR;
            this.targetPosition = tP;
            this.targetRotation = tR;

            this.rotationOnly = rotationOnly;
            this.positionOnly = positionOnly;
            this.useLocalRotation = useLocalRotation;

            this.interpolationTime = 0.0f;
            this.interpolationDuration = length;
        }

        /// <summary>
        /// Apply the current interpolation based on the delta time
        /// </summary>
        /// <param name="dt">Delta time between the last two frames (unity specific)</param>
        public void ApplyInterpolation(float dt)
        {
            this.interpolationTime += dt;

            float percentage = 1.0f;

            if(this.interpolationDuration > 0.0f)
            {
                percentage = Math.Min(this.interpolationTime / this.interpolationDuration, 1.0f);
            }

            if (this.rotationOnly == false)
            {
                this.objectTransform.position = Vector3.Lerp( this.startPosition,this.targetPosition, percentage);
            }

            if (this.positionOnly == false)
            {
                if(this.useLocalRotation == false)
                {
                    this.objectTransform.rotation = Quaternion.Slerp(this.startRotation, this.targetRotation, percentage);
                }
                else
                {
                    this.objectTransform.localRotation = Quaternion.Slerp(this.startRotation, this.targetRotation, percentage);
                }
            }
        }


        /// <summary>
        /// Returns if the interpolation is done or not
        /// </summary>
        /// <returns>bool if the interpolation is completed</returns>
        public bool IsCompleted()
        {
            return this.interpolationTime >= this.interpolationDuration;
        }

    }


    private List<InterpolationData> interpolations;


    /// <summary>
    /// Initialized the interpolation
    /// </summary>
    void Start()
    {
        this.interpolations = new List<InterpolationData>();
    }

    /// <summary>
    /// Updates all of the bones as an lateupdate so that it will be applied after the initial animation updates
    /// </summary>
    void LateUpdate()
    {
        for (int i = 0; i < this.interpolations.Count; i++)
        {
            this.interpolations[i].ApplyInterpolation(Time.deltaTime);
        }

        for (int i = this.interpolations.Count-1; i >= 0; i--)
        {
            if (this.interpolations[i].IsCompleted())
            {
                this.interpolations.RemoveAt(i);
            }
        }

    }

    /// <summary>
    /// Interpolates the rotation of a bone only
    /// </summary>
    /// <param name="_ref">Transform reference this should be applied on</param>
    /// <param name="sR">Quaternion of the start rotation</param>
    /// <param name="tR">Quaternion of the target rotation</param>
    /// <param name="duration">float how long the interpolation should be played</param>
    /// <param name="useLocalRotation">bool to define if the local rotation should be used or the global one</param>
    public void AddRotationInterpolation(Transform _ref, Quaternion sR, Quaternion tR, float duration, bool useLocalRotation = false)
    {
        InterpolationData intp = new InterpolationData(_ref, Vector3.zero, sR, Vector3.zero, tR, duration, true, false, useLocalRotation);
        this.interpolations.Add(intp);
    }

    /// <summary>
    /// Interpolates the postion of a bone only
    /// </summary>
    /// <param name="_ref">Transform reference this should be applied on</param>
    /// <param name="start">Vector3 of the start position</param>
    /// <param name="finish">Vector3 of the target position</param>
    /// <param name="duration">float how long the interpolation should take</param>
    public void AddPositionInterpolation(Transform _ref, Vector3 start, Vector3 finish, float duration)
    {
        InterpolationData intp = new InterpolationData(_ref, start, _ref.rotation, finish, _ref.rotation, duration, false, true, false);
        this.interpolations.Add(intp);
    }

    /// <summary>
    /// Adds an interpolation to the list of interpolations (mostly used internally)
    /// </summary>
    /// <param name="_ref">Transform reference this should be applied on</param>
    /// <param name="sP">Vector3 start position</param>
    /// <param name="sR">Quaternion start rotation</param>
    /// <param name="tP">Vector3 target position</param>
    /// <param name="tQ">Quaternion target rotation</param>
    /// <param name="duration">float how long the interpolation should take</param>
    /// <param name="useLocalRotation">bool to define if the local rotation should be used or the global one</param>
    public void AddInterpolation(Transform _ref, Vector3 sP, Quaternion sR, Vector3 tP, Quaternion tQ, float duration, bool useLocalRotation = false)
    {
        InterpolationData intp = new InterpolationData(_ref, sP, sR, tP, tQ, duration, false, false, useLocalRotation);
        this.interpolations.Add(intp);
    }


    /// <summary>
    /// Finishes all of the current interpolation by completing them immediatly
    /// </summary>
    public void FinishFrame()
    {
        for (int i = 0; i < this.interpolations.Count; i++)
        {
            this.interpolations[i].ApplyInterpolation(this.interpolations[i].interpolationDuration);
        }

        this.interpolations.Clear();
    }
}
