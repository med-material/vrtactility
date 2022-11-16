using UnityEngine;

namespace Project.Runtime
{
    [RequireComponent(typeof(Rigidbody))]
    public class FreeFloatable : MonoBehaviour
    {
        [Tooltip("Accasionally moves floatable in front of player head if enabled.")]
        public bool moveWithPlayerHead;

        [SerializeField] private float _baseForce;
        [SerializeField] private float _restriction;
    
        private Rigidbody _rigidbody;
        private Vector3 _originPoint;
        private float _originDistanceFromCenter;
        private Transform _playerHeadTransform;

        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _originPoint = transform.position;
            _originDistanceFromCenter = Vector3.Distance(new Vector3(_originPoint.x, 0, _originPoint.z), Vector3.zero);
            _playerHeadTransform = GameObject.FindGameObjectWithTag("MainCamera")!.transform;
        }

        private void FixedUpdate()
        {
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
                    // move the ball in front of the player
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

        public void RestrictMovement(float value)
        {
            _restriction = value;
        }
    }
}
