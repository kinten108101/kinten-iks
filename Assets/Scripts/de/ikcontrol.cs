using UnityEngine;
using UnityEditor;

public class ikcontrol : MonoBehaviour
{
    private int n;
    //bone gameobjects could be scanned for, but for now we'll add them manually.
    private Transform rootRef;
    private Quaternion rootRefRot, rotationPlaceholder;
    private Vector3 rootRefPos;
    [SerializeField]private Vector3[] jointPosition;
    [SerializeField]private Quaternion[] jointOrientation;
    private float[] boneLength;
    private Vector3[] testjointPosition;
    private float boneLengthMax;
    [SerializeField]private Transform[] rawJoint;
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
        jointPosition = new Vector3[rawJoint.Length];
        jointOrientation = new Quaternion[rawJoint.Length];
        testjointPosition = new Vector3[rawJoint.Length];
        lastRotation = new Quaternion[rawJoint.Length];
        //bone.SetValue(new Vector3(0f,0f,0f),0);

        //helpful references
        rootRef = rawJoint[0];

        rootRefRot = rootRef.rotation;
        rotationPlaceholder = Quaternion.identity;

        rootRefPos = rootRef.position;
        targetPosition = NormalizePos(realTarget.position);
        
        for (int i = 0; i< rawJoint.Length;i++){
            jointPosition[i] = NormalizePos(rawJoint[i].position);
            jointOrientation[i] = NormalizeRot(rawJoint[i].rotation);
        }
        
        if (jointPosition.Length != rawJoint.Length) throw new UnityException("Bone lengths are not aligning!");
        boneLength = new float[rawJoint.Length];
        lastDirection = new Vector3[rawJoint.Length];
        currentDistance = new float();
        //lengths, directions, etc
        for (int i=0;i<rawJoint.Length;i++){
            if (i==rawJoint.Length-1) {
                lastDirection[i] = targetPosition - jointPosition[i];
            }
            else {
                lastDirection[i] = jointPosition[i+1] - jointPosition[i];
            }
            boneLength[i] = lastDirection[i].magnitude;
            boneLengthMax+=boneLength[i];
        }
    }

    private void ProcessIK(){
        currentDistance = (targetPosition - jointPosition[rawJoint.Length-1]).magnitude;
        
        targetRotation = NormalizeRot(realTarget.rotation);
        if (currentDistance >= boneLengthMax){
            for (int i = 1; i<rawJoint.Length;i++){
                jointPosition[i] = (targetPosition - jointPosition[0]).normalized * boneLength[i-1] + jointPosition[i-1];
            }

            return;
        }

        if (dStretchOnly) return;
        
        for (int i = 1;i< rawJoint.Length;i++){
            jointPosition[i] = Vector3.Lerp(jointPosition[i],jointPosition[i-1] + lastDirection[i-1],SnapBackStrength);                    
        } 
        // the original FABRIK paper used a tolerance check, aka closed loop. Here we use an open loop instead, for historical reasons.
        for (int j = 0;j<ikloops;j++){
            //forward reaching
            Vector3 jointPositionRoot = jointPosition[0];
            for (int i = rawJoint.Length-1; i>0;i--){
                // When a joint is moved, only its preceding joint reorientates (because a joint only moves
                // on one axis, merely readjusting its position). The exception being the tail joint (which is
                // at end effector so vector is zero) and root joint (which has no preceding joint).
                if (i == rawJoint.Length - 1)
                {
                    Vector3 precedingBonePreDirection = jointPosition[i] - jointPosition[i - 1];
                    jointPosition[i] = targetPosition;
                    Vector3 precedingBonePostDirection = jointPosition[i] - jointPosition[i - 1];
                    Quaternion rotation = Quaternion.FromToRotation(precedingBonePreDirection, precedingBonePostDirection);
                    jointOrientation[i] *= rotation;
                    jointOrientation[i - 1] *= rotation;
                    

                    // TODO: Instead of a one-time conditional, idk do something else
                }
                else if (i == 0)
                {
                    jointPosition[i] = (jointPosition[i] - jointPosition[i + 1]).normalized * boneLength[i] +
                                       jointPosition[i + 1];
                  
                }
                else
                {
                    Vector3 precedingBonePreDirection = jointPosition[i] - jointPosition[i-1];
                    jointPosition[i] = (jointPosition[i] - jointPosition[i + 1]).normalized * boneLength[i] +
                                       jointPosition[i + 1];
                    Vector3 precedingBonePostDirection = jointPosition[i] - jointPosition[i-1];
                    Quaternion rotation = Quaternion.FromToRotation(precedingBonePreDirection, precedingBonePostDirection);
                    jointOrientation[i - 1] *=rotation;
                }
                //bone[i-1].rotation = Quaternion.FromToRotation(beforeDirection,jointPosition[i]-jointPosition[i-1])*bone[i-1].rotation;
                Debug.Log(i+" - "+jointOrientation[i-1].eulerAngles);
            }
            //backward reaching
            jointPosition[0] = jointPositionRoot;
            for (int i = 1; i< rawJoint.Length;i++)
            {
                if ( i == rawJoint.Length - 1)
                {
                    jointPosition[i] = (jointPosition[i] - jointPosition[i - 1]).normalized * boneLength[i - 1] +
                                       jointPosition[i - 1];
                    
                }
                else
                {
                    Vector3 supercedingBonePreDirection = jointPosition[i + 1] - jointPosition[i];
                    jointPosition[i] = (jointPosition[i] - jointPosition[i-1]).normalized*boneLength[i-1] + jointPosition[i-1];
                    Vector3 supercedingBonePostDirection = jointPosition[i + 1] - jointPosition[i];
                    Quaternion rotation = Quaternion.FromToRotation(supercedingBonePreDirection, supercedingBonePostDirection);
                    jointOrientation[i] *= rotation;
                    
                }

            }
        }
    
    
    }
    private void ApplyIK(){
        for (int i =0; i<rawJoint.Length; i++)
        {
            rawJoint[i].rotation = DenormalizeRot(jointOrientation[i]);
            //rawJoint[i].rotation = Quaternion.Euler(0f, 0f, 40f);
            rawJoint[i].position = DenormalizePos(jointPosition[i]);
            


        }
        //Debug.Log(rawJoint[2].rotation.eulerAngles);
    }
    private Vector3 NormalizePos(Vector3 inputPos){
        //return Quaternion.Inverse(rootRefRot)*(inputPos-rootRefPos);
        return inputPos;
    }
    private Vector3 DenormalizePos(Vector3 inputPos){
        //return rootRefRot*inputPos + rootRefPos;
        return inputPos;
    }
    private Quaternion NormalizeRot(Quaternion inputRot){
        
        //return inputRot*Quaternion.Inverse(rootRefRot);
        return inputRot;
    }
    private Quaternion DenormalizeRot(Quaternion inputRot){
        //return inputRot*rootRefRot;
        return inputRot;
    }

    // Update is called once per frame

    void Start(){
        SetupIK();
    }
    void Update()
    {
        //root = rawJoint[0];
        
        initIK();
        ProcessIK();
        ApplyIK();
    }
#if UNITY_EDITOR
    void OnDrawGizmos(){

        if (jointPosition.Length != 0){
            for (int i = 1; i< rawJoint.Length;i++){
                if (jointPosition[i]!=null && jointPosition[i-1]!=null){
                    Vector3 position = new Vector3(0f,0f,0f);
                    Handles.color = Color.blue;
                    Handles.DrawLine(jointPosition[i]+position,jointPosition[i-1]+position);
                    Handles.color = Color.black;
                    Handles.DrawLine(testjointPosition[i]+position,testjointPosition[i-1]+position);
                }
            }
        }

        Handles.color = Color.red;
        if (rawJoint != null){
            for (int i = 1; i< rawJoint.Length;i++){
                if (rawJoint[i]!=null && rawJoint[i-1]!=null){
                Handles.DrawLine(rawJoint[i].position,rawJoint[i-1].position);
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

// Should be noting on Trello but too lazy, so here we go
// 12.3.202#.18.27. After some experiment in Q01 (should have just RTFM) I found out that when you assign the rotation you are assigning the world-space one.
// But this only verifies my current approach, so why the error? 
// 12.3.202#.18.54. By using Debug.Log(i+" - "+jointOrientation[i-1].eulerAngles); I can verify that it's the jointOrientation that's causing the problem, not the applyIK. Even these values are inconsistent.
// 12.3.202#.19.03. Never mind, my mistake in the back reaching. I should change jointOrientation[i], not jointOrientation[i+1].
// Now that we've cleared that awkwardness, we managed to improve the code while safely returning the project to the state it once was, as in still having that weird quaternion applyIK issue where rotations go the other way.