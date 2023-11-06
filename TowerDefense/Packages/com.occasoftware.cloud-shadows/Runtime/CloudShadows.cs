using UnityEngine;

namespace OccaSoftware.CloudShadows.Runtime
{
    [AddComponentMenu("OccaSoftware/Cloud Shadows/Cloud Shadows")]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    [ExecuteAlways]
    public class CloudShadows : MonoBehaviour
    {
        [SerializeField]
        private float cloudLayerRelativeHeight = 200;

        [SerializeField]
        private Transform followTarget;

        [SerializeField]
        [Range(0, 360)]
        private float windDirection;

        [SerializeField]
        [Min(0)]
        private float windSpeed = 1.0f;

        [SerializeField]
        [Range(0, 1)]
        private float fadeInExtents = 0.2f;

        [SerializeField]
        [Range(0, 1)]
        private float maximumOpacity = 0.8f;

        [SerializeField]
        [Min(0)]
        private float ditherScale = 1.0f;

        [SerializeField]
        [Min(0)]
        private float tilingDomain = 1000f;

        [SerializeField]
        [Range(0, 360)]
        private float orientation;

        [SerializeField]
        [Range(0, 1)]
        private float cloudiness = 0.3f;

        [SerializeField]
        private Texture2D cloudTexture;

        [SerializeField]
        private DitherNoiseType ditherNoiseType = DitherNoiseType.InterleavedGradient;

        private readonly Vector4[] offsets = new Vector4[3];
        private Material m;

        private struct ShaderParams
        {
            public static readonly int CloudShadowOffsets = Shader.PropertyToID("_CloudShadowOffsets");
            public static readonly int FadeInExtents = Shader.PropertyToID("_FadeInExtents");
            public static readonly int OneOverTilingDomain = Shader.PropertyToID("_OneOverTilingDomain");
            public static readonly int Orientation = Shader.PropertyToID("_Orientation");
            public static readonly int TilingDomain = Shader.PropertyToID("_TilingDomain");
            public static readonly int Cloudiness = Shader.PropertyToID("_Cloudiness");
            public static readonly int Texture = Shader.PropertyToID("_Texture");
            public static readonly int DitherScale = Shader.PropertyToID("_DitherScale");
            public static readonly int MaximumOpacity = Shader.PropertyToID("_MaximumOpacity");
            public static readonly int NoiseOption = Shader.PropertyToID("_NoiseOption");
        }

        private void OnEnable()
        {
            m = GetComponent<MeshRenderer>().sharedMaterial;
            offsets[0] = Vector4.zero;
            offsets[1] = Vector4.zero;
            offsets[2] = Vector4.zero;
            UpdateShaderProperties();
        }

        private void Update()
        {
            UpdateShaderProperties();
        }

        private void UpdateShaderProperties()
        {
            // if (m == null)
            // {
            //     m = GetComponent<MeshRenderer>().sharedMaterial;
            // }
            offsets[0] = SetShadowOffset(windDirection, windSpeed, offsets[0]);
            offsets[1] = SetShadowOffset(windDirection, windSpeed * 2.01f, offsets[1]);
            offsets[2] = SetShadowOffset(windDirection, windSpeed * 4.03f, offsets[2]);
            m.SetVectorArray(ShaderParams.CloudShadowOffsets, offsets);
            m.SetFloat(ShaderParams.FadeInExtents, fadeInExtents);
            m.SetFloat(ShaderParams.Cloudiness, cloudiness);
            m.SetFloat(ShaderParams.TilingDomain, tilingDomain);
            m.SetFloat(ShaderParams.OneOverTilingDomain, 1.0f / tilingDomain);
            m.SetFloat(ShaderParams.Orientation, orientation);
            m.SetTexture(ShaderParams.Texture, cloudTexture);
            m.SetFloat(ShaderParams.DitherScale, ditherScale);
            m.SetFloat(ShaderParams.MaximumOpacity, maximumOpacity);
            m.SetFloat(ShaderParams.NoiseOption, (int)ditherNoiseType);
        }

        private Vector4 SetShadowOffset(float windDir, float windSpeed, Vector2 offset)
        {
            var radians = windDir * Mathf.Deg2Rad;
            var offsetDirection = new Vector2(-Mathf.Sin(radians), -Mathf.Cos(radians));
            var velocity = offsetDirection * windSpeed;
            offset += velocity * Time.deltaTime;

            return offset;
        }

        // private void LateUpdate()
        // {
        //     // if (followTarget != null)
        //     // {
        //         transform.position = followTarget.position + Vector3.up * cloudLayerRelativeHeight;
        //     // }
        //     // UpdateShaderProperties();
        // }

        public enum DitherNoiseType
        {
            InterleavedGradient,
            Blue
        }
    }
}
