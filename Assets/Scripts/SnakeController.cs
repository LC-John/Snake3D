using UnityEngine;
using System.Collections.Generic;

public class SnakeController : MonoBehaviour
{
    public Transform snakeHead;
    public SnakeGameConfig config;

    // 跳跃相关
    public bool isJumping = false;
    private float jumpTime = 0f;
    // 移除headYHistory相关内容
    // 保留跳跃和移动逻辑
    void Start()
    {
        if (snakeHead != null && config != null)
        {
            snakeHead.position = new Vector3(snakeHead.position.x, config.snakeBaseY, snakeHead.position.z);
            // 设置蛇头颜色为条纹第一个颜色
            var renderer = snakeHead.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = config.stripeColorA;
            }
        }
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver)
            return;
        float moveSpeed = config != null ? config.moveSpeed : 5f;
        float turnSpeed = config != null ? config.turnSpeed : 120f;
        float jumpHeight = config != null ? config.jumpHeight : 2f;
        float jumpDuration = config != null ? config.jumpDuration : 0.6f;
        float baseY = config != null ? config.snakeBaseY : 0f;

        // 跳跃触发
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
        {
            isJumping = true;
            jumpTime = 0f;
        }

        float y = baseY;
        if (isJumping)
        {
            jumpTime += Time.deltaTime;
            float t = Mathf.Clamp01(jumpTime / jumpDuration);
            y = baseY + jumpHeight * 4 * t * (1 - t);
            if (jumpTime >= jumpDuration)
            {
                isJumping = false;
                jumpTime = 0f;
            }
        }
        snakeHead.position = new Vector3(snakeHead.position.x, y, snakeHead.position.z);

        snakeHead.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        float h = Input.GetAxis("Horizontal");
        snakeHead.Rotate(Vector3.up * h * turnSpeed * Time.deltaTime);
    }
} 