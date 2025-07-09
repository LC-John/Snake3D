using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // 蛇头
    public SnakeGameConfig config;

    void LateUpdate()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver)
            return;
        Vector3 offset = config != null ? config.cameraOffset : new Vector3(0, 8, -8);
        float followSpeed = config != null ? config.cameraFollowSpeed : 5f;
        Vector3 desiredPosition = target.position + target.rotation * offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
        Vector3 lookDir = target.position + target.forward * 10f - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(lookDir, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, followSpeed * Time.deltaTime);
    }
} 