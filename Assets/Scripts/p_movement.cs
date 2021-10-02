using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class p_movement : MonoBehaviour
{
    // Start is called before the first frame update
    public Entity entity = new Entity();
    
    public GameObject playerMesh;
    private Rigidbody playerMesh_rb;
    public GameObject playerHead;
    //private Rigidbody playerHead_rb;
    public GameObject playerFull;
    private Rigidbody playerFull_rb;

    public GameObject groundPoint;

    public Camera cam;
    private Vector3 move;
    private Vector2 moveHead;
    private float key_ad,key_ws, x_mouse, y_mouse;
    float offVerti,offHori;
    float forwardMagnitude2D;

    public LayerMask ground;
    [Header("Gameplay")]
    [Range(1,3)]public int viewMode = 1;
    

    [Header("Control Setting")]
    [SerializeField][Range(0.1f,5f)] private float mouseSensitivity = 4f;
    [Range(0.1f,5f)] public float forwardSpeed = 3f;
    [Range(0.1f,5f)] public float sideSpeed = 3f;
    [Range(0.1f,5f)] public float jumpForce = 2f;
    
    void Start()
    {
        playerFull_rb = playerFull.GetComponent<Rigidbody>();
        
        Cursor.lockState = CursorLockMode.Locked;
        entity.jump = false;
        entity.onland = false;
    }

    // Update is called once per frame
    void Update()
    {
        key_ad = Input.GetAxisRaw("Horizontal");
        key_ws = Input.GetAxisRaw("Vertical");
        
        //Debug.Log(move);
        x_mouse = Input.GetAxisRaw("Mouse X");
        y_mouse = Input.GetAxisRaw("Mouse Y");
        //moveHead = new Vector3(-y_mouse,x_mouse,0f);
        
        
    
        //omit euler due to goldilock
        
        // opting for RotateAround notwithstanding its depricacy. This is because of the Goldiock problem (that's not the right name).
        if (viewMode !=2){
            this.GetComponent<Rigidbody>().velocity.Set(0f,0f,0f);
            offHori = x_mouse*(mouseSensitivity/100);
            //Debug.Log("offHori");
            cam.transform.RotateAround(Vector3.up,offHori);
            //Vector3 offVerti = new Vector3(moveHead.y*mouseSensitivity,0f,);
            offVerti = -y_mouse*(mouseSensitivity/100);
            //Debug.Log("offVerti");
            cam.transform.RotateAround(cam.transform.right,offVerti);
        }
 
        forwardMagnitude2D = Mathf.Sqrt(Mathf.Pow(playerHead.transform.forward.x,2)+Mathf.Pow(playerHead.transform.forward.z,2));


        //rotate body

        groundCheck();
        //ground check: if overlap

        if (viewMode==1)
        {
            playerFull.transform.position += (Vector3.Scale(cam.transform.forward, new Vector3(1f,0f,1f))
            *(1/forwardMagnitude2D)*key_ws*forwardSpeed + cam.transform.right * key_ad * sideSpeed)*Time.deltaTime;
            cam.transform.position = playerHead.transform.position;
            playerHead.transform.rotation = cam.transform.rotation;
        }
        else if (viewMode==3)
        {
            cam.transform.position += (cam.transform.forward*key_ws*forwardSpeed + cam.transform.right*key_ad*sideSpeed)*Time.deltaTime;
            //cam.transform.rotation = playerHead.transform.rotation;
        }
        
        if (Input.GetKeyDown(KeyCode.Space)){
            entity.jump=true;
        }
        //else entityInfo.jump = false;
    }
    void groundCheck(){
        entity.onland = Physics.Raycast(groundPoint.transform.position,groundPoint.transform.up*-1, 0.1f, ground);

    }

    void FixedUpdate()
    {
        
        if (entity.jump && entity.onland){
            playerFull_rb.AddForce(Vector3.up*jumpForce);
            //entityInfo.jump = false;
            entity.onland = false;
        }
        

        
    }
}
