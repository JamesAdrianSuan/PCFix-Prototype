using UnityEngine;
using TMPro;

public class AssemblyManager : MonoBehaviour
{
    [System.Serializable]
    public class AssemblyGroup
    {
        public string componentName;
        public AssemblyComponent component;
    }

    [Header("UI")]
    public TMP_Text instructionText;
    public TMP_Text progressText;

    [Header("Assembly Components")]
    public AssemblyGroup[] components;

    private int currentStep = 0;

    private void Start()
    {
        Debug.Log("=== ASSEMBLY MANAGER STARTED ===");

        DisableAllComponents();

        SetupCurrentStep();
    }

    // =========================================================
    // DISABLE ALL COMPONENTS
    // =========================================================

    private void DisableAllComponents()
    {
        if (components == null)
        {
            Debug.LogError(
                "Assembly Components array is NULL!"
            );

            return;
        }

        foreach (AssemblyGroup group in components)
        {
            if (group == null ||
                group.component == null)
                continue;

            group.component.Setup(this);
            group.component.SetInteractable(false);
        }
    }

    // =========================================================
    // SETUP CURRENT STEP
    // =========================================================

    private void SetupCurrentStep()
    {
        if (components == null ||
            components.Length == 0)
        {
            Debug.LogError(
                "No Assembly Components assigned!"
            );

            return;
        }

        if (currentStep >= components.Length)
        {
            CompleteAssembly();
            return;
        }

        AssemblyGroup currentGroup =
            components[currentStep];

        Debug.Log(
            "CURRENT ASSEMBLY STEP: " +
            (currentStep + 1) +
            " / " +
            components.Length +
            " | TARGET: " +
            currentGroup.componentName
        );

        DisableAllComponents();

        if (currentGroup.component != null)
        {
            currentGroup.component.Setup(this);
            currentGroup.component.SetInteractable(true);
        }

        UpdateInstruction(
            "STEP " +
            (currentStep + 1) +
            "\n\nInstall the " +
            currentGroup.componentName
        );
    }

    // =========================================================
    // COMPONENT CLICKED
    // =========================================================

    public void ComponentClicked(
        AssemblyComponent component)
    {
        if (component == null)
            return;

        if (currentStep >= components.Length)
            return;

        AssemblyGroup currentGroup =
            components[currentStep];

        Debug.Log(
            "CHECKING ASSEMBLY COMPONENT | " +
            "Clicked: [" +
            component.componentName +
            "] | Expected: [" +
            currentGroup.componentName +
            "]"
        );

        if (string.Equals(
            component.componentName.Trim(),
            currentGroup.componentName.Trim(),
            System.StringComparison.OrdinalIgnoreCase))
        {
            Debug.Log(
                "CORRECT ASSEMBLY COMPONENT: " +
                currentGroup.componentName
            );

            CompleteCurrentStep(component);
        }
        else
        {
            Debug.Log(
                "WRONG COMPONENT! Expected: " +
                currentGroup.componentName
            );
        }
    }

    // =========================================================
    // COMPLETE CURRENT STEP
    // =========================================================

    private void CompleteCurrentStep(
        AssemblyComponent component)
    {
        AssemblyGroup completedGroup =
            components[currentStep];

        Debug.Log(
            "ASSEMBLY STEP " +
            (currentStep + 1) +
            " COMPLETE: " +
            completedGroup.componentName
        );

        component.SetInteractable(false);

        component.Assemble();

        currentStep++;

        if (currentStep >= components.Length)
        {
            CompleteAssembly();
            return;
        }

        SetupCurrentStep();
    }

    // =========================================================
    // UPDATE UI
    // =========================================================

    private void UpdateInstruction(
        string message)
    {
        if (instructionText != null)
        {
            instructionText.text =
                message;
        }

        if (progressText != null)
        {
            progressText.text =
                "Step " +
                (currentStep + 1) +
                " / " +
                components.Length;
        }
    }

    // =========================================================
    // COMPLETE
    // =========================================================

    private void CompleteAssembly()
    {
        if (instructionText != null)
        {
            instructionText.text =
                "ASSEMBLY COMPLETE!";
        }

        if (progressText != null)
        {
            progressText.text =
                "Complete!";
        }

        Debug.Log(
            "=== ALL ASSEMBLY STEPS COMPLETE ==="
        );
    }
}

