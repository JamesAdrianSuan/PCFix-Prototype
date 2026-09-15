using UnityEngine;

public class Level3AssemblyTargetColliderController : MonoBehaviour
{
    [Header("Level 3 Assembly Targets")]
    public GameObject[] assemblyTargets;

    private void Start()
    {
        SetAssemblyTargets(false);
    }

    public void EnableAssemblyTargets()
    {
        SetAssemblyTargets(true);

        Debug.Log("LEVEL 3: Assembly target colliders ENABLED.");
    }

    public void DisableAssemblyTargets()
    {
        SetAssemblyTargets(false);

        Debug.Log("LEVEL 3: Assembly target colliders DISABLED.");
    }

    private void SetAssemblyTargets(bool enabledState)
    {
        if (assemblyTargets == null)
            return;

        foreach (GameObject target in assemblyTargets)
        {
            if (target == null)
                continue;

            Collider[] colliders =
                target.GetComponentsInChildren<Collider>(true);

            foreach (Collider col in colliders)
            {
                col.enabled = enabledState;
            }
        }
    }
}