using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LineProperties
{
    public List<Vector3> pointsList = new List<Vector3>();
    public List<int> directionChangeIndices = new List<int>();

    public void Clear()
    {
        pointsList.Clear();
        directionChangeIndices.Clear();
    }
}

public class LineQuantifier : MonoBehaviour
{
    public bool debug = true;
    public string targetTag = "Line";
    public Text linesText;

    private void Start()
    {
        Quantify();
    }


    public void Quantify(){
        GameObject[] targetObjects = GameObject.FindGameObjectsWithTag(targetTag);
    }

    private void CheckDirectionChange()
    {
        // Check if there is a change in x or y direction
        // int pointCount = lineProperties.pointsList.Count;
        // if (pointCount > 1){
        //     Vector3 point = lineProperties.pointsList[pointCount - 1];
        //     Vector3 previousPoint = lineProperties.pointsList[pointCount - 2];
        //     Vector3 direction = point - previousPoint;

        //     if (Mathf.Sign(direction.x) != Mathf.Sign(previousPoint.x) ||
        //         Mathf.Sign(direction.y) != Mathf.Sign(previousPoint.y)){
        //         lineProperties.directionChangeIndices.Add(pointCount - 2);
        //     }
        // }
    }

    void OnGUI()
    {
        if (debug){
            GameObject[] targetObjects = GameObject.FindGameObjectsWithTag(targetTag);
            int count = targetObjects.Length;
            linesText.text = string.Format("Lines:\nCount: {0}\n", count.ToString());

            int idx = 0;
            foreach (GameObject obj in targetObjects){
                LineRenderer renderer = obj.GetComponent<LineRenderer>();
                int edgeCount = renderer.positionCount - 2;
                linesText.text += string.Format("Line[{0}]: EdgeCount: {1}\n", idx, edgeCount);
                idx++;
            }
        }
    }
}
