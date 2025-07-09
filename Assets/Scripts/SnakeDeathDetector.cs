using UnityEngine;
using System.Collections.Generic;

public class SnakeDeathDetector : MonoBehaviour
{
    public SnakeBodyTubeMesh snakeBodyTubeMesh; // 拖入SnakeBodyTubeMesh
    public SnakeGameConfig config;

    void Update()
    {
        float selfCollisionThreshold = config != null ? config.selfCollisionThreshold : 0.4f;
        int ignoreHeadPoints = config != null ? config.ignoreHeadPoints : 10;
        // 检查是否碰到自己
        if (snakeBodyTubeMesh != null)
        {
            List<Vector3> positions = snakeBodyTubeMesh.GetPositions();
            Vector3 headPos = transform.position;
            for (int i = ignoreHeadPoints; i < positions.Count; i++)
            {
                if (Vector3.Distance(headPos, positions[i]) < selfCollisionThreshold)
                {
                    if (GameManager.Instance != null && !GameManager.Instance.isGameOver)
                        GameManager.Instance.GameOver();
                    break;
                }
            }
        }
        // 检查是否碰到墙体
        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null)
        {
            Vector3 center = transform.TransformPoint(box.center);
            Vector3 halfExtents = Vector3.Scale(box.size, transform.lossyScale) * 0.5f;
            Collider[] hits = Physics.OverlapBox(center, halfExtents, transform.rotation);
            foreach (var hit in hits)
            {
                if (hit.gameObject != gameObject && hit.gameObject.name.Contains("CircleWall"))
                {
                    if (GameManager.Instance != null && !GameManager.Instance.isGameOver)
                        GameManager.Instance.GameOver();
                    break;
                }
            }
        }
    }
} 