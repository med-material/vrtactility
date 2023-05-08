using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class UniformGrabbable : MonoBehaviour
{
    private const float MATCHING_THRESHOLD = 0.001f;

    [Tooltip("The amount of pressure which must be applied for the ball to be grabbed.")]
    [Range(0.1f, 1.0f)] public float pressureThreshold;
    [SerializeField] private HandInitializer handInitializer;

    private SphereCollider _sphereCollider;

    // Managing touch
    private List<OVRBone> _bones;
    private List<OVRBoneCapsule> _boneCapsules;
    private List<OVRBoneCapsule> _touchingBoneCapsules;
    private Dictionary<OVRSkeleton.BoneId, Vector3> _touchingPointVectors;

    // Exposing touch
    [HideInInspector] public List<OVRSkeleton.BoneId> touchingBoneIds;
    public List<float> touchingBonePressures;

    // Exposing grab
    [HideInInspector] public bool isGrabbed = false;

    private void Start()
    {
        _sphereCollider = GetComponent<SphereCollider>();
        
        _touchingBoneCapsules = new List<OVRBoneCapsule>(24);
        _touchingPointVectors = new Dictionary<OVRSkeleton.BoneId, Vector3>(24);
    }

    private void Update()
    {
        // Wait for OVR to initialize bones
        if (_boneCapsules is null && handInitializer.isInitialized)
        {
            // Save bone capsules
            _boneCapsules = handInitializer.fingerCapsulesLeftH
                .Concat(handInitializer.fingerCapsulesRightH)
                .ToList();
            SetIsKinematic(false);
        }
        if (_bones is null && handInitializer.isInitialized)
        {
            // Save bones
            _bones = handInitializer.fingerBonesLeftH
                .Concat(handInitializer.fingerBonesRightH)
                .ToList();
        }
        
        // Update applied pressure for each touching bone if any
        // TODO: Optimize this to only loop through finger tips
        for (var i = 0; i < _touchingBoneCapsules.Count; i++)
            touchingBonePressures[i] = GetAppliedPressure(_touchingBoneCapsules[i]);

        // Stop updating if the applied pressure is less than would be required to grib the object
        // TODO: New pressure calculations must be reflected here...
        if (touchingBonePressures.Count > 0 && touchingBonePressures.Max() < pressureThreshold)
        {
            isGrabbed = false;
            return;
        }

        // Calculate union of all collision point vectors to indicate grip distribution
        // TODO: This is probably a candidate for further optimizations...
        var gripVector = _touchingPointVectors.Values
            .Select(vector => vector - transform.position)
            .Aggregate(Vector3.zero, (current, deltaVector) => current + deltaVector);
        if (gripVector == Vector3.zero)
        {
            isGrabbed = false;
            return;
        }
        gripVector /= _touchingPointVectors.Count;

        // Manage FreeFloatable in accordance with grip
        if (gripVector.magnitude < 0.014)
            isGrabbed = true;            
        else if (gripVector.magnitude > 0.014)
            isGrabbed = false;
    }

    private void OnCollisionStay(Collision collision)
    {
        // Find matching bone
        var closestBoneCapsule = FindMatchingBone(in collision);
        if (closestBoneCapsule is null) return;  // If the colliding object is not a bone
        
        if (!IsBoneOnValidHand(in closestBoneCapsule) || _touchingBoneCapsules.Count == 0) 
            return;  // Ignore collision if the colliding bone is from the wrong hand or state has been reset
        
        // Update the contact points of each touching OVRBoneCapsule
        var boneId = GetBoneId(in closestBoneCapsule);
        _touchingPointVectors[boneId] = collision.contacts[0].point;
    }

    private void SetIsKinematic(in bool state)
    {
        // Loop through all Rigidbodies on all OVRBoneCapsules and update their state
        foreach (var boneCapsule in _boneCapsules)
            SetIsKinematic(in boneCapsule, in state);
    }
    
    private static void SetIsKinematic(in OVRBoneCapsule boneCapsule, in bool state)
    {
        boneCapsule.CapsuleRigidbody.isKinematic = state;
    }

    private float GetAppliedPressure(in OVRBoneCapsule boneCapsule)
    {
        var r = _sphereCollider.transform.localScale.x;
        
        // Find corresponding OVRBone (which doesn't collide with the sphere surface) and its position
        var targetBone = _bones[GetBoneIndex(in boneCapsule)];
        var bonePosition = targetBone.Transform.position;

        // Calculate distance between bone and sphere center and project that into a pressure value between 0 and 1
        var distance = Mathf.Sqrt((bonePosition - transform.position).sqrMagnitude);
        var pressure = Mathf.Clamp(r - distance, 0, r) / r;

        // Return distance as pressure applied
        return pressure;
    }
    private int _lastCount = 0;  // The number of touching bones in the previous frame update
    public float getMaxPressure()
    {
        if (touchingBonePressures.Count + _lastCount == 0) return 0f;
        _lastCount = touchingBonePressures.Count;

        // Calculate total pressure being applied and update sphere material color
        var maxPressure = touchingBonePressures.Count > 0
            ? touchingBonePressures.Max()
            : 0f;
        return maxPressure;
    }

    private int GetBoneIndex(in OVRBoneCapsule boneCapsule)
    {
        // Find out what hand boneCapsule belongs to in order to index properly
        var isLeftHand = _boneCapsules.IndexOf(boneCapsule) < _boneCapsules.Count / 2;
        var indexOffset = isLeftHand ? 0 : _bones.Count / 2;

        return indexOffset + boneCapsule.BoneIndex;
    }
    
    private bool IsIndexOnLeftHand(in int index)
    {
        return index < _bones.Count / 2;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Add the closest matching OVRBone to list of touching bones
        var closestBoneCapsule = FindMatchingBone(in collision);
        if (closestBoneCapsule is null || IsTouching(closestBoneCapsule)) 
            return;  // Don't add a bone that doesn't exist or is already touching
        
        if (!IsBoneOnValidHand(in closestBoneCapsule))
        {
            // Clear state and return if the colliding hand is not the hand currently touching
            _touchingPointVectors.Clear();
            _touchingBoneCapsules.Clear();
            
            touchingBoneIds.Clear();
            touchingBonePressures.Clear();
            isGrabbed = false;
            
            SetIsKinematic(in closestBoneCapsule, true);
            SetIsKinematic(in closestBoneCapsule, false);
            
            return;
        }

        // Add bone and calculate applied pressure
        var boneId = GetBoneId(in closestBoneCapsule);
        try
        {
            _touchingPointVectors.Add(boneId, collision.contacts[0].point);
        }
        catch (ArgumentException)
        {
            return;
        }
        _touchingBoneCapsules.Add(closestBoneCapsule);
        touchingBoneIds.Add(boneId);
        touchingBonePressures.Add(GetAppliedPressure(in closestBoneCapsule));
    }

    private bool IsBoneOnValidHand(in OVRBoneCapsule boneCapsule)
    {
        if (_touchingBoneCapsules.Count == 0) return true;
        
        var collidingBoneIndex = GetBoneIndex(in boneCapsule);
        var firstTouchingBoneIndex = GetBoneIndex(_touchingBoneCapsules[0]);
        
        return IsIndexOnLeftHand(in collidingBoneIndex) == IsIndexOnLeftHand(in firstTouchingBoneIndex);
    }

    private OVRSkeleton.BoneId GetBoneId(in OVRBoneCapsule boneCapsule)
    {   
        return _bones[boneCapsule.BoneIndex].Id;
    }

    [CanBeNull]
    private OVRBoneCapsule FindMatchingBone(in Collision collision)
    {
        // Find the OVRBone that best matches the colliding object
        OVRBoneCapsule closestBone = null;
        foreach (var bone in _boneCapsules)
        {
            var distance = Vector3.Distance(bone.CapsuleCollider.transform.position, collision.transform.position);
            if (distance > MATCHING_THRESHOLD) continue;
        
            closestBone = bone;
            break;  // Breaking here means that MATCHING_THRESHOLD cannot be set too high
        }

        return closestBone;
    }

    private bool IsTouching(OVRBoneCapsule bone)
    {
        return _touchingBoneCapsules.Any(b => b.BoneIndex == bone.BoneIndex);
    }

    private void OnCollisionExit(Collision collision)
    {
        // Find the OVRBone that best matches the colliding object
        var closestBoneCapsule = FindMatchingBone(in collision);
        if (closestBoneCapsule is null) 
            return;  // If the colliding object is not an OVRBone

        var touchingIdx = _touchingBoneCapsules.IndexOf(closestBoneCapsule);
        if (touchingIdx == -1)
            return;  // If the colliding bone was not previously touching
        
        if (!IsBoneOnValidHand(in closestBoneCapsule)) 
            return;  // Ignore collision if the colliding bone is from the wrong hand

        // Remove the no-longer-colliding OVRBone
        SwapRemove(in touchingIdx);
        
        if (_touchingBoneCapsules.Count != 0)
        {
            // Make the bone kinematic briefly to reset position in relation to the rest of the hand
            SetIsKinematic(in closestBoneCapsule, true);
            SetIsKinematic(in closestBoneCapsule, false);
            return;
        }
        isGrabbed = false;
        
        // If no bones are touching anymore we do the same for all bones in the hand, even those that haven't directly touched the object
        SetIsKinematic(true);
        SetIsKinematic(false);
    }

    [CanBeNull]
    public Rigidbody GetTouchingHandRoot()
    {
        if (_touchingBoneCapsules.Count == 0) return null;
        
        // Although GetBoneIndex returns _bones index and not _boneCapsules, we're still free to pipe it in to the left hand check
        return IsIndexOnLeftHand(GetBoneIndex(_touchingBoneCapsules[0]))
            ? _boneCapsules[0].CapsuleRigidbody
            : _boneCapsules[19].CapsuleRigidbody;
    }

    private void SwapRemove(in int index)
    {
        // A more efficient way of removing elements from lists

        // For touchingBoneIds, _touchingBoneCapsules, touchingBonePressures
        touchingBoneIds[index] = touchingBoneIds[touchingBoneIds.Count - 1];
        _touchingBoneCapsules[index] = _touchingBoneCapsules[_touchingBoneCapsules.Count - 1];
        touchingBonePressures[index] = touchingBonePressures[touchingBonePressures.Count - 1];
        touchingBoneIds.RemoveAt(touchingBoneIds.Count - 1);
        _touchingBoneCapsules.RemoveAt(_touchingBoneCapsules.Count - 1);
        touchingBonePressures.RemoveAt(touchingBonePressures.Count - 1);

        // For _touchingPointVectors
        var pairAtIndex = _touchingPointVectors.ElementAt(index);
        var lastPair = _touchingPointVectors.ElementAt(_touchingPointVectors.Count - 1);
        _touchingPointVectors.Remove(lastPair.Key);
        if (!EqualityComparer<OVRSkeleton.BoneId>.Default.Equals(pairAtIndex.Key, lastPair.Key))
        {
            _touchingPointVectors[pairAtIndex.Key] = lastPair.Value;
        }
    }
}
