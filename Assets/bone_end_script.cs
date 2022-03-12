using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class bone_end_script : MonoBehaviour
{
    
    private void OnDrawGizmos()
    {
        Handles.color = Color.green;
        Vector3 a = transform.position;
        Vector3 b = a+transform.up*2;
        
        Handles.DrawLine(a,b);
    }
}
