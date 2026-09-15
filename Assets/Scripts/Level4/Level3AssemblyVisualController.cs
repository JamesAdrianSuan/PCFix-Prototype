using UnityEngine;

public class Level3AssemblyVisualController : MonoBehaviour
{
    [System.Serializable]
    public class AssemblyVisual
    {
        public string componentName;
        public GameObject[] visualObjects;
    }

    [Header("Level 3 Assembly Visuals")]
    public AssemblyVisual[] assemblyVisuals;

    [Header("Restore Settings")]
    public float restoreInterval = 0.1f;

    private void Start()
    {
        // Keep visuals available during Assembly.
        InvokeRepeating(
            nameof(RestoreAssemblyVisuals),
            0.1f,
            restoreInterval
        );
    }

    // This method is called by Level3PerformanceManager.
    public void EnableAllAssemblyVisuals()
    {
        RestoreAssemblyVisuals();

        Debug.Log(
            "LEVEL 3: Assembly visuals enabled."
        );
    }

    private void RestoreAssemblyVisuals()
    {
        if (assemblyVisuals == null)
            return;

        foreach (AssemblyVisual visual in assemblyVisuals)
        {
            if (visual == null ||
                visual.visualObjects == null)
                continue;

            foreach (GameObject obj in visual.visualObjects)
            {
                if (obj == null)
                    continue;

                obj.SetActive(true);

                MeshRenderer[] renderers =
                    obj.GetComponentsInChildren<MeshRenderer>(true);

                foreach (MeshRenderer renderer in renderers)
                {
                    renderer.enabled = true;
                }

                MeshCollider[] colliders =
                    obj.GetComponentsInChildren<MeshCollider>(true);

                foreach (MeshCollider collider in colliders)
                {
                    collider.enabled = true;
                }
            }
        }
    }

    private void OnDestroy()
    {
        CancelInvoke(nameof(RestoreAssemblyVisuals));
    }
}