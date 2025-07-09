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
        float radius = config != null ? config.radius : 0.5f;
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
        // 尾巴锥体采样点补充
        int tailExtraPoints = 4;
        float tailLen = radius * 2.5f;
        if (positions.Count > 1)
        {
            Vector3 tailBase = positions[positions.Count - 1];
            Vector3 tailDir = (positions[positions.Count - 1] - positions[positions.Count - 2]).normalized;
            for (int i = 1; i <= tailExtraPoints; i++)
            {
                float t = (float)i / (tailExtraPoints + 1);
                Vector3 tailPos = tailBase + tailDir * tailLen * t;
                positions.Add(tailPos);
            }
        }
        GenerateTubeMesh();
        // 补点后要移除多余尾巴点，防止累积
        if (positions.Count > currentLength)
        {
            positions.RemoveRange(currentLength, positions.Count - currentLength);
        }
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
        Color[] colors = new Color[vertexCount];
        // 从config读取参数
        float stripeWorldLen = config != null ? config.stripeWorldLen : 0.6f;
        Color colorA = config != null ? config.stripeColorA : new Color(0.2f, 0.7f, 0.2f);
        Color colorB = config != null ? config.stripeColorB : new Color(0.9f, 0.8f, 0.3f);
        float accLen = 0f;
        for (int i = 0; i < smoothPositions.Count; i++)
        {
            if (i > 0)
                accLen += Vector3.Distance(smoothPositions[i], smoothPositions[i - 1]);
            int stripeIdx = Mathf.FloorToInt(accLen / stripeWorldLen);
            Color c = (stripeIdx % 2 == 0) ? colorA : colorB;
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
                colors[i * circleSegment + j] = c;
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
        mesh.colors = colors;
        mesh.RecalculateNormals();

        // === 封口和尾巴 ===
        // 1. 头部封口
        AddCap(mesh, smoothPositions[0], (smoothPositions[0] - smoothPositions[1]).normalized, radius, circleSegment, colorA);
        // 2. 尾部封口
        AddCap(mesh, smoothPositions[smoothPositions.Count - 1], (smoothPositions[smoothPositions.Count - 1] - smoothPositions[smoothPositions.Count - 2]).normalized, radius, circleSegment, colorB);
        // 3. 蛇尾巴（锥形）
        AddTail(mesh, smoothPositions[smoothPositions.Count - 1], (smoothPositions[smoothPositions.Count - 1] - smoothPositions[smoothPositions.Count - 2]).normalized, radius, circleSegment, colorB);
    }

    // 新增：封口和尾巴生成函数
    void AddCap(Mesh mesh, Vector3 center, Vector3 forward, float radius, int circleSegment, Color color)
    {
        List<Vector3> verts = new List<Vector3>(mesh.vertices);
        List<int> tris = new List<int>(mesh.triangles);
        List<Color> cols = new List<Color>(mesh.colors);
        int baseIdx = verts.Count;
        verts.Add(center);
        cols.Add(color);
        Vector3 up = Vector3.up;
        if (Mathf.Abs(Vector3.Dot(forward, up)) > 0.99f) up = Vector3.right;
        Vector3 right = Vector3.Cross(up, forward).normalized;
        up = Vector3.Cross(forward, right).normalized;
        for (int j = 0; j < circleSegment; j++)
        {
            float angle = 2 * Mathf.PI * j / circleSegment;
            Vector3 offset = right * Mathf.Cos(angle) * radius + up * Mathf.Sin(angle) * radius;
            verts.Add(center + offset);
            cols.Add(color);
        }
        for (int j = 0; j < circleSegment; j++)
        {
            int v0 = baseIdx;
            int v1 = baseIdx + 1 + j;
            int v2 = baseIdx + 1 + (j + 1) % circleSegment;
            tris.Add(v0);
            tris.Add(v1);
            tris.Add(v2);
        }
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.colors = cols.ToArray();
    }

    void AddTail(Mesh mesh, Vector3 baseCenter, Vector3 forward, float radius, int circleSegment, Color color)
    {
        List<Vector3> verts = new List<Vector3>(mesh.vertices);
        List<int> tris = new List<int>(mesh.triangles);
        List<Color> cols = new List<Color>(mesh.colors);
        int baseIdx = verts.Count;
        // 锥尖（在管道末端方向延伸更长，正方向）
        Vector3 tip = baseCenter + forward * (radius * 2.5f);
        verts.Add(tip);
        cols.Add(color);
        // 圆环（与管道末端重合，半径与管道一致）
        for (int j = 0; j < circleSegment; j++)
        {
            float angle = 2 * Mathf.PI * j / circleSegment;
            Vector3 offset = Quaternion.LookRotation(forward) * (Vector3.right * Mathf.Cos(angle) * radius + Vector3.up * Mathf.Sin(angle) * radius);
            verts.Add(baseCenter + offset);
            cols.Add(color);
        }
        // 三角面
        for (int j = 0; j < circleSegment; j++)
        {
            int v0 = baseIdx;
            int v1 = baseIdx + 1 + j;
            int v2 = baseIdx + 1 + (j + 1) % circleSegment;
            tris.Add(v0);
            tris.Add(v1);
            tris.Add(v2);
        }
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.colors = cols.ToArray();
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