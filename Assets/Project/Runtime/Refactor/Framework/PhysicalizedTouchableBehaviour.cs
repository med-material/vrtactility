using System.Collections;
using UnityEngine;

[RequireComponent(typeof(IHandParser))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class PhysicalizedTouchableBehaviour : MonoBehaviour, ITouchable
{
    private PhysicalizedHandParserBehaviour _handParser;
    private PressurePoint?[] _pressurePoints;

    private OVRPlugin.Hand _activeHand;
    private int _pressurePointCount;

    private void Start()
    {
        _handParser = GetComponent<PhysicalizedHandParserBehaviour>();  // Tight coupling is required in this case
        
        ResetPressurePoints();
        
        // while (!_handParser.IsInitialized()) yield return null;
        SetIsKinematic(false);
    }

    private void ResetPressurePoints(bool doSoftReset = false)
    {
        if (!doSoftReset) _pressurePoints = new PressurePoint?[24];
        _activeHand = OVRPlugin.Hand.None;
        _pressurePointCount = 0;
    }
    
    private void SetIsKinematic(bool state)
    {
        // Loop through all Rigidbodies on all OVRBoneCapsules and update their state
        foreach (var boneCapsule in _handParser.GetBoneCapsules())
            SetIsKinematic(in boneCapsule, state);
    }
    
    private static void SetIsKinematic(in OVRBoneCapsule capsule, bool state)
    {
        capsule.CapsuleRigidbody.isKinematic = state;
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        // Add the closest matching OVRBone to list of touching bones
        var capsule = _handParser.GetBoneFromCollision(in collision);
        if (capsule is null || _pressurePoints[capsule.BoneIndex] != null) 
            return;  // Don't add a bone that doesn't exist or is already touching
        
        if (!IsBoneOnValidHand(in capsule))
        {
            // Clear state and return if the colliding hand is not the hand currently touching
            ResetPressurePoints();
            
            SetIsKinematic(in capsule, true);
            SetIsKinematic(in capsule, false);
            
            return;
        }

        AddPressurePoint(in collision, in capsule);
    }

    private bool IsBoneOnValidHand(in OVRBoneCapsule capsule)
    {
        var isBoneOnLeftHand = _handParser.IsIndexOnLeftHand(_handParser.GetBoneIndex(in capsule));

        return _activeHand == OVRPlugin.Hand.None
               || (isBoneOnLeftHand && _activeHand == OVRPlugin.Hand.HandLeft)
               || (!isBoneOnLeftHand && _activeHand != OVRPlugin.Hand.HandRight);
    }
    
    private void AddPressurePoint(in Collision collision, in OVRBoneCapsule capsule)
    {
        var newPressurePoint = CreatePressurePoint(in collision, in capsule);
        _pressurePoints[newPressurePoint.Capsule.BoneIndex] = newPressurePoint;

        _pressurePointCount++;

        var isCapsuleOnLeftHand = _handParser.IsIndexOnLeftHand(_handParser.GetBoneIndex(in capsule));
        if (_activeHand == OVRPlugin.Hand.None)
            _activeHand = isCapsuleOnLeftHand
                ? OVRPlugin.Hand.HandLeft
                : OVRPlugin.Hand.HandRight;
    }

    private PressurePoint CreatePressurePoint(in Collision collision, in OVRBoneCapsule capsule)
    {
        var contactPoint = collision.contacts[0].point;
        var newPressurePoint = new PressurePoint(GetPressureVector(in capsule), in contactPoint, in capsule);

        return newPressurePoint;
    }

    private Vector3 GetPressureVector(in OVRBoneCapsule capsule)
    {
        var capsulePosition = capsule.CapsuleRigidbody.position;
        
        var bone = _handParser.GetBones()[_handParser.GetBoneIndex(in capsule)];
        var bonePosition = bone.Transform.position;

        return bonePosition - capsulePosition;
    }
    
    private void OnCollisionStay(Collision collision)
    {
        // Find matching bone
        var capsule = _handParser.GetBoneFromCollision(in collision);
        if (capsule is null) return;  // If the colliding object is not a bone
        
        if (!IsBoneOnValidHand(in capsule) || _pressurePointCount == 0) 
            return;  // Ignore collision if the colliding bone is from the wrong hand or state has been reset
        
        // Update the pressure points of each touching OVRBoneCapsule
        var newPressurePoint = CreatePressurePoint(in collision, in capsule);
    }
    
    private void OnCollisionExit(Collision collision)
    {
        // Find the OVRBone that best matches the colliding object
        var capsule = _handParser.GetBoneFromCollision(in collision);
        if (capsule is null) 
            return;  // If the colliding object is not an OVRBone

        var boneWasTouching = _pressurePoints[capsule.BoneIndex] != null;
        if (!boneWasTouching) 
            return;  // If the colliding bone was not previously touching
        
        if (!IsBoneOnValidHand(in capsule)) 
            return;  // Ignore collision if the colliding bone is from the wrong hand

        // Remove the colliding OVRBone from list of touching bones and applied pressures
        RemovePressurePoint(in capsule);
        
        if (_pressurePointCount != 0)
        {
            // Make the bone kinematic briefly to reset position in relation to the rest of the hand
            SetIsKinematic(in capsule, true);
            SetIsKinematic(in capsule, false);
            return;
        }

        // If no bones are touching anymore we do the same for all bones in the hand, even those that haven't directly touched the object
        SetIsKinematic(true);
        SetIsKinematic(false);
    }

    private void RemovePressurePoint(in OVRBoneCapsule capsule)
    {
        _pressurePoints[capsule.BoneIndex] = null;

        _pressurePointCount--;
        
        if (_pressurePointCount == 0) ResetPressurePoints(doSoftReset:true);
    }

    public ref readonly PressurePoint?[] GetPressurePoints()
    {
        return ref _pressurePoints;
    }

    public ref int GetPressurePointCount()
    {
        return ref _pressurePointCount;
    }
}
