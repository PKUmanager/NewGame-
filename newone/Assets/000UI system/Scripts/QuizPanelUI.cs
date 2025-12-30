using UnityEngine;

public class QuizPanelUI : MonoBehaviour
{
    public GameObject quizPanel;

    public void ClosePanel()
    {
        quizPanel.SetActive(false);
    }
}