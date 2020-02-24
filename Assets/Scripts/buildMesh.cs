using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class buildMesh : MonoBehaviour
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
    public float Size = 1;
    Vector3 v0;
    Mesh m;

    bool start_lerp = false;
    Vector3 ret = Vector3.one;
    // Angular speed in radians per sec.
    public float speed = 1.0f;

    private static float angle = Mathf.PI - Mathf.Acos(1 / 3);

    private List<Vector3> centers = new List<Vector3>();
    private List<Color32> colors = new List<Color32> { Color.yellow, Color.red, Color.blue, Color.green };
 
    public Mesh CreateMesh()
    {
        if (centers.Count == 0)
            centers.Add(Vector3.zero);

        Vector3[] _vertices = new Vector3[centers.Count * 12];
        Vector3[] _normals = new Vector3[_vertices.Length];
        Color32[] _colors32 = new Color32[_vertices.Length];

        float s = Size;
        int i = 0;

        foreach (var c in centers)
        {
            v0 = c + new Vector3(0, s, 0); // pyramid head

            // base triangle /_\
            var v1 = c + new Vector3(-s2_3 * s, -f1_3 * s, -s2_9 * s);  // left
            var v2 = c + new Vector3(s2_3 * s, -f1_3 * s, -s2_9 * s);   // right
            var v3 = c + new Vector3(0, -f1_3 * s, s8_9 * s);           // top

            var v4 = c + new Vector3(0, -f1_3 * s, -fs4_3 * s);    //tip down
            var v5 = v3 + new Vector3(-a, 0, 0);             // tip top left
            var v6 = v3 + new Vector3(a, 0, 0);              // tip top right


            _normals[i] = _normals[i + 1] = _normals[i + 2] = Vector3.Cross(v1 - v4, v2 - v4).normalized;
            _vertices[i++] = v4; _vertices[i++] = v1; _vertices[i++] = v2; // \/

            _normals[i] = _normals[i + 1] = _normals[i + 2] = Vector3.Cross(v3 - v5, v1 - v5).normalized;
            _vertices[i++] = v5; _vertices[i++] = v3; _vertices[i++] = v1;  //<

            _normals[i] = _normals[i + 1] = _normals[i + 2] = Vector3.Cross(v2 - v6, v3 - v6).normalized;
            _vertices[i++] = v6; _vertices[i++] = v2; _vertices[i++] = v3;  // >

            _normals[i] = _normals[i + 1] = _normals[i + 2] = Vector3.down; //Vector3.Cross(v2 - v3, v1 - v3).normalized; //Vector3.down;
            _vertices[i++] = v3; _vertices[i++] = v2; _vertices[i++] = v1;  // /\                 
           
        }

        int[] _triangles = new int[_vertices.Length];
        int nextColor = 0;
        for (int n = 0; n < _triangles.Length; n++)
        {
            _triangles[n] = n;

            if (n % 3 == 0)
                nextColor = (nextColor + 1) % 4;

            /*                _colors32[n] = colors[0]; // (nextColor + 1) % 4;
                        if (n > 2 && n < 6)
                            _colors32[n] = colors[1]; // (nextColor + 1) % 4;
                        if (n > 5 &&  n < 9)
                            _colors32[n] = colors[2]; // (nextColor + 1) % 4;
                        if (n > 8)
                            _colors32[n] = colors[3]; // (nextColor + 1) % 4;
                            */
            //print("n: " + n + " color: " + colors[nextColor]);
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

    // Start is called before the first frame update
    void Start()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        /*Mesh*/
        m = mf.mesh;

        Mesh sierp = CreateMesh();
        m.Clear();
        m.vertices = sierp.vertices;
        m.triangles = sierp.triangles;
        // m.normals = sierp.normals;
        m.colors32 = sierp.colors32;
        // m.uv = uvs;
        //m.Optimize();
        m.RecalculateNormals();

    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.L)) // lerping up
        {
            start_lerp = !start_lerp;
        }

        if (start_lerp)
        {

            print("rotate");
            Vector3[] vertices = m.vertices;
            var targetPosition = v0;
            for (int i = 0; i < m.vertexCount - 3; i += 3)
            {
                // The step size is equal to speed times frame time.
                float singleStep = speed * Time.deltaTime;


                // Rotate the forward vector towards the target direction by one step                   
                vertices[i] = Vector3.MoveTowards(vertices[i], targetPosition, singleStep);

                // Check if the position of the cube and sphere are approximately equal.
                if (Vector3.Distance(vertices[i], targetPosition) < 0.001f)
                {
                    // Swap the position of the cylinder.
                    start_lerp = false;
                }
            }

            m.vertices = vertices;
            //m.RecalculateNormals();            
        }
    }
}
