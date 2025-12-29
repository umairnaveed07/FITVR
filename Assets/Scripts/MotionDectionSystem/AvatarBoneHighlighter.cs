using UnityEngine;
using System.Collections;

using System.Collections.Generic;

/// <summary>
/// Class that manages the avatar bone highlighter figure
/// </summary>
public class AvatarBoneHighlighter : MonoBehaviour 
{

	public bool debugUseMouse = false;
	public AudioSource clickSound;

	public Color32 highlightColor = Color.red;
	public Color32 regularColor = Color.white;
	private Color32 black = Color.black;


	private Transform leftHandRig;
	private Transform rightHandRig;
	private MeshFilter smr;
	private List<int> selectedRegions = new List<int>();

	private const int NONE_GROUP_BONES = 255;
	private const int END_OF_GROUP = -1;
	private static readonly int[] BODY_GROUPS =
	{
		27,26,25,22,27,24,23,17,16,18,19,20,21,29,28,15,14,END_OF_GROUP,//left hand
		49,48,47,46,45,44,39,40,37,42,41,43,51,50,38,36,END_OF_GROUP,   //right hand
		5,6,END_OF_GROUP,												//left shoulder
		30,31,END_OF_GROUP,												//right shoulder
		0,8,12,END_OF_GROUP,											//spine
		10,11,END_OF_GROUP,												//left leg
		33,34,END_OF_GROUP												//right leg
	};

	//handles the mapping/conversion of the transform indices to the bone indices
	private static readonly int[] BONE_MAPPING =
	{
		//Group mapping 
		0, -5, //left hand
		1, -8, //right hand
		2, -3, //left shoulder
		3, -6, //right shoulder
		4, -0, //spine,
		5, -11, //left leg
		6, -14, //right leg

		//Right leg single bones
		NONE_GROUP_BONES + 1, -12,
		NONE_GROUP_BONES + 32, -13,

		//Left leg single bones
		NONE_GROUP_BONES + 2, -9,
		NONE_GROUP_BONES + 9, -10,

		//Head
		NONE_GROUP_BONES + 3, -2,

		//Spine 2 
		NONE_GROUP_BONES + 7, -1,

		//Left fore arm single bone
		NONE_GROUP_BONES + 13, -4,

		//Right fore arm single bone
		NONE_GROUP_BONES + 35, -7,
	};

	private static readonly Color[] COLOR_ARRAY = 
	{
		Color.white
	};

	/// <summary>
    /// Returns the group number (if it is group 3 for example)
    /// </summary>
    /// <param name="groupIndex">int of the index of the group in the array</param>
    /// <returns>Returns the group number (3 for example) </returns>
	private int GetGroupNumber(int groupIndex)
    {
		int groupNumber = 0;

		for (int i = 0; i < groupIndex; i++)
		{
			if (BODY_GROUPS[i] == END_OF_GROUP)
			{
				groupNumber++;
			}
			
		}

		return groupNumber;
	}

	/// <summary>
    /// Returns the starting index for that group in the array
    /// </summary>
    /// <param name="groupNumber">int of the given group number</param>
    /// <returns>int of the starting index for thar group</returns>
	private int GetGroupFromNumber(int groupNumber)
	{
		int groupCount = 0;

		for (int i = 0; i < BODY_GROUPS.Length; i++)
		{
			if (BODY_GROUPS[i] == END_OF_GROUP)
			{
				groupCount++;
			}

			if (groupCount == groupNumber)
			{
				return i + 1;
			}

		}

		return -1;
	}

	/// <summary>
    /// Returns the group starting index based on the given index
    /// </summary>
    /// <param name="idx">int of the index</param>
    /// <returns>int of the starting index of the group</returns>
	private int GetGroupIndex(int idx)
    {
		int groupIndex = 0;

		for(int i = 0; i < BODY_GROUPS.Length; i++)
        {

			if(BODY_GROUPS[i] == END_OF_GROUP)
            {
				groupIndex = i;
            }
			else if (BODY_GROUPS[i] == idx)
            {
				return groupIndex+1;//to not start at the group directly
            }
        }

		return -1;
    }

	/// <summary>
    /// Returns if the given index is included in the current group
    /// </summary>
    /// <param name="group">int of the group that should be checked</param>
    /// <param name="idx">int of the index that should be checked if it is included in the group</param>
    /// <returns>bool if the index is in the group or not</returns>
	private bool IsIndexInsideGroup(int group, int idx)
    {
		if(group == -1)
        {
			return false;
        }

		for(int i = group; i < BODY_GROUPS.Length; i++)
        {
			if (BODY_GROUPS[i] == idx)
				return true;
			else if (BODY_GROUPS[i] == END_OF_GROUP)
				return false;
        }

		return false;
    }

	/// <summary>
	/// Gets the weight distance and returns the highest weight and its index
	/// </summary>
	/// <param name="highestWeight">Vector2 containing the current heighest weight</param>
	/// <param name="weight">BoneWeight from unity containg the 4 weightes influencing a bone</param>
	/// <returns>Vector2 where x is the highest index and y is the distance from the highest given weight to the highest weight from the given one</returns>
	private Vector2 GetWeightDistance(Vector2 highestWeight, BoneWeight weight)
    {
		int highestIdx = -1;
		float highest = -1.0f;

		if (highest < weight.weight0 && highestWeight.x != weight.boneIndex0)
		{
			highestIdx = weight.boneIndex0;
			highest = weight.weight0;
		}

		if (highest < weight.weight1 && highestWeight.x != weight.boneIndex1)
		{
			highestIdx = weight.boneIndex1;
			highest = weight.weight1;
		}

		if (highest < weight.weight2 && highestWeight.x != weight.boneIndex2)
		{
			highestIdx = weight.boneIndex2;
			highest = weight.weight2;

		}

		if (highest < weight.weight3 && highestWeight.x != weight.boneIndex3)
		{
			highestIdx = weight.boneIndex3;
			highest = weight.weight3;

		}

		return new Vector2(highestIdx, highestWeight.y - highest);
	}

	/// <summary>
    /// Precalculate the group data once(for faster processing later)
    /// </summary>
	private void GenerateGroupData()
    {
		Debug.Assert(smr != null);

		var mesh = smr.sharedMesh;
		var weights = mesh.boneWeights;
		Vector2[] customData = new Vector2[weights.Length];


		for (int i = 0; i < weights.Length; ++i)
		{
			Vector2 highestIdx = this.GetHeightBoneInfluence(weights[i]);
			customData[i] = this.GetWeightDistance(highestIdx, weights[i]);

			if (this.IsIndexInsideGroup(this.GetGroupIndex((int)highestIdx.x), (int)customData[i].x) == true)
			{
				customData[i].y = 1.0f;
			}

			
		}

		mesh.uv2 = customData;
	}

	/// <summary>
    /// Returns a list of the selected bones
    /// </summary>
    /// <returns>List of int of the selcted bones (already mapped to the transform indices)</returns>
	public List<int> GetSelectedBones()
    {
		List<int> selectedBones = new List<int>();

		for(int i = 0; i < this.selectedRegions.Count; i++)
        {
			int target = 0;

			if(this.selectedRegions[i] > NONE_GROUP_BONES)
            {
				target = this.selectedRegions[i];
			}
			else
            {
				target = this.GetGroupNumber(this.selectedRegions[i]);
				
            }

			int idx = System.Array.IndexOf(BONE_MAPPING, target);
			
			if(idx != -1)
            {
				selectedBones.Add(-BONE_MAPPING[idx+1]);
            }
			else
            {
				print("Warning couldnt find mapping for index: " + this.selectedRegions[i].ToString());
            }

        }

		return selectedBones;
    }

	/// <summary>
    /// Highlights the given bones on the avatar by the given indices (transform indices should be used here)
    /// </summary>
    /// <param name="selectedBones">List of int with the bones that should be highlighted</param>
	public void SetBoneSelection(List<int> selectedBones)
    {
		this.selectedRegions.Clear();

		for(int i = 0; i < selectedBones.Count; i++)
        {
			int idx = System.Array.IndexOf(BONE_MAPPING, -selectedBones[i], 1);

			if(idx != -1)
            {
				int groupIdx = BONE_MAPPING[idx - 1];

				if (groupIdx >= NONE_GROUP_BONES)
                {
					this.selectedRegions.Add(groupIdx);
				}
				else
                {
					int realGroupIndex = this.GetGroupFromNumber(groupIdx);
					this.selectedRegions.Add(realGroupIndex);
				}
            }

		}

		this.Highlight(-1);
    }

	/// <summary>
	/// Change vertex colors of the highlighting given bone (to make them actually highlighted)
	/// </summary>
	/// <param name="boneIndex">int of the index of that bone</param>
	void Highlight(int boneIndex) 
	{
		//only add an id if we the index makes sense
		if (boneIndex >= 0)
		{
			int groupIndex = this.GetGroupIndex(boneIndex);

			if (groupIndex == -1)
			{
				groupIndex = NONE_GROUP_BONES + boneIndex;
			}

			if (this.selectedRegions.Contains(groupIndex) == true)
			{
				this.selectedRegions.Remove(groupIndex);
			}
			else
			{
				this.selectedRegions.Add(groupIndex);
			}

		}

		var mesh = smr.sharedMesh;
		var weights = mesh.boneWeights;
		var colors = new Color32[weights.Length];

		for (int i = 0; i < colors.Length; ++i)
		{ 
			Vector2 highestIdx = this.GetHeightBoneInfluence(weights[i]);
			Color32 regionColor = COLOR_ARRAY[(int)highestIdx.x % COLOR_ARRAY.Length];

			for (int j = 0; j < this.selectedRegions.Count; j++)
			{

				if(this.selectedRegions[j] >= NONE_GROUP_BONES)
                {
					if (this.selectedRegions.Contains((int)highestIdx.x + NONE_GROUP_BONES) == true)
					{
						regionColor = highlightColor;
						break;
					}

				}
				else if(this.IsIndexInsideGroup(this.selectedRegions[j], (int)highestIdx.x) == true)
                {
					regionColor = highlightColor;
					break;

				}
			}

			colors[i] = regionColor;
		}

		mesh.colors32 = colors;
	}

	/// <summary>
    /// Get the highest influencable bone for that given weight
    /// </summary>
    /// <param name="weight">Boneweight of the weight for that bone / vertices group </param>
    /// <returns>Vector2 wherex contains the index and y contains the highest weight</returns>
	private Vector2 GetHeightBoneInfluence(BoneWeight weight)
    {
		int highestIdx = weight.boneIndex0;
		float highest = weight.weight0;

		if (highest < weight.weight1)
        {
			highestIdx = weight.boneIndex1;
			highest = weight.weight1;
        }

		if (highest < weight.weight2)
		{
			highestIdx = weight.boneIndex2;
			highest = weight.weight2;

		}

		if (highest < weight.weight3)
		{
			highestIdx = weight.boneIndex3;
			highest = weight.weight3;

		}

		return new Vector2 (highestIdx, highest) ;
    }

	/// <summary>
    /// Initially call to create all the specific components and classes that we are needing
    /// </summary>
	void Awake() 
	{

		if (smr == null) 
			smr = GetComponent<MeshFilter>();

		OVRCameraRig rig = FindObjectOfType<OVRCameraRig>();
		Transform parent = rig.transform.parent;
		
		this.leftHandRig  = parent.Find("InputOVR/Controllers/LeftController/ControllerInteractors/ControllerRayInteractor/ControllerPointerPose");
		this.rightHandRig = parent.Find("InputOVR/Controllers/RightController/ControllerInteractors/ControllerRayInteractor/ControllerPointerPose");

		this.GenerateGroupData();
	}

	/// <summary>
    /// To highlight nothing at the beginning when the application starts
    /// </summary>
	void Start()
    {
		this.Highlight(-1);
	}

	/// <summary>
    /// Checks if the controller we pressed every frame
    /// </summary>
	void Update()
	{

		if (this.debugUseMouse == true)
		{
			this.HighlightMouse();
		}
		else
		{
			this.HighlightController();
		}
	}

	/// <summary>
    /// Checks if a controller was pressed and performs a raycast to check if an area on the avatar bone highlighter should be highlighted or not
    /// </summary>
	private void HighlightController()
    {
		if (OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger))
		{
			Ray ray = new Ray();
			ray.direction = this.rightHandRig.forward;
			ray.origin = this.rightHandRig.position;
			this.PerformRaycast(ray);
		}
		else if(OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger))
		{
			Ray ray = new Ray();
			ray.direction = this.leftHandRig.forward;
			ray.origin = this.leftHandRig.position;
			this.PerformRaycast(ray);
		}
	}

	/// <summary>
    /// Small debug function to allow highlighting with the mosue
    /// </summary>
	private void HighlightMouse()
	{
		if (Input.GetMouseButtonDown(0))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			this.PerformRaycast(ray);
		}
	}

	/// <summary>
    /// Performs a raycast to check if the ray hits the bone highlighter avatar and highlights the region if this is the case
    /// </summary>
    /// <param name="ray"></param>
	private void PerformRaycast(Ray ray)
    {
		RaycastHit hit;

		int layerMask = 1 << 14;

		if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
		{
			MeshCollider meshCollider = hit.collider as MeshCollider;

			if (meshCollider == null || meshCollider.sharedMesh == null)
			{
				return;
			}
			else if (meshCollider.sharedMesh != this.smr.sharedMesh)
			{
				return;
			}

			Mesh mesh = meshCollider.sharedMesh;
			int[] triangles = mesh.triangles;
			BoneWeight[] weight = mesh.boneWeights;

			int vertexIndex = triangles[hit.triangleIndex * 3];
			BoneWeight selectedWeight = weight[vertexIndex];

			if (selectedWeight == null)
            {
				return;
            }

			this.clickSound.Play();
			Highlight(selectedWeight.boneIndex0);
		}
	}

}
