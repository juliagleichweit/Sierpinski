using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class createST : MonoBehaviour
{
    private STetrahedon sierp;
    private Mesh m;
    public int level = 0;
    private bool start_lerp = false;
    private int maxLevel = 4;

    // Angular speed in radians per sec.
    public float speed = 1.0f;

    private List<Mesh> meshes = new List<Mesh>();
    private bool lerpUp = false;
    private bool lerpDown = false;
    private bool prepareDown = false;
    private bool upInProgress = false;
    private bool lvl2to1 = false;

    private static float distance = Mathf.PI - Mathf.Acos(1 / 3);
    // Start is called before the first frame update
    void Start()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        m = mf.mesh;

        sierp = new STetrahedon();
        var mesh = sierp.CreateBaseMesh();
        meshes.Add(mesh);
        UpdateMesh(mesh);
        Debug.Log("Create BaseMesh: " + Time.deltaTime);
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
            Debug.Log("Fold up level: " + level);
            start_lerp = true;
            lerpUp = true;
            upInProgress = true;
            lerpDown = false;
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log("Fold down level: " + level);

            if (level > 0 || upInProgress)
            {
                start_lerp = true;
                prepareDown = true;
                lerpDown = true;
                lerpUp = false;
                //LevelDown();
            }
        }

        if (start_lerp)
        {
            Fold();
        }
    }

    private void ResetLerpBools()
    {
        start_lerp = false;
        prepareDown = false;
        lerpDown = false;
        upInProgress = false;
        lerpUp = false;
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
        ResetLerpBools();
        level = Mathf.Min(level + 1, maxLevel);
        //print("Level A entry : " + level);

        if (level <= maxLevel)
        {
            Level();
        }
    }

    public void LevelDown()
    {
        ResetLerpBools();

        level = Mathf.Max(0, level - 1);

        //print("Level S entry: " + level);
        Level();
    }

    public void FoldBaseUp(bool silent = false)
    {
        Vector3[] vertices = m.vertices;
        var targetPosition = sierp.getTargetsPos()[0][0];

        // The step size is equal to speed times frame time.
        float singleStep = speed * distance *Time.deltaTime;

        if (silent)
        {
            singleStep = Vector3.Distance(vertices[0], targetPosition);
            prepareDown = false;
        }


        for (int i = 0; i < m.vertexCount - 3; i += 3)
        {
            // Move the vertice towards the target by one step                   
            vertices[i] = Vector3.MoveTowards(vertices[i], targetPosition, singleStep);

            if (!silent)
            {
                // Check if the position of the old and new level are approximately equal.
                if (Vector3.Distance(vertices[i], targetPosition) < 0.001f)
                {
                    // Swap the position of the cylinder.
                    start_lerp = false;
                    upInProgress = false;
                    level = 1;

                    Level();
                    return;
                }
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
            float singleStep = speed * distance *Time.deltaTime;


            // Move the vertice towards the target by one step                   
            vertices[i] = Vector3.MoveTowards(vertices[i], targetPositions[t], singleStep);

            // Check if the position of the old and new level are approximately equal.
            if (Vector3.Distance(vertices[i], targetPositions[0]) < 0.001f)
            {
                // Swap the position of the cylinder.
                start_lerp = false;
                upInProgress = false;
                level = 0;
            }
        }

        m.vertices = vertices;

    }

    public void FoldUp(bool silent = false)
    {
        Vector3[] vertices = m.vertices;

        // The step size is equal to speed times frame time.
        float singleStep = speed * distance *Time.deltaTime;

        var targetPositions = sierp.getTargetsPos()[level + 2];

        if (silent)
        {
            singleStep = Vector3.Distance(vertices[50], targetPositions[14]);
            upInProgress = false;
            prepareDown = false;
        }     
        

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
                    upInProgress = false;
                    //Debug.Log("Fold up fin level: " + level);
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

        float singleStep = speed * distance  *Time.deltaTime;


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
                upInProgress = false;
                lvl2to1 = false;
                Debug.Log("Fold down fin level: " + level);
                Level();
                return;
            }
        }
        m.vertices = vertices;
    }

    private void Fold()
    {

        if (lerpUp)
        {
            if (level == 0)
            {
                FoldBaseUp();
                lvl2to1 = false;
            }
            else
            {
                if (level == maxLevel)
                {
                    ResetLerpBools();
                    return;
                }

                // we need the target positions from level + 1
                if (meshes.Count < level + 2)
                    meshes.Add(sierp.Subdivide(level + 1).CreateMesh());

                // we are leveling up from  1 to 2
                // if we pause before it is finished we actually go
                // back from level 2 to 1 (since upInProgress)
                if (level == 1)
                    lvl2to1 = true;

                FoldUp();
            }
        }

        if (lerpDown)
        {            
            if (!lvl2to1 && level <= 1)
            {
                Debug.Log("lerpDown level <=1: " + level);
                Debug.Log("lerpDown level <=1 upinProgess: " + upInProgress);
                if (prepareDown && !upInProgress)
                {
                    
                    level = 0;
                    //level = Mathf.Max(0, level - 1);
                    Level();
                    FoldBaseUp(true);
                }

                FoldBaseDown();
            }
            else
            {
                Debug.Log("lerpDown levle else: " + level);
                // we need the positions to be leveled up 
                if (prepareDown && !upInProgress)
                {                   
                    level = Mathf.Max(1, level - 1);
                    Level();

                    if (level == 1)
                        lvl2to1 = true;

                    FoldUp(true);

                }

                //Debug.Log("level before fold down: " + level);
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
