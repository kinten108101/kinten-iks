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
    [Range(10,100)][SerializeField]private int ikloops;
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
        targetPosition = NormalizePos(realTarget.position);
        //rootspace bone
        for (int i = 0; i< realBone.Length;i++){
            //Vector3 placeholder = realBone[i].position;
            bonePosition[i] = NormalizePos(realBone[i].position);
            //bonePosition[i] = placeholder;
            
            //if (realBone[i].position != placeholder) throw new UnityException("realBones are interfered too early.");
            //bonePosition[i].Set(position.x,position.y,position.z);
            //if (bonePosition[i] == realBone[i].position && rootRefPos != Vector3.zero) throw new UnityException("WorldToRoot translation failed");
            //Debug.Log("1/ realBone"+"["+i+"]"+" before changes is: "+bonePosition[i]);
            lastRotation[i] = realBone[i].rotation;
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

    private void ProcessIK(){
        //Debug.Log(targetPosition);
        //Debug.Log(bone[realBone.Length-1].position);
        currentDistance = (targetPosition - bonePosition[realBone.Length-1]).magnitude;
        
        targetRotation = WorldToRootRot(realTarget.rotation);
        if (currentDistance >= boneLengthMax){
            for (int i = 1; i<realBone.Length;i++){
                bonePosition[i] = (targetPosition - bonePosition[0]).normalized * boneLength[i-1] + bonePosition[i-1];
            }

            return;
        }

        if (dStretchOnly) return;
        
        for (int i = 1;i< realBone.Length;i++){
            bonePosition[i] = Vector3.Lerp(bonePosition[i],bonePosition[i-1] + lastDirection[i-1],SnapBackStrength);                    
        } 
        // the original FABRIK paper used a tolerance check, aka closed loop. Here we use an open loop instead, for historical reasons.
        for (int j = 0;j<ikloops;j++){
            //forward reaching
            for (int i = realBone.Length-1; i>0;i--){
                //Vector3 beforeDirection = bonePosition[i]-bonePosition[i-1];
                if (i == realBone.Length - 1){
                    bonePosition[i] = targetPosition;
                }
                else
                    bonePosition[i]=(bonePosition[i]-bonePosition[i+1]).normalized*boneLength[i]+bonePosition[i+1];
                //bone[i-1].rotation = Quaternion.FromToRotation(beforeDirection,bonePosition[i]-bonePosition[i-1])*bone[i-1].rotation;
            }
            //backward reaching
            for (int i = 1; i< realBone.Length;i++){
                bonePosition[i] = (bonePosition[i] - bonePosition[i-1]).normalized*boneLength[i-1] + bonePosition[i-1];
            }
        }
    
    
    }
    private void ApplyIK(){
        for (int i =0; i<realBone.Length; i++){

            Vector3 lastParentDirection = Vector3.zero;
            Vector3 currentParentDirection = Vector3.zero;
            if (i!=0){
                lastParentDirection = lastDirection[i-1];
                currentParentDirection = bonePosition[i]-bonePosition[i-1];
            }

            Vector3 lastSourceDirection = lastDirection[i];
            Vector3 currentSourceDirection = Vector3.zero;
            if (i==realBone.Length-1) currentSourceDirection = targetPosition - bonePosition[0];
            else currentSourceDirection = bonePosition[i+1] - bonePosition[i];


            Quaternion lastLocalRotation = Quaternion.FromToRotation(lastParentDirection,lastDirection[i]);
            if (i==0) lastLocalRotation = Quaternion.identity;
            Quaternion delta = Quaternion.FromToRotation(lastParentDirection,currentParentDirection);

            Vector3 lastVector = delta * lastDirection[i];
            Vector3 vector = currentSourceDirection;
            Quaternion rotation = Quaternion.FromToRotation(lastVector,vector);

            rotationPlaceholder = rotationPlaceholder*lastLocalRotation*rotation;
            
            realBone[i].rotation = RootToWorldRot(rotationPlaceholder);
            realBone[i].position = RootToWorldPos(bonePosition[i]);


        }
    }
    //I mean, this is copypasta. 
    private Vector3 NormalizePos(Vector3 _inputPos){
        //Quaternion.Inverse(root.rotation) = 
        //Debug.Log(root.position);
            
            //Debug.Log(root.position);
            //Debug.Log(Quaternion.Inverse(root.rotation)*(worldpos.position-root.position));
        return Quaternion.Inverse(rootRefRot)*(_inputPos-rootRefPos);
        
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
        ProcessIK();
        ApplyIK();
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

