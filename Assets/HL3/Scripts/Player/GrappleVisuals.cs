using UnityEngine;

namespace HL3.Player
{
    [RequireComponent(typeof(DoomLikeFirstPersonController))]
    public sealed class GrappleVisuals : MonoBehaviour
    {
        [SerializeField] private Transform cameraRoot;
        [SerializeField] private Color lineColor = Color.white;
        [SerializeField] private float lineWidth = 0.045f;

        private DoomLikeFirstPersonController controller;
        private LineRenderer lineRenderer;

        private void Awake()
        {
            controller = GetComponent<DoomLikeFirstPersonController>();
            Camera childCamera = GetComponentInChildren<Camera>();
            if (cameraRoot == null && childCamera != null)
            {
                cameraRoot = childCamera.transform;
            }

            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.enabled = false;
            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = true;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth * 0.35f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = lineColor;
            lineRenderer.endColor = lineColor;
        }

        private void LateUpdate()
        {
            if (controller == null || cameraRoot == null || !controller.IsGrappling)
            {
                lineRenderer.enabled = false;
                return;
            }

            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, cameraRoot.position + cameraRoot.forward * 0.35f + cameraRoot.right * 0.18f - cameraRoot.up * 0.12f);
            lineRenderer.SetPosition(1, controller.GrapplePoint);
        }
    }
}
