using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

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

    [Header("Wrong Click Feedback")]
    public GameObject wrongClickFeedback;
    public TMP_Text wrongClickFeedbackText;
    public float feedbackDuration = 2f;

    [Header("Results Panel")]
    public GameObject resultsPanel;
    public TMP_Text resultsTitle;
    public TMP_Text starText;
    public TMP_Text performanceText;
    public TMP_Text finalScoreText;
    public TMP_Text finalXPText;
    public TMP_Text finalCorrectText;
    public TMP_Text finalMistakesText;
    public TMP_Text finalAccuracyText;
    public Button retryButton;

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
    private int correctClicks = 0;
    private int mistakeClicks = 0;

    private bool disassemblyComplete = false;

    private bool psuCablesComplete = false;
    private bool ssd1CableComplete = false;
    private bool ssd2CableComplete = false;

    private HashSet<DisassemblyCable> disconnectedCables =
        new HashSet<DisassemblyCable>();

    private Coroutine feedbackCoroutine;

    private void Start()
    {
        if (resultsPanel != null)
            resultsPanel.SetActive(false);

        HideWrongFeedback();

        DisableAllCables();

        SetupCableManagers();

        SetupCurrentStep();
    }

    private void SetupCableManagers()
    {
        if (psuCables != null)
        {
            foreach (DisassemblyCable cable in psuCables)
            {
                if (cable != null)
                    cable.Setup(this);
            }
        }

        if (ssd1Cable != null)
            ssd1Cable.Setup(this);

        if (ssd2Cable != null)
            ssd2Cable.Setup(this);
    }

    private void SetupCurrentStep()
    {
        if (disassemblyComplete)
            return;

        HideWrongFeedback();

        DisableAllCables();

        if (currentStep == 0 && !psuCablesComplete)
        {
            EnablePSUCables();
            EnableActiveComponents();

            SetInstruction(
                "STEP 1\n\nDisconnect PSU cables"
            );

            UpdateProgress();

            return;
        }

        if (currentStep == 0 && psuCablesComplete)
        {
            EnableActiveComponents();

            SetInstruction(
                "STEP 2\n\nRemove Power Supply"
            );

            UpdateProgress();

            return;
        }

        if (currentStep == 6 && !ssd1CableComplete)
        {
            if (ssd1Cable != null)
                ssd1Cable.SetInteractable(true);

            EnableActiveComponents();

            SetInstruction(
                "STEP 8\n\nDisconnect SSD 1 cable"
            );

            UpdateProgress();

            return;
        }

        if (currentStep == 6 && ssd1CableComplete)
        {
            EnableActiveComponents();

            SetInstruction(
                "STEP 9\n\nRemove SSD 1"
            );

            UpdateProgress();

            return;
        }

        if (currentStep == 7 && !ssd2CableComplete)
        {
            if (ssd2Cable != null)
                ssd2Cable.SetInteractable(true);

            EnableActiveComponents();

            SetInstruction(
                "STEP 10\n\nDisconnect SSD 2 cable"
            );

            UpdateProgress();

            return;
        }

        if (currentStep == 7 && ssd2CableComplete)
        {
            EnableActiveComponents();

            SetInstruction(
                "STEP 11\n\nRemove SSD 2"
            );

            UpdateProgress();

            return;
        }

        EnableActiveComponents();

        if (components != null &&
            currentStep < components.Length)
        {
            SetInstruction(
                "STEP " +
                GetDisplayStepNumber() +
                "\n\nRemove " +
                components[currentStep].componentName
            );
        }

        UpdateProgress();
    }

    private void EnableActiveComponents()
    {
        if (components == null)
            return;

        for (int i = 0; i < components.Length; i++)
        {
            ComponentGroup group = components[i];

            if (group == null || group.parts == null)
                continue;

            foreach (DisassemblyComponent part in group.parts)
            {
                if (part == null)
                    continue;

                bool active =
                    IsComponentStillActive(part);

                part.SetInteractable(active);
            }
        }
    }

    private bool IsComponentStillActive(
        DisassemblyComponent component)
    {
        if (component == null)
            return false;

        if (!component.IsVisibleForInteraction())
        {
            Debug.Log(
                "ACTIVE CHECK FAILED: Not visible | " +
                component.gameObject.name
            );
            return false;
        }

        if (components == null)
        {
            Debug.Log(
                "ACTIVE CHECK FAILED: components is null"
            );
            return false;
        }

        for (int i = 0; i < components.Length; i++)
        {
            ComponentGroup group = components[i];

            if (group == null || group.parts == null)
                continue;

            foreach (DisassemblyComponent part in group.parts)
            {
                if (part == null)
                    continue;

                Debug.Log(
                    "COMPARE | Clicked=" +
                    component.gameObject.name +
                    " | Array=" +
                    part.gameObject.name +
                    " | SameReference=" +
                    (part == component) +
                    " | Group=" + i
                );

                if (part == component)
                {
                    bool allowed = i >= currentStep;

                    Debug.Log(
                        "MATCH FOUND | Group=" + i +
                        " | CurrentStep=" + currentStep +
                        " | Allowed=" + allowed
                    );

                    return allowed;
                }
            }
        }

        Debug.Log(
            "ACTIVE CHECK FAILED: Component was not found in any group | " +
            component.gameObject.name
        );

        return false;
    }

    public void ComponentClicked(
     DisassemblyComponent component)
    {
        if (disassemblyComplete)
            return;

        if (component == null)
            return;

        Debug.Log(
            "CLICK DEBUG | " +
            component.gameObject.name +
            " | CurrentStep=" +
            currentStep
        );

        Debug.Log("CHECKING ACTIVE: " + component.gameObject.name);

        if (!IsComponentStillActive(component))
        {
            Debug.LogWarning(
                "BLOCKED! baseMB_13 is NOT considered active. " +
                "CurrentStep=" + currentStep
            );
            return;
        }

        Debug.Log("ACTIVE CHECK PASSED: " + component.gameObject.name);

        if (currentStep == 0 &&
            !psuCablesComplete)
        {
            RegisterMistake(
                "❌ Incorrect!\n\n" +
                "💡 Hint: Please disconnect the PSU cables first."
            );

            return;
        }

        if (currentStep == 6 &&
            !ssd1CableComplete)
        {
            RegisterMistake(
                "❌ Incorrect!\n\n" +
                "💡 Hint: Please disconnect the SSD 1 cable first."
            );

            return;
        }

        if (currentStep == 7 &&
            !ssd2CableComplete)
        {
            RegisterMistake(
                "❌ Incorrect!\n\n" +
                "💡 Hint: Please disconnect the SSD 2 cable first."
            );

            return;
        }

        if (components == null ||
            currentStep >= components.Length)
            return;

        ComponentGroup currentGroup =
            components[currentStep];

        if (currentGroup == null)
            return;

        bool correctComponent = false;

        if (currentGroup.parts != null)
        {
            foreach (DisassemblyComponent part
                     in currentGroup.parts)
            {
                if (part == component)
                {
                    correctComponent = true;
                    break;
                }
            }
        }

        if (correctComponent)
        {
            correctClicks++;

            HideWrongFeedback();

            CompleteCurrentStep();
        }
        else
        {
            RegisterMistake(
                "❌ Incorrect!\n\n" +
                "💡 Hint: Please remove " +
                currentGroup.componentName +
                " first."
            );
        }
    }

    public void CableClicked(
        DisassemblyCable cable)
    {
        if (disassemblyComplete)
            return;

        if (cable == null)
            return;

        if (disconnectedCables.Contains(cable))
            return;

        if (currentStep == 0 &&
            !psuCablesComplete)
        {
            bool correctCable = false;

            if (psuCables != null)
            {
                foreach (DisassemblyCable psuCable
                         in psuCables)
                {
                    if (psuCable == cable)
                    {
                        correctCable = true;
                        break;
                    }
                }
            }

            if (!correctCable)
            {
                RegisterMistake(
                    "❌ Incorrect!\n\n" +
                    "💡 Hint: Please disconnect the PSU cables first."
                );

                return;
            }

            correctClicks++;

            HideWrongFeedback();

            disconnectedCables.Add(cable);

            cable.SetInteractable(false);
            cable.Detach();

            if (AreAllPSUCablesDisconnected())
            {
                psuCablesComplete = true;

                EnableActiveComponents();

                SetInstruction(
                    "STEP 2\n\nRemove Power Supply"
                );

                UpdateProgress();
            }
            else
            {
                SetInstruction(
                    "STEP 1\n\nDisconnect PSU cables (" +
                    disconnectedCables.Count +
                    "/" +
                    psuCables.Length +
                    ")"
                );

                UpdateProgress();
            }

            return;
        }

        if (currentStep == 6 &&
            !ssd1CableComplete)
        {
            if (cable != ssd1Cable)
            {
                RegisterMistake(
                    "❌ Incorrect!\n\n" +
                    "💡 Hint: Please disconnect the SSD 1 cable first."
                );

                return;
            }

            correctClicks++;

            HideWrongFeedback();

            ssd1CableComplete = true;

            disconnectedCables.Add(cable);

            cable.SetInteractable(false);
            cable.Detach();

            EnableActiveComponents();

            SetInstruction(
                "STEP 9\n\nRemove SSD 1"
            );

            UpdateProgress();

            return;
        }

        if (currentStep == 7 &&
            !ssd2CableComplete)
        {
            if (cable != ssd2Cable)
            {
                RegisterMistake(
                    "❌ Incorrect!\n\n" +
                    "💡 Hint: Please disconnect the SSD 2 cable first."
                );

                return;
            }

            correctClicks++;

            HideWrongFeedback();

            ssd2CableComplete = true;

            disconnectedCables.Add(cable);

            cable.SetInteractable(false);
            cable.Detach();

            EnableActiveComponents();

            SetInstruction(
                "STEP 11\n\nRemove SSD 2"
            );

            UpdateProgress();

            return;
        }
    }

    private void CompleteCurrentStep()
    {
        if (components == null)
            return;

        if (currentStep >= components.Length)
        {
            CompleteDisassembly();
            return;
        }

        ComponentGroup group =
            components[currentStep];

        if (group == null)
            return;

        if (group.parts != null)
        {
            foreach (DisassemblyComponent part in group.parts)
            {
                if (part == null)
                    continue;

                part.RemoveComponent();
            }
        }

        // 👇 DITO ilagay ang inventory code
        if (inventory != null)
        {
            Debug.Log(
                "CALLING INVENTORY: " +
                group.componentName
            );

            inventory.MarkRemoved(
                group.componentName
            );
        }
        else
        {
            Debug.LogError(
                "INVENTORY IS NULL!"
            );
        }

        currentStep++;

        if (currentStep >= components.Length)
        {
            CompleteDisassembly();
            return;
        }

        SetupCurrentStep();
    }

    private void EnablePSUCables()
    {
        DisableAllCables();

        if (psuCables == null)
            return;

        foreach (DisassemblyCable cable in psuCables)
        {
            if (cable == null)
                continue;

            if (!disconnectedCables.Contains(cable))
                cable.SetInteractable(true);
        }
    }

    private void DisableAllCables()
    {
        if (psuCables != null)
        {
            foreach (DisassemblyCable cable
                     in psuCables)
            {
                if (cable != null)
                    cable.SetInteractable(false);
            }
        }

        if (ssd1Cable != null)
            ssd1Cable.SetInteractable(false);

        if (ssd2Cable != null)
            ssd2Cable.SetInteractable(false);
    }

    private bool AreAllPSUCablesDisconnected()
    {
        if (psuCables == null ||
            psuCables.Length == 0)
        {
            return true;
        }

        foreach (DisassemblyCable cable
                 in psuCables)
        {
            if (cable == null)
                continue;

            if (!disconnectedCables.Contains(cable))
                return false;
        }

        return true;
    }

    private void RegisterMistake(string message)
    {
        mistakeClicks++;

        ShowWrongFeedback(message);
    }

    private void ShowWrongFeedback(string message)
    {
        if (wrongClickFeedbackText != null)
            wrongClickFeedbackText.text = message;

        if (wrongClickFeedback != null)
            wrongClickFeedback.SetActive(true);

        if (feedbackCoroutine != null)
            StopCoroutine(feedbackCoroutine);

        feedbackCoroutine =
            StartCoroutine(
                HideFeedbackAfterDelay()
            );
    }

    private IEnumerator HideFeedbackAfterDelay()
    {
        yield return new WaitForSeconds(
            feedbackDuration
        );

        HideWrongFeedback();

        feedbackCoroutine = null;
    }

    private void HideWrongFeedback()
    {
        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);

            feedbackCoroutine = null;
        }

        if (wrongClickFeedback != null)
            wrongClickFeedback.SetActive(false);
    }

    private void SetInstruction(string message)
    {
        if (instructionText != null)
            instructionText.text = message;
    }

    private void UpdateProgress()
    {
        if (progressText == null)
            return;

        int totalComponents =
            components != null
                ? components.Length
                : 0;

        int completedComponents =
            Mathf.Clamp(
                currentStep,
                0,
                totalComponents
            );

        progressText.text =
            "Progress: " +
            completedComponents +
            "/" +
            totalComponents;
    }

    private int GetDisplayStepNumber()
    {
        /*
         * 0 PSU         = Step 2
         * 1 GPU         = Step 3
         * 2 RAM         = Step 4
         * 3 Fan         = Step 5
         * 4 Cooler      = Step 6
         * 5 CPU         = Step 7
         * 6 SSD1        = Step 9
         * 7 SSD2        = Step 11
         * 8 Motherboard = Step 12
         */

        if (currentStep <= 0)
            return 2;

        if (currentStep >= 6)
            return 9 + ((currentStep - 6) * 2);

        return currentStep + 2;
    }

    private void CompleteDisassembly()
    {
        disassemblyComplete = true;

        DisableAllCables();

        if (components != null)
        {
            foreach (ComponentGroup group
                     in components)
            {
                if (group == null ||
                    group.parts == null)
                    continue;

                foreach (DisassemblyComponent part
                         in group.parts)
                {
                    if (part != null)
                        part.SetInteractable(false);
                }
            }
        }

        HideWrongFeedback();
        // Hide instruction and progress text
        if (instructionText != null)
            instructionText.gameObject.SetActive(false);

        if (progressText != null)
            progressText.gameObject.SetActive(false);

        ShowResultsPanel();
    }

    private void ShowResultsPanel()
    {
        if (resultsPanel != null)
            resultsPanel.SetActive(true);

        int totalInteractions =
            correctClicks + mistakeClicks;

        float accuracy =
            totalInteractions > 0
                ? ((float)correctClicks /
                   totalInteractions) * 100f
                : 0f;

        int score =
            Mathf.RoundToInt(accuracy);

        int xp =
            Mathf.RoundToInt(score * 1.5f);

        int stars;

        if (accuracy >= 90f)
            stars = 3;
        else if (accuracy >= 70f)
            stars = 2;
        else
            stars = 1;

        if (resultsTitle != null)
            resultsTitle.text =
                "DISASSEMBLY COMPLETE";

        if (starText != null)
            starText.text =
                "Stars: " + stars + "/3";

        if (performanceText != null)
        {
            if (accuracy >= 90f)
            {
                performanceText.text =
                    "Excellent! Great job!";
            }
            else if (accuracy >= 70f)
            {
                performanceText.text =
                    "Good job! Keep practicing!";
            }
            else
            {
                performanceText.text =
                    "Keep practicing!";
            }
        }

        if (finalScoreText != null)
            finalScoreText.text =
                "Score: " + score;

        if (finalXPText != null)
            finalXPText.text =
                "XP: " + xp;

        if (finalCorrectText != null)
            finalCorrectText.text =
                "Correct Interactions: " +
                correctClicks;

        if (finalMistakesText != null)
            finalMistakesText.text =
                "Wrong Clicks: " +
                mistakeClicks;

        if (finalAccuracyText != null)
        {
            finalAccuracyText.text =
                "Accuracy: " +
                accuracy.ToString("F1") +
                "%";
        }

        if (retryButton != null)
        {
            retryButton.onClick.RemoveAllListeners();

            retryButton.onClick.AddListener(
                RetryLevel
            );
        }
    }

    private void RetryLevel()
    {
        UnityEngine.SceneManagement.Scene currentScene =
            UnityEngine.SceneManagement.SceneManager
                .GetActiveScene();

        UnityEngine.SceneManagement.SceneManager
            .LoadScene(currentScene.name);
    }

}
