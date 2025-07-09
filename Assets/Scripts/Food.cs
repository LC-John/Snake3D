using UnityEngine;

public class Food : MonoBehaviour
{
    public SnakeGameConfig config;

    private void Start()
    {
        // 无需本地参数，全部从config读取
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.name.Contains("SnakeHead"))
        {
            var foodManager = FindObjectOfType<FoodManager>();
            if (foodManager != null)
                foodManager.RemoveFood(gameObject);
            // 增长蛇身
            var snake = FindObjectOfType<SnakeBodyTubeMesh>();
            if (snake != null && config != null)
                snake.Grow(config.growCount);
            // 积分系统
            var scoreManager = FindObjectOfType<ScoreManager>();
            if (scoreManager != null)
                scoreManager.AddFood();
        }
    }
} 