using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Entity{
    //enum State {jump, onLand}
    public float health;
    public bool jump, onland;
    public float jumpforce;
    private float speedWalk = 1f;
    public float SpeedWalk {
        get {return speedWalk;}
        set {speedWalk = value;}
    }
    private float speedRotate = 1f;
    public float SpeedRotate {
        get {return speedRotate;}
        set {speedRotate = value;}
    }
    //This could be used to rotate vertically, if provided.
    public void bodyRotate(GameObject obj, float rotateX, float multiplier){
        //Could be consumptuous but it works for now
        obj.GetComponent<Rigidbody>().velocity.Set(0f,0f,0f);
        float offHori = rotateX*(multiplier/100);
            //Debug.Log("offHori");
            
        obj.transform.RotateAround(Vector3.up,offHori);
            //Vector3 offVerti = new Vector3(moveHead.y*mouseSensitivity,0f,);
        //float offVerti = -y_mouse*(mouseSensitivity/100);
            //Debug.Log("offVerti");
        //cam.transform.RotateAround(cam.transform.right,offVerti);
    }
}