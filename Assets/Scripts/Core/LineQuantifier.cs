using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Diagnostics;

public class LineProperties
{
    public List<Vector3> pointsList = new List<Vector3>();
    public List<int> edgeIndices = new List<int>();

    public void Clear()
    {
        pointsList.Clear();
        edgeIndices.Clear();
    }
}

public class LineQuantifier : MonoBehaviour
{
    public bool debug = true;
    public string lineTag = "Line";

    [Range(0f, 0.1f)]
    public float lineSimplifyTolerance = 0.05f;

    [Range(0f, 1f)]
    public float lineEdgeTolerance = 0.4f;

    [Range(0f, 0.5f)]
    public float isSolidTolerance = 0.3f;

    public Text debugText;

    private List<LineProperties> linePropertiesList = new List<LineProperties>();

    private void Start()
    {
    }

    public void QuantifyLine(ref GameObject line){
        LineProperties lineProperties =  new LineProperties();

        LineRenderer renderer = line.GetComponent<LineRenderer>();
        SetPoints(ref lineProperties, ref renderer);
        SetEdgeIndices(ref lineProperties, ref renderer);

        linePropertiesList.Add(lineProperties);
    }

    private void SetPoints(ref LineProperties lineProperties, ref LineRenderer renderer)
    {
        lineProperties.pointsList = GetSimplifiedPoints(ref renderer, lineSimplifyTolerance);
    }

    private void SetEdgeIndices(ref LineProperties lineProperties, ref LineRenderer renderer)
    {
        List<Vector3> pointsList = lineProperties.pointsList;
        List<Vector3> edges = GetSimplifiedPoints(ref renderer, lineEdgeTolerance);

        List<int> edgeIndices = lineProperties.edgeIndices;
        foreach(Vector3 edge in edges){
            int index = pointsList.IndexOf(edge);
            System.Diagnostics.Debug.Assert(index != -1);
            edgeIndices.Add(index);     
        }

        if(formsSolid(ref renderer, isSolidTolerance)){
            edgeIndices.Add(0); 
        }
    }

    private List<Vector3> GetSimplifiedPoints(ref LineRenderer renderer, float tolerance)
    {
        Vector3[] positions = new Vector3[renderer.positionCount];
        renderer.GetPositions(positions);

        renderer.Simplify(tolerance);

        Vector3[] positionsSimplified = new Vector3[renderer.positionCount];
        renderer.GetPositions(positionsSimplified);

        // Reset the renderer
        renderer.positionCount = positions.Length;
        renderer.SetPositions(positions);

        return positionsSimplified.ToList();
    }

    private bool formsSolid(ref LineRenderer renderer, float tolerance)
    {
        Vector3 firstPos = renderer.GetPosition(0);
        Vector3 lastPos = renderer.GetPosition(renderer.positionCount - 1);
        return Vector3.Distance(firstPos, lastPos) < tolerance;
    }

    void OnGUI()
    {
        if (debug){
            int count = linePropertiesList.Count;
            debugText.text = string.Format("Lines:\nCount: {0}\n", count.ToString());

            int idx = 0;
            foreach (LineProperties lineProperties in linePropertiesList){
                int posCount = lineProperties.pointsList.Count;
                int edgeCount = lineProperties.edgeIndices.Count - 2;
                debugText.text += string.Format("Line[{0}]: PosCount: {1}, EdgeCount: {2}\n", idx, posCount, edgeCount);
                idx++;
            }
        }
    }

    public void Clear()
    {
        linePropertiesList.Clear();

        if (debug){
            debugText.text = "";
        }
    }
}
