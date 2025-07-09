using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SnakeBodyTubeMesh : MonoBehaviour
{
    public Transform snakeHead;
    public SnakeGameConfig config;
    public SnakeController snakeController; // 需要在Inspector中拖拽赋值

    private List<Vector3> positions = new List<Vector3>();
    private Mesh mesh;
    private int currentLength = 1;

    void Start()
    {
        int initialLength = config != null ? config.initialLength : 5;
        float minDistance = config != null ? config.minDistance : 0.2f;
        int circleSegment = config != null ? config.circleSegment : 12;
        float radius = config != null ? config.radius : 0.5f;
        float baseY = config != null ? config.snakeBaseY : 0f;
        currentLength = initialLength;
        positions.Add(new Vector3(snakeHead.position.x, baseY, snakeHead.position.z));
        Vector3 dir = -snakeHead.forward;
        for (int i = 1; i < initialLength; i++)
        {
            Vector3 pos = snakeHead.position + dir * i * minDistance;
            positions.Add(new Vector3(pos.x, baseY, pos.z));
        }
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    void Update()
    {
        float minDistance = config != null ? config.minDistance : 0.2f;
        float baseY = config != null ? config.snakeBaseY : 0f;
        // 只用XZ平面距离判断是否插入新采样点
        float xzDist = Vector2.Distance(
            new Vector2(snakeHead.position.x, snakeHead.position.z),
            new Vector2(positions[0].x, positions[0].z)
        );
        if (xzDist > minDistance)
        {
            positions.Insert(0, new Vector3(snakeHead.position.x, snakeHead.position.y, snakeHead.position.z));
        }
        while (positions.Count > currentLength)
        {
            positions.RemoveAt(positions.Count - 1);
        }
        GenerateTubeMesh();
    }

    // Catmull-Rom插值函数
    Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        return 0.5f * (
            2f * p1 +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t
        );
    }

    void GenerateTubeMesh()
    {
        int circleSegment = config != null ? config.circleSegment : 12;
        float radius = config != null ? config.radius : 0.5f;
        if (positions.Count < 2) return;

        // Catmull-Rom平滑采样点
        int smoothCount = positions.Count * 4;
        List<Vector3> smoothPositions = new List<Vector3>();
        for (int i = 0; i < smoothCount; i++)
        {
            float t = (float)i / (smoothCount - 1) * (positions.Count - 1);
            int idx = Mathf.FloorToInt(t);
            float localT = t - idx;
            int i0 = Mathf.Clamp(idx - 1, 0, positions.Count - 1);
            int i1 = Mathf.Clamp(idx, 0, positions.Count - 1);
            int i2 = Mathf.Clamp(idx + 1, 0, positions.Count - 1);
            int i3 = Mathf.Clamp(idx + 2, 0, positions.Count - 1);
            smoothPositions.Add(CatmullRom(positions[i0], positions[i1], positions[i2], positions[i3], localT));
        }

        int vertexCount = smoothPositions.Count * circleSegment;
        int triangleCount = (smoothPositions.Count - 1) * circleSegment * 2 * 3;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[triangleCount];
        Vector2[] uvs = new Vector2[vertexCount];
        for (int i = 0; i < smoothPositions.Count; i++)
        {
            Vector3 forward = (i == 0) ? (smoothPositions[0] - smoothPositions[1]).normalized : (smoothPositions[i - 1] - smoothPositions[i]).normalized;
            Vector3 up = Vector3.up;
            if (Vector3.Dot(forward, up) > 0.99f) up = Vector3.right;
            Vector3 right = Vector3.Cross(up, forward).normalized;
            up = Vector3.Cross(forward, right).normalized;
            for (int j = 0; j < circleSegment; j++)
            {
                float angle = 2 * Mathf.PI * j / circleSegment;
                Vector3 offset = right * Mathf.Cos(angle) * radius + up * Mathf.Sin(angle) * radius;
                vertices[i * circleSegment + j] = smoothPositions[i] + offset;
                uvs[i * circleSegment + j] = new Vector2((float)j / circleSegment, (float)i / smoothPositions.Count);
            }
        }
        int tri = 0;
        for (int i = 0; i < smoothPositions.Count - 1; i++)
        {
            for (int j = 0; j < circleSegment; j++)
            {
                int current = i * circleSegment + j;
                int next = i * circleSegment + (j + 1) % circleSegment;
                int currentNextRow = (i + 1) * circleSegment + j;
                int nextNextRow = (i + 1) * circleSegment + (j + 1) % circleSegment;
                triangles[tri++] = current;
                triangles[tri++] = next;
                triangles[tri++] = currentNextRow;
                triangles[tri++] = next;
                triangles[tri++] = nextNextRow;
                triangles[tri++] = currentNextRow;
            }
        }
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
    }

    public List<Vector3> GetPositions()
    {
        return positions;
    }

    public void Grow(int count)
    {
        currentLength += count;
        if (positions.Count == 0) return;
        Vector3 last = positions[positions.Count - 1];
        for (int i = 0; i < count; i++)
        {
            positions.Add(last);
        }
    }
} 