using UnityEngine;

public class UniformPressurePointProvider : MonoBehaviour, IPressurePointProvider
{
    private IHandParser _handParser;

    private void Start()
    {
        _handParser = GetComponent<IHandParser>();
    }

    public PressurePoint GetPressurePoint(in Vector3 point, in OVRBoneCapsule capsule)
    {
        var newPressurePoint = new PressurePoint(GetPressureVector(in capsule), in point, in capsule);
        return newPressurePoint;
    }

    private Vector3 GetPressureVector(in OVRBoneCapsule boneCapsule)
    {
        // Define OVRBone and OVRBoneCapsule for comparison
        var capsulePosition = boneCapsule.CapsuleRigidbody.position;
        var bone = _handParser.GetBones()[_handParser.GetBoneIndex(in boneCapsule)];
        var bonePosition = bone.Transform.position;
        
        // Compare the two bones
        var differenceVector = bonePosition - capsulePosition;
        
        // Scale and clamp the differenceVector 
        var localTransform = transform;
        var r = localTransform.localScale.x;
        var d = Mathf.Sqrt((bonePosition - localTransform.position).sqrMagnitude);
        var pressure = Mathf.Clamp(r - d, 0, r) / r;
        var resultVector = differenceVector.normalized * pressure;

        return resultVector;
    }
}
