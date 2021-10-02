using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class verticleBalancing : MonoBehaviour
{
    private RaycastHit hit;
    private Vector3 hitSenderPos = Vector3.zero, 
                    hitBodyPos = Vector3.zero;
    private Entity body_en = new Entity();
    public GameObject body;
    private Rigidbody body_rb;
    
    
    private Vector3 stanceRotation;
    private float deltaL;
    private int groundInt;
    [SerializeField]private GameObject sender;
    

    private Vector2 direction;
    [Header("Configure")]
    [Tooltip("Default height for height-based spring physics")]public float springHeight = 5f;
    //[SerializeField]private float deltaL, deltaX, deltaZ;
    [Tooltip("Multiplier for height-based spring physics")]public float k_Height = 10f;
    [Tooltip("Foward walking speed for X-body. (Right now the value must be referenced in the Update function instead of being addressed as a property. What a meme.)")]public float WalkingSpeed;
    [Tooltip("Turning speed for X-body.")]public float TurningSpeed;
    [Tooltip("")][SerializeField]private float SenderRotationOffset;
    public float kround = 10f;
    // Start is called before the first frame update
    void Start()
    {
        sender.transform.rotation.eulerAngles.Set(sender.transform.rotation.eulerAngles.x, sender.transform.rotation.eulerAngles.y + SenderRotationOffset, sender.transform.rotation.eulerAngles.z);
        //Debug.Log(groundInt);
        body_rb = body.GetComponent<Rigidbody>();
        stanceRotation = body_rb.transform.rotation.eulerAngles;
    }
    void Update(){
        groundInt = gameManager.Instance.gndLayer.value;
        body_en.SpeedWalk = WalkingSpeed;
        body_en.SpeedWalk = TurningSpeed;

        //Is it possible to use switch-case here? How to even check Input?
        if (Input.GetKey(KeyCode.U)) direction.y = 1f;
        else if (Input.GetKey(KeyCode.J)) direction.y = -1f;
        else direction.y = 0f;
        //reset velocity. Unity's velocity system kinda falls flat when working in kinetic mode.
        
        if (Input.GetKey(KeyCode.H)) direction.x = -1f;
        else if (Input.GetKey(KeyCode.K)) direction.x = 1f;
        else direction.x = 0f;

    }
    // Update is called once per frame
    void FixedUpdate()
    {
        //update some field values. This is not optimal, like, at all.
        


        //sender does not have rigidbody for obvious reasons.
        sender.transform.position = sender.transform.position + Vector3.Scale(sender.transform.forward, new Vector3(1f,0f,1f))*direction.y*body_en.SpeedWalk*Time.deltaTime;
        

        //X-body's rotation follows sender's rotation. This is not optimal.
        sender.transform.RotateAround(Vector3.up, direction.x*(body_en.SpeedRotate/100));
        body_en.bodyRotate(this.gameObject, direction.x, body_en.SpeedRotate);

        //Don't even mention it why the latter code is repetitive. Can't skip the rest of the kwargs.
        body_rb.rotation.eulerAngles.Set(
            newX: body_rb.rotation.eulerAngles.x,
            newY: sender.transform.rotation.eulerAngles.y,
            newZ: body_rb.rotation.eulerAngles.z);

        

        //There's a seperate body-ground raycast to measure height
        if (Physics.Raycast(body_rb.transform.position,transform.TransformDirection(Vector3.down),out hit,Mathf.Infinity,groundInt))
        {
            hitBodyPos = hit.point;
            //Debug.Log("hitSenderPos: "+hitSenderPos);

            deltaL = springHeight - (float)hit.distance;
            deltaL = Mathf.Max(0f,deltaL);
            body_rb.AddForce(0f,k_Height*deltaL,0f,ForceMode.Impulse);
        }
        
        //This raycast is to determine walkable positions
        if (Physics.Raycast(sender.transform.position, transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity, groundInt)){
            hitSenderPos = hit.point;
            //Debug.Log(" hitBodyPos: "+hitBodyPos);
        }
        //Debug.Log("body.transform: "+body.transform.position);
        Vector3 bodyPosNew = (hitSenderPos-hitBodyPos) + body.transform.position;
        
        //Debug.Log("bodyPosNew: "+bodyPosNew);
        //Debug.Log("Different to new position: "+Vector3.Magnitude(bodyPosNew - hitBodyPos).ToString());
        
        body_rb.MovePosition(bodyPosNew);
        body_rb.velocity.Set(0f,0f,0f);
        


    }
    
    
        
   
}
