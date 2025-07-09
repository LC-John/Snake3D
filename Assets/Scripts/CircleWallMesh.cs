using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CircleWallMesh : MonoBehaviour
{
    public SnakeGameConfig config;

    void Start()
    {
        GenerateWallMesh();
        GenerateGroundMesh();
    }

    void GenerateWallMesh()
    {
        float radius = config != null ? config.wallRadius : 10f;
        float wallThickness = config != null ? config.wallThickness : 0.5f;
        float wallHeight = config != null ? config.wallHeight : 2f;
        int segmentCount = config != null ? config.segmentCount : 100;
        Mesh mesh = new Mesh();
        int vertsPerCircle = segmentCount * 2;
        Vector3[] vertices = new Vector3[vertsPerCircle * 2];
        int[] triangles = new int[segmentCount * 12];
        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < segmentCount; i++)
        {
            float angle = i * Mathf.PI * 2 / segmentCount;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);
            Vector3 outer = new Vector3((radius + wallThickness / 2) * cos, 0, (radius + wallThickness / 2) * sin);
            Vector3 inner = new Vector3((radius - wallThickness / 2) * cos, -0.001f, (radius - wallThickness / 2) * sin);
            vertices[i * 2] = outer;
            vertices[i * 2 + 1] = inner;
            vertices[vertsPerCircle + i * 2] = outer + Vector3.up * wallHeight;
            vertices[vertsPerCircle + i * 2 + 1] = inner + Vector3.up * wallHeight;
            float u = (float)i / segmentCount;
            uvs[i * 2] = new Vector2(u, 0);
            uvs[i * 2 + 1] = new Vector2(u, 0.2f);
            uvs[vertsPerCircle + i * 2] = new Vector2(u, 1);
            uvs[vertsPerCircle + i * 2 + 1] = new Vector2(u, 0.8f);
        }
        int tri = 0;
        for (int i = 0; i < segmentCount; i++)
        {
            int next = (i + 1) % segmentCount;
            triangles[tri++] = i * 2;
            triangles[tri++] = next * 2;
            triangles[tri++] = vertsPerCircle + i * 2;
            triangles[tri++] = next * 2;
            triangles[tri++] = vertsPerCircle + next * 2;
            triangles[tri++] = vertsPerCircle + i * 2;
            triangles[tri++] = vertsPerCircle + i * 2 + 1;
            triangles[tri++] = vertsPerCircle + next * 2 + 1;
            triangles[tri++] = i * 2 + 1;
            triangles[tri++] = vertsPerCircle + next * 2 + 1;
            triangles[tri++] = next * 2 + 1;
            triangles[tri++] = i * 2 + 1;
        }
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        GetComponent<MeshFilter>().mesh = mesh;
        var meshCollider = GetComponent<MeshCollider>();
        if (meshCollider == null)
            meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
        meshCollider.convex = false;
        meshCollider.isTrigger = false;
    }

    void GenerateGroundMesh()
    {
        float radius = config != null ? config.wallRadius : 10f;
        int segmentCount = config != null ? config.segmentCount : 100;
        float wallThickness = config != null ? config.wallThickness : 0.5f;
        float groundThickness = config != null ? config.groundThickness : 0.2f;
        float groundRadius = radius + 0.01f;
        GameObject ground = new GameObject("Ground", typeof(MeshFilter), typeof(MeshRenderer));
        ground.transform.SetParent(transform);
        ground.transform.localPosition = new Vector3(0, -0.001f, 0);
        ground.transform.localRotation = Quaternion.identity;
        ground.transform.localScale = Vector3.one;
        var wallMat = GetComponent<MeshRenderer>().sharedMaterial;
        if (wallMat != null)
            ground.GetComponent<MeshRenderer>().sharedMaterial = wallMat;
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[segmentCount + 1];
        int[] triangles = new int[segmentCount * 3];
        Vector2[] uvs = new Vector2[segmentCount + 1];
        vertices[0] = Vector3.zero;
        uvs[0] = new Vector2(0.5f, 0.5f);
        for (int i = 0; i < segmentCount; i++)
        {
            float angle = i * Mathf.PI * 2 / segmentCount;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);
            vertices[i + 1] = new Vector3(groundRadius * cos, 0, groundRadius * sin);
            uvs[i + 1] = new Vector2(0.5f + 0.5f * cos, 0.5f + 0.5f * sin);
        }
        for (int i = 0; i < segmentCount; i++)
        {
            int next = (i + 1) % segmentCount;
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = next + 1;
            triangles[i * 3 + 2] = i + 1;
        }
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        ground.GetComponent<MeshFilter>().mesh = mesh;
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("碰撞到物体: " + collision.gameObject.name);
        if (collision.gameObject.name.Contains("CircleWall"))
        {
            Debug.Log("Game Over! 撞墙死亡");
        }
    }
} 