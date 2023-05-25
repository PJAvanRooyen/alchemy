using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Diagnostics;
using UnityEngine.U2D;

public class LineProperties
{
    public class SubLine
    {
        public Vector3 start = new Vector3();
        public Vector3 end = new Vector3();
    }

    public class Intersection
    {
        public SubLine line1 = new SubLine();
        public SubLine line2 = new SubLine();
    }

    public enum Shape
    {
        Line,
        IntersectingLine,
        Solid,
        IntersectingSolid,
        Other,
    }

    public enum ShapeType
    {
        // LineType
        Straight,
        Wave,
        ZigZag,
        Curve,
        OtherLine,

        // IntersectingLineType
        OtherIntersectingLine,

        // SolidType  
        Triangle,
        Square,
        Pentagon,
        Hexagon,
        Heptagon,
        Octagon,
        Circle,
        OtherSolid,

        // IntersectingSolidType 
        Infinity,
        OtherIntersectingSolid,

        // Other
        Other,
    }

    public List<Vector3> pointsList = new List<Vector3>();
    public List<int> edgeIndices = new List<int>();
    public List<Intersection> intersections = new List<Intersection>();
    public Shape shape = Shape.Other;
    public ShapeType shapeType = ShapeType.Other;

    public void Clear()
    {
        pointsList.Clear();
        edgeIndices.Clear();
        shape = Shape.Other;
        shapeType = ShapeType.Other;
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
        SetLineIntersections(ref lineProperties, ref renderer);
        SetShape(ref lineProperties, ref renderer);
        SetShapeType(ref lineProperties, ref renderer);

        linePropertiesList.Add(lineProperties);
    }

    public void Clear()
    {
        linePropertiesList.Clear();

        if (debug){
            debugText.text = "";
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

    private bool FormsSolid(ref LineRenderer renderer, float tolerance)
    {
        Vector3 firstPos = renderer.GetPosition(0);
        Vector3 lastPos = renderer.GetPosition(renderer.positionCount - 1);
        return Vector3.Distance(firstPos, lastPos) < tolerance;
    }

    private bool LinesIntersect(Vector3 a1, Vector3 a2, Vector3 b1, Vector3 b2)
    {
        if(a1 == b1 || a2 == b1 || a1 == b2 || a2 == b2){
            // Touching vertices does not count as an intersection.
            return false;
        }

        Vector3 lineVec1 = a2-a1;
        Vector3 lineVec2 = b2-b1;

        Vector3 lineVec3 = b1 - a1;
        Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
        Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

        float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

        //is coplanar, and not parallel
        if( Mathf.Abs(planarFactor) < 0.0001f && crossVec1and2.sqrMagnitude > 0.0001f){
            float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
            Vector3 intersectionPoint = a1 + (lineVec1 * s);
            
            float line1SqrMagnitude = lineVec1.sqrMagnitude;
            float line2SqrMagnitude = lineVec2.sqrMagnitude;

            if ((intersectionPoint - a1).sqrMagnitude <= line1SqrMagnitude  
                && (intersectionPoint - a2).sqrMagnitude <= line1SqrMagnitude  
                && (intersectionPoint - b1).sqrMagnitude <= line2SqrMagnitude 
                && (intersectionPoint - b2).sqrMagnitude <= line2SqrMagnitude){
                return true;
            }
        }

        return false;
    }

    void OnGUI()
    {
        if (debug){
            int count = linePropertiesList.Count;
            debugText.text = string.Format("Lines:\nCount: {0}\n", count.ToString());

            int idx = 0;
            foreach (LineProperties lineProperties in linePropertiesList){
                int posCount = lineProperties.pointsList.Count;
                int edgeCount = lineProperties.edgeIndices.Count;
                int intersectionCount = lineProperties.intersections.Count;
                LineProperties.Shape shape = lineProperties.shape;
                LineProperties.ShapeType shapeType = lineProperties.shapeType;
                debugText.text += string.Format("Line[{0}]: ShapeType {5}, Shape: {4}, PosCount: {1}, EdgeCount: {2}, IntersectionCount: {3}\n", idx, posCount, edgeCount, intersectionCount, shape, shapeType);
                idx++;
            }
        }
    }

    private void SetPoints(ref LineProperties lineProperties, ref LineRenderer renderer)
    {
        lineProperties.pointsList.Clear();
        lineProperties.pointsList = GetSimplifiedPoints(ref renderer, lineSimplifyTolerance);
    }

    //! NOTE: this method depends on the lineProperties pointsList.
    private void SetEdgeIndices(ref LineProperties lineProperties, ref LineRenderer renderer)
    {
        List<Vector3> pointsList = lineProperties.pointsList;
        List<Vector3> edges = GetSimplifiedPoints(ref renderer, lineEdgeTolerance);

        List<int> edgeIndices = lineProperties.edgeIndices;
        edgeIndices.Clear();

        // NOTE: don't include line start and end as edges.
        for(int edgeIdx = 1; edgeIdx < edges.Count - 1; edgeIdx++){
            Vector3 edge = edges[edgeIdx];
            int index = pointsList.IndexOf(edge);
            System.Diagnostics.Debug.Assert(index != -1);
            edgeIndices.Add(index);     
        }

        if(FormsSolid(ref renderer, isSolidTolerance)){
            //pointsList[pointsList.Count - 1] = pointsList[0];
            edgeIndices.Insert(0, 0);
        }
    }

    //! NOTE: this method depends on the lineProperties pointsList and edge count.
    private void SetLineIntersections(ref LineProperties lineProperties, ref LineRenderer renderer)
    {
        List<LineProperties.Intersection> intersections = lineProperties.intersections;
        intersections.Clear();

        List<Vector3> points = lineProperties.pointsList;
        List<int> edgeIndices = lineProperties.edgeIndices;
        int pointCount = edgeIndices.Count;

        if(pointCount < 2){
            return;
        }

        // Add the start and end of the line as vertices.
        List<int> vertices = new List<int>(edgeIndices);
        if(FormsSolid(ref renderer, isSolidTolerance)){
            vertices.Add(0);  
            pointCount += 1;
        }else{
            vertices.Insert(0, 0);
            vertices.Add(points.Count - 1);  
            pointCount += 2;
        }

        if(pointCount < 4){
            return;
        }
        
        // Iterate over each pair of adjacent points
        for (int i = 0; i < pointCount - 1; i++){
            int p1_idx = vertices[i];
            Vector3 p1 = points[p1_idx];
            int p2_idx = vertices[i + 1];
            Vector3 p2 = points[p2_idx];

            // Check this line against all other lines
            for (int j = 0; j < pointCount - 1; j++){
                if(j == i - 1 || j == i || j == i + 1){
                    // No need to check for intersections with self and neighbors.
                    continue;
                }

                int p3_idx = vertices[j];
                Vector3 p3 = points[p3_idx];
                int p4_idx = vertices[j + 1];
                Vector3 p4 = points[p4_idx];

                // Check if the two lines intersect
                if(LinesIntersect(p1, p2, p3, p4)){
                    LineProperties.SubLine subLine1 = new LineProperties.SubLine();
                    subLine1.start = p1;
                    subLine1.end = p2;

                    LineProperties.SubLine subLine2 = new LineProperties.SubLine();
                    subLine2.start = p3;
                    subLine2.end = p4;

                    LineProperties.Intersection intersection = new LineProperties.Intersection();
                    intersection.line1 = subLine1;
                    intersection.line2 = subLine2;

                    intersections.Add(intersection);
                }
            }
        }
    }

    //! NOTE: this method depends on the lineProperties edge count and intersections.
    private void SetShape(ref LineProperties lineProperties, ref LineRenderer renderer)
    {
        if(FormsSolid(ref renderer, isSolidTolerance)){
            if(lineProperties.intersections.Count == 0){
                lineProperties.shape = LineProperties.Shape.Solid;
            }else{
                lineProperties.shape = LineProperties.Shape.IntersectingSolid;
            }
        }else{
            if(lineProperties.intersections.Count == 0){
                lineProperties.shape = LineProperties.Shape.Line;
            }else{
                lineProperties.shape = LineProperties.Shape.IntersectingLine;
            }
        }
    }

    //! NOTE: this method depends on the lineProperties edge count and intersections.
    private void SetShapeType(ref LineProperties lineProperties, ref LineRenderer renderer)
    {
        int edgeCount = lineProperties.edgeIndices.Count;
        int intersectionCount = lineProperties.intersections.Count;

        if(FormsSolid(ref renderer, isSolidTolerance)){
            if(intersectionCount== 0){
                if(edgeCount == 3){
                    lineProperties.shapeType = LineProperties.ShapeType.Triangle;
                }else if(edgeCount == 4){
                    lineProperties.shapeType = LineProperties.ShapeType.Square;   
                }else if(edgeCount == 5){
                    lineProperties.shapeType = LineProperties.ShapeType.Pentagon;   
                }else if(edgeCount == 6){
                    lineProperties.shapeType = LineProperties.ShapeType.Hexagon;   
                }else if(edgeCount == 7){
                    lineProperties.shapeType = LineProperties.ShapeType.Heptagon;   
                }else if(edgeCount == 8){
                    lineProperties.shapeType = LineProperties.ShapeType.Octagon;   
                }else if(edgeCount > 8){
                    lineProperties.shapeType = LineProperties.ShapeType.Circle;   
                }else{
                    lineProperties.shapeType = LineProperties.ShapeType.OtherSolid; 
                }
            }else{
                if(edgeCount == 4){
                    lineProperties.shapeType = LineProperties.ShapeType.Triangle;
                }else{
                    lineProperties.shapeType = LineProperties.ShapeType.OtherIntersectingSolid;
                }
            }
        }else{
            if(intersectionCount == 0){
                if(edgeCount == 0){
                    lineProperties.shapeType = LineProperties.ShapeType.Straight;
                }else{
                    lineProperties.shapeType = LineProperties.ShapeType.ZigZag;
                }
            }else{
                lineProperties.shapeType = LineProperties.ShapeType.OtherIntersectingLine;
            }
        }
    }
}
