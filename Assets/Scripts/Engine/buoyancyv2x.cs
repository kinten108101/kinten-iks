using System;
using System.Security.Cryptography.X509Certificates;
using UnityEditor.PackageManager;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Engine
{
    public class buoyancyv2x : MonoBehaviour
    {
        private Rigidbody _rigidbody;
        
        public float _delta_max = 1f; // For now we use vector instead of angle. This of course makes no sense.
        public float _delta = 0f;
        public float fKp = 8f;
        public float fKd = 21f;
        public float _error = 0;
        public float _errorPrev = 0;
        public float _difference;
        public float pid;
        private Vector3 forceDirection = new Vector3(-1f, 0f, 0f);
        public Vector3 destination = new Vector3(5f, 0f, 4f);
        private void Start()
        {
            
            _rigidbody = this.GetComponent<Rigidbody>();
            if (!_rigidbody) throw new NullReferenceException("No rigidbody detected.");
        }
        private void FixedUpdate()
        {
                
                _delta = _rigidbody.transform.up.x;
                _error = _delta;
                _difference = _error - _errorPrev;
                var p = fKp * _error;
                //var i = fKi * 0;
                var d = fKd * _difference;
                
                pid = p + d;
                _errorPrev = _error;
                
                Vector3 forceVector = forceDirection * pid;
                _rigidbody.AddForceAtPosition(forceVector,transform.position + new Vector3(2f,-1f,0f),ForceMode.Force);

            

            // I realize that energy here is reserved as the worldspace is frictionless. 
            // I need to create a stopping force. Conventional F=kx doesn't help. Need a formula that keeps it balanced.
            // After some digging I (re)discovered PID controllers. This is the type of problem I've been looking for. 

        }
    }
}