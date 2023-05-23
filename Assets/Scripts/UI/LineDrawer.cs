using System.Collections.Generic;
using UnityEngine;

public class LineDrawer : MonoBehaviour
{
    public GameObject linePrefab;
    public string lineTag = "Line";

    [Range(0f, 0.1f)]
    public float lineSimplifyTolerance = 0.05f;

    [Range(0f, 1f)]
    public float lineEdgeTolerance = 0.4f;

    private LineQuantifier lineQuantifier;
    private LineRenderer currentRenderer;

    void Start()
    {
        lineQuantifier = FindObjectOfType<LineQuantifier>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            GameObject[] targetObjects = GameObject.FindGameObjectsWithTag(lineTag);
            foreach (GameObject obj in targetObjects){
                obj.SetActive(false);
            }
        }


        if (Input.GetMouseButtonDown(0))
        {
            // Instantiate a new LineRenderer for the new line
            GameObject newLine = Instantiate(linePrefab, Vector3.zero, Quaternion.identity);
            newLine.tag = lineTag;
            currentRenderer = newLine.GetComponent<LineRenderer>();
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
            currentRenderer.Simplify(lineSimplifyTolerance);
            lineQuantifier.Quantify();
        }
    }
}
