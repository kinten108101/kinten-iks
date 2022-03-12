using UnityEngine;
using UnityEditor;

public class Q01 : MonoBehaviour
{
    public Transform rootTransform;
    private Vector3 _rootPos;
    public Transform target1Transform;
    private Vector3 _target1Pos;
    public Transform target2Transform;
    private Vector3 _target2Pos;
    public Transform target3Transform;
    private Vector3 _target3Pos;
    public Quaternion quaternion;
    private void Update(){
        if (quaternion == null) return;
        
            target3Transform.position = quaternion*target3Transform.position;
            
            Debug.Log("Key pressed");    
        
        
        
    }
    private void FixedUpdate() {
        _rootPos = rootTransform.position;
        _target1Pos = target1Transform.position;
        _target2Pos = target2Transform.position;
        _target3Pos = target3Transform.position;
        quaternion = Quaternion.FromToRotation(_target1Pos-_rootPos, _target2Pos-_rootPos);
        //TODO:Check this
    }
    
    private void OnDrawGizmos() {
        Handles.color = Color.blue;
        Handles.DrawLine(_target1Pos,_rootPos);
        Handles.color = Color.red;
        Handles.DrawLine(_target2Pos,_rootPos);
        Handles.color = Color.green;
        Handles.DrawLine(_target3Pos,_rootPos);
    }
    

}
