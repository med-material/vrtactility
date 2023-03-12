using UnityEngine;

// [RequireComponent(typeof(SphereCollider))]
public class TEMPUniformGrabbable : MonoBehaviour, IGrabbable
{
    [Tooltip("The amount of pressure required for grabbing.")]
    [Range(0.1f, 1.0f)] public float pressureThreshold;
    
    private ITouchable _touchable;
    // private SphereCollider _sphereCollider;

    private bool _isGrabbed;

    private void Start()
    {
        _touchable = GetComponent<ITouchable>();
        // _sphereCollider = GetComponent<SphereCollider>();

        _isGrabbed = false;
    }

    private void Update()
    {
        var r = transform.localScale.x;
        var pressurePoints = _touchable.GetPressurePoints();

        var maxPressure = float.MinValue;
        var cumulativeGripVector = Vector3.zero;

        foreach (var p in pressurePoints)
        {
            if (!p.HasValue) continue;

            var distance = p.Value.Pressure.magnitude;
            var pressure = Mathf.Clamp(r - distance, 0, r) / r;
            if (pressure > maxPressure) maxPressure = pressure;

            var contactPosition = p.Value.Point;
            var gripVector = contactPosition - transform.position;
            cumulativeGripVector += gripVector;
        }

        if (cumulativeGripVector == Vector3.zero)
        {
            _isGrabbed = false;
            return;
        };
        cumulativeGripVector /= _touchable.GetPressurePointCount();

        _isGrabbed = cumulativeGripVector.magnitude < 0.014;
    }

    public virtual bool IsGrabbed()
    {
        return _isGrabbed;
    }
}
