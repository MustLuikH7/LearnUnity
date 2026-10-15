using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Builds maze walls from a text file as one combined mesh with a collider.
// The file has N+1 lines of horizontal walls (N chars each) followed by
// N lines of vertical walls (N+1 chars each); '1' means there is a wall.
// This object's position is the maze's first corner: rows go along -X, columns along +Z.
[ExecuteAlways]
public class MazeBuilder : MonoBehaviour
{
    public TextAsset mazeData;
    public Material wallMaterial;
    public float cellSize = 4f;
    public float wallHeight = 2f;
    public float wallThickness = 0.2f;
    // If set, puts one coin in the middle of every maze cell (except the exit)
    public GameObject coinPrefab;
    public float coinHeight = 0.275f;

    private GameObject walls;

    void OnEnable()
    {
        // Remove a leftover copy (e.g. after a script reload) before building a new one
        var old = transform.Find("MazeWalls");
        if (old != null)
        {
            DestroyImmediate(old.gameObject);
        }
        Build();
    }

    void OnDisable()
    {
        if (walls != null)
        {
            DestroyImmediate(walls.GetComponent<MeshFilter>().sharedMesh);
            DestroyImmediate(walls);
        }
    }

    void OnValidate()
    {
        // Rebuild when values change in the Inspector (can't destroy objects inside OnValidate itself)
#if UNITY_EDITOR
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this != null && isActiveAndEnabled)
            {
                OnDisable();
                Build();
            }
        };
#endif
    }

    void Build()
    {
        if (mazeData == null || walls != null)
        {
            return;
        }

        var lines = new List<string>();
        foreach (var line in mazeData.text.Split('\n'))
        {
            if (line.Trim().Length > 0)
            {
                lines.Add(line.Trim());
            }
        }
        int n = lines[0].Length;

        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();
        var triangles = new List<int>();
        float y = wallHeight / 2f;

        // Horizontal walls: line r runs along Z at x = -r * cellSize
        for (int r = 0; r <= n; r++)
        {
            string row = lines[r];
            int c = 0;
            while (c < n)
            {
                if (row[c] != '1') { c++; continue; }
                int start = c;
                while (c < n && row[c] == '1') c++;
                float length = (c - start) * cellSize;
                var center = new Vector3(-r * cellSize, y, (start + c) / 2f * cellSize);
                var size = new Vector3(wallThickness, wallHeight, length + wallThickness);
                AddBox(center, size, vertices, normals, uvs, triangles);
            }
        }

        // Vertical walls: line c runs along -X at z = c * cellSize
        for (int c = 0; c <= n; c++)
        {
            int r = 0;
            while (r < n)
            {
                if (lines[n + 1 + r][c] != '1') { r++; continue; }
                int start = r;
                while (r < n && lines[n + 1 + r][c] == '1') r++;
                float length = (r - start) * cellSize;
                var center = new Vector3(-(start + r) / 2f * cellSize, y, c * cellSize);
                var size = new Vector3(length + wallThickness, wallHeight, wallThickness);
                AddBox(center, size, vertices, normals, uvs, triangles);
            }
        }

        var mesh = new Mesh { name = "MazeWalls", indexFormat = IndexFormat.UInt32 };
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();

        walls = new GameObject("MazeWalls");
        walls.hideFlags = HideFlags.DontSave; // rebuilt on load, never stored in the scene
        walls.transform.SetParent(transform, false);
        walls.AddComponent<MeshFilter>().sharedMesh = mesh;
        walls.AddComponent<MeshRenderer>().sharedMaterial = wallMaterial;
        walls.AddComponent<MeshCollider>().sharedMesh = mesh;

        if (coinPrefab != null)
        {
            AddCoins(lines, n);
        }
    }

    void AddCoins(List<string> lines, int n)
    {
        int exitColumn = lines[n].IndexOf('0');
        for (int r = 0; r < n; r++)
        {
            for (int c = 0; c < n; c++)
            {
                if (r == n - 1 && c == exitColumn)
                {
                    continue; // the finish line is here
                }
                var coin = Instantiate(coinPrefab, walls.transform);
                coin.name = "Coin";
                coin.hideFlags = HideFlags.DontSave;
                coin.transform.localPosition = new Vector3(-(r + 0.5f) * cellSize, coinHeight, (c + 0.5f) * cellSize);
            }
        }
    }

    static void AddBox(Vector3 center, Vector3 size, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> triangles)
    {
        Vector3 h = size / 2f;
        Vector3[] faceNormals = { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back };
        foreach (var normal in faceNormals)
        {
            // Two axes spanning the face, ordered so triangles face outward
            Vector3 u = new Vector3(normal.y, normal.z, normal.x);
            Vector3 v = Vector3.Cross(normal, u);
            Vector3 faceCenter = center + Vector3.Scale(normal, h);
            Vector3 du = Vector3.Scale(u, h);
            Vector3 dv = Vector3.Scale(v, h);

            int i = vertices.Count;
            vertices.Add(faceCenter - du - dv);
            vertices.Add(faceCenter + du - dv);
            vertices.Add(faceCenter + du + dv);
            vertices.Add(faceCenter - du + dv);
            for (int k = 0; k < 4; k++) normals.Add(normal);
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(1, 1));
            uvs.Add(new Vector2(0, 1));
            triangles.AddRange(new[] { i, i + 1, i + 2, i, i + 2, i + 3 });
        }
    }
}
