using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(IHandParser))]
[RequireComponent(typeof(IPressurePointProvider))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class PhysicalizedTouchable : MonoBehaviour, ITouchable
{
    private IHandParser _handParser;
    private IPressurePointProvider _pressurePointProvider;
    
    private PressurePoint?[] _allPressurePoints;
    private List<PressurePoint> _activePressurePoints;

    private OVRPlugin.Hand _activeHand;

    private void Start()
    {
        _handParser = GetComponent<IHandParser>();
        _pressurePointProvider = GetComponent<IPressurePointProvider>();
        
        ResetPressurePoints();
    }

    private void ResetPressurePoints(bool doSoftReset = false)
    {
        if (!doSoftReset)
        {
            _allPressurePoints = new PressurePoint?[24];
            _activePressurePoints = new List<PressurePoint>(24);
        }
        _activeHand = OVRPlugin.Hand.None;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Add the closest matching OVRBone to list of touching bones
        var capsule = _handParser.GetBoneFromCollision(in collision);
        if (capsule is null || _allPressurePoints[capsule.BoneIndex] != null) 
            return;  // Don't add a bone that doesn't exist or is already touching
        
        if (!IsBoneOnValidHand(in capsule))
            // Clear state and return if the colliding hand is not the hand currently touching
            ResetPressurePoints();
        else AddPressurePoint(in collision, in capsule);
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
        var newPressurePoint = GetPressurePointFromCollision(in collision, in capsule);
        _allPressurePoints[newPressurePoint.Capsule.BoneIndex] = newPressurePoint;
        
        _activePressurePoints.Add(newPressurePoint);
        
        var isCapsuleOnLeftHand = _handParser.IsIndexOnLeftHand(_handParser.GetBoneIndex(in capsule));
        if (_activeHand == OVRPlugin.Hand.None)
            _activeHand = isCapsuleOnLeftHand
                ? OVRPlugin.Hand.HandLeft
                : OVRPlugin.Hand.HandRight;
    }

    private PressurePoint GetPressurePointFromCollision(in Collision collision, in OVRBoneCapsule capsule)
    {
        var contactPoint = collision.contacts[0].point;
        return _pressurePointProvider.GetPressurePoint(in contactPoint, in capsule);
    }

    private void OnCollisionStay(Collision collision)
    {
        // Find matching bone
        var capsule = _handParser.GetBoneFromCollision(in collision);
        if (capsule is null) return;  // If the colliding object is not a bone
        
        if (!IsBoneOnValidHand(in capsule) || _activePressurePoints.Count == 0) 
            return;  // Ignore collision if the colliding bone is from the wrong hand or state has been reset
        
        // Define new PressurePoint to replace the previous one
        var newPressurePoint = GetPressurePointFromCollision(in collision, in capsule);

        // Update list of active points
        var currentPressurePoint = _allPressurePoints[capsule.BoneIndex]!.Value;
        var currentIndex = _activePressurePoints.IndexOf(currentPressurePoint);
        _activePressurePoints[currentIndex] = newPressurePoint;
        
        // Update bone-index list of points
        _allPressurePoints[capsule.BoneIndex] = newPressurePoint;
    }
    
    private void OnCollisionExit(Collision collision)
    {
        // Find the OVRBone that best matches the colliding object
        var capsule = _handParser.GetBoneFromCollision(in collision);
        if (capsule is null) 
            return;  // If the colliding object is not an OVRBone

        var boneWasTouching = _allPressurePoints[capsule.BoneIndex] != null;
        if (!boneWasTouching) 
            return;  // If the colliding bone was not previously touching
        
        if (!IsBoneOnValidHand(in capsule)) 
            return;  // Ignore collision if the colliding bone is from the wrong hand

        // Remove the colliding OVRBone from list of touching bones and applied pressures
        RemovePressurePoint(in capsule);
    }

    private void RemovePressurePoint(in OVRBoneCapsule capsule)
    {
        _activePressurePoints.Remove(_allPressurePoints[capsule.BoneIndex]!.Value);
        
        _allPressurePoints[capsule.BoneIndex] = null;

        if (_activePressurePoints.Count == 0) ResetPressurePoints(doSoftReset:true);
    }

    public ref readonly PressurePoint?[] GetAllPressurePoints()
    {
        return ref _allPressurePoints;
    }

    public ref readonly List<PressurePoint> GetActivePressurePoints()
    {
        return ref _activePressurePoints;
    }

    public int GetPressurePointCount()
    {
        return _activePressurePoints.Count;
    }
}
