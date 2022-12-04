using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(UniformGrabbable))]
public class FreeFloatable : MonoBehaviour
{
    [Tooltip("Occasionally moves floatable in front of player head if enabled.")]
    public bool moveWithPlayerHead;

    [SerializeField] private float _baseForce;
    [SerializeField] private float _restriction;

    private Rigidbody _rigidbody;
    private Vector3 _originPoint;
    private float _originDistanceFromCenter;
    private Transform _playerHeadTransform;

    private UniformGrabbable _localGrabbable;
    private FixedJoint _localFixedJoint;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _originPoint = transform.position;
        _originDistanceFromCenter = Vector3.Distance(new Vector3(_originPoint.x, 0, _originPoint.z), Vector3.zero);
        _playerHeadTransform = GameObject.FindGameObjectWithTag("MainCamera")!.transform;
        _localGrabbable = GetComponent<UniformGrabbable>();
    }

    private void FixedUpdate()
    {
        if (_localGrabbable.isGrabbed && _localFixedJoint is null)
        {
            var touchingHand = _localGrabbable.GetTouchingHandRoot();
            var joint = gameObject.AddComponent<FixedJoint>();
            
            joint.anchor = touchingHand!.position;
            joint.connectedBody = touchingHand!.GetComponentInParent<Rigidbody>();
            joint.enableCollision = false;
            _localFixedJoint = joint;
            touchingHand!.isKinematic = true;
        }
        else if (!_localGrabbable.isGrabbed && _localFixedJoint is not null)
        {
            var touchingHand = _localGrabbable.GetTouchingHandRoot();
            if (!touchingHand) return;
            
            touchingHand.isKinematic = false;
            Destroy(_localFixedJoint);
            _localFixedJoint = null;
        }

        if (moveWithPlayerHead)
        {
            // Calculate the the ball position in the player's field of view
            var forward = _playerHeadTransform.TransformDirection(Vector3.forward);
            forward.y = 0f;
            var delta = Vector3.Normalize(transform.position - _playerHeadTransform.position);
            delta.y = 0f;
            var dot = Vector3.Dot(forward, delta);

            if (dot < 0.4f)
            {
                // Move the ball in front of the player
                var newPosition = Vector3.Normalize(forward) * _originDistanceFromCenter;
                newPosition.y = _originPoint.y;
                _originPoint = newPosition;
            }
        }
        
        // Add force to the ball, moving it towards the origin point
        var distanceVector = _originPoint - transform.position;
        var movementVector = distanceVector * (_baseForce * (1f - _restriction));
        _rigidbody.AddForce(movementVector, ForceMode.Impulse);
        _rigidbody.velocity *= distanceVector.magnitude;
    }
}
