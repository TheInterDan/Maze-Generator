using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshHelper
{
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();
    Mesh mesh;

    public void Clear()
    {
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
    }

    public int[] AddArray(Vector3[] points)
    {
        int[] verticesIndex = new int[points.Length];
        for (int i = 0; i < points.Length; i++) { verticesIndex[i] = vertices.Count; vertices.Add(points[i]); }
        return verticesIndex;
    }

    public void CreateQuad(int a, int b, int c, int d, bool inverse = false)
    {
        AddTriangle(a, b, c, inverse);
        AddTriangle(b, d, c, inverse);
    }

    public void AddTriangle(int a, int b, int c, bool inverse = false)
    {
        triangles.Add(a);
        if (!inverse) triangles.Add(b);
        triangles.Add(c);
        if (inverse) triangles.Add(b);
    }

    public void ConectRings(int[] ring1, int[] ring2)
    {
        if (ring1.Length == ring2.Length)
        {
            int faces = ring1.Length;
            for (int i = 0; i < faces; i++)
            {
                int iPlus = (i + 1) % faces;
                CreateQuad(ring1[i], ring2[i], ring1[iPlus], ring2[iPlus]);
            }
        }
    }

    public void Cap(int[] ring, bool inverse = false)
    {
        for(int i = 1; i < ring.Length - 1; i++)
        {
            AddTriangle(ring[0], ring[i+1], ring[i], inverse);
        }
    }

    public int[] AddArray(Vector3[] points, float uvx)
    {
        int[] verticesIndex = AddArray(points);
        SetRingUv(verticesIndex, uvx);
        return verticesIndex;
    }

    public void SetRingUv(int[] ring, float x)
    {
        SyncUvs();
        float increment = 2f / ring.Length;
        float y = 0;
        for (int i = 0; i < ring.Length - 1; i++)
        {
            uvs[ring[i]] = new Vector2(x, y);
            if (y >= 1) increment = -increment;
            y += increment;
        }
        uvs[ring[ring.Length-1]] = new Vector2(x, 0 - increment);
    }

    public Mesh CreateMesh()
    {
        Mesh newMesh = new Mesh();
        newMesh.vertices = vertices.ToArray();
        newMesh.triangles = triangles.ToArray();
        newMesh.uv = uvs.ToArray();
        newMesh.RecalculateNormals();
        mesh = newMesh;
        return newMesh;
    }

    public void UpdateMesh(ref Mesh meshToUpdate)
    {
        meshToUpdate.vertices = vertices.ToArray();
        meshToUpdate.triangles = triangles.ToArray();
        meshToUpdate.uv = uvs.ToArray();
        meshToUpdate.RecalculateNormals();
    }

    public void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
    }

    void SyncUvs()
    {
        while (uvs.Count < vertices.Count)
        {
            uvs.Add(new Vector2(0, 0));
        }
    }
}
