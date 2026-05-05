using UnityEngine;

namespace HL3.Combat
{
    public sealed class ExplosionVisual : MonoBehaviour
    {
        [SerializeField] private float lifetime = 0.35f;
        [SerializeField] private float maxScale = 6f;

        private Renderer visualRenderer;
        private Material material;
        private float timer;

        public static void Spawn(Vector3 position, float radius)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "HL3_ExplosionVisual";
            sphere.transform.position = position;
            Object.Destroy(sphere.GetComponent<Collider>());
            ExplosionVisual visual = sphere.AddComponent<ExplosionVisual>();
            visual.maxScale = radius * 2f;
        }

        private void Awake()
        {
            visualRenderer = GetComponent<Renderer>();
            material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            material.color = new Color(1f, 0.35f, 0.05f, 0.55f);
            visualRenderer.material = material;
            transform.localScale = Vector3.zero;
        }

        private void Update()
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / lifetime);
            transform.localScale = Vector3.one * Mathf.Lerp(0.1f, maxScale, t);
            material.color = new Color(1f, Mathf.Lerp(0.55f, 0.1f, t), 0.02f, 1f - t);

            if (t >= 1f)
            {
                Destroy(gameObject);
            }
        }
    }
}
