using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace Project.Runtime
{
    [RequireComponent(typeof(Collider))]
    public class UniformGrabbable : MonoBehaviour
    {
        private const float MATCHING_THRESHOLD = 0.001f;
        private const float GRABBING_THRESHOLD = 0.3f;
        
        [SerializeField] private HandInitializer handInitializer;

        // Managing touch
        private List<OVRBone> _bones;
        private List<OVRBoneCapsule> _boneCapsules;
        private List<OVRBoneCapsule> _touchingBoneCapsules;
        private Dictionary<OVRSkeleton.BoneId, Vector3> _touchingPointVectors;

        // Exposing touch
        [HideInInspector] public List<OVRSkeleton.BoneId> touchingBoneIds;
        [HideInInspector] public List<float> touchingBonePressures;

        // Exposing grab
        public bool isGrabbed;

        private void Start()
        {
            _touchingBoneCapsules = new List<OVRBoneCapsule>();
            _touchingPointVectors = new Dictionary<OVRSkeleton.BoneId, Vector3>();

            isGrabbed = false;
        }

        private void Update()
        {
            Debug.Log(isGrabbed);
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
            Debug.Log(gripVector.magnitude);
            if (gripVector.magnitude < 0.026)
                isGrabbed = true;
            else if (gripVector.magnitude > 0.038)
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
            var bone = _bones[GetBoneIndex(boneCapsule)];
            var bonePosition = bone.Transform.position;
            var boneCapsulePosition = boneCapsule.CapsuleCollider.transform.position;
            var distance = Vector3.Distance(bonePosition, boneCapsulePosition);
            
            // Return distance as pressure applied
            return distance;
        }

        private int GetBoneIndex(OVRBoneCapsule boneCapsule)
        {
            // Find out what hand boneCapsule belongs to in order to index properly
            var isLeftHand = IsLeftHand(_boneCapsules.IndexOf(boneCapsule));
            var indexOffset = isLeftHand ? _bones.Count / 2 : 0;
            return indexOffset + boneCapsule.BoneIndex;
        }
        
        private bool IsLeftHand(int index)
        {
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
        public Transform GetTouchingHandTransform()
        {
            return _touchingBoneCapsules.Count == 0 ? null : _touchingBoneCapsules[0]!.CapsuleRigidbody.transform.parent.parent;
        }
    }
}
