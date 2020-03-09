using System.Collections;
using System.Collections.Generic;
using UnityEngine; 

public class createST : MonoBehaviour
{
    private STetrahedron sierp;
    private Mesh m;
    public int level = 0;
    public int maxLevel = 8;

    // Angular speed in radians per sec.
    public float speed = 0.6f;

    private List<Mesh> meshes = new List<Mesh>();

    // booleans to manage folding 
    private bool start_lerp = false;
    private bool lerpUp = false;
    private bool lerpDown = false;
    private bool prepare = false;
    private bool upInProgress = false;
    private bool lvl2to1 = false;
    public bool animateTillEnd = false;

    // indices for the targetpositions
    private int[] fdDwnTargetIdx = { 3, 3, 3, 1, 1, 1, 2, 2, 2, 0, 0, 0 };
    private int[] fdUpTargetIdx = { 0, 9, 6, 0, 6, 15, 0, 15, 9, 15, 9, 6 };

    // for intepolating the colors during folding
    private float lerpBy = 0f;

    // Start is called before the first frame update
    void Start()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        m = mf.mesh;

        sierp = new STetrahedron();
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

    /*
     * Change if folding animation shall be shown until maxLevel. 
     * Default false. 
     */
    public void ChangeAnimateTillEnd() {
        animateTillEnd = !animateTillEnd;
    }

    /*
     * Sets the new folding speed
     */
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
     * Implements the stop functionality and displays the base tetrahedron (4 triangles).
     **/
    public void JumpToBase()
    {
        ResetLerpBools();
        level = 0;
        Level();
    }

    /*
   * Prepare folding operation to level+1.
   * Subsequent folding up calls are ignored.
   */
    public void InitFoldUp()
    {
        if (lerpUp)
            return;

        if (level < maxLevel)
        {
            start_lerp = lerpUp = upInProgress = true;

            //change direction midway, colors go back
            if (lerpDown)
                lerpBy = 1 - lerpBy;

            lerpDown = false;
        }
    }

    /*
     * Prepare folding operation to level-1.
     * Subsequent folding down calls are ignored.
     */
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
     * Cannot go further than level 6 --> Untiy cuts vertices (reason unknow, no camera clipping or similar)
     * Therefore we have to simulate level 6 by folding level 5 up and displaying this 
     */
    private void SimulateLevel6()
    {
        level = 5;
        UpdateMesh(meshes[5]);
        FoldUp(true);
        level = 6;
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
     * Subdivide mesh to a coarser level until base mesh is reached.
     * Cancels all pending fold operations.
     */
    public void LevelDown()
    {
        if (!lerpDown)
            level = Mathf.Max(0, level - 1);

        ResetLerpBools();
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

                    if(animateTillEnd)
                        InitFoldUp();
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

        if (silent)
        {
            singleStep = Vector3.Distance(vertices[11], targetPositions[0]);
            lerpBy = 1;
            upInProgress = false;
            prepare = false;
        }

        colors = m.colors32;
        float dist = 0f;

        // 4*12 4 block pyramid (0...47) + 3 (center is always the third vertex)
        // i += (50+33+1)
        for (int i = 11, t = 0; t < targetPositions.Count; i += 72, t += 16) // per pyramide
        {
            dist = Vector3.Distance(vertices[11], targetPositions[0]);
            FoldInner(vertices, targetPositions, i, t, fdUpTargetIdx, singleStep, colors, dist);

            if (!silent)
            {
                // Check if the position of the old and new level are approximately equal.
                if (dist < 0.001f)
                {
                    ResetLerpBools();
                    lerpBy = 0f;
                    level += 1; 
                    Level();

                    if(animateTillEnd)
                        InitFoldUp();
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
    private void FoldDown()
    {
        Vector3[] vertices = m.vertices;
        var targetPositions = sierp.getTargetsPos()[level + 1];
        Color32[] colors = null;

        float singleStep = (speed / (Mathf.Pow(2, level))) * Time.deltaTime;

        colors = m.colors32;
        float dist = 0f;

        // 4*12 4 block pyramid (0...47) + 3 (center is always the third vertex)
        // i += (50+33)
        for (int i = 11, t = 0; t < targetPositions.Count; i += 72, t += 4)
        {
            dist = Vector3.Distance(vertices[11], targetPositions[3]);
            FoldInner(vertices, targetPositions, i, t, fdDwnTargetIdx, singleStep, colors, dist);

            // Check if the position of the old and new level are approximately equal.
            if (dist < 0.001f)
            {
                ResetLerpBools();
                lvl2to1 = false;
                lerpBy = 0f;
                Level();

                if(animateTillEnd)
                    InitFoldDown();
                return;
            }
        }

        m.vertices = vertices;
        m.colors32 = colors;

    }

    // color transition between folding up and down
    private Color32[] colorUp = { Color.yellow, Color.blue, Color.green, Color.yellow, Color.green, Color.red, Color.yellow, Color.red, Color.blue, Color.red, Color.blue, Color.green };
    private Color32[] colorDown = { Color.red, Color.red, Color.red, Color.blue, Color.blue, Color.blue, Color.green, Color.green, Color.green, Color.yellow, Color.yellow, Color.yellow };
    
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
        var lerpFrom = lerpDown ? colorUp : colorDown;
        var lerpTo = lerpDown ? colorDown : colorUp;
        lerpBy += (dist / level) * Time.deltaTime * speed;

        for (int i = 0, t = 0, col = 0; i <= 65; i += 18, t += 3)
        {
            vertices[idx_v + i] = Vector3.MoveTowards(vertices[idx_v + i], targets[idx_t + targetIdx[t]], step);
            vertices[idx_v + i + 3] = Vector3.MoveTowards(vertices[idx_v + i + 3], targets[idx_t + targetIdx[t + 1]], step);
            vertices[idx_v + i + 6] = Vector3.MoveTowards(vertices[idx_v + i + 6], targets[idx_t + targetIdx[t + 2]], step);

            //top inner triangle
            colors[idx_v + i] = Color32.Lerp(lerpFrom[col], lerpTo[col], lerpBy);
            colors[idx_v + i - 2] = Color32.Lerp(lerpFrom[col], lerpTo[col], lerpBy);
            colors[idx_v + i - 1] = Color32.Lerp(lerpFrom[col], lerpTo[col++], lerpBy);

            //right inner triangle
            colors[idx_v + i + 3] = Color32.Lerp(lerpFrom[col], lerpTo[col], lerpBy);
            colors[idx_v + i + 2] = Color32.Lerp(lerpFrom[col], lerpTo[col], lerpBy);
            colors[idx_v + i + 1] = Color32.Lerp(lerpFrom[col], lerpTo[col++], lerpBy);

            //left inner triangle
            colors[idx_v + i + 6] = Color32.Lerp(lerpFrom[col], lerpTo[col], lerpBy);
            colors[idx_v + i + 5] = Color32.Lerp(lerpFrom[col], lerpTo[col], lerpBy);
            colors[idx_v + i + 4] = Color32.Lerp(lerpFrom[col], lerpTo[col++], lerpBy);
        }
    }

    /*
     * Handles the folding up and down procedures. 
     * To allow down-folding to level-1 the mesh has to be displayed at level already folded up. 
     * Otherwise the mesh does not have the needed vertices.
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
                if (prepare && !upInProgress) // we need the positions to be leveled up 
                {
                    level = Mathf.Max(1, level - 1);
                    Level();

                    if (level == 1)
                        lvl2to1 = true;

                    FoldUp(true);
                    lerpBy = 0f;
                }

                FoldDown();
            }
        }
    }


    /*
     * Clears the display mesh und updates it with the data provided by ms
     */
    void UpdateMesh(Mesh ms)
    {
        m.Clear();
        m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;  // to support meshes over 65k vertices       
        m.vertices = ms.vertices;
        m.colors32 = ms.colors32;
        m.SetIndices(ms.triangles, MeshTopology.Triangles, 0);
        //m.RecalculateNormals();
    }
}
