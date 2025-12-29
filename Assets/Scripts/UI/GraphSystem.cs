using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Class that manages the displaying of the graph for the virtual trainer
/// </summary>
public class GraphSystem : MonoBehaviour
{
    private const float MIN_SLOPE = 0.1f;

    public LineRenderer lineRenderer;
    public RectTransform contentGraphArea;
    public RectTransform xLabelSection;
    public RectTransform dots;
    public GameObject areaSection;
    public MotionMatchingSystem motionMatchingSystem;
    public AudioSource btnClick;

    public GameObject lineHandlerPrefab;
    public GameObject xLabelPrefab;


    /// <summary>
    /// Plot the graph with the given data
    /// </summary>
    /// <param name="xValues">float array of the x-entries</param>
    /// <param name="yValues">float array of the y-entries</param>
    public void PlotGraph(float[] xValues, float[] yValues)
    {
        this.PlotLineGraph(xValues, yValues);
        this.PlotSurface(yValues);
    }


    /// <summary>
    /// Gets the points of interest (slope direction changes or high slopes) and also returns which index is the worst performed one
    /// </summary>
    /// <param name="yValues">float array of all the y-Values</param>
    /// <returns>List of int with all the points that could be interesting for us and the worst poforming index from the list</returns>
    (List<int>, int) GetPointsOfInterests(float[] yValues)
    {
        List<int> pointsOfInterest = new List<int>();

        pointsOfInterest.Add(0);//the first element is always interesting for us 

        //variables to indicate the worst performing entry
        float worstSetup = pointsOfInterest[0];
        int worstIdx = 0;
        int worstArrayIdx = 0;

        for (int i = 1; i < yValues.Length - 1; i++)
        {
            float prevY = yValues[i-1];
            float currentY = yValues[i];
            float nextY = yValues[i+1];


            float slopePrev = currentY - prevY;
            float slopeNext = nextY - currentY;

            if(currentY < worstSetup)
            {
                worstSetup = currentY;
                worstIdx = i;
            }

            if(Mathf.Sign(slopePrev) != Mathf.Sign(slopeNext))
            {
                pointsOfInterest.Add(i);
            }
            else if(Mathf.Abs(slopeNext) >= MIN_SLOPE)
            {
                pointsOfInterest.Add(i);
            }

        }

        bool worstContained = false;

        for(int i = 0; i< pointsOfInterest.Count; i++)
        {
            if(pointsOfInterest[i] == worstIdx)
            {
                worstArrayIdx = i;
                worstContained = true;
                break;
            }
        }

        if(worstContained == false)
        {
            pointsOfInterest.Add(worstIdx);
            worstArrayIdx = pointsOfInterest.Count - 1;
        }

        pointsOfInterest.Add(yValues.Length - 1);//the last element is always interesting for us 

        return (pointsOfInterest, worstArrayIdx);
    }

    /// <summary>
    /// Called when a user pressed on a dot in the graph. Sets the specific frame on the motionsystem to show it etc.
    /// </summary>
    /// <param name="frameIndex">int of the selected frame of the recording</param>
    /// <param name="dotIndex">int of the selected dot index</param>
    /// <param name="worstDotIndex">int of the worst performing dot in the array (currently not used)</param>
    public void DotSelected(int frameIndex, int dotIndex, int worstDotIndex)
    {

        //reset all colors
        this.ResetDotColors(worstDotIndex);

        this.btnClick.Play();
        this.motionMatchingSystem.ShowTimeFrame(frameIndex);

        this.dots.GetChild(dotIndex).GetComponent<Image>().color = new Color32(255, 0, 0, 255);
    }

    /// <summary>
    /// Resets the colors for all of the dots
    /// </summary>
    /// <param name="worstDotIndex">int of the worst performing dot to not reset this (currently not used)</param>
    void ResetDotColors(int worstDotIndex)
    {
        //reset all colors
        for (int i = 0; i < this.dots.childCount; i++)
        {
            if (false)
            {
                this.dots.GetChild(i).GetComponent<Image>().color = new Color32(255, 0, 255, 255);//special color for the worst index to make it more visible for the user
            }
            else
            {
                this.dots.GetChild(i).GetComponent<Image>().color = new Color32(255, 255, 255, 255);
            }
        }

    }

    /// <summary>
    /// Plot the line graph with an additional surface below it 
    /// </summary>
    /// <param name="xValues">float array of the x-entries for the x-axis</param>
    /// <param name="yValues">float array of the y-entries for the y-axis</param>
    void PlotLineGraph(float[] xValues, float[] yValues)
    {
        float width = this.contentGraphArea.rect.width;
        float height = this.contentGraphArea.rect.height;

        var bestPoints = this.GetPointsOfInterests(yValues);
        List<int> pointsOfInterest = bestPoints.Item1;
        int worstDotIndex = bestPoints.Item2;


        //cleanup
        for (int i = this.xLabelSection.childCount-1; i>= 0; i--)
        {
            Object.Destroy(this.xLabelSection.GetChild(i).gameObject);
        }

        for (int i = this.dots.childCount - 1; i >= 0; i--)
        {
            Object.Destroy(this.dots.GetChild(i).gameObject);
        }

        this.lineRenderer.positionCount = yValues.Length;

        for(int i = 0; i < yValues.Length; i++)
        {
            float targetX = ( (float)i / ((float)yValues.Length - 1) ) * width;
            float targetY = height * yValues[i];

            this.lineRenderer.SetPosition(i, new Vector3(targetX, targetY, 0.0f));

        }

        for(int i = 0; i < pointsOfInterest.Count; i++)
        {
            GameObject label = Instantiate(this.xLabelPrefab, this.xLabelSection);
            GameObject btn = Instantiate(this.lineHandlerPrefab, this.dots);

            float positionX = this.lineRenderer.GetPosition(pointsOfInterest[i]).x;
            float positionY = this.lineRenderer.GetPosition(pointsOfInterest[i]).y;

            int desiredIndex = pointsOfInterest[i];
            int idx = i;

            float value = xValues[desiredIndex];
            value = Mathf.Round(value * 100.0f) / 100.0f;

            RectTransform labelTransform = (RectTransform)label.transform;
            labelTransform.anchoredPosition = new Vector2(positionX, 41.0f);
            labelTransform.sizeDelta = new Vector2(50.0f, 50.0f);


            RectTransform btnTransform = (RectTransform)btn.transform;
            btnTransform.anchoredPosition = new Vector2(positionX, positionY);

            Button b = btn.GetComponent<Button>();
            b.onClick.AddListener(delegate () { DotSelected(desiredIndex, idx, worstDotIndex); });


            label.transform.GetChild(1).gameObject.GetComponent<Text>().text = value.ToString() + "s";
        }

        this.ResetDotColors(worstDotIndex);
    }

    /// <summary>
    /// Creates the surface area for the given y-values
    /// </summary>
    /// <param name="yValues">float array of the y-values</param>
    void PlotSurface(float[] yValues)
    {
        float width = this.contentGraphArea.rect.width;
        float height = this.contentGraphArea.rect.height;

        MeshRenderer meshRenderer = this.areaSection.GetComponent<MeshRenderer>();
        MeshFilter meshFilter = this.areaSection.GetComponent<MeshFilter>();

        int vertexCount = yValues.Length * 3 * 2;
        int numTriangles = yValues.Length * 3 * 2;

        Vector3[] vertices = new Vector3[vertexCount];
        Vector2[] uv = new Vector2[vertexCount];
        int[] tris = new int[numTriangles];

        for (int i = 0; i < yValues.Length - 1; i++)
        {
            float targetX = ((float)i / ((float)yValues.Length - 1)) * width;
            float targetY = height * yValues[i];

            float targetNextX = ((float)(i+1) / ((float)yValues.Length - 1)) * width;
            float targetNextY = height * yValues[i+1];

            int vIndex = i * 6;
            int tIndex = i * 6;

            vertices[vIndex + 0] = new Vector3(targetX, targetY, 0.0f);
            vertices[vIndex + 1] = new Vector3(targetNextX, targetNextY, 0.0f);
            vertices[vIndex + 2] = new Vector3(targetX, 0.0f, 0.0f);

            vertices[vIndex + 3] = new Vector3(targetNextX, targetNextY, 0.0f);
            vertices[vIndex + 5] = new Vector3(targetNextX, 0.0f, 0.0f);
            vertices[vIndex + 4] = new Vector3(targetX, 0.0f, 0.0f);


            tris[tIndex + 0] = tIndex + 0;
            tris[tIndex + 1] = tIndex + 1;
            tris[tIndex + 2] = tIndex + 2;

            tris[tIndex + 3] = tIndex + 3;
            tris[tIndex + 4] = tIndex + 4;
            tris[tIndex + 5] = tIndex + 5;

        }


        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.uv = uv;

        meshFilter.mesh = mesh;
    }
}
