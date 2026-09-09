using UnityEngine;

public class Level2Instructions : MonoBehaviour
{
    public GameObject instructionsPanel;
    public GameObject disassemblyInstruction;
    public GameObject progressText;

    private void Start()
    {
        if (instructionsPanel != null)
            instructionsPanel.SetActive(true);

        if (disassemblyInstruction != null)
            disassemblyInstruction.SetActive(false);

        if (progressText != null)
            progressText.SetActive(false);
    }

    public void StartGame()
    {
        if (instructionsPanel != null)
            instructionsPanel.SetActive(false);

        if (disassemblyInstruction != null)
            disassemblyInstruction.SetActive(true);

        if (progressText != null)
            progressText.SetActive(true);
    }
}