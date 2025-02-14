using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WallManager : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject rightControllerReference;
    public float threshold;
    public MyPlayerController controller;
    public Material highlight_mat;
    public Material default_mat;
    public float surface;

    public bool can_spawn_vertex = false;
    public Vector3 spawn_point = Vector3.zero;

    public int depth_axis;
    public List<GameObject> rendered_vertices = new List<GameObject>();
    // Update is called once per frame

    /*
    * When controller hit the wall:
    * Turn on can_spawn_vertex, 
    * Set spawn_point attributes on the collided wall with other.contacts[0].point, depth_axis and surface. 
    */
    void OnCollisionEnter(Collision other) {
        if (other.gameObject.tag == "GameController") {
            Debug.Log("Start Collision with controller");
            can_spawn_vertex = true;
            spawn_point = other.contacts[0].point;
            switch (depth_axis)
            {
                case 0:
                    spawn_point.x = surface;
                    break;
                case 1:
                    spawn_point.y = surface;
                    break;

                case 2:
                    spawn_point.z = surface;
                    break;
                default:
                    break;
            }
        } 
    }

    //Continue setting spawn_point
    void OnCollisionStay(Collision other) {
        if (other.gameObject.tag == "GameController") {
            spawn_point = other.contacts[0].point;
            // Debug.Log(spawn_point);
            switch (depth_axis)
            {
                case 0:
                    spawn_point.x = surface;
                    break;
                case 1:
                    spawn_point.y = surface;
                    break;
                case 2:
                    spawn_point.z = surface;
                    break;
                default:
                    break;
            }
        } 
    }

    //disable can_spawn_vertex when exit
    private void OnCollisionExit(Collision other) {
        if (other.gameObject.tag == "GameController") {
            Debug.Log("End Collision with controller");
            can_spawn_vertex = false;
        } 
    }

    public Vector3 makeV(Vector3 pos)
    {
        Debug.Log("Spawning Vertex");
        GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        GameObject myVert = GameObject.Instantiate(temp);
        myVert.tag = "wall_item";
        Vector3 n_pos = pos;
        myVert.transform.position = n_pos;
        myVert.transform.localScale = new Vector3(.015f,.015f,.015f);
        MyVertex mref = myVert.AddComponent<MyVertex>();
        mref.controller = controller;
        mref.rightControllerReference = rightControllerReference;
        mref.threshold = threshold;
        mref.default_mat = default_mat;
        mref.highlight_mat = highlight_mat;
        mref.SetWall(gameObject.GetComponent<WallManager>());
        rendered_vertices.Add(myVert);
        Destroy(temp);
        return n_pos;
    }

    

}
