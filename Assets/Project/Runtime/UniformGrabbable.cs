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
    [HideInInspector] public List<float> touchingBonePressures;

    // Exposing grab
    [HideInInspector] public bool isGrabbed = false;

    private void Start()
    {
        _sphereCollider = GetComponent<SphereCollider>();

        // if (pressureThreshold > _sphereCollider.radius) pressureThreshold = _sphereCollider.radius;
        
        _touchingBoneCapsules = new List<OVRBoneCapsule>();
        _touchingPointVectors = new Dictionary<OVRSkeleton.BoneId, Vector3>();
    }

    private void Update()
    {
        // Wait for OVR to initialize bones
        if (_boneCapsules is null && handInitializer.isInitialized)
        {
            // ...Save bone capsules
            _boneCapsules = handInitializer.fingerCapsulesLeftH
                .Concat(handInitializer.fingerCapsulesRightH)
                .ToList();
            SetIsKinematic(false);
        }
        if (_bones is null && handInitializer.isInitialized)
        {
            // ...Save bones
            _bones = handInitializer.fingerBonesLeftH
                .Concat(handInitializer.fingerBonesRightH)
                .ToList();

            // var lineRenderers = FindObjectsOfType<LineRenderer>();
            // foreach (var line in lineRenderers)
            //     line.enabled = false;
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
        var closestBoneCapsule = FindMatchingBone(collision);
        if (closestBoneCapsule is null) return;  // If the colliding object is not a bone
        
        if (!IsBoneOnValidHand(closestBoneCapsule) || _touchingBoneCapsules.Count == 0) 
            return;  // Ignore collision if the colliding bone is from the wrong hand or state has been reset
        
        // Update the contact points of each touching OVRBoneCapsule
        var boneId = GetBoneId(closestBoneCapsule);
        _touchingPointVectors[boneId] = collision.contacts[0].point;
    }

    private void SetIsKinematic(bool state)
    {
        // Loop through all Rigidbodies on all OVRBoneCapsules and update their state
        foreach (var boneCapsule in _boneCapsules)
            SetIsKinematic(boneCapsule, state);
    }
    
    private static void SetIsKinematic(OVRBoneCapsule boneCapsule, bool state)
    {
        boneCapsule.CapsuleRigidbody.isKinematic = state;
    }

    private float GetAppliedPressure(in OVRBoneCapsule boneCapsule)
    {
        var r = _sphereCollider.transform.localScale.x;
        
        // Find corresponding OVRBone (which doesn't collide with the sphere surface) and its position
        var targetBone = _bones[GetBoneIndex(boneCapsule)];
        var bonePosition = targetBone.Transform.position;

        // Calculate distance between bone and sphere center and project that into a pressure value between 0 and 1
        var distance = Mathf.Sqrt((bonePosition - transform.position).sqrMagnitude);
        var pressure = Mathf.Clamp(r - distance, 0, r) / r;

        // Return distance as pressure applied
        return pressure;
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
        if (index < 0 || index >= _bones.Count)
            throw new IndexOutOfRangeException(
                "Parameter cannot be greater than the number of bone capsules or less than zero");
        return index < _bones.Count / 2;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Add the closest matching OVRBone to list of touching bones
        var closestBoneCapsule = FindMatchingBone(collision);
        if (closestBoneCapsule is null || IsTouching(closestBoneCapsule)) 
            return;  // Don't add a bone that doesn't exist or is already touching
        
        if (!IsBoneOnValidHand(closestBoneCapsule))
        {
            // Clear state and return if the colliding hand is not the hand currently touching
            _touchingPointVectors.Clear();
            _touchingBoneCapsules.Clear();
            
            touchingBoneIds.Clear();
            touchingBonePressures.Clear();
            isGrabbed = false;
            
            SetIsKinematic(closestBoneCapsule, true);
            SetIsKinematic(closestBoneCapsule, false);
            
            return;
        }

        // Add bone and calculate applied pressure
        var boneId = GetBoneId(closestBoneCapsule);
        try
        {
            _touchingPointVectors.Add(boneId, collision.contacts[0].point);
        }
        catch (ArgumentException e)
        {
            return;
        }
        _touchingBoneCapsules.Add(closestBoneCapsule);
        touchingBoneIds.Add(boneId);
        touchingBonePressures.Add(GetAppliedPressure(closestBoneCapsule));
    }

    private bool IsBoneOnValidHand(in OVRBoneCapsule boneCapsule)
    {
        if (_touchingBoneCapsules.Count == 0) return true;
        
        var collidingBoneIndex = GetBoneIndex(boneCapsule);
        var firstTouchingBoneIndex = GetBoneIndex(_touchingBoneCapsules[0]);
        
        return IsIndexOnLeftHand(collidingBoneIndex) == IsIndexOnLeftHand(firstTouchingBoneIndex);
    }

    private OVRSkeleton.BoneId GetBoneId(in OVRBoneCapsule boneCapsule)
    {
        // Raise an error if the provided BoneIndex is for a body part and not just the hands
        if (boneCapsule.BoneIndex > 18)
            throw new IndexOutOfRangeException(
                "Error trying to process collision with unsupported OVR body part");
        
        return _bones[boneCapsule.BoneIndex].Id;
    }

    [CanBeNull]
    private OVRBoneCapsule FindMatchingBone(in Collision collision)
    {
        // Find the OVRBone that best matches the colliding object
        OVRBoneCapsule closestBone = null;
        var smallestDistance = float.MaxValue;
        foreach (var bone in _boneCapsules)
        {
            var distance = Vector3.Distance(bone.CapsuleCollider.transform.position, collision.transform.position);
            if (distance > MATCHING_THRESHOLD || distance > smallestDistance) continue;  // NOTE: MATCHING_THRESHOLD check may be redundant
        
            closestBone = bone;
            smallestDistance = distance;
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
        var closestBoneCapsule = FindMatchingBone(collision);
        if (closestBoneCapsule is null) 
            return;  // If the colliding object is not an OVRBone

        var boneWasTouching = _touchingBoneCapsules.Contains(closestBoneCapsule);
        if (!boneWasTouching)
            return;  // If the colliding bone was not previously touching
        
        if (!IsBoneOnValidHand(closestBoneCapsule)) 
            return;  // Ignore collision if the colliding bone is from the wrong hand

        // Remove the colliding OVRBone from list of touching bones and applied pressures
        var boneId = GetBoneId(closestBoneCapsule);
        touchingBonePressures.RemoveAt(_touchingBoneCapsules.IndexOf(closestBoneCapsule));
        touchingBoneIds.Remove(boneId);
        _touchingPointVectors.Remove(boneId);
        _touchingBoneCapsules.Remove(closestBoneCapsule);
        
        if (_touchingBoneCapsules.Count != 0)
        {
            // Make the bone kinematic briefly to reset position in relation to the rest of the hand
            SetIsKinematic(closestBoneCapsule, true);
            SetIsKinematic(closestBoneCapsule, false);
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
}
