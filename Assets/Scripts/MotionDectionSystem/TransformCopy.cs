using System.Collections;
using System.Collections.Generic;
using System;

using UnityEngine;

/// <summary>
/// Small helper class to copy over a transform list from a reference (mainly used for the side view avatar)
/// </summary>
public class TransformCopy : MonoBehaviour
{

    [Serializable]
    public struct CopyTransform
    {
        public Transform toCopy;
        public Transform toApply;
    }


    public int rootIndex = 0;
    public List<CopyTransform> copyTransforms;

    private Vector3 startPosition;
    private Vector3 copyStartPosition;
    private Quaternion startRotation;

    /// <summary>
    /// Initialize the copying by storing the original start position etc.
    /// </summary>
    void Start()
    {
        this.copyStartPosition = this.copyTransforms[this.rootIndex].toCopy.position;
        this.startPosition = this.copyTransforms[this.rootIndex].toApply.position;
        this.startRotation = this.copyTransforms[this.rootIndex].toApply.rotation;
    }

    /// <summary>
    /// LateUpdate to make it update after all the normal animations were updated. Copies over all of the reference transform and applies them to this object
    /// </summary>
    void LateUpdate()
    {
        Transform rootTransform = this.copyTransforms[this.rootIndex].toCopy;

        for(int i = 0; i < this.copyTransforms.Count; i++)
        {
            this.copyTransforms[i].toApply.localRotation = this.copyTransforms[i].toCopy.localRotation;
        }

        this.copyTransforms[this.rootIndex].toApply.position = this.startPosition + (rootTransform.position - this.copyStartPosition);
    }
}
