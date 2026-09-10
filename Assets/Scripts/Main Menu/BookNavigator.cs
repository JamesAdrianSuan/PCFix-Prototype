using UnityEngine;

public class BookNavigator : MonoBehaviour
{
    [Header("UI Sections")]
    [Tooltip("Add all your Section GameObjects here in order.")]
    public GameObject[] sections;

    private int currentIndex = 0;

    void Start()
    {
        ShowCurrentSection();
    }

    public void NextSection()
    {
        if (sections.Length == 0) return;

        // Move to next section (loops back to first if at the end)
        currentIndex = (currentIndex + 1) % sections.Length;
        ShowCurrentSection();
    }

    public void PreviousSection()
    {
        if (sections.Length == 0) return;

        // Move to previous section (loops to last if at the start)
        currentIndex--;
        if (currentIndex < 0)
        {
            currentIndex = sections.Length - 1;
        }
        ShowCurrentSection();
    }

    private void ShowCurrentSection()
    {
        for (int i = 0; i < sections.Length; i++)
        {
            if (sections[i] != null)
            {
                sections[i].SetActive(i == currentIndex);
            }
        }
    }
}
