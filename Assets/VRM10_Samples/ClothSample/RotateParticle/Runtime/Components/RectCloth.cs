using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RotateParticle.Components
{
    [AddComponentMenu("RotateParticle/RectCloth")]
    [DisallowMultipleComponent]
    public class RectCloth : MonoBehaviour
    {
        [SerializeField]
        public bool LoopIsClosed = false;

        [SerializeField]
        public List<Warp> Warps = new();
    }
}