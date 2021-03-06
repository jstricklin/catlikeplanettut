﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PlanetGenBckp: MonoBehaviour
{

	public int gridSize = 50;
    public float radius = 5f;
	Mesh mesh;
	Vector3[] vertices;
	Vector3[] normals;
    Vector2[] sphereUV;

    GameObject body;

	private void Awake()
	{
		Generate();

	}

    private void Update()
    {
       Ray uvRay = Camera.main.ScreenPointToRay(Input.mousePosition);
    }

    private void Generate()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Procedural Sphere";
        CreateVertices();
        CreateTriangles();
        CreateColliders();
        SetUV();
    }

    private void CreateColliders()
    {
        gameObject.AddComponent<SphereCollider>();
    }

    private void CreateVertices()
    {
        int cornerVertices = 8;
        int edgeVertices = (gridSize + gridSize + gridSize - 3) * 4;
        int faceVertices = (
        (gridSize - 1) * (gridSize - 1) +
        (gridSize - 1) * (gridSize - 1) +
        (gridSize - 1) * (gridSize - 1)) * 2;
        vertices = new Vector3[cornerVertices + edgeVertices + faceVertices];
        normals = new Vector3[vertices.Length];
        sphereUV = new Vector2[vertices.Length];

        int v = 0;
        for (int y = 0; y <= gridSize; y++)
        {
            for (int x = 0; x <= gridSize; x++)
            {
                SetVertex(v++, x, y, 0);
            }
            for (int z = 1; z <= gridSize; z++)
            {
                SetVertex(v++, gridSize, y, z);
            }
            for (int x = gridSize - 1; x >= 0; x--)
            {
                SetVertex(v++, x, y, gridSize);
            }
            for (int z = gridSize - 1; z > 0; z--)
            {
                SetVertex(v++, 0, y, z);
            }
        }

		 //cap top and bottom
		for (int z = 1; z < gridSize; z++)
		{
			for (int x = 1; x < gridSize; x++)
			{
				SetVertex(v++, x, gridSize, z);
			}
		}
        for (int z = 1; z < gridSize; z++)
        {
            for (int x = 1; x < gridSize; x++)
            {
                SetVertex(v++, x, 0, z);
            }
        }
        mesh.vertices = vertices;
        mesh.normals = vertices;

    }

    private void SetVertex(int i , int x, int y, int z)
    {

        //find out how this math works to center vertex points around origin
        Vector3 v =  vertices[i] = new Vector3(x, y, z) * 2f / gridSize - Vector3.one;
        float x2 = v.x * v.x;
        float y2 = v.y * v.y;
        float z2 = v.z * v.z;

        Vector3 s;

        s.x = v.x * Mathf.Sqrt(1f - y2 * 0.5f - z2 * 0.5f + y2 * z2 / 3f);
        s.y = v.y * Mathf.Sqrt(1f - x2 * 0.5f - z2 * 0.5f + x2 * z2 / 3f);
        s.z = v.z * Mathf.Sqrt(1f - x2 * 0.5f - y2 * 0.5f + x2 * y2 / 3f);

        normals[i] = s;

        //sphereUV[i] = V3toUV(s);

        vertices[i] = normals[i] * radius;

	}

    Vector2 V3toUV(Vector3 d)
    {
        float u = 0.5f + Mathf.Atan2(d.z, d.x) / 2 * Mathf.PI;
        float v = 0.5f - Mathf.Asin(-d.y) / Mathf.PI;
        return new Vector2(u, v); 
    }

    private void CreateTriangles()
    {
        int quads = (gridSize * gridSize + gridSize * gridSize + gridSize * gridSize) * 2;
        int[] triangles = new int[quads * 6];
        int ring = (gridSize + gridSize) * 2;
        int t = 0, v = 0;

        for (int y = 0; y < gridSize; y++, v++)
        {
            for (int q = 0; q < gridSize; q++, v++)
            {
                t = SetQuad(triangles, t, v, v + 1, v + ring, v + ring + 1);
            }
            for (int q = 0; q < gridSize; q++, v++)
            {
                t = SetQuad(triangles, t, v, v + 1, v + ring, v + ring + 1);
            }
            for (int q = 0; q < gridSize; q++, v++)
            {
                t = SetQuad(triangles, t, v, v + 1, v + ring, v + ring + 1);
            }
            for (int q = 0; q < gridSize - 1; q++, v++)
            {
                t = SetQuad(triangles, t, v, v + 1, v + ring, v + ring + 1);
            }
            t = SetQuad(triangles, t, v, v - ring + 1, v + ring, v + 1);
        }

		t = CreateTopFace(triangles, t, ring);
		t = CreateBottomFace(triangles, t, ring);
        mesh.triangles = triangles;

	}

	private int CreateTopFace(int[] triangles, int t, int ring)
	{
        //First Row
        int v = ring * gridSize;
		for (int x = 0; x < gridSize - 1; x++, v++)
		{
			t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + ring);
		}
		t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + 2);

        //Mid Rows
        int vMin = ring * (gridSize + 1) - 1;
        int vMid = vMin + 1;
        int vMax = v + 2;

		for (int z = 1; z < gridSize - 1; z++, vMid++, vMin--, vMax++)
		{
		    t = SetQuad(triangles, t, vMin, vMid, vMin - 1, vMid + gridSize - 1);

            for (int x = 1; x < gridSize - 1; x++, vMid++)
            {
                t = SetQuad(triangles, t, vMid, vMid + 1, vMid + gridSize - 1, vMid + gridSize);
            }
            t = SetQuad(triangles, t, vMid, vMax, vMid + gridSize - 1, vMax + 1);
        }
        //Last Row
        int vTop = vMin - 2;
        t = SetQuad(triangles, t, vMin, vMid, vTop + 1, vTop);
        for (int x = 1; x < gridSize - 1; x++, vMid++, vTop--)
        {
            t = SetQuad(triangles, t, vMid, vMid + 1, vTop, vTop - 1);
        }
        t = SetQuad(triangles, t, vMid, vTop - 2, vTop, vTop - 1);
        return t;
	}

    private int CreateBottomFace(int[] triangles, int t, int ring)
    {
        //First Row
        int v = 1;
        int vMid = vertices.Length - (gridSize - 1) * (gridSize - 1);
        t = SetQuad(triangles, t, ring - 1, vMid, 0, 1);
        for (int x = 1; x < gridSize - 1; x++, v++, vMid++)
        {
            t = SetQuad(triangles, t, vMid, vMid + 1, v, v + 1);
        }
        t = SetQuad(triangles, t, vMid, v + 2, v, v + 1);

        //Mid Rows
        int vMin = ring -2;
        vMid -= gridSize - 2;
        int vMax = v + 2;

        for (int z = 1; z < gridSize - 1; z++, vMid++, vMin--, vMax++)
        {
            t = SetQuad(triangles, t, vMin, vMid + gridSize - 1, vMin + 1, vMid);

            for (int x = 1; x < gridSize - 1; x++, vMid++)
            {
                t = SetQuad(triangles, t, vMid + gridSize - 1, vMid + gridSize, vMid, vMid + 1);
            }
            t = SetQuad(triangles, t, vMid + gridSize - 1, vMax + 1, vMid, vMax);
        }

        //Last Row
        int vTop = vMin - 1;
        t = SetQuad(triangles, t, vTop + 1, vTop, vTop + 2, vMid);
        for (int x = 1; x < gridSize - 1; x++, vMid++, vTop--)
        {
            t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vMid + 1);
        }
        t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vTop - 2);
        return t;
    }

	private static int SetQuad(int[] triangles, int i, int v00, int v10, int v01, int v11)
	{
		triangles[i] = v00;
        triangles[i + 1] = triangles[i + 4] = v01;
        triangles[i + 2] = triangles[i + 3] = v10;
        triangles[i + 5] = v11;
        return i + 6;
	}

    void SetUV()
    {
        mesh.uv = sphereUV;
    }


 //   private void OnDrawGizmos()
	//{
		//if (vertices == null)
		//{
		//	return;
		//}
		//Gizmos.color = Color.black;
  //      for (int i = 0; i < vertices.Length; i++)
  //      {
  //          Gizmos.DrawRay(cubeUV[i], cubeUV[i].normalized);
		//}
	//}
}