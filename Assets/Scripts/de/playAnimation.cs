using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playAnimation : MonoBehaviour
{
    GameObject target;
    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = target.GetComponent<Animator>();
        animator.SetBool("play",true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
