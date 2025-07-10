using UnityEngine;

public class Food : MonoBehaviour
{
    public SnakeGameConfig config;
    public GameObject fragmentEffectPrefab; // 拖入碎片特效

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

    public void Eat()
    {
        // 1. 隐藏食物本体
        gameObject.SetActive(false);

        // 2. 实例化碎片特效
        if (fragmentEffectPrefab != null)
        {
            GameObject fx = Instantiate(fragmentEffectPrefab, transform.position, Quaternion.identity);
            Destroy(fx, 1.5f); // 1.5秒后自动销毁特效
        }

        // 3. 延迟销毁食物对象
        Destroy(gameObject, 1.0f); // 1秒后销毁
    }
} 