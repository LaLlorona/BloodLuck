using UnityEngine;

namespace InGame.Effects
{
    public class BloodLuckSectorAreaEffectComponent : MonoBehaviour
    {
        [SerializeField] private Material _material;
        [SerializeField] private Color _backgroundColor = new(0.2f, 0.9f, 1f, 0.25f);
        [SerializeField] private Color _fillColor = new(0.2f, 0.9f, 1f, 0.45f);
        [SerializeField] private Color _outlineColor = new(0.6f, 1f, 1f, 1f);
        [SerializeField, Min(0.005f)] private float _outlineWidth = 0.06f;
        [SerializeField, Range(3, 128)] private int _arcSegments = 32;
        [SerializeField] private int _sortingOrder = 10;

        private Transform _fillTransform;
        private bool _isBuilt;

        public float Radius { get; private set; }
        public float AngleDegrees { get; private set; }

        public void Setup(float radius, float angleDegrees)
        {
            Radius = radius;
            AngleDegrees = Mathf.Clamp(angleDegrees, 1f, 360f);
            Build();
            SetProgress(0f);
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

        public void SetProgress(float progress)
        {
            if (!_isBuilt)
            {
                return;
            }

            float clamped = Mathf.Clamp01(progress);
            _fillTransform.localScale = new Vector3(clamped, clamped, 1f);
        }

        private void Build()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }

            var sectorMesh = CreateSectorMesh(Radius, AngleDegrees, _arcSegments);
            CreateMeshObject("Background", sectorMesh, _backgroundColor, _sortingOrder);
            _fillTransform = CreateMeshObject("Fill", sectorMesh, _fillColor, _sortingOrder + 1);
            CreateOutline();
            _isBuilt = true;
        }

        private Transform CreateMeshObject(string objectName, Mesh mesh, Color color, int sortingOrder)
        {
            var child = new GameObject(objectName);
            child.transform.SetParent(transform, false);

            child.AddComponent<MeshFilter>().sharedMesh = mesh;
            var meshRenderer = child.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = _material;
            meshRenderer.sortingOrder = sortingOrder;

            var block = new MaterialPropertyBlock();
            block.SetColor("_Color", color);
            meshRenderer.SetPropertyBlock(block);
            return child.transform;
        }

        private void CreateOutline()
        {
            var child = new GameObject("Outline");
            child.transform.SetParent(transform, false);

            var line = child.AddComponent<LineRenderer>();
            line.sharedMaterial = _material;
            line.useWorldSpace = false;
            line.loop = true;
            line.widthMultiplier = _outlineWidth;
            line.numCornerVertices = 2;
            line.sortingOrder = _sortingOrder + 2;
            line.startColor = _outlineColor;
            line.endColor = _outlineColor;

            var points = GetSectorPoints(Radius, AngleDegrees, _arcSegments);
            line.positionCount = points.Length;
            line.SetPositions(points);
        }

        private static Vector3[] GetSectorPoints(float radius, float angleDegrees, int arcSegments)
        {
            var points = new Vector3[arcSegments + 2];
            points[0] = Vector3.zero;

            float startAngle = -angleDegrees * 0.5f * Mathf.Deg2Rad;
            float step = angleDegrees * Mathf.Deg2Rad / arcSegments;
            for (int i = 0; i <= arcSegments; i++)
            {
                float angle = startAngle + step * i;
                points[i + 1] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
            }

            return points;
        }

        private static Mesh CreateSectorMesh(float radius, float angleDegrees, int arcSegments)
        {
            var vertices = GetSectorPoints(radius, angleDegrees, arcSegments);

            var triangles = new int[arcSegments * 3];
            for (int i = 0; i < arcSegments; i++)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 2;
                triangles[i * 3 + 2] = i + 1;
            }

            var mesh = new Mesh { name = "SectorMesh" };
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
