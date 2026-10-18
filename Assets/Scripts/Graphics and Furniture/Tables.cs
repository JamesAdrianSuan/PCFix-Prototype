using UnityEngine;

public class Tables : MonoBehaviour
{
    [Header("Layout Settings")]
    public int rows = 3;               // Number of desk rows going backward
    public int columns = 4;            // Number of desks per row (left to right)
    public float spacingX = 5f;        // Distance between desks horizontally
    public float spacingZ = 4f;        // Distance between desks vertically (rows)
    public Vector3 startPosition = new Vector3(-4f, 0f, -4f); // Starting offset in the room

    [Header("Desk Dimensions")]
    public float deskWidth = 3f;
    public float deskDepth = 1.5f;
    public float deskHeight = 1f;
    public float deskThickness = 0.15f;

    void Start()
    {
        BuildLabTablesAndPCs();
    }

    void BuildLabTablesAndPCs()
    {
        // Oak Wood Material (Warm golden-brown wood tone with natural polish)
        Material oakWoodMat = new Material(Shader.Find("Universal Render Pipeline/Lit"))
        {
            color = new Color(0.78f, 0.58f, 0.40f)
        };
        oakWoodMat.SetFloat("_Smoothness", 0.45f);

        Material caseMat = new Material(Shader.Find("Universal Render Pipeline/Lit"))
        {
            color = new Color(0.15f, 0.15f, 0.15f)
        };

        Material monitorMat = new Material(Shader.Find("Universal Render Pipeline/Lit"))
        {
            color = new Color(0.1f, 0.1f, 0.1f)
        };

        Material screenMat = new Material(Shader.Find("Universal Render Pipeline/Lit"))
        {
            color = new Color(0.1f, 0.3f, 0.5f)
        };

        Material chairMat = new Material(Shader.Find("Universal Render Pipeline/Lit"))
        {
            color = new Color(0.2f, 0.2f, 0.22f)
        };
        chairMat.SetFloat("_Smoothness", 0.3f);

        GameObject labContainer = new GameObject("ICT_Lab_Workstations");

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                Vector3 pos = startPosition + new Vector3(c * spacingX, 0f, r * spacingZ);

                GameObject workstation = new GameObject($"Workstation_Row{r}_Col{c}");
                workstation.transform.parent = labContainer.transform;
                workstation.transform.position = pos;

                // 1. Desk Top
                GameObject deskTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
                deskTop.name = "Desk_Top";
                deskTop.transform.parent = workstation.transform;
                deskTop.transform.localPosition = new Vector3(0f, deskHeight, 0f);
                deskTop.transform.localScale = new Vector3(deskWidth, deskThickness, deskDepth);
                deskTop.GetComponent<Renderer>().material = oakWoodMat;

                // 2. Desk Legs
                float legWidth = 0.12f;
                float halfW = (deskWidth / 2f) - 0.2f;
                float halfD = (deskDepth / 2f) - 0.2f;

                Vector3[] legLocalPositions = new Vector3[]
                {
                    new Vector3(-halfW, deskHeight / 2f, -halfD),
                    new Vector3(halfW, deskHeight / 2f, -halfD),
                    new Vector3(-halfW, deskHeight / 2f, halfD),
                    new Vector3(halfW, deskHeight / 2f, halfD)
                };

                for (int i = 0; i < 4; i++)
                {
                    GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    leg.name = $"Leg_{i + 1}";
                    leg.transform.parent = workstation.transform;
                    leg.transform.localPosition = legLocalPositions[i];
                    leg.transform.localScale = new Vector3(legWidth, deskHeight, legWidth);
                    leg.GetComponent<Renderer>().material = oakWoodMat;
                }

                float surfaceY = deskHeight + (deskThickness / 2f);

                // 3. System Unit (Nasa ibabaw ng mesa)
                GameObject pcTower = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pcTower.name = "System_Unit";
                pcTower.transform.parent = workstation.transform;
                pcTower.transform.localPosition = new Vector3(0.9f, surfaceY + 0.35f, 0f);
                pcTower.transform.localScale = new Vector3(0.35f, 0.7f, 0.7f);
                pcTower.GetComponent<Renderer>().material = caseMat;

                // 4. Monitor
                GameObject monitor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                monitor.name = "Monitor";
                monitor.transform.parent = workstation.transform;
                monitor.transform.localPosition = new Vector3(-0.3f, surfaceY + 0.45f, -0.2f);
                monitor.transform.localScale = new Vector3(0.8f, 0.5f, 0.1f);
                monitor.GetComponent<Renderer>().material = monitorMat;

                // Screen
                GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Cube);
                screen.name = "Screen";
                screen.transform.parent = monitor.transform;
                screen.transform.localPosition = new Vector3(0f, 0f, -0.55f);
                screen.transform.localScale = new Vector3(0.9f, 0.9f, 0.1f);
                screen.GetComponent<Renderer>().material = screenMat;

                Destroy(screen.GetComponent<Collider>());

                // 5. Workstation Chair (Nasa likod ng mesa / system unit, nakaharap sa monitor)
                GameObject chair = new GameObject("Chair");
                chair.transform.parent = workstation.transform;
                chair.transform.localPosition = new Vector3(-0.3f, 0f, 0.95f); // Nakapwesto sa likuran ng desk

                // Seat
                GameObject seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
                seat.name = "Seat";
                seat.transform.parent = chair.transform;
                seat.transform.localPosition = new Vector3(0f, 0.45f, 0f);
                seat.transform.localScale = new Vector3(0.5f, 0.08f, 0.5f);
                seat.GetComponent<Renderer>().material = chairMat;

                // Backrest (Nasa likod ng upuan, malayo sa mesa)
                GameObject backrest = GameObject.CreatePrimitive(PrimitiveType.Cube);
                backrest.name = "Backrest";
                backrest.transform.parent = chair.transform;
                backrest.transform.localPosition = new Vector3(0f, 0.8f, 0.2f);
                backrest.transform.localScale = new Vector3(0.5f, 0.6f, 0.08f);
                backrest.GetComponent<Renderer>().material = chairMat;

                // Chair Legs
                float chairLegHeight = 0.45f;
                Vector3[] chairLegs = {
                    new Vector3(-0.2f, chairLegHeight / 2f, -0.2f),
                    new Vector3(0.2f, chairLegHeight / 2f, -0.2f),
                    new Vector3(-0.2f, chairLegHeight / 2f, 0.2f),
                    new Vector3(0.2f, chairLegHeight / 2f, 0.2f)
                };

                for (int i = 0; i < 4; i++)
                {
                    GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    leg.name = $"Chair_Leg_{i + 1}";
                    leg.transform.parent = chair.transform;
                    leg.transform.localPosition = chairLegs[i];
                    leg.transform.localScale = new Vector3(0.05f, chairLegHeight, 0.05f);
                    leg.GetComponent<Renderer>().material = chairMat;
                }
            }
        }

        Debug.Log($"Successfully generated workstations with matching chairs at the back!");
    }
}