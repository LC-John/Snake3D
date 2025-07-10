using UnityEngine;
using System.Collections.Generic;

public class FoodManager : MonoBehaviour
{
    public GameObject foodPrefab;
    public SnakeBodyTubeMesh snakeBodyTubeMesh;
    public SnakeGameConfig config;

    private List<GameObject> foods = new List<GameObject>();

    void Start()
    {
        // 参数全部从config读取
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver)
            return;
        int maxFoodCount = config != null ? config.maxFoodCount : 5;
        float foodRadius = config != null ? config.foodRadius : 0.5f;
        float wallRadius = config != null ? config.wallRadius : 10f;
        float wallThickness = config != null ? config.wallThickness : 1f;
        while (foods.Count < maxFoodCount)
        {
            Vector3 pos;
            if (TryGetValidPosition(out pos, foodRadius, wallRadius, wallThickness))
            {
                GameObject food = Instantiate(foodPrefab, pos, Quaternion.identity, transform);
                foods.Add(food);
            }
            else
            {
                break;
            }
        }
        foods.RemoveAll(f => f == null);
    }

    bool TryGetValidPosition(out Vector3 pos, float foodRadius, float wallRadius, float wallThickness)
    {
        int tryCount = 30;
        float minR = 0.5f + foodRadius;
        float maxR = wallRadius - wallThickness / 2f - foodRadius;
        for (int i = 0; i < tryCount; i++)
        {
            float r = Random.Range(minR, maxR);
            float angle = Random.Range(0, Mathf.PI * 2);
            Vector3 candidate = new Vector3(Mathf.Cos(angle) * r, 0.5f, Mathf.Sin(angle) * r);
            bool overlap = false;
            foreach (var f in foods)
            {
                if (f != null && Vector3.Distance(candidate, f.transform.position) < foodRadius * 2f)
                {
                    overlap = true;
                    break;
                }
            }
            if (overlap) continue;
            if (snakeBodyTubeMesh != null)
            {
                var positions = snakeBodyTubeMesh.GetPositions();
                foreach (var p in positions)
                {
                    if (Vector3.Distance(candidate, p) < foodRadius * 2f)
                    {
                        overlap = true;
                        break;
                    }
                }
            }
            if (overlap) continue;
            pos = candidate;
            return true;
        }
        pos = Vector3.zero;
        return false;
    }

    public void RemoveFood(GameObject food)
    {
        if (foods.Contains(food))
            food.GetComponent<Food>().Eat();
    }
} 