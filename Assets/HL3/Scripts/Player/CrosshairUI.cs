using UnityEngine;
using UnityEngine.UI;

namespace HL3.Player
{
    public sealed class CrosshairUI : MonoBehaviour
    {
        [SerializeField] private float size = 14f;
        [SerializeField] private float gap = 7f;
        [SerializeField] private float thickness = 3f;
        [SerializeField] private Color color = Color.white;

        private Canvas canvas;

        private void Awake()
        {
            EnsureCanvasCrosshair();
        }

        private void OnEnable()
        {
            EnsureCanvasCrosshair();
        }

        private void Update()
        {
            if (canvas == null)
            {
                EnsureCanvasCrosshair();
            }
        }

        private void EnsureCanvasCrosshair()
        {
            if (canvas != null)
            {
                canvas.gameObject.SetActive(true);
                return;
            }

            GameObject existing = GameObject.Find("HL3_CrosshairCanvas");
            if (existing != null && existing.TryGetComponent(out canvas))
            {
                canvas.sortingOrder = 32767;
                return;
            }

            canvas = new GameObject("HL3_CrosshairCanvas").AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32767;
            canvas.pixelPerfect = true;
            Object.DontDestroyOnLoad(canvas.gameObject);

            CanvasScaler scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            canvas.gameObject.AddComponent<GraphicRaycaster>();

            AddLine("Left", new Vector2(-(gap + size * 0.5f), 0f), new Vector2(size, thickness));
            AddLine("Right", new Vector2(gap + size * 0.5f, 0f), new Vector2(size, thickness));
            AddLine("Top", new Vector2(0f, gap + size * 0.5f), new Vector2(thickness, size));
            AddLine("Bottom", new Vector2(0f, -(gap + size * 0.5f)), new Vector2(thickness, size));
        }

        private void AddLine(string lineName, Vector2 anchoredPosition, Vector2 dimensions)
        {
            GameObject line = new GameObject(lineName);
            line.transform.SetParent(canvas.transform, false);

            Image image = line.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;

            RectTransform rect = line.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = dimensions;
        }
    }
}
