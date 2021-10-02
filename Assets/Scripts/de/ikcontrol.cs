using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ikcontrol : MonoBehaviour
{
    private int n;
    //bone gameobjects could be scanned for, but for now we'll add them manually.
    private Transform rootRef;
    private Quaternion rootRefRot, rotationPlaceholder;
    private Vector3 rootRefPos;
    [SerializeField]private Vector3[] bonePosition;
    [SerializeField]private Quaternion[] boneRotation;
    private float[] boneLength;
    private Vector3[] testbonePosition;
    private float boneLengthMax;
    [SerializeField]private Transform[] realBone;
    [SerializeField]private Transform realTarget;
    private Vector3 targetPosition;
    [Range(10,100)][SerializeField]private int iterations;
    [Range(0f,1f)][SerializeField]private float SnapBackStrength;
    [Header("DEBUGGING ONLY")]
    [Range(0f,360f)]public float a,b,c;
    [SerializeField] private bool dStretchOnly = false;
    private Vector3[] lastDirection;
    
    private Quaternion[] lastRotation;
    
    private float currentDistance;
    //private Quaternion rotation;
    private Transform[] rotationtransform;
    private Quaternion targetRotation, lastTargetRotation;
    //private Vector3 direction;
    
    private void SetupIK(){
        
    }
    private void initIK(){
        boneLengthMax = 0f;
        bonePosition = new Vector3[realBone.Length];
        boneRotation = new Quaternion[realBone.Length];
        testbonePosition = new Vector3[realBone.Length];
        lastRotation = new Quaternion[realBone.Length];
        //bone.SetValue(new Vector3(0f,0f,0f),0);

        //helpful references
        rootRef = realBone[0];

        rootRefRot = rootRef.rotation;
        rotationPlaceholder = Quaternion.identity;

        rootRefPos = rootRef.position;
        targetPosition = WorldToRootPos(realTarget.position);
        //rootspace bone
        for (int i = 0; i< realBone.Length;i++){ 
            //if (root==null) root = realBone[0];
            ////Debug.Log(root.position);
            ////Debug.Log(i);
            //Debug.Log("0/ realBone"+"["+i+"]"+" before changes is: "+bonePosition[i]);
            Vector3 placeholder = realBone[i].position;
            bonePosition[i] = WorldToRootPos(realBone[i].position);
            //bonePosition[i] = placeholder;
            
            if (realBone[i].position != placeholder) throw new UnityException("realBones are interfered too early.");
            //bonePosition[i].Set(position.x,position.y,position.z);
            if (bonePosition[i] == realBone[i].position && rootRefPos != Vector3.zero) throw new UnityException("WorldToRoot translation failed");
            //Debug.Log("1/ realBone"+"["+i+"]"+" before changes is: "+bonePosition[i]);
            lastRotation[i] = realBone[i].rotation;
            //lastTargetRotation = WorldToRootRot(realTarget.rotation);
            //boneRotation[i].rotation = 
//            testbone.position[i] = bone[i];
            testbonePosition[i] = bonePosition[i];
            
            //Debug.Log("bone at "+i+" is "+bonePosition[i]);
        }
        
        //rootspace target
        
        

        //Debug.Log("target position is "+targetPosition);

        ////Debug.Log(Quaternion.Inverse(root.rotation));
        if (bonePosition.Length != realBone.Length) throw new UnityException("Bone lengths are not aligning!");
        boneLength = new float[realBone.Length];
        lastDirection = new Vector3[realBone.Length];
        currentDistance = new float();
        //lengths, directions, etc
        for (int i=0;i<realBone.Length;i++){
            if (i==realBone.Length-1) {
                lastDirection[i] = targetPosition - bonePosition[i];
            }
            else {
                lastDirection[i] = bonePosition[i+1] - bonePosition[i];
            }
            boneLength[i] = lastDirection[i].magnitude;
            //Debug.Log(bone[i].rotation);
            //Debug.Log(i);
            boneLengthMax+=boneLength[i];
        }
        
        //Debug.Log("before"+targetPosition);
        //Debug.Log((targetPosition - bonePosition[0]));
        
    }

    private void runIK(){
        //Debug.Log(targetPosition);
        //Debug.Log(bone[realBone.Length-1].position);
        currentDistance = (targetPosition - bonePosition[realBone.Length-1]).magnitude;
        Debug.Log(currentDistance);
        targetRotation = WorldToRootRot(realTarget.rotation);
        //if too far, stretch it
        if (currentDistance >= boneLengthMax){
            for (int i = 1; i<realBone.Length;i++){
                bonePosition[i] = (targetPosition - bonePosition[0]).normalized * boneLength[i-1] + bonePosition[i-1];
                //Debug.Log("first part is running");
                
                //Debug.Log(i);
                //Debug.Log("after"+targetPosition);
                
                //Debug.Log(bonePosition[0]);
                //Debug.Log("target vector is: "+(targetPosition - bonePosition[0]));
            }
        }
        else{
            if (!dStretchOnly)
            {
                //this is snapping back. No idea what it does for now but I'll implement it just in case.
                //Improvement: we actually ignore the root. We did it.
                //Apparently I've been miscomprehending the algorithm. This is why before implementing ANYTHING, you must read the paper.
                for (int i = 1;i< realBone.Length;i++){
                    bonePosition[i] = Vector3.Lerp(bonePosition[i],bonePosition[i-1] + lastDirection[i-1],SnapBackStrength);                    
                } 
                for (int j = 0;j<iterations;j++){
                    
                    for (int i = realBone.Length-1; i>0;i--){
                        //Vector3 beforeDirection = bonePosition[i]-bonePosition[i-1];
                        if (i == realBone.Length - 1){
                            bonePosition[i] = targetPosition;
                        }
                        else
                            bonePosition[i]=(bonePosition[i]-bonePosition[i+1]).normalized*boneLength[i]+bonePosition[i+1];
                        //bone[i-1].rotation = Quaternion.FromToRotation(beforeDirection,bonePosition[i]-bonePosition[i-1])*bone[i-1].rotation;
                    }
                    for (int i = 1; i< realBone.Length;i++){
                        bonePosition[i] = (bonePosition[i] - bonePosition[i-1]).normalized*boneLength[i-1] + bonePosition[i-1];
                    }
                }
            }
        } 
    }
    private void appIK(){
        
        //if (realBone.Length != realBone.Length) throw new UnityException("Bones are inconsistent.");
        
        for (int i =0; i<realBone.Length; i++){
            //RootSpace
            /*
            Quaternion lastParentRotation = Quaternion.identity;
            Quaternion parentRotation = Quaternion.identity;
            if (i!=0){
                lastParentRotation = lastRotation[i-1];
                parentRotation = boneRotation[i-1];
            }   
            Quaternion lastParentRotation = Quaternion.identity;
            if (i!=0){
                lastParentRotation = realBone[i-1].rotation;
            }
            */

            Vector3 lastParentDirection = Vector3.zero;
            // lfet right forward up down
            Vector3 parentDirection = Vector3.zero;
            if (i!=0){
                lastParentDirection = lastDirection[i-1];
                parentDirection = bonePosition[i]-bonePosition[i-1];
            }

            Vector3 direction = Vector3.zero;
            if (i==realBone.Length-1) direction = targetPosition - bonePosition[0];
            else direction = bonePosition[i+1] - bonePosition[i];


            Quaternion lastLocalRotation = Quaternion.FromToRotation(lastParentDirection,lastDirection[i]);
            if (i==0) lastLocalRotation = Quaternion.identity;
            //Vector3 lastVector = Quaternion.FromToRotation(lastParentDirection,parentDirection);
            Quaternion delta = Quaternion.FromToRotation(lastParentDirection,parentDirection);
            Vector3 lastVector = delta*lastDirection[i];
            
            Vector3 vector = direction;
            //if (direction.magnitude < 0.1f) direction = parentDirection;
            Quaternion rotation = Quaternion.FromToRotation(lastVector,vector);

            //Quaternion one = rotationPlaceholder;
            if (i==2){
                Debug.Log("rotationPlaceholder is:"+rotationPlaceholder);
                Debug.Log("lastLocalRotattion is:"+lastLocalRotation);
                Debug.Log("angle of local rotation is:"+Quaternion.Angle(rotationPlaceholder,rotationPlaceholder*lastLocalRotation));
                Debug.Log("rotation is:"+rotation);
                Debug.Log("angle of rotation is:"+Quaternion.Angle(rotationPlaceholder*lastLocalRotation,rotationPlaceholder*lastLocalRotation*rotation));

                
            }
            Quaternion placeholder = rotationPlaceholder;
            rotationPlaceholder = rotationPlaceholder*lastLocalRotation*rotation;
            
            //if (one==rotationPlaceholder && rotation != new Quaternion(0f,0f,0f,1f)) throw new UnityException("ref error");
            //realBone[i].rotation = rotationPlaceholder;   
            realBone[i].rotation = RootToWorldRot(rotationPlaceholder);
            if (i==2){
                Debug.Log("angle of total change:"+Quaternion.Angle(placeholder,WorldToRootRot(realBone[i].rotation)));
            }

            /*
            Vector3 direction;
            
            if (i==realBone.Length-1) direction = targetPosition - bonePosition[i];
            else direction = bonePosition[i+1] - bonePosition[i];

            */
            //left right forward back up down
            realBone[i].position = RootToWorldPos(bonePosition[i]);


            /*
            if (i==0){
                lastParentRotation = Quaternion.identity;
                parentRotation = Quaternion.identity; //any
            }
            else {
                lastParentRotation = lastRotation[i-1];
                parentRotation = boneRotation[i-1];
            }
            if (i==1){
            Debug.Log("direction is: "+direction);
            Debug.Log("last direction is: "+lastDirection[i]);
            Debug.Log("fromtorotation is: "+Quaternion.FromToRotation(lastDirection[i],direction));
            //Debug.Log("rotation is: "+rotation);
            
            //Debug.Log("realbone is: "+rotation*realBone[i].rotation);
            //Debug.Log(lastParentRotation.eulerAngles.x - parentRotation.eulerAngles.x);
            }
            */


            
            //realBone[i].rotation = RootToWorldRot(rotation*lastRotation[i]);
            
            //Debug.Log("realbone at "+i+" is "+bonePosition[i]);
            
        }
    }
    //I mean, this is copypasta. 
    private Vector3 WorldToRootPos(Vector3 worldpos){
        //Quaternion.Inverse(root.rotation) = 
        //Debug.Log(root.position);
            
            //Debug.Log(root.position);
            //Debug.Log(Quaternion.Inverse(root.rotation)*(worldpos.position-root.position));
        return Quaternion.Inverse(rootRefRot)*(worldpos-rootRefPos);
        
        //if (result != rotation) throw new UnityException("void does not pass on value");
    }
    private Vector3 RootToWorldPos(Vector3 rootpos){
        return rootRefRot*rootpos + rootRefPos;
    }
    private Quaternion WorldToRootRot(Quaternion worldrot){
        //return rootRefRot*Quaternion.Inverse(worldrot);
        return Quaternion.Inverse(rootRefRot)*worldrot;
    }
    private Quaternion RootToWorldRot(Quaternion rootrot){
        return rootRefRot*rootrot;
        //Debug.Log("result in void is: "+result);
//        if (result != rotation) throw new UnityException("void does not pass on value");
    }

    // Update is called once per frame

    void Start(){
        SetupIK();
    }
    void Update()
    {
        //root = realBone[0];
        
        initIK();
        runIK();
        appIK();
    }
#if UNITY_EDITOR
    void OnDrawGizmos(){

        if (bonePosition.Length != 0){
            for (int i = 1; i< realBone.Length;i++){
                if (bonePosition[i]!=null && bonePosition[i-1]!=null){
                    Vector3 position = new Vector3(5f,5f,5f);
                    Handles.color = Color.blue;
                    Handles.DrawLine(bonePosition[i]+position,bonePosition[i-1]+position);
                    Handles.color = Color.black;
                    Handles.DrawLine(testbonePosition[i]+position,testbonePosition[i-1]+position);
                }
            }
        }

        Handles.color = Color.red;
        if (realBone != null){
            for (int i = 1; i< realBone.Length;i++){
                if (realBone[i]!=null && realBone[i-1]!=null){
                Handles.DrawLine(realBone[i].position,realBone[i-1].position);
                }
            }
        }
        //Handles.color = Color.cyan;
        //if (direction != null) Handles.DrawLine(Vector3.zero, direction);
        //Handles.color = Color.blue;
//        if (lastDirection[1] != null )Handles.DrawLine(Vector3.zero, lastDirection[1]);
        Handles.color = Color.red;
        Vector3 positiona = new Vector3(5f,5f,5f);
        Handles.DrawWireCube(targetPosition+positiona, new Vector3(2f,2f,2f));
        
    }
#endif
}

