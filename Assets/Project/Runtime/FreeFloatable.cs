using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FreeFloatable : MonoBehaviour
{
    [SerializeField] private float _baseForce;
    [SerializeField] private float _restriction;
    
    private Rigidbody _rigidbody;
    private Vector3 _originPoint;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _originPoint = transform.position;
    }

    private void FixedUpdate()
    {
        var distanceVector = _originPoint - transform.position;
        var movementVector = distanceVector * (_baseForce * (1f - _restriction));
        _rigidbody.AddForce(movementVector, ForceMode.Impulse);
        _rigidbody.velocity *= distanceVector.magnitude;
    }

    public void RestrictMovement(float value)
    {
        _restriction = value;
    }
}
