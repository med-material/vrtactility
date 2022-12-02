using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class UniformGrabbable : MonoBehaviour
{
    private const float MATCHING_THRESHOLD = 0.001f;

    [Tooltip("The amount of pressure which must be applied for the ball to be grabbed")]
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

        if (pressureThreshold > _sphereCollider.radius) pressureThreshold = _sphereCollider.radius;
        
        _touchingBoneCapsules = new List<OVRBoneCapsule>();
        _touchingPointVectors = new Dictionary<OVRSkeleton.BoneId, Vector3>();
    }

    private void Update()
    {
        // Debug.Log(isGrabbed);
        // Wait for OVR to initialize bones
        if (_boneCapsules is null && handInitializer.isInitialized)
        {
            _boneCapsules = handInitializer.fingerCapsulesLeftH
                .Concat(handInitializer.fingerCapsulesRightH)
                .ToList();
            SetIsKinematic(false);
        }
        if (_bones is null && handInitializer.isInitialized)
        {
            _bones = handInitializer.fingerBonesLeftH
                .Concat(handInitializer.fingerBonesRightH)
                .ToList();
        }
        
        // Update applied pressure for each touching bone if any
        // TODO: Optimize this to only loop through finger tips
        for (var i = 0; i < _touchingBoneCapsules.Count; i++)
            touchingBonePressures[i] = GetAppliedPressure(_touchingBoneCapsules[i]);
        
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
        //Debug.Log(gripVector.magnitude);
        if (gripVector.magnitude < 0.014)
            isGrabbed = true;
        else if (gripVector.magnitude > 0.014)
            isGrabbed = false;
    }

    private void OnCollisionStay(Collision collision)
    {
        // Find matching bone
        var boneCapsule = FindMatchingBone(collision);
        if (boneCapsule is null) return;  // If the colliding object is not a bone
        
        // Update the contact points of each touching OVRBoneCapsule
        var boneId = GetBoneId(boneCapsule);
        _touchingPointVectors[boneId] = collision.contacts[0].point;
    }

    private void SetIsKinematic(bool state)
    {
        // Loop through all Rigidbodies on all OVRBoneCapsules and update their state
        foreach (var boneCapsule in _boneCapsules)
            boneCapsule.CapsuleRigidbody.isKinematic = state;
    }

    private float GetAppliedPressure(OVRBoneCapsule boneCapsule)
    {
        // Find corresponding OVRBone and calculate applied pressure as distance between the two transforms
        var targetBone = _bones[GetBoneIndex(boneCapsule)];
        var bonePosition = transform.TransformPoint(targetBone.Transform.position);
        // var boneCapsulePosition = boneCapsule.CapsuleCollider.transform.position;
        // var distance = Vector3.Distance(bonePosition, boneCapsulePosition);
        // var touchingHand = IsLeftHand(_boneCapsules.IndexOf(boneCapsule))
        //     ? handInitializer.leftHandSkeleton
        //     : handInitializer.rightHandSkeleton;
        var pressure = /*Mathf.Clamp01(pressureThreshold - */Vector3.Distance(bonePosition, transform.position)/*)*/;

        // Return distance as pressure applied
        return pressure;
    }

    private int GetBoneIndex(OVRBoneCapsule boneCapsule)
    {
        // Find out what hand boneCapsule belongs to in order to index properly
        var isLeftHand = IsLeftHand(_boneCapsules.IndexOf(boneCapsule));
        var indexOffset = isLeftHand ? 0 : _bones.Count / 2;
        return indexOffset + boneCapsule.BoneIndex;
    }
    
    private bool IsLeftHand(int index)
    {
        if (index < 0 || index >= _boneCapsules.Count)
            throw new IndexOutOfRangeException(
                "Parameter cannot be greater than the number of bone capsules or less than zero");
        return index < _boneCapsules.Count / 2;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Add the closest matching OVRBone to list of touching bones
        var closestBoneCapsule = FindMatchingBone(collision);
        if (closestBoneCapsule is null || IsTouching(closestBoneCapsule)) 
            return;  // Don't add a bone that doesn't exist or is already touching
        
        // Add bone and calculate applied pressure
        var boneId = GetBoneId(closestBoneCapsule);
        _touchingBoneCapsules.Add(closestBoneCapsule);
        _touchingPointVectors.Add(boneId, collision.contacts[0].point);
        touchingBoneIds.Add(boneId);
        touchingBonePressures.Add(GetAppliedPressure(closestBoneCapsule));
    }

    private OVRSkeleton.BoneId GetBoneId(OVRBoneCapsule boneCapsule)
    {
        var index = GetBoneIndex(boneCapsule);
        return _bones[index].Id;
    }

    [CanBeNull]
    private OVRBoneCapsule FindMatchingBone(Collision collision)
    {
        // Find the OVRBone that best matches the colliding object
        OVRBoneCapsule closestBone = null;
        var smallestDistance = float.MaxValue;
        foreach (var bone in _boneCapsules)
        {
            var distance = Vector3.Distance(bone.CapsuleCollider.transform.position, collision.transform.position);
            if (distance > MATCHING_THRESHOLD || distance > smallestDistance) continue;
        
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
        var bone = FindMatchingBone(collision);
        if (bone is null) return;  // if the colliding object is not an OVRBone
    
        // Remove the colliding OVRBone from list of touching bones and applied pressures
        var boneId = GetBoneId(bone);
        touchingBonePressures.RemoveAt(_touchingBoneCapsules.IndexOf(bone));
        touchingBoneIds.Remove(boneId);
        _touchingPointVectors.Remove(boneId);
        _touchingBoneCapsules.Remove(bone);
        
        // If no bones are touching we make the hands kinematic briefly to reset potential broken bones
        if (_touchingBoneCapsules.Count != 0) return;
        SetIsKinematic(true);
        SetIsKinematic(false);
    }

    [CanBeNull]
    public Rigidbody GetTouchingHandRoot()
    {
        if (_touchingBoneCapsules.Count == 0) return null;
        
        return IsLeftHand(_boneCapsules.IndexOf(_touchingBoneCapsules[0]))
            ? _boneCapsules[0].CapsuleRigidbody
            : _boneCapsules[19].CapsuleRigidbody;
    }
}
