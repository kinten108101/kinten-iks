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
    public Transform target4Transform;
    public Quaternion quaternion;
    
    private void Update(){
        if (quaternion == null) return;
        
        if (Input.GetKeyDown(KeyCode.J))
        {
            Debug.Log("Key J pressed");
            
            Vector3 pos = target3Transform.position;
            Vector3 preDirection = pos - rootTransform.position; 
            pos = rootTransform.position + quaternion * (target3Transform.position - rootTransform.position);
            Vector3 postDirection = pos - rootTransform.position;
            target3Transform.position = pos;
            target3Transform.rotation *= Quaternion.FromToRotation(preDirection, postDirection);

            

        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            
            target4Transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        }
        
        if (Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log("Key H pressed");
            target3Transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        }

    

    }
    private void FixedUpdate() {
        _rootPos = rootTransform.position;
        _target1Pos = target1Transform.position;
        _target2Pos = target2Transform.position;
        _target3Pos = target3Transform.position;
        quaternion = Quaternion.FromToRotation(_target1Pos-_rootPos, _target2Pos-_rootPos);
        //TODO:Check this. So are we rotating around the up vector or something?
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
