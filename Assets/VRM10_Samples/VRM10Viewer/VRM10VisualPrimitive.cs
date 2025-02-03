using UnityEngine;
using UnityEngine.Rendering;

namespace UniVRM10.VRM10Viewer
{
    /// <summary>
    /// Built-in RP と URP の差異を楽に吸収してプリミティブを表示するためのクラス
    /// </summary>
    public class VRM10VisualPrimitive : MonoBehaviour
    {
        [SerializeField] private PrimitiveType _primitiveType;

        [SerializeField] private Material _urpMaterial;

        public PrimitiveType PrimitiveType
        {
            get => _primitiveType;
            set => _primitiveType = value;
        }

        private void Start()
        {
            var visual = GameObject.CreatePrimitive(_primitiveType);
            visual.transform.SetParent(transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one;
            if (Application.platform == RuntimePlatform.WebGLPlayer || GraphicsSettings.renderPipelineAsset != null)
            {
                if (_urpMaterial != null)
                {
                    visual.GetComponent<Renderer>().material = Instantiate(_urpMaterial);
                }
            }
        }

        void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                switch (_primitiveType)
                {
                    case PrimitiveType.Sphere:
                        Gizmos.DrawSphere(Vector3.zero, 1.0f);
                        break;

                    case PrimitiveType.Plane:
                        Gizmos.DrawWireCube(Vector3.zero, new Vector3(10, 0.001f, 10));
                        break;

                    default:
                        // TODO
                        break;
                }
            }
        }
    }
}