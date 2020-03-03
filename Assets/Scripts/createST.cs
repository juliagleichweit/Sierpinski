using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class createST : MonoBehaviour
{
    private STetrahedon sierp;
    private Mesh m;
    public int level = 0;
    private bool start_lerp = false;
    private int maxLevel = 3;

    // Angular speed in radians per sec.
    public float speed = 0.5f;

    private List<Mesh> meshes = new List<Mesh>();
    private bool lerp_up;
    private bool lerp_down;
    private bool prepare_down;

    // Start is called before the first frame update
    void Start()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        m = mf.mesh;

        sierp = new STetrahedon();
        var mesh = sierp.CreateBaseMesh();
        meshes.Add(mesh);
        UpdateMesh(mesh);
        Debug.Log("Create BaseMesh: " + level++);
        level = 0;
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
            /*if(!start_lerp)
            {
                lerp_up = lerp_down = false;
            }*/
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            start_lerp = true;
            lerp_up = true;
            lerp_down = false;
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            start_lerp = true;
            prepare_down = true;
            lerp_down = true;
            lerp_up = false;
        }

        if (start_lerp)
        {
            Fold();
        }
    }

    private void Level()
    {
        //Debug.Log("A key was pressed: " + level);    
        if (meshes.Count > level)
        {
            UpdateMesh(meshes[level]);
        }
        else
        {
            var s = sierp.Subdivide(level);
            var mesh = s.CreateMesh();
            meshes.Add(mesh);
            UpdateMesh(mesh);
        }
    }

    public void LevelUp()
    {
        level = Mathf.Min(level + 1, maxLevel);
        //print("Level A entry : " + level);

        if (level <= maxLevel)
        {
            Level();
        }
    }

    public void LevelDown()
    {
        level = Mathf.Max(0, level - 1);

        //print("Level S entry: " + level);
        Level();
    }

    public void FoldBaseUp()
    {
        Vector3[] vertices = m.vertices;
        var targetPosition = sierp.getTargetsPos()[0][0];

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

                Level();
                return;
            }
        }

        m.vertices = vertices;
    }

    public void FoldBaseDown()
    {
        Vector3[] vertices = m.vertices;
        var targetPositions = sierp.getTargetsPos()[1];

        for (int i = 0, t = 0; t < targetPositions.Count; i += 3, t++)
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
                level = 0;
            }
        }

        m.vertices = vertices;

    }

    public void FoldUp(bool silent = false)
    {
        Vector3[] vertices = m.vertices;        

        // The step size is equal to speed times frame time.
        float singleStep = speed * Time.deltaTime;

        if (silent)
        {
            singleStep = 1.0f;
            prepare_down = false;
            Debug.Log("prepare_Down : false");
        }

        var targetPositions = sierp.getTargetsPos()[level + 2];              

        // 4*12 4 block pyramid (0...47) + 3 (center is always the third vertex)
        // i += (50+33+1)
        for (int i = 50, t = 0; t < targetPositions.Count; i += 84, t += 16)
        {
            // Move the vertice towards the target by one step   
            //bottom move to higher level
            vertices[i] = Vector3.MoveTowards(vertices[i], targetPositions[t + 14], singleStep);
            vertices[i + 3] = Vector3.MoveTowards(vertices[i + 3], targetPositions[t + 11], singleStep);
            vertices[i + 6] = Vector3.MoveTowards(vertices[i + 6], targetPositions[t + 7], singleStep);

            //front move to higher level
            vertices[i + 9] = Vector3.MoveTowards(vertices[i + 9], targetPositions[t], singleStep);
            vertices[i + 12] = Vector3.MoveTowards(vertices[i + 12], targetPositions[t + 11], singleStep);
            vertices[i + 15] = Vector3.MoveTowards(vertices[i + 15], targetPositions[t + 7], singleStep);

            //left move to higher level
            vertices[i + 18] = Vector3.MoveTowards(vertices[i + 18], targetPositions[t], singleStep);
            vertices[i + 21] = Vector3.MoveTowards(vertices[i + 21], targetPositions[t + 7], singleStep);
            vertices[i + 24] = Vector3.MoveTowards(vertices[i + 24], targetPositions[t + 14], singleStep);

            //right move to higher level
            vertices[i + 27] = Vector3.MoveTowards(vertices[i + 27], targetPositions[t], singleStep);
            vertices[i + 30] = Vector3.MoveTowards(vertices[i + 30], targetPositions[t + 14], singleStep);
            vertices[i + 33] = Vector3.MoveTowards(vertices[i + 33], targetPositions[t + 11], singleStep);

            if (!silent)
            {
                // Check if the position of the old and new level are approximately equal.
                if (Vector3.Distance(vertices[i], targetPositions[t + 14]) < 0.001f)
                {
                    // Swap the position of the cylinder.
                    start_lerp = false;
                    level += 1;

                    Level();
                    return;
                }
            }
        }

        m.vertices = vertices;
    }

    public void FoldDown()
    {
        Vector3[] vertices = m.vertices;
        var targetPositions = sierp.getTargetsPos()[level + 1];

        float singleStep = speed * Time.deltaTime;

        // 4*12 4 block pyramid (0...47) + 3 (center is always the third vertex)
        // i += (50+33)
        for (int i = 50, t = 0; t < targetPositions.Count; i += 84, t += 4)
        {

            // Move the vertice towards the target by one step   
            //bottom move to higher level
            vertices[i] = Vector3.MoveTowards(vertices[i], targetPositions[t], singleStep);
            vertices[i + 3] = Vector3.MoveTowards(vertices[i + 3], targetPositions[t], singleStep);
            vertices[i + 6] = Vector3.MoveTowards(vertices[i + 6], targetPositions[t], singleStep);

            //front move to higher level
            vertices[i + 9] = Vector3.MoveTowards(vertices[i + 9], targetPositions[t + 1], singleStep);
            vertices[i + 12] = Vector3.MoveTowards(vertices[i + 12], targetPositions[t + 1], singleStep);
            vertices[i + 15] = Vector3.MoveTowards(vertices[i + 15], targetPositions[t + 1], singleStep);

            //left move to higher level
            vertices[i + 18] = Vector3.MoveTowards(vertices[i + 18], targetPositions[t + 2], singleStep);
            vertices[i + 21] = Vector3.MoveTowards(vertices[i + 21], targetPositions[t + 2], singleStep);
            vertices[i + 24] = Vector3.MoveTowards(vertices[i + 24], targetPositions[t + 2], singleStep);

            //right move to higher level
            vertices[i + 27] = Vector3.MoveTowards(vertices[i + 27], targetPositions[t + 3], singleStep);
            vertices[i + 30] = Vector3.MoveTowards(vertices[i + 30], targetPositions[t + 3], singleStep);
            vertices[i + 33] = Vector3.MoveTowards(vertices[i + 33], targetPositions[t + 3], singleStep);

            // Check if the position of the old and new level are approximately equal.
            if (Vector3.Distance(vertices[i], targetPositions[t]) < 0.001f)
            {
                // Swap the position of the cylinder.
                start_lerp = false;
                //level += 1;
                Level();
                return;
            }
        }
        m.vertices = vertices;
    }

    private void Fold()
    {
       
        if (lerp_up)
        {
            if (level == 0)
            {
                FoldBaseUp();
            }
            else
            {
                // we need the target positions from level + 1
                if (meshes.Count < level + 2)
                    meshes.Add(sierp.Subdivide(level + 1).CreateMesh());
                
                FoldUp();
            }
        }

        if (lerp_down)
        {            
            Debug.Log("Fold level: " + level);
            if (level == 0)
            {
                if (prepare_down)
                {
                    // we need the target positions from level + 1
                    /*if (meshes.Count < level + 2)
                        meshes.Add(sierp.Subdivide(level + 1).CreateMesh());
                        */
                    //level -= 1;
                    level = Mathf.Max(0, level - 1);
                    //Level();
                    //FoldBaseUp(true);
                }

                FoldBaseDown();
            }
            else
            {
                
                // we need the positions to be leveled up 
                if (prepare_down)
                {
                    Debug.Log("level before silent fold up: " + level);
                    level = Mathf.Max(0, level - 1);
                    Level();
                    FoldUp(true);
                    Debug.Log("level after silent fold up: " + level);
                }

                Debug.Log("level before fold down: " + level);
                FoldDown();
            }
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
        //m.RecalculateNormals();


    }

}
