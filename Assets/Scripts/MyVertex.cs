using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;

public class MyVertex : MonoBehaviour
{
    public GameObject rightControllerReference;
    public float threshold;
    bool RinSelectableRange;
    bool LinSelectableRange;
    public MyPlayerController controller;

    MeshRenderer r;
    public Material highlight_mat;
    public Material default_mat;
    public bool selected;
    Model3D model = null;
    WallManager wall = null;

    public Model3D GetModel() {
        return model;
    }

    public WallManager GetWallManager() {
        return wall;
    }

    public void SetModel(Model3D m) {
        model = m;
    }

    public void SetWall(WallManager w) {
        wall = w;
    }
    
    private void Start() 
    {
        r = GetComponent<MeshRenderer>();
        if(!selected)
            r.material = default_mat;  
    }

    // Update is called once per frame
    void Update()
    {
        bool Rtemp = RinSelectableRange;

        RinSelectableRange = Vector3.Distance(gameObject.transform.position, rightControllerReference.transform.position) < threshold; 

        //depends on previous and current frame 
        if ((RinSelectableRange && !Rtemp)) {
            highlightOn();
            controller.setHighlightedVertex(gameObject, true);
        }
        else if ((!RinSelectableRange && Rtemp)) {
            highlightOff();
            controller.setHighlightedVertex(gameObject, false);
        }
    }

    void highlightOn()
    {
        if(!selected)
            r.material = highlight_mat;
    }

    public void highlightOff()
    {
        if(!selected)
            r.material = default_mat;
    }

    public (bool,bool) getSelectable()
    {
        return (RinSelectableRange, LinSelectableRange);
    }
}
