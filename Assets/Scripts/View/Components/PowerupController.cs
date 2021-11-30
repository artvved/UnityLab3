using System;
using System.Collections.Generic;
using UnityEngine;

namespace View.Components
{
    public class PowerupController : MonoBehaviour
    {
        public event Action<PowerupController, PlayerController> TargetReachedEvent;


        private Rigidbody _rigidbody;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            var target = collision.gameObject.GetComponentInParent<PlayerController>();
            if (target != null)
            {
                
                TargetReachedEvent?.Invoke(this, target);
            }
        }
    }
}