using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class DisassemblyManager : MonoBehaviour
{
    [System.Serializable]
    public class ComponentGroup
    {
        public string componentName;
        public DisassemblyComponent[] parts;
    }

    [Header("UI")]
    public TMP_Text instructionText;
    public TMP_Text progressText;

    [Header("Inventory")]
    public DisassemblyInventory inventory;

    [Header("Disassembly Components")]
    public ComponentGroup[] components;

    [Header("PSU Cables")]
    public DisassemblyCable[] psuCables;

    [Header("SSD 1 Cable")]
    public DisassemblyCable ssd1Cable;

    [Header("SSD 2 Cable")]
    public DisassemblyCable ssd2Cable;

    private int currentStep = 0;

    private HashSet<DisassemblyCable> disconnectedCables =
        new HashSet<DisassemblyCable>();

    private bool psuCablesComplete = false;
    private bool ssd1CableComplete = false;
    private bool ssd2CableComplete = false;

    private void Start()
    {
        Debug.Log("=== DISASSEMBLY MANAGER STARTED ===");

        DisableAllComponents();
        DisableAllCables();

        SetupCurrentStep();
    }

    // =========================================================
    // DISABLE EVERYTHING
    // =========================================================

    private void DisableAllComponents()
    {
        if (components == null)
        {
            Debug.LogError("Components array is NULL!");
            return;
        }

        foreach (ComponentGroup group in components)
        {
            if (group == null || group.parts == null)
                continue;

            foreach (DisassemblyComponent part in group.parts)
            {
                if (part != null)
                {
                    part.Setup(this);
                    part.SetInteractable(false);
                }
            }
        }
    }

    private void DisableAllCables()
    {
        if (psuCables != null)
        {
            foreach (DisassemblyCable cable in psuCables)
            {
                if (cable != null)
                {
                    cable.Setup(this);
                    cable.SetInteractable(false);
                }
            }
        }

        if (ssd1Cable != null)
        {
            ssd1Cable.Setup(this);
            ssd1Cable.SetInteractable(false);
        }

        if (ssd2Cable != null)
        {
            ssd2Cable.Setup(this);
            ssd2Cable.SetInteractable(false);
        }
    }

    // =========================================================
    // CURRENT STEP
    // =========================================================

    private void SetupCurrentStep()
    {
        if (components == null || components.Length == 0)
        {
            Debug.LogError("No disassembly components assigned!");
            return;
        }

        if (currentStep >= components.Length)
        {
            CompleteDisassembly();
            return;
        }

        ComponentGroup currentGroup =
            components[currentStep];

        Debug.Log(
            "CURRENT STEP: " +
            (currentStep + 1) +
            " / " +
            components.Length +
            " | TARGET: " +
            currentGroup.componentName
        );

        // -----------------------------------------------------
        // STEP 1 - PSU CABLES
        // -----------------------------------------------------

        if (currentStep == 0 && !psuCablesComplete)
        {
            EnablePSUCables();

            int totalPSUCables =
                psuCables != null ? psuCables.Length : 0;

            UpdateInstruction(
                "STEP 1\n\nDisconnect PSU cables\n" +
                "(" +
                disconnectedCables.Count +
                " / " +
                totalPSUCables +
                ")"
            );

            Debug.Log("PSU CABLE STAGE ACTIVE.");

            return;
        }

        // -----------------------------------------------------
        // STEP 7 - SSD 1 CABLE
        // currentStep 6 = SSD 1
        // -----------------------------------------------------

        if (currentStep == 6 && !ssd1CableComplete)
        {
            DisableAllComponents();

            if (ssd1Cable != null)
            {
                ssd1Cable.Setup(this);
                ssd1Cable.SetInteractable(true);
            }

            UpdateInstruction(
                "STEP 7\n\nDisconnect SSD 1 cable"
            );

            Debug.Log("SSD 1 CABLE STAGE ACTIVE.");

            return;
        }

        // -----------------------------------------------------
        // STEP 8 - SSD 2 CABLE
        // currentStep 7 = SSD 2
        // -----------------------------------------------------

        if (currentStep == 7 && !ssd2CableComplete)
        {
            DisableAllComponents();

            if (ssd2Cable != null)
            {
                ssd2Cable.Setup(this);
                ssd2Cable.SetInteractable(true);
            }

            UpdateInstruction(
                "STEP 8\n\nDisconnect SSD 2 cable"
            );

            Debug.Log("SSD 2 CABLE STAGE ACTIVE.");

            return;
        }

        // -----------------------------------------------------
        // NORMAL COMPONENT STEP
        // -----------------------------------------------------

        EnableCurrentComponent();

        UpdateInstruction(
            "STEP " +
            (currentStep + 2) +
            "\n\nRemove the " +
            currentGroup.componentName
        );
    }

    // =========================================================
    // ENABLE PSU CABLES
    // =========================================================

    private void EnablePSUCables()
    {
        if (psuCables == null)
            return;

        foreach (DisassemblyCable cable in psuCables)
        {
            if (cable != null &&
                !disconnectedCables.Contains(cable))
            {
                cable.Setup(this);
                cable.SetInteractable(true);
            }
        }
    }

    // =========================================================
    // ENABLE CURRENT COMPONENT
    // =========================================================

    private void EnableCurrentComponent()
    {
        if (currentStep >= components.Length)
            return;

        ComponentGroup currentGroup =
            components[currentStep];

        foreach (DisassemblyComponent part in currentGroup.parts)
        {
            if (part != null)
            {
                part.Setup(this);
                part.SetInteractable(true);
            }
        }

        Debug.Log(
            "CURRENT COMPONENT IS NOW CLICKABLE: " +
            currentGroup.componentName
        );
    }

    // =========================================================
    // CABLE CLICKED
    // =========================================================

    public void CableClicked(DisassemblyCable cable)
    {
        if (cable == null)
            return;

        Debug.Log(
            "CABLE CLICKED: " +
            cable.gameObject.name +
            " | Group: " +
            cable.cableGroup
        );

        // =====================================================
        // PSU CABLES
        // =====================================================

        if (currentStep == 0)
        {
            if (cable.cableGroup != "PSU")
            {
                Debug.Log(
                    "This cable is not a PSU cable."
                );

                return;
            }

            if (disconnectedCables.Contains(cable))
                return;

            disconnectedCables.Add(cable);

            cable.SetInteractable(false);

            cable.Detach();

            Debug.Log(
                "PSU CABLE DISCONNECTED: " +
                cable.gameObject.name
            );

            int totalPSUCables =
                psuCables != null ? psuCables.Length : 0;

            Debug.Log(
                "PSU CABLE PROGRESS: " +
                disconnectedCables.Count +
                " / " +
                totalPSUCables
            );

            if (disconnectedCables.Count >= totalPSUCables)
            {
                psuCablesComplete = true;

                Debug.Log(
                    "=== ALL PSU CABLES DISCONNECTED ==="
                );

                UpdateInstruction(
                    "STEP 1\n\nRemove the Power Supply"
                );

                EnableCurrentComponent();
            }
            else
            {
                UpdateInstruction(
                    "STEP 1\n\nDisconnect PSU cables\n" +
                    "(" +
                    disconnectedCables.Count +
                    " / " +
                    totalPSUCables +
                    ")"
                );
            }

            return;
        }

        // =====================================================
        // SSD 1 CABLE
        // =====================================================

        if (currentStep == 6)
        {
            if (cable != ssd1Cable)
            {
                Debug.Log(
                    "This is not the SSD 1 cable."
                );

                return;
            }

            if (ssd1CableComplete)
                return;

            ssd1CableComplete = true;

            cable.SetInteractable(false);

            cable.Detach();

            Debug.Log(
                "=== SSD 1 CABLE DISCONNECTED ==="
            );

            EnableCurrentComponent();

            UpdateInstruction(
                "STEP 7\n\nRemove SSD 1"
            );

            return;
        }

        // =====================================================
        // SSD 2 CABLE
        // =====================================================

        if (currentStep == 7)
        {
            if (cable != ssd2Cable)
            {
                Debug.Log(
                    "This is not the SSD 2 cable."
                );

                return;
            }

            if (ssd2CableComplete)
                return;

            ssd2CableComplete = true;

            cable.SetInteractable(false);

            cable.Detach();

            Debug.Log(
                "=== SSD 2 CABLE DISCONNECTED ==="
            );

            EnableCurrentComponent();

            UpdateInstruction(
                "STEP 8\n\nRemove SSD 2"
            );

            return;
        }
    }

    // =========================================================
    // COMPONENT CLICKED
    // =========================================================

    public void ComponentClicked(
        DisassemblyComponent component)
    {
        if (component == null)
            return;

        if (components == null ||
            currentStep >= components.Length)
            return;

        ComponentGroup currentGroup =
            components[currentStep];

        Debug.Log(
            "CHECKING COMPONENT | " +
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
                "CORRECT COMPONENT: " +
                currentGroup.componentName
            );

            CompleteCurrentStep();
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
    // COMPLETE COMPONENT STEP
    // =========================================================

    private void CompleteCurrentStep()
    {
        if (components == null ||
            currentStep >= components.Length)
            return;

        ComponentGroup completedGroup =
            components[currentStep];

        Debug.Log(
            "STEP " +
            (currentStep + 1) +
            " COMPLETE: " +
            completedGroup.componentName
        );

        // -----------------------------------------------------
        // REMOVE COMPONENT
        // -----------------------------------------------------

        foreach (DisassemblyComponent part
            in completedGroup.parts)
        {
            if (part != null)
            {
                part.SetInteractable(false);
                part.RemoveComponent();
            }
        }

        // -----------------------------------------------------
        // UPDATE INVENTORY
        // Only after the actual component is removed.
        // -----------------------------------------------------

        if (inventory != null)
        {
            inventory.MarkRemoved(
                completedGroup.componentName
            );
        }

        // -----------------------------------------------------
        // NEXT STEP
        // -----------------------------------------------------

        currentStep++;

        if (currentStep >= components.Length)
        {
            CompleteDisassembly();
            return;
        }

        SetupCurrentStep();
    }

    // =========================================================
    // UPDATE UI
    // =========================================================

    private void UpdateInstruction(string message)
    {
        if (instructionText != null)
        {
            instructionText.text = message;
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

    private void CompleteDisassembly()
    {
        if (instructionText != null)
        {
            instructionText.text =
                "DISASSEMBLY COMPLETE!";
        }

        if (progressText != null)
        {
            progressText.text =
                "Complete!";
        }

        Debug.Log(
            "=== ALL DISASSEMBLY STEPS COMPLETE ==="
        );
    }
}