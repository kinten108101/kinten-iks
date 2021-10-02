using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class controlJoints : MonoBehaviour
{
    
    private int j = 0;          //global joint index.
    // Start is called before the first frame update
    private float LerpTime = 1f, currentTime, currentTimeAlt;
    private void TimeReset(){
        currentTime = 0f;
        //currentTimeAlt goes in opposite direction to utilize absolution.
        currentTimeAlt = 1f;
    }
    private Vector3 startPos;
    private float startY;
    private bool running;
    float footDistance0;
    Vector3 posDisplacement;
    public GameObject[] joint;
    public GameObject jointUpright;
    public GameObject[] limb;
    public GameObject pole;
    
    public GameObject target;
    public GameObject targetObject;
    //public Camera cam;
    private float[] l;
    public int iteration = 10;

    float currentDistance;
    Vector3 currentVector;
    float d;
    float distanceMax;

    Vector3 lastroot, lastend;
    Vector3 mousePosition;

    private Vector3 root0, pole0, joint0, RootToJoint,RootToPole,Joint0ToJoint;
    private float angle;
    private float RootToJointDis, RootToPoleDis;
    private int groundInt;

    public float FootDistance{
        get {return footDistance;}
    }

    private RaycastHit hit;
    [SerializeField]private GameObject footCheck;
    
    [Tooltip("This one is for display only. Is there a way to disable editing?")][SerializeField]private float footDistance;
    [Tooltip("How further will the leg strive forward once being too far from body?")][SerializeField]private float footPlacementMultiplier = 1.5f;
    [Tooltip("How fast will the leg strive forward once being too far from body. Use in conjunction with footPlacementMultiplier.")][SerializeField]private float footSpeed = 0.5f;
    [Tooltip("How far will the leg be before being flagged as too far from body?")][SerializeField]private float footDisplacementMax = 4f;

    [Header("Configure")]
    public bool freeAnkle = true;
    public bool fixedRoot = true;
    public bool fixedTarget = true;
    [Space]
    public bool AllowUprightLimb = true; 
    

    //public bool TargetAnimationPresetA = false;

    
    //[Tooltip("The range of limb (furthest from root) which remain upright. For specific cases of creature legs.")]public int uprightLimb;


    //Credit: https://answers.unity.com/questions/8338/how-to-draw-a-line-using-script.html
    void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0.1f)
         {
             GameObject myLine = new GameObject();
             myLine.transform.position = start;
             myLine.AddComponent<LineRenderer>();
             LineRenderer lr = myLine.GetComponent<LineRenderer>();
             lr.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
             lr.SetColors(color, color);
             lr.SetWidth(0.1f, 0.1f);
             lr.SetPosition(0, start);
             lr.SetPosition(1, end);
             GameObject.Destroy(myLine, duration);
         }

    void Start()
    {
        l = new float[joint.Length-1];
        //Vector3[] prevcoord = new Vector3[l.Length];
        for (int i = 0; i< l.Length;i++)
        {
            l[i] = Vector3.Magnitude(joint[i+1].transform.position-joint[i].transform.position);
            distanceMax+=l[i];
        }
        
        TimeReset();
        //Vector3.Magnitude(joint[4].transform.position - joint[1].transform.position)
        
    }
    void SetPositionWorldSpace(Vector3 position){
        
    }
    void ResolvePole(){
        if (pole != null && j-1>0 && j+1<joint.Length-1){
                Vector3 PlaneNormal = joint[j-1].transform.position - joint[j+1].transform.position;
                Plane plane = new Plane(PlaneNormal,joint[j-1].transform.position);
                Vector3 pRoot = joint[j-1].transform.position;    //Root position, which is already on plane
                Vector3 pPole = plane.ClosestPointOnPlane(pole.transform.position); //Pole position projected on plane
                Vector3 pJoint = plane.ClosestPointOnPlane(joint[j].transform.position); //Current Joint position projected on plane

                Vector3 pRoot_pJoint = pJoint - pRoot;
                //DrawLine(joint0, root0, Color.green);
                //pRootToJointDis = Vector3.Magnitude(pRootToJoint);
                
                Vector3 pRoot_pPole = pPole - pRoot;
                //DrawLine(pole0, root0, Color.blue);
                //pRootToPoleDis = Vector3.Magnitude(pRootToPole);
                
                Vector3 pJoint_Joint = joint[j].transform.position - pJoint;

                //RootToPole *= (RootToJointDis/RootToPoleDis);
                pRoot_pPole = pRoot_pPole.normalized * Vector3.Magnitude(pRoot_pJoint);

                //Instead of rotating the pJoint to the appropriate direction, modifying the root-pole vector to BE that appropriate direction
                pJoint = pRoot_pPole + pRoot;
                joint[j].transform.position = pJoint_Joint + pJoint;
                //angle = Vector3.SignedAngle(RootToJoint,RootToPole, PlaneNormal);
                //joint[j].transform.RotateAround(Vector3.Normalize(PlaneNormal),angle);
            
        }
    }
    void ResolveIK(){
        if (d > distanceMax){
            //Debug.Log("further away!");
            joint[joint.Length-1].transform.position = (mousePosition-joint[0].transform.position)*(distanceMax/d) + joint[0].transform.position;    
        }
        else {
            joint[joint.Length-1].transform.position = mousePosition;
            }
        
            //int j = joint.Length-2;
            for (int i = 0; i < iteration;i++){
                //create plane for pole projection. Now why here specifically?
                

                lastroot = joint[0].transform.position;
                for (j = joint.Length - 2; j>=0;j--){
                    currentVector = joint[j].transform.position - joint[j+1].transform.position;
                    currentDistance = Vector3.Magnitude(currentVector);
                    joint[j].transform.position = (currentVector.normalized)*(l[j])+joint[j+1].transform.position;
                    limb[j].transform.position = (currentVector.normalized)*(l[j]/(2))+joint[j+1].transform.position;
                    //limb[j].transform.forward = Vector3.RotateTowards(joint[j+1].transform.position);
                    limb[j].transform.forward = currentVector;
                    
                    ResolvePole();
                    //The part below is devoted to polarization. This is not optimal.
                    
                }
                if (fixedRoot) joint[0].transform.position = lastroot;
                
                lastend = joint[joint.Length-1].transform.position;
                for (j = 1; j<joint.Length;j++){
                    currentVector = joint[j].transform.position - joint[j-1].transform.position;
                    currentDistance = Vector3.Magnitude(currentVector);
                    joint[j].transform.position = (currentVector.normalized)*(l[j-1])+joint[j-1].transform.position;
                    limb[j-1].transform.position = (currentVector.normalized)*(l[j-1]/(2))+joint[j-1].transform.position;
                    limb[j-1].transform.forward = currentVector;
                    ResolvePole();
                    /*
                    if (pole != null) {
                        root0 = joint[0].transform.position;
                        pole0 = plane.ClosestPointOnPlane(pole.transform.position);
                        joint0 = plane.ClosestPointOnPlane(joint[j].transform.position);

                        RootToJoint = joint0 - root0;
                        RootToJointDis = Vector3.Magnitude(RootToJoint);
                        
                        RootToPole = pole0 - root0;
                        RootToPoleDis = Vector3.Magnitude(RootToPole);
                        
                        Joint0ToJoint = joint[j].transform.position - joint0;

                        RootToPole *= (RootToJointDis/RootToPoleDis);
                        joint0 = RootToPole + root0;
                        joint[j].transform.position = Joint0ToJoint + joint0;
                    }
                    */
          
                }
                if (fixedTarget) joint[joint.Length-1].transform.position = lastend;

            }
        if (AllowUprightLimb) {
            target.transform.position = joint[joint.Length-1].transform.position-jointUpright.transform.position + targetObject.transform.position;
        }
    }

    // Update is called once per frame
    void FootIK(){
        //Raycast a footcheck to calculate the leg's displacement relative to body.
        //raycast start point: can't think of a better place than the pole itself. That is because, the pole is with the root joint, but the root joint moves while the target doesn't. This is due to change.
        //raycast direction: supposed to be perpendicular to body. For now, using the upright limb's direction.
        
        //another implementation of moving foot, props to https://weaverdev.io/blog/bonehead-procedural-animation
        if (Physics.Raycast(pole.transform.position, transform.TransformPoint(jointUpright.transform.position)-transform.TransformPoint(joint[joint.Length-1].transform.position), out hit, 1000f, groundInt ))
        {
            //Debug.Log(transform.TransformPoint(jointUpright.transform.position));
            //Debug.Log(transform.TransformPoint(joint[joint.Length-1].transform.position));
            footCheck.transform.position = hit.point;
            //DrawLine(transform.TransformPoint(pole.transform.position), transform.TransformPoint(hit.point),Color.blue);
            footDistance = Vector3.Magnitude(footCheck.transform.position - jointUpright.transform.position);
            
            //Draw this line to view footDistance
            //DrawLine(footCheck.transform.position, jointUpright.transform.position,Color.green);
            //Debug.Log(footDistance);

            //Double the distance from foot to footCheck. Issue is, this presumes that the surface is flat. This is purely for demonstration.
            float magnitude = Vector3.Magnitude(footCheck.transform.position - jointUpright.transform.position);
            
            //Vector3 posDisplacement = footCheck.transform.position;
            //Apparently, maxDistanceDelta requires time.deltatime as a factor, or else. Neat. 
            float step = footSpeed * Time.deltaTime;
            //Replaced lerp with movetowards
            
            
            //For the initialization, remember the original pos. As time goes on the full fixed distance will be completed.
            
            if (footDistance > footDisplacementMax) {
                running = true;
                posDisplacement = (footCheck.transform.position - jointUpright.transform.position)*footPlacementMultiplier+jointUpright.transform.position;
                startPos = targetObject.transform.position;
                startY = targetObject.transform.position.y;
                TimeReset();
                StartCoroutine(moveFoot());
                //Use Lerp, why not
                /*
                footDistance0 = Vector3.Magnitude(jointUpright.transform.position - posDisplacement);
                currentTime += Time.deltaTime;
                targetObject.transform.position = Vector3.Lerp(
                    startPos,
                    posDisplacement,
                    (currentTime/LerpTime)
                    );
                
                
                //Once destination reached, reset status and currentTime
                if (footDistance0 < 0.1f) {
                    running = !running;
                    TimeReset();
                    
                }
                */
                //Debug.Log(posDisplacement);
                //DrawLine(transform.TransformPoint(posDisplacement),transform.TransformPoint(hit.point),Color.green);
                
                
            }
            
            //Translate: with velocity
            //Lerp: from a to b, with linear interpolation
        }

    }
    IEnumerator moveFoot(){
        //currenttime and lerptime should have been local.
            while (currentTime < LerpTime){
                currentTime = Mathf.Min(currentTime,LerpTime);
                targetObject.transform.position = Vector3.Lerp(
                        startPos,
                        posDisplacement,
                        (currentTime/LerpTime)
                );

                currentTime += Time.deltaTime;
                yield return null;
            }
    }
    void Update()
    {
        groundInt = gameManager.Instance.gndLayer.value;
        mousePosition = target.transform.position;
        d = Vector3.Magnitude(mousePosition-joint[0].transform.position);
        
        ResolveIK();
        //FootIK();

    }

    
}
