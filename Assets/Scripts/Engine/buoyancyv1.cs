using System;
using System.Security.Cryptography.X509Certificates;
using TMPro;
using UnityEditor.PackageManager;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Engine
{
    public class buoyancyv1 : MonoBehaviour
    {
        private Rigidbody _rigidbody;
        public float height = 10f;
        public float _distance = 0f;
        public float fKp = 36f;
        public float fKd = 110f;
        public float _error = 0;
        public float _errorPrev = 0;
        public float _difference;
        public float pidMultiplier;
        public float distanceXTravel = 0f;
        public float distanceZTravel = 0f;
        public float forceMultiplier3 = 2f;
        private Vector3 forceDirection = new Vector3(0f, 1f, 0f);
        private Vector3 forcePosition = new Vector3(0f, 0f, 0f);
        private void Start()
        {
            _rigidbody = this.GetComponent<Rigidbody>();
            if (!_rigidbody) throw new NullReferenceException("No rigidbody detected.");
        }
        private void FixedUpdate()
        {
            Ray ray = new Ray(transform.position,-forceDirection);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1000f))
            {
                _distance = hit.distance;
                _error = height - _distance;
                _difference = _error - _errorPrev;
                var p = fKp * _error;
                //var i = fKi * 0;
                var d = fKd * _difference;
                
                pidMultiplier = p + d;
                _errorPrev = _error;
                
                Vector3 forceVector = forceDirection * pidMultiplier;
                _rigidbody.AddForceAtPosition(forceVector,transform.position + new Vector3(0f,-2f,0f),ForceMode.Force);
                
                
                
                
            }
            

            // I realize that energy here is reserved as the worldspace is frictionless. 
            // I need to create a stopping force. Conventional F=kx doesn't help. Need a formula that keeps it balanced.
            // After some digging I (re)discovered PID controllers. This is the type of problem I've been looking for. 

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
}