using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static bool restartDirectly = false;
    public bool isGameOver = false;
    public bool isGameStarted = false;
    public GameObject gameOverUI;
    public GameObject mainMenuUI;
    public Camera mainCamera;
    public Transform snakeHead;
    public CameraFollow cameraFollow;
    public GameObject scoreUI;
    public GameObject miniMapUI;
    public GameObject pauseUI;
    public GameObject helpUI;
    public GameObject explosionPrefab; // 在Inspector拖入爆炸特效Prefab
    public SnakeGameConfig config; // 在Inspector拖入SnakeGameConfig
    private bool isPaused = false;

    void Awake()
    {
        Instance = this;
        if (gameOverUI != null)
            gameOverUI.SetActive(false);
        if (mainMenuUI != null)
            mainMenuUI.SetActive(!restartDirectly);
        if (cameraFollow != null)
            cameraFollow.enabled = false;
        if (scoreUI != null) scoreUI.SetActive(false);
        if (miniMapUI != null) miniMapUI.SetActive(false);
        if (pauseUI != null) pauseUI.SetActive(false);
        if (helpUI != null) helpUI.SetActive(false);
        isGameStarted = false;
        isGameOver = false;
        isPaused = false;
        if (restartDirectly)
        {
            restartDirectly = false;
            if (scoreUI != null) scoreUI.SetActive(true);
            if (miniMapUI != null) miniMapUI.SetActive(true);
            StartCoroutine(MoveCameraToSnakeHead());
        }
        else
        {
            Time.timeScale = 0f;
        }
    }

    void Update()
    {
        if (isGameStarted && !isGameOver)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!isPaused)
                    PauseGame();
                else
                    ResumeGame();
            }
            // 检查自撞
            CheckSelfCollision();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        if (pauseUI != null) pauseUI.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;
        if (pauseUI != null) pauseUI.SetActive(false);
        Time.timeScale = 1f;
    }

    public void StartGame()
    {
        isGameStarted = true;
        if (mainMenuUI != null)
            mainMenuUI.SetActive(false);
        if (scoreUI != null) scoreUI.SetActive(true);
        if (miniMapUI != null) miniMapUI.SetActive(true);
        StartCoroutine(MoveCameraToSnakeHead());
    }

    System.Collections.IEnumerator MoveCameraToSnakeHead()
    {
        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;
        Vector3 targetPos = snakeHead.position + snakeHead.rotation * (cameraFollow != null ? cameraFollow.config.cameraOffset : new Vector3(0,8,-8));
        Quaternion targetRot = Quaternion.LookRotation(snakeHead.position + snakeHead.forward * 10f - targetPos, Vector3.up);
        float t = 0;
        float duration = 1.2f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / duration;
            mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }
        if (cameraFollow != null)
            cameraFollow.enabled = true;
        Time.timeScale = 1f;
    }

    public void GameOver()
    {
        isGameOver = true;
        StartCoroutine(ExplodeSnakeSequence()); // 死亡时推进爆炸
    }

    public void RestartGame()
    {
        restartDirectly = true;
        if (scoreUI != null) scoreUI.SetActive(true);
        if (miniMapUI != null) miniMapUI.SetActive(true);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMenu()
    {
        if (scoreUI != null) scoreUI.SetActive(false);
        if (miniMapUI != null) miniMapUI.SetActive(false);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ShowHelp()
    {
        if (mainMenuUI != null) mainMenuUI.SetActive(false);
        if (helpUI != null) helpUI.SetActive(true);
    }

    public void HideHelp()
    {
        if (helpUI != null) helpUI.SetActive(false);
        if (mainMenuUI != null) mainMenuUI.SetActive(true);
    }

    public void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public IEnumerator ExplodeSnakeSequence()
    {
        if (explosionPrefab == null) yield break;
        var snakeBody = FindObjectOfType<SnakeBodyTubeMesh>();
        if (snakeBody == null) yield break;
        float delay = config != null ? config.explosionStepDelay : 0.05f; // 爆炸推进速度
        // 1. 先爆炸蛇头
        if (snakeHead != null)
        {
            GameObject fx = Instantiate(explosionPrefab, snakeHead.position, Quaternion.identity);
            Destroy(fx, 2f);
            snakeHead.gameObject.SetActive(false); // 或 Destroy(snakeHead.gameObject);
        }
        yield return new WaitForSeconds(delay); // 等待与身体推进一致
        // 2. 再推进蛇身爆炸
        int step = 3; // 每隔几个点爆一次
        if (cameraFollow != null) cameraFollow.enabled = false;
        Vector3 camPos = mainCamera.transform.position; // 死亡时摄像机位置
        int minRemain = 2; // 至少保留2个点，防止Mesh异常
        while (snakeBody.GetPositions().Count - snakeBody.hiddenHeadCount > minRemain)
        {
            int idx = snakeBody.hiddenHeadCount;
            if (idx >= snakeBody.GetPositions().Count) break;
            // 爆炸特效
            GameObject fx = Instantiate(explosionPrefab, snakeBody.GetPositions()[idx], Quaternion.identity);
            Destroy(fx, 2f);
            // 摄像机只旋转，不移动，且平滑旋转
            if (mainCamera != null)
            {
                mainCamera.transform.position = camPos;
                Vector3 targetPos = snakeBody.GetPositions()[idx];
                Quaternion startRot = mainCamera.transform.rotation;
                Quaternion targetRot = Quaternion.LookRotation(targetPos - camPos, Vector3.up);
                float t = 0f;
                float rotateDuration = delay;
                while (t < 1f)
                {
                    t += Time.deltaTime / rotateDuration;
                    mainCamera.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
                    yield return null;
                }
            }
            // 隐藏已爆炸的蛇身段
            snakeBody.HideHeadSegments(step);
            // yield return new WaitForSeconds(delay); // 已用旋转时间代替
        }
        // 全部爆炸后弹出GameOver
        if (gameOverUI != null)
            gameOverUI.SetActive(true);
        // 全部爆炸后彻底清空蛇身
        snakeBody.ClearAll();
        Time.timeScale = 0f;
    }

    // 检查蛇头与身体的碰撞（带Y轴高度判定）
    void CheckSelfCollision()
    {
        var snakeBody = FindObjectOfType<SnakeBodyTubeMesh>();
        var snakeController = FindObjectOfType<SnakeController>();
        if (snakeBody == null || snakeController == null) return;
        var positions = snakeBody.GetPositions();
        var head = snakeController.snakeHead;
        float threshold = snakeController.config != null ? snakeController.config.selfCollisionThreshold : 0.4f;
        float yThreshold = snakeController.config != null ? snakeController.config.jumpYThreshold : 0.3f;
        int ignoreHeadPoints = snakeController.config != null ? snakeController.config.ignoreHeadPoints : 10;
        float baseY = snakeController.config != null ? snakeController.config.snakeBaseY : 0f;
        for (int i = ignoreHeadPoints; i < positions.Count; i++)
        {
            float xzDist = Vector2.Distance(new Vector2(head.position.x, head.position.z), new Vector2(positions[i].x, positions[i].z));
            float yDist = Mathf.Abs((head.position.y - baseY) - (positions[i].y - baseY));
            if (xzDist < threshold && yDist < yThreshold)
            {
                GameOver();
                break;
            }
        }
    }
} 