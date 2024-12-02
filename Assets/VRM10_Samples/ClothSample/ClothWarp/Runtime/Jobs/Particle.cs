using UniGLTF.SpringBoneJobs.Blittables;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;


namespace UniVRM10.ClothWarp.Jobs
{
    public struct TransformInfo
    {
        public TransformType TransformType;
        public int ParentIndex;
        public Quaternion InitLocalRotation;
        public Vector3 InitLocalPosition;
        public BlittableJointMutable Settings;
        public int WarpIndex;
    }

    public struct TransformData
    {
        public Matrix4x4 ToWorld;
        public Vector3 Position => ToWorld.GetPosition();
        public Quaternion Rotation => ToWorld.rotation;
        public Matrix4x4 ToLocal;

        public TransformData(TransformAccess t)
        {
            ToWorld = t.localToWorldMatrix;
            ToLocal = t.worldToLocalMatrix;
        }
        public TransformData(Transform t)
        {
            ToWorld = t.localToWorldMatrix;
            ToLocal = t.worldToLocalMatrix;
        }
    }

    // [Input]
    public struct InputTransformJob : IJobParallelForTransform
    {
        [ReadOnly] public NativeArray<TransformInfo> Info;
        [WriteOnly] public NativeArray<TransformData> InputData;
        [WriteOnly] public NativeArray<Vector3> CurrentPositions;

        [WriteOnly] public NativeArray<int> CollisionCount;
        [WriteOnly] public NativeArray<Vector3> CollisionDelta;
        [WriteOnly] public NativeArray<Vector3> Forces;

        public void Execute(int particleIndex, TransformAccess transform)
        {
            InputData[particleIndex] = new TransformData(transform);

            var particle = Info[particleIndex];
            if (particle.TransformType.PositionInput())
            {
                // only warp root position update
                CurrentPositions[particleIndex] = transform.position;
            }

            // clear cloth
            CollisionCount[particleIndex] = 0;
            CollisionDelta[particleIndex] = Vector3.zero;
            Forces[particleIndex] = Vector3.zero;
        }
    }

    public struct VerletJob : IJobParallelFor
    {
        public FrameInfo Frame;
        [ReadOnly] public NativeArray<TransformInfo> Info;
        [ReadOnly] public NativeArray<TransformData> CurrentTransforms;
        [ReadOnly] public NativeArray<Vector3> Forces;
        [ReadOnly] public NativeArray<Vector3> CurrentPositions;
        [ReadOnly] public NativeArray<Vector3> PrevPositions;
        [WriteOnly] public NativeArray<Vector3> NextPositions;
        [WriteOnly] public NativeArray<Quaternion> NextRotations;

        public void Execute(int particleIndex)
        {
            var particle = Info[particleIndex];
            if (particle.TransformType.Movable())
            {
                var parent = Info[particle.ParentIndex];
                var parentParentRotation = CurrentTransforms[parent.ParentIndex].Rotation;

                var local_rest = parentParentRotation * parent.InitLocalRotation * particle.InitLocalPosition;
                var world_rest = CurrentPositions[particle.ParentIndex] + local_rest;
                var resilience_force = world_rest - CurrentPositions[particleIndex];
                var velocity = (CurrentPositions[particleIndex] - PrevPositions[particleIndex]) * (1.0f - particle.Settings.dragForce);
                var resilience = parentParentRotation * parent.InitLocalRotation * particle.InitLocalPosition.normalized *
                           particle.Settings.stiffnessForce;
                var external = particle.Settings.gravityDir * particle.Settings.gravityPower + Frame.Force;

                var newPosition = CurrentPositions[particleIndex]
                     + velocity
                     + resilience * Frame.DeltaTime
                     + external * Frame.DeltaTime
                     ;

                NextPositions[particleIndex] = newPosition;
            }
            else
            {
                // kinematic
                NextPositions[particleIndex] = CurrentPositions[particleIndex];
            }

            NextRotations[particleIndex] = CurrentTransforms[particleIndex].Rotation;
        }
    }

    // [Output]
    public struct OutputTransformJob : IJobParallelForTransform
    {
        [ReadOnly] public NativeArray<TransformInfo> Info;
        [ReadOnly] public NativeArray<Quaternion> NextRotations;
        public void Execute(int particleIndex, TransformAccess transform)
        {
            var info = Info[particleIndex];
            if (info.TransformType.Writable())
            {
                transform.rotation = NextRotations[particleIndex];
            }
        }
    }
}