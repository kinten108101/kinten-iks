using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class travel : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector3 destination;
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        _error = destination.x - transform.position.x;
        _difference = _error - _errorPrev;
        var p = fKp * _error;
        var d = fKd * _difference;
        
        pidMultiplier = p + d;
        _errorPrev = _error;
        
        Vector3 forceVector = forceDirection * pidMultiplier;
        _rigidbody.AddForceAtPosition(forceVector,transform.position + new Vector3(0f,-2f,0f),ForceMode.Force);
    }

    void travel()
    {
        var delta = destination.x - transform.position.x;
        distanceXTravel = delta>0?Mathf.Min(delta, forceMultiplier3):Mathf.Max(delta,-forceMultiplier3);
        Vector3 forceVectorXTravel = new Vector3(1f, 0f, 0f) * distanceXTravel;
        _rigidbody.AddForceAtPosition(forceVectorXTravel,transform.position + new Vector3(-2f,0f,0f),ForceMode.Force);
        
        delta = destination.z - transform.position.z;
        distanceZTravel = delta>0?Mathf.Min(delta, forceMultiplier3):Mathf.Max(delta,-forceMultiplier3);
        Vector3 forceVectorZTravel = new Vector3(0f, 0f, 1f) * distanceZTravel;
        _rigidbody.AddForceAtPosition(forceVectorZTravel,transform.position + new Vector3(0f,0f,-2f),ForceMode.Force);
    }
}
