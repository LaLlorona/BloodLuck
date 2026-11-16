using UnityEngine;

namespace InGame.UI
{
    public class SlotDrumProjection
    {
        private const float HiddenFaceCosine = 0.02f;
        private const float DarkestBrightness = 0.3f;

        private readonly float _radius;
        private readonly float _cameraDistance;

        public SlotDrumProjection(float faceHeight, int faceCount, float cameraDistance)
        {
            _radius = faceHeight * 0.5f / Mathf.Tan(Mathf.PI / faceCount);
            _cameraDistance = Mathf.Max(cameraDistance, _radius + faceHeight);
        }

        public bool IsFacingViewer(float faceAngleRadians)
        {
            return Mathf.Cos(faceAngleRadians) > HiddenFaceCosine;
        }

        public Vector2 Project(Vector2 faceLocalPoint, float faceAngleRadians, out float brightness)
        {
            float cos = Mathf.Cos(faceAngleRadians);
            float sin = Mathf.Sin(faceAngleRadians);

            float rotatedY = faceLocalPoint.y * cos + _radius * sin;
            float rotatedZ = _radius * cos - faceLocalPoint.y * sin;

            float perspectiveScale = (_cameraDistance - _radius) / (_cameraDistance - rotatedZ);
            brightness = Mathf.Lerp(DarkestBrightness, 1f, Mathf.Clamp01(rotatedZ / _radius));

            return new Vector2(faceLocalPoint.x * perspectiveScale, rotatedY * perspectiveScale);
        }
    }
}
