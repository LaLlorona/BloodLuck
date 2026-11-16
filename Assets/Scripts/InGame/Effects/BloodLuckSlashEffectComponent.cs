using UnityEngine;

namespace InGame.Effects
{

    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class BloodLuckSlashEffectComponent : MonoBehaviour
    {
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int ProgressId = Shader.PropertyToID("_Progress");
        private static readonly int AlphaId = Shader.PropertyToID("_Alpha");
        private static readonly int TailId = Shader.PropertyToID("_Tail");
        private static readonly int IntensityId = Shader.PropertyToID("_Intensity");

        [SerializeField] private Material _material;
        [SerializeField, ColorUsage(true, true)] private Color _color = Color.white;
        [SerializeField, Min(0.01f)] private float _duration = 0.25f;
        [SerializeField, Range(0.05f, 1f)] private float _tail = 0.6f;
        [SerializeField, Range(0f, 8f)] private float _intensity = 3f;
        [SerializeField, Range(0.05f, 0.95f)] private float _innerRadiusRatio = 0.35f;
        [SerializeField, Range(0f, 1f)] private float _fadeOutStart = 0.7f;
        [SerializeField] private AnimationCurve _progressCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField, Range(3, 128)] private int _arcSegments = 48;
        [SerializeField] private int _sortingOrder = 20;

        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private MaterialPropertyBlock _block;
        private Mesh _mesh;
        private bool _isInitialized;

        public float Duration => _duration;
        public float Radius { get; private set; } = 2f;
        public float AngleDegrees { get; private set; } = 140f;

        private void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
            _block = new MaterialPropertyBlock();
            _meshRenderer.sharedMaterial = _material;
            _meshRenderer.sortingOrder = _sortingOrder;
            _meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _meshRenderer.receiveShadows = false;
            _isInitialized = true;
            ApplyProgress(0f, 0f);
        }

        private void OnDestroy()
        {
            if (_mesh != null)
            {
                Destroy(_mesh);
            }
        }

        public void Setup(float radius, float angleDegrees)
        {
            Initialize();
            Radius = Mathf.Max(0.01f, radius);
            AngleDegrees = Mathf.Clamp(angleDegrees, 1f, 360f);

            if (_mesh != null)
            {
                Destroy(_mesh);
            }

            _mesh = CreateRingSectorMesh(Radius, Radius * _innerRadiusRatio, AngleDegrees, _arcSegments);
            _meshFilter.sharedMesh = _mesh;
        }

        public void SetDirection(Vector2 direction)
        {
            if (direction.sqrMagnitude < 0.0001f)
            {
                return;
            }

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        // Counter-clockwise by default.
        public void SetClockwise(bool clockwise)
        {
            var scale = transform.localScale;
            scale.y = Mathf.Abs(scale.y) * (clockwise ? -1f : 1f);
            transform.localScale = scale;
        }

        // normalizedTime is 0..1; the progress curve and fade-out are applied here.
        public void SetNormalizedTime(float normalizedTime)
        {
            float t = Mathf.Clamp01(normalizedTime);
            float alpha = 1f - Mathf.InverseLerp(_fadeOutStart, 1f, t);
            ApplyProgress(_progressCurve.Evaluate(t), alpha);
        }

        private void ApplyProgress(float progress, float alpha)
        {
            _meshRenderer.GetPropertyBlock(_block);
            _block.SetColor(ColorId, _color);
            _block.SetFloat(ProgressId, progress);
            _block.SetFloat(AlphaId, alpha);
            _block.SetFloat(TailId, _tail);
            _block.SetFloat(IntensityId, _intensity);
            _meshRenderer.SetPropertyBlock(_block);
        }
        private static Mesh CreateRingSectorMesh(float outerRadius, float innerRadius, float angleDegrees, int segments)
        {
            var vertices = new Vector3[(segments + 1) * 2];
            var uvs = new Vector2[vertices.Length];
            var triangles = new int[segments * 6];

            float start = -angleDegrees * 0.5f * Mathf.Deg2Rad;
            float step = angleDegrees * Mathf.Deg2Rad / segments;

            for (int i = 0; i <= segments; i++)
            {
                float angle = start + step * i;
                var dir = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
                float u = (float)i / segments;

                vertices[i * 2] = dir * innerRadius;
                vertices[i * 2 + 1] = dir * outerRadius;
                uvs[i * 2] = new Vector2(u, 0f);
                uvs[i * 2 + 1] = new Vector2(u, 1f);
            }

            for (int i = 0; i < segments; i++)
            {
                int a = i * 2;
                int index = i * 6;
                triangles[index] = a;
                triangles[index + 1] = a + 1;
                triangles[index + 2] = a + 2;
                triangles[index + 3] = a + 2;
                triangles[index + 4] = a + 1;
                triangles[index + 5] = a + 3;
            }

            var mesh = new Mesh { name = "SlashRingSectorMesh" };
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
