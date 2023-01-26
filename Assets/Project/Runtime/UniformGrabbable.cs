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
    public GameObject _sphere;
    public OVRHand rightHand;

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

        // TASK IMPLEMENTATION -M

        for (int i = 0; i < 5; i++)
        {
            if (rightHand.GetFingerIsPinching((OVRHand.HandFinger)i))
            {
                pressureThreshold += rightHand.GetFingerPinchStrength((OVRHand.HandFinger)i);
            }
        }

        pressureThreshold /= 5;

        // Managing what happens with the sphere when reaching certain pressure threshold marks (sleep, grab, disappear). - M
        if (pressureThreshold <= 0.3f)
        {
            // do nothing / sleep 
            isGrabbed = false;
            }
            else if (pressureThreshold > 0.3f && pressureThreshold <= 0.5f) 
            {
                // take the sphere 
                isGrabbed = true;
                _sphere.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            }
            else if (pressureThreshold > 0.5f)
            {
                // sphere breaks / disappears 
                Debug.Log("Destroyed");
                Destroy(_sphere);
            }

        if (isGrabbed)
        {
            Debug.Log("pressure applied: " + pressureThreshold);
        }
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
            SetIsKinematic(boneCapsule, state);
    }
    
    private static void SetIsKinematic(OVRBoneCapsule boneCapsule, bool state)
    {
        boneCapsule.CapsuleRigidbody.isKinematic = state;
    }

    private float GetAppliedPressure(OVRBoneCapsule boneCapsule)
    {
        var r = _sphereCollider.transform.localScale.x;
        
        // Find corresponding OVRBone (which doesn't collide with the sphere surface) and its position
        var targetBone = _bones[GetBoneIndex(boneCapsule)];
        var bonePosition = targetBone.Transform.position;

        // Calculate distance between bone and sphere center and project that into a pressure value between 0 and 1
        var distance = Mathf.Sqrt((bonePosition - transform.position).sqrMagnitude);
        var pressure = Mathf.Clamp(r - distance / 2, 0, r) / r;

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
        var bone = FindMatchingBone(collision);
        if (bone is null) return;  // if the colliding object is not an OVRBone
    
        // Remove the colliding OVRBone from list of touching bones and applied pressures
        var boneId = GetBoneId(bone);
        touchingBonePressures.RemoveAt(_touchingBoneCapsules.IndexOf(bone));
        touchingBoneIds.Remove(boneId);
        _touchingPointVectors.Remove(boneId);
        _touchingBoneCapsules.Remove(bone);
        
        if (_touchingBoneCapsules.Count != 0)
        {
            // Make the bone kinematic briefly to reset position in relation to the rest of the hand
            SetIsKinematic(bone, true);
            SetIsKinematic(bone, false);
            return;
        }
        // If no bones are touching anymore we do the same for all bones in the hand, even those that haven't directly touched the object
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
