using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class controlBody : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]private GameObject[] legs;
    [SerializeField]private GameObject[] legsLast;
    
    void Start()
    {
        if (legs.Length != legsLast.Length) Debug.Log("WARNING: Legs' inputs for body are not consistent.");
    }

    // Update is called once per frame
    void Update()
    {   
        for (int i = 0; i<legs.Length;i++)
        {
            legs[i].transform.position = legsLast[i].transform.position;
        }
        
    }
}
