using System.Collections.Generic;
using System.Linq;
using RotateParticle;
using RotateParticle.Components;
using SphereTriangle;
using UnityEngine;


namespace UniVRM10.Cloth.Viewer
{
    public static class ClothGuess
    {
        public enum StrandConnectionType
        {
            Cloth,
            ClothLoop,
            Strand,
        }

        public static void Guess(RotateParticleSystem _system, Animator animator)
        {
            // skirt
            {
                if (TryAddGroup(animator, HumanBodyBones.Hips,
                    new[] { "skirt", "ｽｶｰﾄ", "スカート" }, out var g))
                {
                    _system._warps.AddRange(g);
                    // StrandConnectionType.ClothLoop
                }
            }
            {
                if (TryAddGroupChildChild(animator, HumanBodyBones.Hips,
                    new[] { "skirt", "ｽｶｰﾄ", "スカート" }, new string[] { }, out var g))
                {
                    _system._warps.AddRange(g);
                    // StrandConnectionType.ClothLoop, 
                }
            }
            {
                if (TryAddGroup(animator, HumanBodyBones.Head,
                    new[] { "髪", "hair" }, out var g))
                {
                    _system._warps.AddRange(g);
                    // StrandConnectionType.Strand,
                }
            }
            {
                if (TryAddGroup(animator, HumanBodyBones.Hips,
                    new[] { "裾" }, out var g))
                {
                    _system._warps.AddRange(g);
                    // StrandConnectionType.Cloth,
                }
            }
            {
                if (TryAddGroupChildChild(animator, HumanBodyBones.LeftUpperArm,
                    new[] { "袖" }, new[] { "ひじ袖" }, out var g))
                {
                    _system._warps.AddRange(g);
                    // StrandConnectionType.ClothLoop,
                }
            }
            {
                if (TryAddGroupChildChild(animator, HumanBodyBones.LeftLowerArm,
                    new[] { "袖" }, new string[] { }, out var g))
                {
                    _system._warps.AddRange(g);
                    // StrandConnectionType.ClothLoop,
                }
            }
            {
                if (TryAddGroupChildChild(animator, HumanBodyBones.RightUpperArm,
                    new[] { "袖" }, new[] { "ひじ袖" }, out var g))
                {
                    _system._warps.AddRange(g);
                    // , StrandConnectionType.ClothLoop
                }
            }
            {
                if (TryAddGroupChildChild(animator, HumanBodyBones.RightLowerArm,
                    new[] { "袖" }, new string[] { }, out var g))
                {
                    _system._warps.AddRange(g);
                    // StrandConnectionType.ClothLoop, false, 
                }
            }
            {
                if (TryAddGroup(animator, HumanBodyBones.Chest, new[] { "マント" },
                    out var g))
                {
                    _system._warps.AddRange(g);
                    // StrandConnectionType.Cloth, 
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="name"></param>
        /// <param name="animator"></param>
        /// <param name="humanBone"></param>
        /// <param name="targets"></param>
        /// <param name="excludes"></param>
        /// <param name="type"></param>
        /// <param name="sort"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        static bool TryAddGroupChildChild(
            Animator animator, HumanBodyBones humanBone,
            string[] targets, string[] excludes,
            out List<Warp> group)
        {
            var bone = animator.GetBoneTransform(humanBone);
            if (bone == null)
            {
                Debug.LogWarning($"{humanBone} not found");
                group = default;
                return false;
            }

            List<Warp> transforms = new();
            foreach (Transform child in bone)
            {
                foreach (Transform childchild in child)
                {
                    if (excludes.Any(x => childchild.name.ToLower().Contains(x.ToLower())))
                    {
                        continue;
                    }

                    foreach (var target in targets)
                    {
                        if (childchild.name.ToLower().Contains(target.ToLower()))
                        {
                            var warp = childchild.gameObject.AddComponent<Warp>();
                            //     Name = name,
                            //     CollisionMask = mask,
                            warp.BaseSettings.HitRadius = 0.02f;
                            //     Connection = type
                            transforms.Add(warp);
                            break;
                        }
                    }
                }
            }
            if (transforms.Count == 0)
            {
                // Debug.LogWarning($"{string.Join(',', targets)} not found");
                group = default;
                return false;
            }

            group = transforms;
            return true;
        }

        static bool TryAddGroup(Animator animator, HumanBodyBones humanBone, string[] targets,
            out List<Warp> group)
        {
            var bone = animator.GetBoneTransform(humanBone);
            if (bone == null)
            {
                Debug.LogWarning($"{humanBone} not found");
                group = default;
                return false;
            }

            List<Warp> transforms = new();
            foreach (Transform child in bone)
            {
                foreach (var target in targets)
                {
                    if (child.name.ToLower().Contains(target.ToLower()))
                    {
                        var warp = child.gameObject.AddComponent<Warp>();
                        // CollisionMask = mask,
                        warp.BaseSettings.HitRadius = 0.02f;
                        // Connection = type
                        transforms.Add(warp);
                        break;
                    }
                }
            }
            if (transforms.Count == 0)
            {
                // Debug.LogWarning($"{string.Join(',', targets)} not found");
                group = default;
                return false;
            }

            group = transforms;
            return true;
        }
    }
}