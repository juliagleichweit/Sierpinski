using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class createST : MonoBehaviour
{
    private STetrahedon sierp;
    private Mesh m;
    public int level;
    private bool start_lerp = false;
    private int maxLevel = 5;

    // Angular speed in radians per sec.
    public float speed = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        m = mf.mesh;

        sierp = new STetrahedon();
        UpdateMesh(sierp.CreateBaseMesh());
        Debug.Log("Create BaseMesh: " + level);
        level = -1;
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.A))
        {
            LevelUp();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            LevelDown();
        }

        if (Input.GetKeyDown(KeyCode.L)) // lerping up
        {
            start_lerp = !start_lerp;
        }

        if (start_lerp)
        {
            Fold();
        }
    }

   

    public void LevelUp()
    {
        level = Mathf.Min(level + 1, maxLevel);
        print("Level A entry : " + level);

        if (level == 0)
        {

            //  UpdateMesh(sierp.CreateMesh()); 
            ////////////////////////////////////
            //////////////////////////////////////
            /////////////////////////////////////
            //var s = sierp.Subdivide(level);
            UpdateMesh(sierp.CreateMesh());
            ////////////////////////////////////
            //////////////////////////////////////
            /////////////////////////////////////
        }
        else if (level <= maxLevel)
        {
            //Debug.Log("A key was pressed: " + level);                
            var s = sierp.Subdivide(level);
            UpdateMesh(s.CreateMesh()); 
        }    
        
    }

    public void LevelDown()
    {
        level = Mathf.Max(-1, level - 1);

        print("Level S entry: " + level);

        if (level > 0)
        {
            var s = sierp.Subdivide(level);
            UpdateMesh(s.CreateMesh());

        }
        else if (level == 0)
        {
            UpdateMesh(sierp.CreateMesh());

        }
        else if (level == -1)
        {
            UpdateMesh(sierp.CreateBaseMesh());
        }

    }

    public void FoldBaseUp()
    {         
        Vector3[] vertices = m.vertices;
        var targetPosition = sierp.getTargetsPos()[level+1][0];

        for (int i = 0; i < m.vertexCount - 3; i += 3)
        {
            // The step size is equal to speed times frame time.
            float singleStep = speed * Time.deltaTime;

            // Move the vertice towards the target by one step                   
            vertices[i] = Vector3.MoveTowards(vertices[i], targetPosition, singleStep);

            // Check if the position of the old and new level are approximately equal.
            if (Vector3.Distance(vertices[i], targetPosition) < 0.001f)
            {
                // Swap the position of the cylinder.
                start_lerp = false;
                level = 1;
            }
        }

        m.vertices = vertices; 
    }

    public void FoldBaseDown()
    {
        Vector3[] vertices = m.vertices;
        var targetPositions = sierp.getTargetsPos()[level+1];

        for (int i = 0, t = 0; t < targetPositions.Length; i += 3, t++)
        {
            // The step size is equal to speed times frame time.
            float singleStep = speed * Time.deltaTime;

            // Move the vertice towards the target by one step                   
            vertices[i] = Vector3.MoveTowards(vertices[i], targetPositions[t], singleStep);

            // Check if the position of the old and new level are approximately equal.
            if (Vector3.Distance(vertices[i], targetPositions[0]) < 0.001f)
            {
                // Swap the position of the cylinder.
                start_lerp = false;
                level = -1;
            }
        }

        m.vertices = vertices;

    }


    private void Fold()
    {
        switch(level)
        {
            case -1:
                FoldBaseUp();                
                break;
            case 0:
                FoldBaseDown();
                break;
            default:
                break;
        }
    }


    void UpdateMesh(Mesh ms)
    {
        m.Clear();
        m.vertices = ms.vertices;
        m.triangles = ms.triangles;
        //m.normals = ms.normals;
        m.colors32 = ms.colors32;
        //m.Optimize();
        m.RecalculateNormals();
    }

}
