using UnityEditor;
using UnityEngine;

namespace Engine.IK.v1recursive
{
    public class ikcontrolv1 : MonoBehaviour
    {
    
    // TODO: Articulated model detection, no need for drag and drop objects into fields.
    private Transform rootRef;
    private Quaternion rootRefRot, rotationPlaceholder;
    private Vector3 rootRefPos;
    [SerializeField]private Vector3[] jointPosition;
    [SerializeField]private Quaternion[] jointOrientation;
    private float[] boneLength;
    private float stretchLength;
    [SerializeField]private Transform[] worldJoint;
    [SerializeField]private Transform worldTarget;
    private Vector3 targetPosition;
    [Range(10,100)][SerializeField]private int ikloops;
    [Range(0f,1f)][SerializeField]private float SnapBackStrength;
    [Header("DEBUGGING ONLY")]
    [Range(0f,360f)]public float a,b,c;
    [SerializeField] private bool bStretchOnly = false;
    
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
        stretchLength = 0f;
        jointPosition = new Vector3[worldJoint.Length];
        jointOrientation = new Quaternion[worldJoint.Length];
        
        lastRotation = new Quaternion[worldJoint.Length];
        //bone.SetValue(new Vector3(0f,0f,0f),0);

        //helpful references
        rootRef = worldJoint[0];

        rootRefRot = rootRef.rotation;
        rotationPlaceholder = Quaternion.identity;

        rootRefPos = rootRef.position;
        targetPosition = NormalizePos(worldTarget.position);
        
        for (int i = 0; i< worldJoint.Length;i++){
            jointPosition[i] = NormalizePos(worldJoint[i].position);
            jointOrientation[i] = NormalizeRot(worldJoint[i].rotation);
        }
        
        if (jointPosition.Length != worldJoint.Length) throw new UnityException("Bone lengths are not aligning!");
        boneLength = new float[worldJoint.Length];
        lastDirection = new Vector3[worldJoint.Length];
        currentDistance = new float();
        //lengths, directions, etc
        for (int i=0;i<worldJoint.Length;i++){
            if (i==worldJoint.Length-1) {
                lastDirection[i] = targetPosition - jointPosition[i];
            }
            else {
                lastDirection[i] = jointPosition[i+1] - jointPosition[i];
            }
            boneLength[i] = lastDirection[i].magnitude;
            stretchLength+=boneLength[i];
        }
    }

    private void ProcessIK(){
        currentDistance = (targetPosition - jointPosition[worldJoint.Length-1]).magnitude;
        
        targetRotation = NormalizeRot(worldTarget.rotation);
        if (currentDistance >= stretchLength){
            for (int i = 1; i<worldJoint.Length;i++){
                jointPosition[i] = (targetPosition - jointPosition[0]).normalized * boneLength[i-1] + jointPosition[i-1];
            }

            return;
        }

        if (bStretchOnly) return;
        
        for (int i = 1;i< worldJoint.Length;i++){
            jointPosition[i] = Vector3.Lerp(jointPosition[i],jointPosition[i-1] + lastDirection[i-1],SnapBackStrength);                    
        } 
        // the original FABRIK paper used a tolerance check, aka closed loop. Here we use an open loop instead, for historical reasons.
        // also this version's root isn't necessarily fixed, so that can cause an infinite loop.
        for (int j = 0;j<ikloops;j++){
            //forward reaching
            Vector3 jointPositionRoot = jointPosition[0];
            for (int i = worldJoint.Length-1; i>0;i--){
                // When a joint is moved, only its preceding joint reorientates (because a joint only moves
                // on one axis, merely readjusting its position). The exception being the tail joint (which is
                // at end effector so vector is zero) and root joint (which has no preceding joint).
                if (i == worldJoint.Length - 1)
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
            for (int i = 1; i< worldJoint.Length;i++)
            {
                if ( i == worldJoint.Length - 1)
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
    private void ApplyIKQuaternion(){
        for (int i =0; i<worldJoint.Length; i++)
        {
            worldJoint[i].rotation = DenormalizeRot(jointOrientation[i]);
            //worldJoint[i].rotation = Quaternion.Euler(0f, 0f, 40f);
            worldJoint[i].position = DenormalizePos(jointPosition[i]);
            


        }
        //Debug.Log(worldJoint[2].rotation.eulerAngles);
    }

    private void ApplyIKVector()
    {
        worldJoint[0].position = DenormalizePos(jointPosition[0]);
        
        
        for (int i = 1; i < worldJoint.Length; i++)
        {
        
            worldJoint[i].position = DenormalizePos(jointPosition[i]);
            worldJoint[i-1].up = jointPosition[i] - jointPosition[i-1];
            
            
        }

        
        for (int i = worldJoint.Length - 2; i >= 0; i--)
        {
            
        }

        worldJoint[worldJoint.Length - 1].up = worldJoint[worldJoint.Length - 2].up;

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
        //root = worldJoint[0];
        
        initIK();
        ProcessIK();
        ApplyIKQuaternion();
    }
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {

        if (jointPosition.Length != 0){
            for (int i = 1; i< worldJoint.Length;i++){
                if (jointPosition[i]!=null && jointPosition[i-1]!=null){
                    Vector3 position = new Vector3(2f,2f,2f);
                    Handles.color = Color.blue;
                    Handles.DrawLine(jointPosition[i]+position,jointPosition[i-1]+position);
                }
            }
        }

        Handles.color = Color.red;
        if (worldJoint != null){
            for (int i = 1; i< worldJoint.Length;i++){
                if (worldJoint[i]!=null && worldJoint[i-1]!=null){
                Handles.DrawLine(worldJoint[i].position,worldJoint[i-1].position);
                }
            }
        }
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
}
