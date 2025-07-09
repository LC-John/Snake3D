using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public SnakeBodyTubeMesh snakeBodyTubeMesh;
    private int foodCount = 0;

    public void AddFood()
    {
        foodCount++;
        UpdateScore();
    }

    void Update()
    {
        UpdateScore();
    }

    void UpdateScore()
    {
        int snakeLength = snakeBodyTubeMesh != null ? snakeBodyTubeMesh.GetPositions().Count : 0;
        scoreText.text = $"Food: {foodCount}\nSnake Length: {snakeLength}";
    }
} 