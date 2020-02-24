using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class STetrahedon
{
    // H = 4/3 *s 
    // a = 2 * sqrt(2/3)
    // h = sqrt(2)

    static float s8_9 = Mathf.Sqrt(8f / 9f);    // 2/3 * sqrt(2) => 2/3 * height of triangle 
    static float s2_9 = Mathf.Sqrt(2f / 9f);    // 1/3 * sqrt(2) => 1/3 * height of triangle
    static float s2_3 = Mathf.Sqrt(2f / 3f);    // half length of triangle side    
    static float f1_3 = 1f / 3f;                // distance center to bottom
    static float fs4_3 = (4f * Mathf.Sqrt(2f)) / 3f;                // triangle height 
    static float a = 2 * Mathf.Sqrt(2f / 3f);   // triangle side length
    public float Size = 2;

    private List<Vector3> centers = new List<Vector3>();
    private List<Color32> colors = new List<Color32> { Color.yellow, Color.red, Color.blue, Color.green };

    private List<Vector3[]> targetPositions = new List<Vector3[]>();

    public List<Vector3[]> getTargetsPos()
    {
        return targetPositions;
    }

    public STetrahedon Subdivide(int aCount)
    {
        var res = this;
        for (int i = 0; i < aCount; i++)
            res = res.Subdivide();
        return res;
    }

    public STetrahedon Subdivide()
    {
        var result = new STetrahedon();
        float s = result.Size = Size * 0.5f;

        if (centers.Count == 0)
            centers.Add(Vector3.zero);

        foreach (var c in centers)
        {
            result.centers.Add(c + new Vector3(0, s, 0));
            result.centers.Add(c + new Vector3(-s2_3 * s, -f1_3 * s, -s2_9 * s));
            result.centers.Add(c + new Vector3(s2_3 * s, -f1_3 * s, -s2_9 * s));
            result.centers.Add(c + new Vector3(0, -f1_3 * s, s8_9 * s));
        }
        return result;
    }

    public Mesh CreateBaseMesh()
    {

        Vector3[] _vertices = new Vector3[12];
        Vector3[] _normals = new Vector3[_vertices.Length];
        Color32[] _colors32 = new Color32[_vertices.Length];

        float s = Size;
        int i = 0;

        var c = Vector3.zero;

        var v0 = c + new Vector3(0, s, 0); // pyramid head
        
        // base triangle /_\
        var v1 = c + new Vector3(-s2_3 * s, -f1_3 * s, -s2_9 * s);  // left
        var v2 = c + new Vector3(s2_3 * s, -f1_3 * s, -s2_9 * s);   // right
        var v3 = c + new Vector3(0, -f1_3 * s, s8_9 * s);           // top

        var v4 = c + new Vector3(0, -f1_3 * s, -fs4_3 * s);    //tip down
        var v5 = v3 + new Vector3(-a*s, 0, 0);             // tip top left
        var v6 = v3 + new Vector3(a*s, 0, 0);              // tip top right


        _normals[i] = _normals[i + 1] = _normals[i + 2] = Vector3.Cross(v1 - v4, v2 - v4).normalized;
        _vertices[i++] = v4; _vertices[i++] = v1; _vertices[i++] = v2; // \/

        _normals[i] = _normals[i + 1] = _normals[i + 2] = Vector3.Cross(v3 - v5, v1 - v5).normalized;
        _vertices[i++] = v5; _vertices[i++] = v3; _vertices[i++] = v1;  //<

        _normals[i] = _normals[i + 1] = _normals[i + 2] = Vector3.Cross(v2 - v6, v3 - v6).normalized;
        _vertices[i++] = v6; _vertices[i++] = v2; _vertices[i++] = v3;  // >

        _normals[i] = _normals[i + 1] = _normals[i + 2] = Vector3.down; //Vector3.Cross(v2 - v3, v1 - v3).normalized; //Vector3.down;
        _vertices[i++] = v3; _vertices[i++] = v2; _vertices[i++] = v1;  // /\                 

        var targetPosUp = new Vector3[1] { v0 };
        targetPositions.Add(targetPosUp);

        var targetPosDown = new Vector3[3] { v4, v5, v6 };
        targetPositions.Add(targetPosDown);


        int[] _triangles = new int[_vertices.Length];
        int nextColor = 0;
        for (int n = 0; n < _triangles.Length; n++)
        {
            _triangles[n] = n;

            if (n % 3 == 0)
                nextColor = (nextColor + 1) % 4;

            _colors32[n] = colors[nextColor];

        }

        var m = new Mesh
        {
            vertices = _vertices,
            normals = _normals,
            triangles = _triangles,
            colors32 = _colors32
        };

        return m;
    }

    public Mesh CreateMesh()
    {
        if (centers.Count == 0)
            centers.Add(Vector3.zero);

        Vector3[] _vertices = new Vector3[centers.Count * 12];
        Vector3[] _normals = new Vector3[_vertices.Length];
        Color32[] _colors32 = new Color32[_vertices.Length];

        float s = Size;
        int i = 0;
        Debug.Log("CreateMesh #centers.Count: " + centers.Count);

        //var targets = new Vector3[centers.Count * 4];

        foreach (var c in centers)
        {
            var v0 = c + new Vector3(0, s, 0);                              // head
            var v1 = c + new Vector3(-s2_3 * s, -f1_3 * s, -s2_9 * s);      // left
            var v2 = c + new Vector3(s2_3 * s, -f1_3 * s, -s2_9 * s);       // right
            var v3 = c + new Vector3(0, -f1_3 * s, s8_9 * s);               // top

            _normals[i] = _normals[i + 1] = _normals[i + 2] = Vector3.Cross(v2 - v0, v1 - v0).normalized;
            _vertices[i++] = v0; _vertices[i++] = v2; _vertices[i++] = v1;

            _normals[i] = _normals[i + 1] = _normals[i + 2] = Vector3.Cross(v1 - v0, v3 - v0).normalized;
            _vertices[i++] = v0; _vertices[i++] = v1; _vertices[i++] = v3;

            _normals[i] = _normals[i + 1] = _normals[i + 2] = Vector3.Cross(v3 - v0, v2 - v0).normalized;
            _vertices[i++] = v0; _vertices[i++] = v3; _vertices[i++] = v2;

            _normals[i] = _normals[i + 1] = _normals[i + 2] = Vector3.down;
            _vertices[i++] = v1; _vertices[i++] = v2; _vertices[i++] = v3;


        }

        int[] _triangles = new int[_vertices.Length];
        int nextColor = 0;

        for (int n = 0; n < _triangles.Length; n++)
        {
            _triangles[n] = n;

            if (n % 3 == 0)
                nextColor = (nextColor + 1) % 4;

            _colors32[n] = colors[nextColor];
        }

        var m = new Mesh
        {
            vertices = _vertices,
            normals = _normals,
            triangles = _triangles,
            colors32 = _colors32
        };

        return m;
    }
}
