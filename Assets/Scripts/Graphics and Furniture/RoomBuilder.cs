using UnityEngine;
using UnityEngine.Rendering;


public class RoomBuilder : MonoBehaviour
{
    [Header("ICT Lab Room Settings")]
    public float roomWidth = 16f;
    public float roomLength = 16f;
    public float wallHeight = 4f;
    public float wallThickness = 0.2f;

    [Header("Window Settings")]
    public float windowWidth = 8f;
    public float windowHeight = 3f;
    public float windowBottomHeight = 0.8f;

    [Header("Table & PC Settings")]
    public Vector3 tablePosition = new Vector3(0f, 0f, 0f);
    public float pcHeightOffset = 0.8f;

    [Header("Lighting Settings")]
    public Color ceilingLightColor = new Color(0.95f, 0.98f, 1f); // Cool white lab light
    public float ceilingLightIntensity = 2.5f;
    public float ceilingLightRange = 15f;

    void Start()
    {
        InitializeEnvironment();
        BuildICTLaboratory();
    }

    void InitializeEnvironment()
    {
        RenderSettings.ambientMode = AmbientMode.Skybox;
        RenderSettings.ambientIntensity = 1.2f;
        QualitySettings.globalTextureMipmapLimit = 0;
    }

    void BuildICTLaboratory()
    {
        // 1. Materials
        Material wallMat = new Material(Shader.Find("Universal Render Pipeline/Lit"))
        {
            color = new Color(0.9f, 0.92f, 0.95f)
        };
        wallMat.SetFloat("_Smoothness", 0.2f);

        // Naayos na Glass Material (Transparent at hindi na masyadong makintab/maliwanag)
        Material glassMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        Color glassColor = new Color(0.85f, 0.92f, 0.98f, 0.15f);
        glassMat.color = glassColor;
        if (glassMat.HasProperty("_BaseColor"))
        {
            glassMat.SetColor("_BaseColor", glassColor);
        }
        glassMat.SetFloat("_Surface", 1); // Transparent mode
        glassMat.SetFloat("_Blend", 0);   // Alpha blend
        glassMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        glassMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        glassMat.SetInt("_ZWrite", 0);
        glassMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        glassMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        glassMat.SetFloat("_Smoothness", 0.1f); // Binabaan ang kislap/shine para hindi masakit sa mata

        Material deskMat = new Material(Shader.Find("Universal Render Pipeline/Lit"))
        {
            color = new Color(0.25f, 0.2f, 0.15f)
        };
        deskMat.SetFloat("_Smoothness", 0.7f);

        Material legMat = new Material(Shader.Find("Universal Render Pipeline/Lit"))
        {
            color = new Color(0.2f, 0.2f, 0.2f)
        };
        legMat.SetFloat("_Smoothness", 0.1f);

        Material lightFixtureMat = new Material(Shader.Find("Universal Render Pipeline/Lit"))
        {
            color = Color.white
        };
        lightFixtureMat.SetFloat("_Smoothness", 0.9f);

        GameObject labContainer = new GameObject("ICT_Lab_Room");

        // 2. Build Walls & Roof
        GameObject backWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backWall.name = "Lab_Wall_Back";
        backWall.transform.parent = labContainer.transform;
        backWall.transform.position = new Vector3(0f, wallHeight / 2f, roomLength / 2f);
        backWall.transform.localScale = new Vector3(roomWidth, wallHeight, wallThickness);
        backWall.GetComponent<Renderer>().material = wallMat;

        GameObject frontWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frontWall.name = "Lab_Wall_Front";
        frontWall.transform.parent = labContainer.transform;
        frontWall.transform.position = new Vector3(0f, wallHeight / 2f, -roomLength / 2f);
        frontWall.transform.localScale = new Vector3(roomWidth, wallHeight, wallThickness);
        frontWall.GetComponent<Renderer>().material = wallMat;

        float sideLength = (roomLength - windowWidth) / 2f;

        GameObject leftWall1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftWall1.name = "Lab_Wall_Left_1";
        leftWall1.transform.parent = labContainer.transform;
        leftWall1.transform.position = new Vector3(-roomWidth / 2f, wallHeight / 2f, roomLength / 2f - sideLength / 2f);
        leftWall1.transform.localScale = new Vector3(wallThickness, wallHeight, sideLength);
        leftWall1.GetComponent<Renderer>().material = wallMat;

        GameObject leftWall2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftWall2.name = "Lab_Wall_Left_2";
        leftWall2.transform.parent = labContainer.transform;
        leftWall2.transform.position = new Vector3(-roomWidth / 2f, wallHeight / 2f, -roomLength / 2f + sideLength / 2f);
        leftWall2.transform.localScale = new Vector3(wallThickness, wallHeight, sideLength);
        leftWall2.GetComponent<Renderer>().material = wallMat;

        float wallTopHeight = wallHeight - windowBottomHeight - windowHeight;

        GameObject leftBottom = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftBottom.name = "Lab_Window_Bottom";
        leftBottom.transform.parent = labContainer.transform;
        leftBottom.transform.position = new Vector3(-roomWidth / 2f, windowBottomHeight / 2f, 0f);
        leftBottom.transform.localScale = new Vector3(wallThickness, windowBottomHeight, windowWidth);
        leftBottom.GetComponent<Renderer>().material = wallMat;

        GameObject leftTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftTop.name = "Lab_Window_Top";
        leftTop.transform.parent = labContainer.transform;
        leftTop.transform.position = new Vector3(-roomWidth / 2f, wallHeight - (wallTopHeight / 2f), 0f);
        leftTop.transform.localScale = new Vector3(wallThickness, wallTopHeight, windowWidth);
        leftTop.GetComponent<Renderer>().material = wallMat;

        GameObject windowGlass = GameObject.CreatePrimitive(PrimitiveType.Cube);
        windowGlass.name = "Lab_Window_Glass";
        windowGlass.transform.parent = labContainer.transform;
        windowGlass.transform.position = new Vector3(-roomWidth / 2f, windowBottomHeight + (windowHeight / 2f), 0f);
        windowGlass.transform.localScale = new Vector3(wallThickness * 0.3f, windowHeight, windowWidth);
        windowGlass.GetComponent<Renderer>().material = glassMat;

        Collider glassCollider = windowGlass.GetComponent<Collider>();
        if (glassCollider != null)
        {
            Destroy(glassCollider);
        }

        float doorWidth = 2.2f;
        float doorHeight = 3f;
        float rightSideLength = (roomLength - doorWidth) / 2f;

        GameObject rightWall1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightWall1.name = "Lab_Wall_Right_1";
        rightWall1.transform.parent = labContainer.transform;
        rightWall1.transform.position = new Vector3(roomWidth / 2f, wallHeight / 2f, roomLength / 2f - rightSideLength / 2f);
        rightWall1.transform.localScale = new Vector3(wallThickness, wallHeight, rightSideLength);
        rightWall1.GetComponent<Renderer>().material = wallMat;

        GameObject rightWall2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightWall2.name = "Lab_Wall_Right_2";
        rightWall2.transform.parent = labContainer.transform;
        rightWall2.transform.position = new Vector3(roomWidth / 2f, wallHeight / 2f, -roomLength / 2f + rightSideLength / 2f);
        rightWall2.transform.localScale = new Vector3(wallThickness, wallHeight, rightSideLength);
        rightWall2.GetComponent<Renderer>().material = wallMat;

        float doorTopHeight = wallHeight - doorHeight;
        GameObject rightTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightTop.name = "Lab_Door_Top";
        rightTop.transform.parent = labContainer.transform;
        rightTop.transform.position = new Vector3(roomWidth / 2f, wallHeight - (doorTopHeight / 2f), 0f);
        rightTop.transform.localScale = new Vector3(wallThickness, doorTopHeight, doorWidth);
        rightTop.GetComponent<Renderer>().material = wallMat;

        GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roof.name = "Lab_Roof";
        roof.transform.parent = labContainer.transform;
        roof.transform.position = new Vector3(0f, wallHeight + (wallThickness / 2f), 0f);
        roof.transform.localScale = new Vector3(roomWidth, wallThickness, roomLength);
        roof.GetComponent<Renderer>().material = wallMat;

        // 3. Build Realistic Computer Desk (Top + 4 Legs)
        float deskTopThickness = 0.15f;
        GameObject deskTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
        deskTop.name = "Desk_Top";
        deskTop.transform.parent = labContainer.transform;
        deskTop.transform.position = new Vector3(tablePosition.x, tablePosition.y, tablePosition.z);
        deskTop.transform.localScale = new Vector3(4f, deskTopThickness, 2f);
        deskTop.GetComponent<Renderer>().material = deskMat;
        if (deskTop.GetComponent<Collider>() == null) deskTop.AddComponent<BoxCollider>();

        float legHeight = 1.2f;
        float halfWidth = 1.8f;
        float halfDepth = 0.8f;

        Vector3[] legPositions = new Vector3[]
        {
            new Vector3(tablePosition.x - halfWidth, tablePosition.y - (legHeight / 2f), tablePosition.z - halfDepth),
            new Vector3(tablePosition.x + halfWidth, tablePosition.y - (legHeight / 2f), tablePosition.z - halfDepth),
            new Vector3(tablePosition.x - halfWidth, tablePosition.y - (legHeight / 2f), tablePosition.z + halfDepth),
            new Vector3(tablePosition.x + halfWidth, tablePosition.y - (legHeight / 2f), tablePosition.z + halfDepth)
        };

        for (int i = 0; i < 4; i++)
        {
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leg.name = "Desk_Leg_" + (i + 1);
            leg.transform.parent = labContainer.transform;
            leg.transform.position = legPositions[i];
            leg.transform.localScale = new Vector3(0.15f, legHeight, 0.15f);
            leg.GetComponent<Renderer>().material = legMat;
        }

        // 4. Place System Unit on Top of Desk
        GameObject pc = GameObject.Find("personal_computer");
        if (pc != null)
        {
            pc.transform.parent = labContainer.transform;
            float deskSurfaceY = tablePosition.y + (deskTopThickness / 2f);
            pc.transform.position = new Vector3(tablePosition.x, deskSurfaceY + pcHeightOffset, tablePosition.z);
            Debug.Log("System unit placed on desk!");
        }
        else
        {
            Debug.LogWarning("Personal computer object not found!");
        }

        // 5. Overhead Fluorescent Ceiling Lights (Grid Layout)
        float[] lightXPositions = { -4f, 4f };
        float[] lightZPositions = { -4f, 0f, 4f };

        foreach (float x in lightXPositions)
        {
            foreach (float z in lightZPositions)
            {
                // Visual Light Fixture (Tube strip on ceiling)
                GameObject lightFixture = GameObject.CreatePrimitive(PrimitiveType.Cube);
                lightFixture.name = "Ceiling_Light_Fixture";
                lightFixture.transform.parent = labContainer.transform;
                lightFixture.transform.position = new Vector3(x, wallHeight - 0.05f, z);
                lightFixture.transform.localScale = new Vector3(2f, 0.05f, 0.4f);
                lightFixture.GetComponent<Renderer>().material = lightFixtureMat;
                Destroy(lightFixture.GetComponent<Collider>()); // No collision needed

                // Actual Point Light inside the fixture
                GameObject pointLightObj = new GameObject("Lab_Ceiling_Light");
                pointLightObj.transform.parent = lightFixture.transform;
                pointLightObj.transform.localPosition = new Vector3(0f, -0.2f, 0f);

                Light lightComp = pointLightObj.AddComponent<Light>();
                lightComp.type = LightType.Point;
                lightComp.color = ceilingLightColor;
                lightComp.intensity = ceilingLightIntensity;
                lightComp.range = ceilingLightRange;
                lightComp.shadows = LightShadows.Soft;
            }
        }
    }
}