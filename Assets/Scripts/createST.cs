using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class createST : MonoBehaviour
{
    private STetrahedon sierp;
    private Mesh m;
    public int level = 0;
    private bool start_lerp = false;
    public int maxLevel = 5;

    // Angular speed in radians per sec.
    public float speed = 1.0f;

    private List<Mesh> meshes = new List<Mesh>();
    private bool lerpUp = false;
    private bool lerpDown = false;
    private bool prepare = false;
    private bool upInProgress = false;
    private bool lvl2to1 = false;

    private int[] fdDwnTargetIdx = { 0, 0, 0, 3, 3, 3, 1, 1, 1, 2, 2, 2 };
    private int[] fdUpTargetIdx = { 15, 9, 6, 0, 9, 6, 0, 6, 15, 0, 15, 9 };

    private float lerpBy = 0f;
    // Start is called before the first frame update
    void Start()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        m = mf.mesh;

        sierp = new STetrahedon();
        var mesh = sierp.CreateBaseMesh();
        meshes.Add(mesh);
        UpdateMesh(mesh);
        level = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (start_lerp)
        {
            Fold();
        }
    }


    public void Speed(float newSpeed)
    {
        speed = newSpeed;
    }

    public void PauseResumeFolding()
    {
        if (lerpUp || lerpDown)
            start_lerp = !start_lerp;
    }

    /*
     * Implements the stop functionality and displays the base triangle
     **/    
    public void JumpToBase()
    {     
        ResetLerpBools();
        level = 0;
        Level();
    }

    public void InitFoldUp()
    {
        if (lerpUp)
            return;

        if (level < maxLevel)
        {
            start_lerp = lerpUp = prepare = upInProgress = true;

            //change direction midway, colors go back
            if (lerpDown)
                lerpBy = 1 - lerpBy;

            lerpDown = false;
        }
    }

    public void InitFoldDown()
    {
        if (lerpDown)
            return;

        if (level > 0 || upInProgress)
        {
            //change direction midway, colors go back
            if (lerpUp)
                lerpBy = 1 - lerpBy;

            start_lerp = prepare = lerpDown = true;
            lerpUp = false;
        }
    }
    /*
     * Cancel all pending folds 
     */
    private void ResetLerpBools()
    {
        start_lerp = false;
        prepare = false;
        lerpDown = false;
        upInProgress = false;
        lerpUp = false;
    }

    /*
     * Update the display mesh to the current level
     * Uses cached meshes if present, otherwise
     * it creates the level mesh
     */
    private void Level()
    {
        
        if (meshes.Count > level)
        {
            UpdateMesh(meshes[level]);
        }
        else
        {
            var mesh = sierp.Subdivide(level).CreateMesh();
            meshes.Add(mesh);
            UpdateMesh(mesh);
        }
    }

    /*
     * Subdivide mesh to a finer level until max depth is reached
     * Cancels all pending fold operations
     */
    public void LevelUp()
    {
        ResetLerpBools();

        level = Mathf.Min(level + 1, maxLevel);

        if (level <= maxLevel)
        {
            Level();
        }
    }

    /*
     * Subdivide mesh to a coarser level until base mesh is reached
     * Cancels all pending fold operations
     */
    public void LevelDown()
    {
        ResetLerpBools();

        level = Mathf.Max(0, level - 1);
        Level();
    }

    /*
     * Folds the base triangle into a tetrahedron.
     * @param silent - true if folding operation should not be shown, 
     *                 otherwise false (default)
     */
    private void FoldBaseUp(bool silent = false)
    {
        Vector3[] vertices = m.vertices;
        var targetPosition = sierp.getTargetsPos()[0][0];

        // The step size is equal to speed times frame time.
        float singleStep = (speed / (Mathf.Pow(2, level))) * Time.deltaTime;

        if (silent)
        {
            singleStep = Vector3.Distance(vertices[0], targetPosition);
            prepare = false;
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
                    ResetLerpBools();
                    level = 1;

                    Level();
                    return;
                }
            }
        }

        m.vertices = vertices;
    }

    /*
     * Folds the base tetrahedron into the base triangle .   
    */
    private void FoldBaseDown()
    {
        Vector3[] vertices = m.vertices;
        var targetPositions = sierp.getTargetsPos()[1];

        for (int i = 0, t = 0; t < targetPositions.Count; i += 3, t++)
        {
            // The step size is equal to speed times frame time.
            float singleStep = (speed / (Mathf.Pow(2, level))) * Time.deltaTime;

            // Move the vertice towards the target by one step                   
            vertices[i] = Vector3.MoveTowards(vertices[i], targetPositions[t], singleStep);

            // Check if the position of the old and new level are approximately equal.
            if (Vector3.Distance(vertices[i], targetPositions[0]) < 0.001f)
            {
                ResetLerpBools();
                level = 0;
            }
        }

        m.vertices = vertices;
    }

    /*
   * Fold the tetrahedron to the current finer level.
   * @param silent - true if folding operation should not be shown, 
   *                 otherwise false (default)
   */
    private void FoldUp(bool silent = false)
    {
        Vector3[] vertices = m.vertices;
        Color32[] colors = null;

        // The step size is equal to speed times frame time.
        float singleStep = (speed / (Mathf.Pow(2, level))) * Time.deltaTime;

        var targetPositions = sierp.getTargetsPos()[level + 2];

        if (prepare && level != 4)
            RemoveTrianglesFoldingPt();

        if (silent)
        {
            singleStep = Vector3.Distance(vertices[50], targetPositions[14]);
            upInProgress = false;
            prepare = false;
        }

        colors = m.colors32;
        float dist = 0f;

        // 4*12 4 block pyramid (0...47) + 3 (center is always the third vertex)
        // i += (50+33+1)
        for (int i = 50, t = 0; t < targetPositions.Count; i += 84, t += 16)
        {
            dist = Vector3.Distance(vertices[50], targetPositions[15]);
            FoldInner(vertices, targetPositions, i, t, fdUpTargetIdx, singleStep, colors, dist );

            if (!silent)
            {
                // Check if the position of the old and new level are approximately equal.
                if (dist < 0.001f)
                {                    
                    ResetLerpBools();
                    lerpBy = 0f;
                    level += 1;
                    Level();
                    return;
                }
            }
        }

        m.vertices = vertices;
        m.colors32 = colors;
    }

    /*
   * Folds the tetrahedron to the current coarser level.   
   */
    private void FoldDown(bool silent = false)
    {
        Vector3[] vertices = m.vertices;
        var targetPositions = sierp.getTargetsPos()[level + 1];
        Color32[] colors = null;

        float singleStep = (speed / (Mathf.Pow(2,level))) * Time.deltaTime;

        if (silent)
        {
            singleStep = Vector3.Distance(vertices[50], targetPositions[14]);
            upInProgress = false;
            prepare = false;
        }

        colors = m.colors32;
        float dist = 0f;

        // 4*12 4 block pyramid (0...47) + 3 (center is always the third vertex)
        // i += (50+33)
        for (int i = 50, t = 0; t < targetPositions.Count; i += 84, t += 4)
        {
            dist = Vector3.Distance(vertices[i], targetPositions[t]);
            FoldInner(vertices, targetPositions, i, t, fdDwnTargetIdx, singleStep, colors,dist);

            if (!silent)
            {
                // Check if the position of the old and new level are approximately equal.
                if (dist < 0.001f)
                {
                    ResetLerpBools();
                    lvl2to1 = false;
                    lerpBy = 0f;
                    Level();
                    return;
                }
            }
        }

        m.vertices = vertices;
        m.colors32 = colors;

    }

    private Color32[] colorUp2 = { Color.red, Color.green,Color.blue, Color.yellow, Color.green,Color.blue,Color.yellow,Color.blue,Color.red, Color.yellow, Color.red, Color.green };    
    private Color32[] colorUp = { Color.red, Color.blue, Color.green, Color.yellow, Color.blue, Color.green, Color.yellow, Color.green, Color.red, Color.yellow, Color.red, Color.blue };
    private Color32[] colorDown = { Color.yellow, Color.yellow, Color.yellow, Color.red, Color.red, Color.red, Color.blue, Color.blue, Color.blue, Color.green, Color.green, Color.green };
    /*
     * Fold the inner triangles to the target positions on level+/-1
     * @param vertices - mesh vertices
     * @param targets - targetPositions to move the center points of the folding triangles
     * @param idx_v - current starting index for the next block of folding triangles
     * @param idx_t - current starting index for the next block of target positions
     * @param targetIdx - array id indices giving the hop positions for the target positions
     * @param step - distance to move
     */    
    private void FoldInner(Vector3[] vertices, List<Vector3> targets, int idx_v, int idx_t, int[] targetIdx, float step, Color32[] colors, float dist)
    {
        var lerpFrom = lerpDown ? colorUp2: colorDown;
        var lerpTo = lerpDown ? colorDown : colorUp;
        lerpBy += (dist/ level) * Time.deltaTime * speed;
       
        for (int i = 0, t = 0; i <= 33; i += 3, t++)
        {
            vertices[idx_v + i] = Vector3.MoveTowards(vertices[idx_v + i], targets[idx_t + targetIdx[t]], step);

            colors[idx_v + i] =  Color32.Lerp(lerpFrom[t], lerpTo[t], lerpBy);
            colors[idx_v + i - 2] = Color32.Lerp(lerpFrom[t], lerpTo[t], lerpBy);
            colors[idx_v + i - 1] = Color32.Lerp(lerpFrom[t], lerpTo[t], lerpBy);            
        }
    }

    /*
     * Handles the folding up and down procedures
     * To allow down-folding the mesh to the desired level it has to 
     * be displayed at this level already folded up. Otherwise the mesh
     * does not have the needed vertices.
     * Then it gets folded down again. 
     */
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
            // only fold to base if level went from 1 to 0
            if (!lvl2to1 && level <= 1)
            {
                if (prepare && !upInProgress)
                {
                    level = 0;
                    Level();
                    FoldBaseUp(true);
                }                

                FoldBaseDown();
            }
            else
            {
                // we need the positions to be leveled up 
                if (prepare && !upInProgress)
                {
                    level = Mathf.Max(1, level - 1);
                    Level();

                    if (level == 1)
                        lvl2to1 = true;

                    FoldUp(true);
                }               

                FoldDown();
            }
        }
    }

    /*
    * Skip the triangles at the target folding positions. 
    * Color the folding vertices in the target positions color
    */
    public void RemoveTrianglesFoldingPt()
    {               
        prepare = false;
        int[] triangles = m.triangles;

        for (int i = 0; i < m.vertexCount; i += 1)
        {
            int check = i % 84; // (48+36);
            if (check < 48)
            {
                if (check == 9 || check == 18 || check == 27 || check == 36)
                {
                    triangles[check] = triangles[check + 1] = triangles[i + 2] = i;
                    i += 3;
                    continue;
                }
            }
        }

        m.triangles = triangles;
    }

    /*
     * Clears the display mesh und updates it with the data provided by ms
     */
    void UpdateMesh(Mesh ms)
    {        
        m.Clear();
        m.vertices = ms.vertices;
        m.triangles = ms.triangles;
        m.colors32 = ms.colors32;
    }
}
