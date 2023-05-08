using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(UniformGrabbable))]
public class GoalObjectLogic : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private Vector3 _originPoint;
    private float _originDistanceFromCenter;

    private UniformGrabbable _localGrabbable;
    private FixedJoint _localFixedJoint;

    public bool hidden;


    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _originPoint = transform.position;
        _originDistanceFromCenter = Vector3.Distance(new Vector3(_originPoint.x, 0, _originPoint.z), Vector3.zero);
        _localGrabbable = GetComponent<UniformGrabbable>();

        _rigidbody.constraints = RigidbodyConstraints.FreezeAll;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (_localGrabbable.isGrabbed && _localFixedJoint is null)
        {
            var touchingHand = _localGrabbable.GetTouchingHandRoot();
            var joint = gameObject.AddComponent<FixedJoint>();

            _rigidbody.constraints = RigidbodyConstraints.None;
            joint.connectedBody = touchingHand!.GetComponentInParent<Rigidbody>();
            joint.anchor = touchingHand!.position; 
            joint.enableCollision = false;
            _localFixedJoint = joint;
            touchingHand!.isKinematic = true;
        }
        else if (!_localGrabbable.isGrabbed && _localFixedJoint is not null)
        {
            var touchingHand = _localGrabbable.GetTouchingHandRoot();
            if (!touchingHand) return;

            _rigidbody.constraints = RigidbodyConstraints.FreezeAll;
            touchingHand.isKinematic = false;
            Destroy(_localFixedJoint);
            _localFixedJoint = null;
        }

    }
    
    public float pres;//debug variable for reading pressure
    private void Update()
    {

        pres = _localGrabbable.getMaxPressure();
    }

    public bool pop()
    {
        return pres >= 0.8f;
    }

    public void hideObject()
    {
        hidden = true;
        GetComponent<Renderer>().enabled = false;
        transform.position = new Vector3(100, 100, 100);
    }

    public void resetObject()
    {
        hidden = false;
        GetComponent<Renderer>().enabled = true;
        transform.position = _originPoint;
    }
}
