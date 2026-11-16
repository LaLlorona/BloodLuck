using UnityEngine;
using UnityEngine.UI;

namespace InGame.UI
{
    public class SlotDrumFaceMeshEffect : BaseMeshEffect
    {
        private SlotDrumProjection _projection;
        private float _faceAngleRadians;

        public void Configure(SlotDrumProjection projection)
        {
            _projection = projection;
            RefreshMesh();
        }

        public void SetFaceAngle(float faceAngleRadians)
        {
            _faceAngleRadians = faceAngleRadians;
            RefreshMesh();
        }

        public override void ModifyMesh(VertexHelper vertexHelper)
        {
            if (!IsActive() || _projection == null || vertexHelper.currentVertCount == 0)
            {
                return;
            }

            if (!_projection.IsFacingViewer(_faceAngleRadians))
            {
                vertexHelper.Clear();
                return;
            }

            var vertex = new UIVertex();
            for (int i = 0; i < vertexHelper.currentVertCount; i++)
            {
                vertexHelper.PopulateUIVertex(ref vertex, i);
                vertex.position = _projection.Project(vertex.position, _faceAngleRadians, out float brightness);
                vertex.color = ScaleBrightness(vertex.color, brightness);
                vertexHelper.SetUIVertex(vertex, i);
            }
        }

        private void RefreshMesh()
        {
            if (graphic != null)
            {
                graphic.SetVerticesDirty();
            }
        }

        private static Color32 ScaleBrightness(Color32 color, float brightness)
        {
            return new Color32(
                (byte)(color.r * brightness),
                (byte)(color.g * brightness),
                (byte)(color.b * brightness),
                color.a);
        }
    }
}
