using UnityEngine;

[CreateAssetMenu(fileName = "SnakeGameConfig", menuName = "Snake/SnakeGameConfig")]
public class SnakeGameConfig : ScriptableObject
{
    [Header("蛇身参数")]
    public int initialLength = 5;
    public float minDistance = 0.2f;
    public int growCount = 5;
    public int circleSegment = 12;
    public float radius = 0.5f;

    [Header("食物参数")]
    public int maxFoodCount = 5;
    public float foodRadius = 0.5f;

    [Header("场地参数")]
    public float wallRadius = 10f;
    public float wallThickness = 1f;
    public float wallHeight = 2f;
    public int segmentCount = 100;
    public float groundThickness = 0.2f;

    [Header("控制参数")]
    public float moveSpeed = 5f;
    public float turnSpeed = 120f;

    [Header("摄像机参数")]
    public Vector3 cameraOffset = new Vector3(0, 8, -8);
    public float cameraFollowSpeed = 5f;

    [Header("碰撞检测参数")]
    public float selfCollisionThreshold = 0.4f;
    public int ignoreHeadPoints = 10;

    [Header("跳跃参数")]
    public float jumpHeight = 2f;
    public float jumpDuration = 0.6f;
    public float jumpYThreshold = 0.3f;

    [Header("蛇基准高度")]
    public float snakeBaseY = 0f;
} 