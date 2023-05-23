using System.Collections.Generic;
using UnityEngine;

public class LineDrawer : MonoBehaviour
{
    public GameObject linePrefab;
    public string lineTag = "Line";

    private LineQuantifier lineQuantifier;
    private GameObject currentLine;
    private LineRenderer currentRenderer;

    void Start()
    {
        lineQuantifier = FindObjectOfType<LineQuantifier>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Clear();
            lineQuantifier.Clear();
        }


        if (Input.GetMouseButtonDown(0))
        {
            // Instantiate a new LineRenderer for the new line
            currentLine = Instantiate(linePrefab, Vector3.zero, Quaternion.identity);
            currentLine.tag = lineTag;
            currentRenderer = currentLine.GetComponent<LineRenderer>();
            currentRenderer.positionCount = 0;
        }

        if (Input.GetMouseButton(0))
        {
            // Add points to the line while the mouse button is held down
            Vector3 mousePosition = Input.mousePosition;
            Vector3 point = Camera.main.ScreenToWorldPoint(mousePosition);
            point.z = 0f;

            currentRenderer.positionCount ++;
            currentRenderer.SetPosition(currentRenderer.positionCount - 1, point);
        }


        if (Input.GetMouseButtonUp(0))
        {
            lineQuantifier.QuantifyLine(ref currentLine);
        }
    }

    void Clear()
    {
        GameObject[] targetObjects = GameObject.FindGameObjectsWithTag(lineTag);
        foreach (GameObject obj in targetObjects){
            Destroy(obj);
        }
    }
}
