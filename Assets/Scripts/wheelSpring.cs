using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class wheelSpring : MonoBehaviour
{
    // Start is called before the first frame update
    public Camera cam;

    public GameObject chasis;
    private Rigidbody chasis_rb;

    public GameObject[] frontWheel = new GameObject[2];
    public GameObject[] backWheel = new GameObject[2];
    public GameObject[] frontOrigin = new GameObject[2];
    public GameObject[] backOrigin = new GameObject[2];
     

    private GameObject[,] wheel = new GameObject[2,2];
    private GameObject[,] origin = new GameObject[2,2];
    private Transform[,] wheelTransform = new Transform[2,2];
    private Rigidbody[,] rb = new Rigidbody[2,2];
    private Vector3[,] currentPos = new Vector3[2,2];
    private Vector3[,] vectorToOrigin = new Vector3[2,2];
    private Vector3[,] vectorToOriginNormal = new Vector3[2,2];
    private Vector3[,] originPos = new Vector3[2,2];
    private float[,] d = new float[2,2];

    private Vector3 force;
    private Collider[] overlap;

    [Header("config")]
    [SerializeField]private float k=1.5f;
    [SerializeField]private float threshold=1.5f;
    [SerializeField]private float radius=1.5f;
    void Start()
    {
        wheel[0,0]=frontWheel[0];
        wheel[0,1]=frontWheel[1];
        wheel[1,0]=backWheel[0];
        wheel[1,1]=backWheel[1];
        origin[0,0]=frontOrigin[0];
        origin[0,1]=frontOrigin[1];
        origin[1,0]=backOrigin[0];
        origin[1,1]=backOrigin[1];


        chasis_rb = chasis.GetComponent<Rigidbody>();
        for (int i = 0; i<2;i++){
            for (int j = 0; j<2;j++){

                
                rb[i,j] = wheel[i,j].GetComponent<Rigidbody>();
                
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i<2;i++){
            for (int j = 0; j<2;j++){
                originPos[i,j] = origin[i,j].transform.position;
                currentPos[i,j] = wheel[i,j].transform.position;
                vectorToOrigin[i,j] =  originPos[i,j] - currentPos[i,j];
                vectorToOriginNormal[i,j] = vectorToOrigin[i,j]* 1/Vector3.Magnitude(vectorToOrigin[i,j])  ;
                d[i,j] = Mathf.Abs(Vector3.Magnitude(vectorToOrigin[i,j])- threshold);
                force = d[i,j]*k*vectorToOriginNormal[i,j];
                
                rb[i,j].AddForce(force, ForceMode.Impulse);
                //if overlap with anything, add a force to it. 
                overlap = Physics.OverlapSphere(originPos[i,j],2.5f);
                if ( overlap.GetLength(0) > 0) {
                    chasis_rb.AddForce(-force*0.25f, ForceMode.Impulse);
                }
                wheel[i,j].transform.SetPositionAndRotation(wheel[i,j].transform.position, Quaternion.Euler(chasis.transform.rotation.x+180f, wheel[i,j].transform.rotation.y, wheel[i,j].transform.rotation.z));
                wheel[i,j].transform.SetPositionAndRotation(wheel[i,j].transform.position, Quaternion.Euler(wheel[i,j].transform.rotation.x, wheel[i,j].transform.rotation.y, chasis.transform.rotation.z+90f));
                
            }
        }
    }
}
