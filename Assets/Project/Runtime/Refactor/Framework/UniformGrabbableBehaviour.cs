using System.Linq;
using UnityEngine;

[RequireComponent(typeof(ITouchable))]
public class UniformGrabbableBehaviour : MonoBehaviour, IGrabbable
{
    [Tooltip("The amount of pressure required for grabbing.")]
    [Range(0.1f, 1.0f)] public float pressureThreshold;
    
    private ITouchable _touchable;
    // private SphereCollider _sphereCollider;

    private bool _isGrabbed;

    private void Start()
    {
        _touchable = GetComponent<ITouchable>();

        _isGrabbed = false;
    }

    private void Update()
    {
        var pressurePoints = _touchable.GetActivePressurePoints();

        var cumulativeGripVector = pressurePoints
            .Aggregate(Vector3.zero, (cumulativeVector, point) => cumulativeVector + point.Pressure);
        if (cumulativeGripVector == Vector3.zero)
        {
            _isGrabbed = false;
            return;
        };
        cumulativeGripVector /= _touchable.GetPressurePointCount();

        _isGrabbed = cumulativeGripVector.magnitude < 0.014;
    }

    public bool IsGrabbed()
    {
        return _isGrabbed;
    }
}
