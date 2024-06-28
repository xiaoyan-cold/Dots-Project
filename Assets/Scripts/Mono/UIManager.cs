using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private int score = 0;
    public Text scoreText;

    void Update()
    {
        if (score != SharedData.gameSharedData.Data.deadCounter)
        {
            score = SharedData.gameSharedData.Data.deadCounter;
            scoreText.text = score.ToString();
        }
    }
}