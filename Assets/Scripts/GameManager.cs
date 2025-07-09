using UnityEngine;
using UnityEngine.SceneManagement;

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
        if (gameOverUI != null)
            gameOverUI.SetActive(true);
        Time.timeScale = 0f;
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