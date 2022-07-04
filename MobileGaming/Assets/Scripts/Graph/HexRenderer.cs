using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct Face
{
    public List<Vector3> vertices { get; private set; }
    public List<int> triangles { get; private set; }
    public List<Vector2> uvs { get; private set; }

    public Face(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
    {
        this.vertices = vertices;
        this.triangles = triangles;
        this.uvs = uvs;
    }
}


[RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
public class HexRenderer : MonoBehaviour
{
    private Mesh m_mesh;
    private MeshFilter m_meshFilter;
    private MeshRenderer m_meshRenderer;

    private List<Face> m_faces;

    public Material material;

    [Min(0f)] public float innerSize;
    [Min(0f)] public float outerSize = 1.5f;
    public float height = 0;
    public bool isFlatTopped;

    private void Awake()
    {
        m_meshFilter = GetComponent<MeshFilter>();
        m_meshRenderer = GetComponent<MeshRenderer>();

        m_mesh = new Mesh
        {
            name = "Hex"
        };

        m_meshFilter.mesh = m_mesh;
        m_meshRenderer.material = material;
    }

    private void OnEnable()
    {
        DrawMesh();
    }

    private void OnValidate()
    {
        if(Application.isPlaying) DrawMesh();
    }
    
    public void SetMaterial(Material mat)
    {
        m_meshRenderer.material = mat;
    }

    public void DrawMesh()
    {
        if(m_mesh == null) return;
        DrawFaces();
        CombineFaces();
    }

    private void CombineFaces()
    {
        var vertices = new List<Vector3>();
        var tris = new List<int>();
        var uvs = new List<Vector2>();

        for (var i = 0; i < m_faces.Count; i++)
        {
            vertices.AddRange(m_faces[i].vertices);
            uvs.AddRange(m_faces[i].uvs);

            var offset = 4 * i;
            tris.AddRange(m_faces[i].triangles.Select(triangle => triangle + offset));
        }
        
        m_mesh.vertices = vertices.ToArray();
        m_mesh.triangles = tris.ToArray();
        m_mesh.uv = uvs.ToArray();
        m_mesh.RecalculateNormals();
    }

    private void DrawFaces()
    {
        m_faces = new List<Face>();

        // Top Faces
        for (int point = 0; point < 6; point++)
        {
            m_faces.Add(CreateFace(innerSize,outerSize,height/2f,height/2f,point));
        }
        
        // Bottom Faces
        for (int point = 0; point < 6; point++)
        {
            m_faces.Add(CreateFace(innerSize,outerSize,-height/2f,-height/2f,point,true));
        }
        
        // Outer Faces
        for (int point = 0; point < 6; point++)
        {
            m_faces.Add(CreateFace(outerSize,outerSize,height/2f,-height/2f,point,true));
        }
        
        // Inner Faces
        for (int point = 0; point < 6; point++)
        {
            m_faces.Add(CreateFace(innerSize,innerSize,height/2f,-height/2f,point));
        }
    }

    private Face CreateFace(float innerRad, float outerRad, float heightA, float heightB, int point,
        bool reverse = false)
    {
        var pointA = GetPoint(innerRad, heightB, point);
        var pointB = GetPoint(innerRad, heightB, (point < 5) ? point + 1 : 0);
        var pointC = GetPoint(outerRad, heightA, (point < 5) ? point + 1 : 0);
        var pointD = GetPoint(outerRad, heightA, point);

        var vertices = new List<Vector3>() {pointA, pointB, pointC, pointD};
        var triangles = new List<int>() {0, 1, 2, 2, 3, 0};
        var uvs = new List<Vector2>() {Vector2.zero, Vector2.right, Vector2.one, Vector2.up};
        if(reverse) vertices.Reverse();
        
        return new Face(vertices,triangles,uvs);
    }

    protected Vector3 GetPoint(float size, float height, int index)
    {
        var angleDeg = 60f * index;
        if (isFlatTopped) angleDeg -= 30f;
        var angleRad = Mathf.PI / 180f * angleDeg;
        return new Vector3((size * Mathf.Cos(angleRad)), height, size * Mathf.Sin(angleRad));
    }
}
