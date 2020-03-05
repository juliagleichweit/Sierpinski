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
    public float Size = 3;

    private List<Vector3> centers = new List<Vector3>();
    public static List<Color32> colors = new List<Color32> { Color.yellow, Color.red, Color.blue, Color.green };

    private static List<List<Vector3>> targetPositions = new List<List<Vector3>>();

    public List<List<Vector3>> getTargetsPos()
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
        var v5 = v3 + new Vector3(-a * s, 0, 0);             // tip top left
        var v6 = v3 + new Vector3(a * s, 0, 0);              // tip top right

        _vertices[i++] = v4; _vertices[i++] = v1; _vertices[i++] = v2; // \/             
        _vertices[i++] = v5; _vertices[i++] = v3; _vertices[i++] = v1;  //<        
        _vertices[i++] = v6; _vertices[i++] = v2; _vertices[i++] = v3;  // >        
        _vertices[i++] = v3; _vertices[i++] = v2; _vertices[i++] = v1;  // /\ 

        var targetPosUp = new List<Vector3>();
        targetPosUp.Add(v0);
        targetPositions.Add(targetPosUp);

        var targetPosDown = new List<Vector3>();
        targetPosDown.Add(v4);
        targetPosDown.Add(v5);
        targetPosDown.Add(v6);
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
            triangles = _triangles,
            colors32 = _colors32
        };

        return m;
    }

    public Mesh CreateMesh()
    {
        if (centers.Count == 0)
            centers.Add(Vector3.zero);

        // centers * 12 per pyramid + 3 extra per 4-block pyramid
        var vert_count = centers.Count * (12 + (centers.Count / 4) * 36);

        Vector3[] _vertices = new Vector3[vert_count];
        Color32[] _colors32 = new Color32[_vertices.Length];

        float s = Size;
        int i = 0;
         
        var targetPos = new List<Vector3>();
         
        for (int k = 0; k < centers.Count; k++)
        {
            var c = centers[k];

            var v0 = c + new Vector3(0, s, 0);                              // head
            var v1 = c + new Vector3(-s2_3 * s, -f1_3 * s, -s2_9 * s);      // left
            var v2 = c + new Vector3(s2_3 * s, -f1_3 * s, -s2_9 * s);       // right
            var v3 = c + new Vector3(0, -f1_3 * s, s8_9 * s);               // top

            _colors32[i] = _colors32[i+1] = _colors32[i + 2] = colors[1]; 
            _vertices[i++] = v0; _vertices[i++] = v2; _vertices[i++] = v1;

            _colors32[i] = _colors32[i + 1] = _colors32[i + 2] = colors[2];
            _vertices[i++] = v0; _vertices[i++] = v1; _vertices[i++] = v3;

            _colors32[i] = _colors32[i + 1] = _colors32[i + 2] = colors[3];
            _vertices[i++] = v0; _vertices[i++] = v3; _vertices[i++] = v2;

            _colors32[i] = _colors32[i + 1] = _colors32[i + 2] = colors[0];
            _vertices[i++] = v1; _vertices[i++] = v2; _vertices[i++] = v3;

            if ((k + 1) % 4 == 0) //one 4.block pyramid finished
            {
                //Debug.Log("Add Extra Vertices");
                // i-1 is the last vertex of the 4th pyramid  (top of the bottom triangle) 
                
                var bt_rt = _vertices[i - 2];
                var bt_lft = _vertices[i - 3];
                var bt_frt = _vertices[i - 15];

                var head_top = _vertices[i - 37];
                var head_rt = _vertices[i - 38];
                var head_lft = _vertices[i - 39];

                var center_ft = 1 / 3f * (bt_frt +head_lft + head_rt);
                var center_lft = 1 / 3f * (head_top + head_lft + bt_lft);
                var center_rt = 1 / 3f * (head_top + bt_rt + head_rt);
                var center_bt = 1 / 3f * (bt_lft + bt_rt + bt_frt);

                //bottom part
                for (int col = 0; col < 9; col++)
                    _colors32[i + col] = colors[0];

                _vertices[i++] = bt_lft; _vertices[i++] = bt_rt; _vertices[i++] = center_bt;
                _vertices[i++] = bt_rt; _vertices[i++] = bt_frt; _vertices[i++] = center_bt;
                _vertices[i++] = bt_frt; _vertices[i++] = bt_lft; _vertices[i++] = center_bt;

                // front part  - top in clockwise dir
                for (int col = 0; col < 9; col++)
                    _colors32[i + col] = colors[1];

                _vertices[i++] = head_lft; _vertices[i++] = head_rt; _vertices[i++] = center_ft;
                _vertices[i++] = head_rt; _vertices[i++] = bt_frt; _vertices[i++] = center_ft;
                _vertices[i++] = bt_frt; _vertices[i++] = head_lft; _vertices[i++] = center_ft;

                // left part  - top in clockwise dir
                for (int col = 0; col < 9; col++)
                    _colors32[i + col] = colors[2];

                _vertices[i++] = head_top; _vertices[i++] = head_lft; _vertices[i++] = center_lft;
                _vertices[i++] = head_lft; _vertices[i++] = bt_lft; _vertices[i++] = center_lft;
                _vertices[i++] = bt_lft; _vertices[i++] = head_top; _vertices[i++] = center_lft;

                // right part  - top in clockwise dir
                for(int col = 0; col <9; col++)
                    _colors32[i+col] = colors[3];
                
                _vertices[i++] = head_rt; _vertices[i++] = head_top; _vertices[i++] = center_rt;
                _vertices[i++] = head_top; _vertices[i++] = bt_rt; _vertices[i++] = center_rt;
                _vertices[i++] = bt_rt; _vertices[i++] = head_rt; _vertices[i++] = center_rt;

                // add target positions for level to level-1
                
                targetPos.Add(center_bt);                                
                targetPos.Add(center_lft);                
                targetPos.Add(center_rt);
                targetPos.Add(center_ft);
                                
            }
        }

        int[] _triangles = new int[_vertices.Length];
         
        for (int n = 0; n < _triangles.Length; n++)
        {
            /*if( n < 9)
            {
                _triangles[n] = 0;
            }
            else
            {*/
                _triangles[n] = n;
            //}
            
        }
        
        // targetpositions are collected for each level (in order bottom, front, left, right)
        targetPositions.Add(targetPos);

        var m = new Mesh
        {
            vertices = _vertices, 
            triangles = _triangles,
            colors32 = _colors32
        };

        return m;
    }
}
