using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class MyPlayerController : MonoBehaviour
{

    public GameObject model = null;
    GrabStatusEnum GrabStatus;
    bool selecting = false;

    bool grabbing;
    GameObject highlighted_vertex = null;
    bool currentlyScaling;

    public WallManager[] walls;
    enum GrabStatusEnum
    {
        Right,
        Left,
        Both,
        None
    }

    Vector3 GrabStartScale;
    float GrabStartDistance;
    public GameObject ControllerLeft;
    public GameObject ControllerRight;
    LineRenderer lr;
    LineRenderer lrs;

    Vector3 lineStart;
    Quaternion GrabStartRotation;
    Vector3 GrabStartDirection;
    Vector3 GrabStartPosition;
    Vector3 GrabStartCenter;
    GameObject active_line;
    GameObject s_line;
    public Material default_line;
    public Material projection_line;
    public Material wall_line;
    public Material projection_line_p;
    public Material wall_line_p;
    public Material selected_v;
    public Material dotted_line;
    public GameObject ton;
    public GameObject toff;
    GameObject active_v;
    bool rendered_proj_lines = true;
    bool dotted = false;
    public bool freeze = false;

    public InputActionReference leftGrab;
    public InputActionReference rightGrab;

    // public InputActionReference leftTrigger;
    public InputActionReference rightTrigger;
    public InputActionReference rightPrimary;
    public List<GameObject> chain = new List<GameObject>();

    private void Start() {
        leftGrab.action.started += StartGrabGraph;
        rightGrab.action.started += StartGrabGraph;
        leftGrab.action.canceled += EndGrabGraph;
        rightGrab.action.canceled += EndGrabGraph;        
        leftGrab.action.performed += GrabGraph;
        rightGrab.action.performed += GrabGraph;
        // leftTrigger.action.started += trySelect;
        rightTrigger.action.started += trySelect;
        // leftTrigger.action.canceled += tryRelease;
        rightTrigger.action.canceled += tryRelease;
        rightPrimary.action.started += tryChain;
    }

    public void setHighlightedVertex(GameObject o, bool b) {
        if (!b) {
            highlighted_vertex = null;
        }
        else {
            highlighted_vertex = o;
        }
    }

    public void tryChain(InputAction.CallbackContext ctx) {
        if (ctx.action.actionMap.name == "XRI RightHand" && highlighted_vertex)
        {
            chain.Add(highlighted_vertex);
            highlighted_vertex.GetComponent<MyVertex>().selected = true;
            highlighted_vertex.GetComponent<MeshRenderer>().material = selected_v;
            Debug.Log("Added Vertex: " + highlighted_vertex.transform.position);
        }
    }

    private void trySelect(InputAction.CallbackContext ctx) {
        //check if we can select a vertex
        //start the line
        if (ctx.action.actionMap.name == "XRI RightHand" && highlighted_vertex)
        {
            //create the line start
            active_line = new GameObject("line");
            active_line.transform.parent = highlighted_vertex.transform;
            active_v = highlighted_vertex;
            active_line.transform.localPosition = Vector3.zero;

            s_line = new GameObject("s_line");
            s_line.transform.parent = highlighted_vertex.transform;
            s_line.transform.localPosition = Vector3.zero;
            
            lr = active_line.AddComponent<LineRenderer>();
            lrs = s_line.AddComponent<LineRenderer>();

            if (highlighted_vertex.GetComponent<MyVertex>().GetModel() != null) {
                lr.material = projection_line;
                lr.startWidth = .01f;
                lr.endWidth = .01f;
                lrs.material = projection_line_p;
                lrs.startWidth = .01f;
                lrs.endWidth = .01f;      
            }
            else if (highlighted_vertex.GetComponent<MyVertex>().GetWallManager() != null) {
                lr.material = wall_line;
                lr.startWidth = .01f;
                lr.endWidth = .01f;
                lrs.material = wall_line_p; 
                lrs.startWidth = .01f;
                lrs.endWidth = .01f;       
            }
            else {
                lr.material = default_line;
                lr.startWidth = .01f;
                lr.endWidth = .01f;   
                lrs.material = default_line;
                lrs.startWidth = .01f;
                lrs.endWidth = .01f;              
            }

            Vector3[] positions = new Vector3[2];
            lineStart = active_line.transform.position;
            positions[0] = lineStart;
            positions[1] = ControllerRight.transform.position;
            lr.positionCount = positions.Length;
            lr.SetPositions(positions);

            Vector3[] positions2 = new Vector3[2];
            lineStart = active_line.transform.position;
            positions2[0] = lineStart;
            positions2[1] = Snap(ControllerRight.transform.position, lineStart);
            lrs.positionCount = positions2.Length;
            lrs.SetPositions(positions2);
            selecting = true;

        }

    }

    public void ToggleDottedOn() {
        dotted = true;
        Debug.Log("Toggling Dotted, is now: " + dotted);
        ton.SetActive(false);
        toff.SetActive(true);
    }

    public void ToggleDottedOff() {
        dotted = false;
        Debug.Log("Toggling Dotted, is now: " + dotted);
        toff.SetActive(false);
        ton.SetActive(true);
    }

    public void ToggleDotted(bool b) {
        dotted = b;
        Debug.Log("Toggling Dotted, is now: " + dotted);
        // toff.SetActive(false);
        // ton.SetActive(true);
    }

    private void tryRelease(InputAction.CallbackContext ctx) {
        //check if line is close enough to wall
        //check if line is close enough to snap
        //generate vertex at line end
        if (ctx.action.actionMap.name == "XRI RightHand" && selecting)
        {
            bool line_attached = false;
            //end the line
            foreach (WallManager wall in walls)
            {
                if (wall.can_spawn_vertex) {
                    Vector3 t_pos;
                    if (highlighted_vertex) 
                        t_pos = Snap(highlighted_vertex.transform.position, lineStart);
                    else
                        t_pos = Snap(wall.spawn_point, lineStart);
                    Vector3[] positions = new Vector3[2];
                    positions[0] = lineStart;
                    positions[1] = wall.makeV(t_pos);
                    WallManager parent_wall = active_v.GetComponent<MyVertex>().GetWallManager();
                    Model3D parent_model = active_v.GetComponent<MyVertex>().GetModel();
                    if(parent_model)
                    {
                        lr.tag = "p_line";
                    }
                    else if (parent_wall) {
                        lr.tag = "w_line";
                    }
                    lr.positionCount = positions.Length;
                    lr.SetPositions(positions);
                    if(highlighted_vertex) {
                        if (dotted && highlighted_vertex.GetComponent<MyVertex>().GetWallManager() != null)
                        {
                            Debug.Log("Drawing Dotted");
                            lr.material = dotted_line;
                            lr.textureMode = LineTextureMode.Tile;
                            float width =  lr.startWidth;
                            lr.material.SetTextureScale("_MainTex", new Vector2(0.5f/width, 1.0f));
                        }
                    }
                    if (active_line) {
                        LineManager LM = active_line.AddComponent<LineManager>();
                        line_attached = true;
                    }
                    freeze = true;
                    break;
                }
            }
            Destroy(s_line);
            if (!line_attached)
                Destroy(active_line);
            selecting = false;
        }
    }

    private void StartGrabGraph(InputAction.CallbackContext ctx) {
        // TeleportRay.SetActive(false);
        // UIObject.SetActive(false);
        if (freeze)
            return;
        if (ctx.action.actionMap.name == "XRI LeftHand" && GrabStatus == GrabStatusEnum.Right) {
            GrabStatus = GrabStatusEnum.Both;
        } else if (ctx.action.actionMap.name == "XRI RightHand" && GrabStatus == GrabStatusEnum.Left) {
            GrabStatus = GrabStatusEnum.Both;
        } else {
            GrabStatus = (ctx.action.actionMap.name == "XRI LeftHand") ? GrabStatusEnum.Left : GrabStatusEnum.Right;
        }
    }

    /*
    *   Clean up GrabStatus variable when user stops holding grip button
    */
    private void EndGrabGraph(InputAction.CallbackContext ctx) {
        // TeleportRay.SetActive(true);
        // UIObject.SetActive(true);
        if (freeze)
            return;
        if (GrabStatus == GrabStatusEnum.Both && ctx.action.actionMap.name == "XRI LeftHand") {
            GrabStatus = GrabStatusEnum.Right;
        } else if (GrabStatus == GrabStatusEnum.Both) {
            GrabStatus = GrabStatusEnum.Left;
        } else {
            GrabStatus = GrabStatusEnum.None;
        }
        
        grabbing = false;
        currentlyScaling = false;
        Vector3 rot = Vector3.zero;
        if (model) {
            model.transform.SetParent(null);
        //transform snapping
            rot = model.transform.rotation.eulerAngles;
        }
        if(rot.x % 90 > 45) {
             rot.x = rot.x + 90 - (rot.x % 90);
        }
        else {
             rot.x = rot.x - (rot.x % 90);
        }
        if(rot.y % 90 > 45) {
             rot.y = rot.y + 90 - (rot.y % 90);
        }
        else {
             rot.y = rot.y - (rot.y % 90);
        }
        if(rot.z % 90 > 45) {
             rot.z = rot.z + 90 - (rot.z % 90);
        }
        else {
             rot.z = rot.z - (rot.z % 90);
        }
        if(model)      
            model.transform.eulerAngles = rot;
    }

    private void GrabGraph(InputAction.CallbackContext ctx) {
        if (freeze)
            return;
        if (!grabbing && GrabStatus != GrabStatusEnum.Both && model) {
            grabbing = true;
            model.transform.SetParent((GrabStatus == GrabStatusEnum.Left) ? ControllerLeft.transform : ControllerRight.transform);
        } else if (GrabStatus == GrabStatusEnum.Both && !currentlyScaling && model) {
            model.transform.SetParent(null);
            GrabStartScale = model.transform.localScale;
            GrabStartDistance = (ControllerLeft.transform.position - ControllerRight.transform.position).magnitude;
            GrabStartRotation = model.transform.localRotation;
            GrabStartDirection = (ControllerLeft.transform.position - ControllerRight.transform.position).normalized;
            GrabStartPosition = (ControllerLeft.transform.position + ControllerRight.transform.position) / 2;
            GrabStartCenter = model.transform.position;
            currentlyScaling = true;
        }
    }

    public void Update()
    {
        if (GrabStatus == GrabStatusEnum.Both && currentlyScaling && model) {
            Vector3 midpoint = (ControllerLeft.transform.position + ControllerRight.transform.position) / 2;
            Vector3 betweenControllers = ControllerLeft.transform.position - ControllerRight.transform.position;

            model.transform.position = GrabStartCenter + midpoint - GrabStartPosition;
            float newScale = betweenControllers.magnitude / GrabStartDistance;
            model.transform.localScale = GrabStartScale * newScale;

            Quaternion rot = Quaternion.FromToRotation(GrabStartDirection, betweenControllers.normalized);
            model.transform.rotation = rot * GrabStartRotation;
        }

        if (selecting)
        {
            //update the line
            Vector3[] positions = new Vector3[2];
            positions[0] = lineStart;
            positions[1] = ControllerRight.transform.position;
            lr.positionCount = positions.Length;
            lr.SetPositions(positions);

            Vector3[] positions2 = new Vector3[2];
            positions2[0] = lineStart;
            positions2[1] = Snap(ControllerRight.transform.position, lineStart);
            lrs.positionCount = positions2.Length;
            lrs.SetPositions(positions2);
        } 

    }

    private Vector3 Snap(Vector3 t, Vector3 s) {
        float x_diff = Math.Abs(t.x - s.x);
        float y_diff = Math.Abs(t.y - s.y);
        float z_diff = Math.Abs(t.z - s.z);

        if (z_diff > x_diff && z_diff > y_diff) {
            return new Vector3((float)Math.Round(s.x * 100f) /100f, (float)Math.Round(s.y * 100f) /100f, (float)Math.Round(t.z * 100f) /100f);
        }
        else if (y_diff > x_diff && y_diff > z_diff) {
            return new Vector3((float)Math.Round(s.x * 100f) /100f, (float)Math.Round(t.y * 100f) /100f, (float)Math.Round(s.z * 100f) /100f);
        }
        else if (x_diff > y_diff && x_diff > z_diff) {
            return new Vector3((float)Math.Round(t.x * 100f) /100f, (float)Math.Round(s.y * 100f) /100f, (float)Math.Round(s.z * 100f) /100f);
        }
        return t;

    }

    public void Draw() {

        WallManager parent_wall = chain[0].GetComponent<MyVertex>().GetWallManager();
        Model3D parent_model = chain[0].GetComponent<MyVertex>().GetModel();
        //Vector3[] pos = new Vector3[chain.Count + 1];
        Vector3[] pos = new Vector3[chain.Count];
        Debug.Log("Chain count: " + chain.Count);
        if (chain.Count == 1)
            return;
        for(int i = 0; i < chain.Count; i++)
        {
            pos[i] = chain[i].transform.position;
            Debug.Log("Adding Vertex to pos: " + chain[i].transform.position);
        }
        //pos[chain.Count] = chain[0].transform.position;
        LineRenderer lr;
        GameObject go = new GameObject("LR holder");
        lr = go.gameObject.AddComponent<LineRenderer>();
        if(parent_model)
        {
            go.transform.parent = parent_model.transform;
            lr.material = dotted ? dotted_line : projection_line;
            go.tag = "p_line";
        }
        else if (parent_wall) {
            go.transform.parent = parent_wall.transform;
            lr.material = dotted ? dotted_line : wall_line;
            go.tag = "w_line";
        }
        else {
            go.transform.parent = gameObject.transform;
            lr.material = dotted ? dotted_line : default_line;
            go.tag = "p_line";
        }
        lr.startWidth = .01f;
        lr.endWidth = .01f;
        lr.positionCount = pos.Length;
        Debug.Log("Positions: " + String.Join(", ", new List<Vector3>(pos).ConvertAll(pos => pos.ToString()).ToArray()));
        lr.SetPositions(pos);
        if (dotted)
            {
                Debug.Log("Drawing Dotted");
                lr.textureMode = LineTextureMode.Tile;
                float width =  lr.startWidth;
                lr.material.SetTextureScale("_MainTex", new Vector2(0.5f/width, 1.0f));
            }
        foreach (GameObject item in chain)
        {
            item.GetComponent<MyVertex>().selected = false;
            item.GetComponent<MyVertex>().highlightOff();
        }
        chain = new List<GameObject>();
    }

    public void DrawCurved() {
        if (chain.Count == 1)
            return;
        for(int i = 0; i < chain.Count; i++)
        {
            if (i == chain.Count - 1)
            {

            }
        }
        foreach (GameObject item in chain)
        {
            item.GetComponent<MyVertex>().selected = false;
            item.GetComponent<MyVertex>().highlightOff();
        }
        chain = new List<GameObject>();
    }

    public void Clear() {
        foreach (GameObject item in chain)
        {
            item.GetComponent<MyVertex>().selected = false;
            item.GetComponent<MyVertex>().highlightOff();
        }
        chain = new List<GameObject>();
    }

    public void TogglePLines() {
        GameObject[] p_go =  GameObject.FindGameObjectsWithTag ("p_line");

        foreach (var o in p_go)
        {
            Debug.Log("Unrendering lines");
            o.GetComponent<LineRenderer>().enabled = !rendered_proj_lines;
        }
        rendered_proj_lines = !rendered_proj_lines;
    }

    public void ToggleWLines() {
        GameObject[] p_go =  GameObject.FindGameObjectsWithTag ("w_line");

        foreach (var o in p_go)
        {
            Debug.Log("Unrendering lines");
            o.GetComponent<LineRenderer>().enabled = !rendered_proj_lines;
        }
        rendered_proj_lines = !rendered_proj_lines;
    }

    public void attach_to_planes()
    {
        GameObject[] gos = GameObject.FindGameObjectsWithTag("wall_item");
        foreach (var item in gos)
        {
            if (item.transform.position.x == -0.32f)
            {
                //attach to right plane
                item.transform.parent = walls[0].transform;
            }
            else if (item.transform.position.y == 2.01f)
            {
                //attach to top plane
                item.transform.parent = walls[1].transform;
            }
            else if (item.transform.position.z == 0.09f)
            {
                //attach to front plane
                item.transform.parent = walls[2].transform;
            }
        }
    }

    void updateLines()
    {
        //foreach line on the wall, rotate end positions.
        GameObject[] gos = GameObject.FindGameObjectsWithTag("w_lines");
    }

    public void Unfold()
    {
        GameObject[] go = GameObject.FindGameObjectsWithTag("foldingcube");
        if(go.Length > 0) {
            go[0].GetComponent<AniController>().OnUnfold();
        }
    }
}
