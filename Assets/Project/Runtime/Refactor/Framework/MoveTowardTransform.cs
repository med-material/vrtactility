using UnityEngine;

[RequireComponent(typeof(IGrabbable))]
[RequireComponent(typeof(Rigidbody))]
public class MoveTowardTransform : MonoBehaviour
{
    public Transform targetTransform;
    public float baseForce;
    public float restriction;
    
    private IGrabbable _grabbable;
    private Rigidbody _rigidbody;

    private void Start()
    {
        _grabbable = GetComponent<IGrabbable>();
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (_grabbable.IsGrabbed()) return;
        
        // Add force to the object, moving it towards the origin point
        var distanceVector = targetTransform.position - transform.position;
        var movementVector = distanceVector * (baseForce * (1f - restriction));
        _rigidbody.AddForce(movementVector, ForceMode.Impulse);
        _rigidbody.velocity *= distanceVector.magnitude;
    }
}
