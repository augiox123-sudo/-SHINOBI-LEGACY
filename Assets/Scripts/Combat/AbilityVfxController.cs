using UnityEngine;

namespace ShinobiLegacy.Combat
{
    /// <summary>
    /// Lightweight runtime particle and line effects for combat abilities.
    /// Uses built-in Unity primitives/particle systems so no external art assets are required.
    /// Replace these first-pass effects with authored VFX prefabs during the art pass.
    /// </summary>
    public sealed class AbilityVfxController : MonoBehaviour
    {
        private static Material particleMaterial;
        private static Material lineMaterial;

        public void PlayFire(Vector3 origin, Vector3 direction, float range, string modifier)
        {
            Color fire = modifier == "fire_meteor"
                ? new Color(1f, 0.22f, 0.025f)
                : new Color(1f, 0.48f, 0.08f);

            if (modifier == "fire_wave")
            {
                for (int i = 1; i <= 3; i++)
                    EmitBurst(origin + direction.normalized * (range * i / 3f), fire, 34, 1.4f, 5f, 0.75f);
            }
            else if (modifier == "fire_dragon")
            {
                EmitCone(origin - transform.right * 0.8f, direction, fire, 65, range, 8f, 0.55f, 8f);
                EmitCone(origin + transform.right * 0.8f, direction, new Color(1f, 0.72f, 0.12f), 65, range, 8f, 0.55f, 8f);
            }
            else if (modifier == "fire_meteor" || modifier == "fire_burst")
            {
                Vector3 impact = origin + direction.normalized * Mathf.Min(range * 0.65f, 5.5f);
                if (Physics.Raycast(origin, direction, out RaycastHit hit, range + 1f,
                    Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                    impact = hit.point;
                EmitBurst(impact, fire, modifier == "fire_meteor" ? 110 : 75,
                    modifier == "fire_meteor" ? 3.5f : 2.5f, 7f, 1.1f);
                EmitRing(impact + Vector3.up * 0.12f, modifier == "fire_meteor" ? 4.5f : 3f, fire);
            }
            else
            {
                EmitCone(origin, direction, fire, 85, range, 7f, 0.45f, 7f);
            }
        }

        public void PlayChakra(Vector3 position, Vector3 forward, string modifier, float radius)
        {
            Color chakra = modifier == "burst_chain"
                ? new Color(0.35f, 0.82f, 1f)
                : modifier == "burst_guard"
                    ? new Color(0.3f, 1f, 0.72f)
                    : new Color(0.25f, 0.62f, 1f);

            EmitBurst(position + Vector3.up * 0.8f, chakra, 90, radius * 0.9f, 5f, 0.8f);
            EmitRing(position + Vector3.up * 0.08f, radius * 1.5f, chakra);

            if (modifier == "burst_storm")
            {
                EmitBurst(position + Vector3.up * 1.1f + forward * 2f, new Color(0.55f, 0.85f, 1f), 70, radius, 4f, 0.7f);
                EmitRing(position + Vector3.up * 0.1f + forward * 2f, radius * 1.3f, chakra);
            }
            else if (modifier == "burst_guard")
            {
                EmitRing(position + Vector3.up * 0.1f, radius * 2f, new Color(0.3f, 1f, 0.72f));
            }
        }

        public void PlayLightning(Vector3 from, Vector3 to)
        {
            GameObject bolt = new GameObject("VFX_ChainLightning");
            LineRenderer line = bolt.AddComponent<LineRenderer>();
            line.positionCount = 7;
            line.useWorldSpace = true;
            line.startWidth = 0.16f;
            line.endWidth = 0.035f;
            line.material = GetLineMaterial();
            line.startColor = new Color(0.35f, 0.85f, 1f, 1f);
            line.endColor = new Color(0.75f, 0.95f, 1f, 0.15f);
            Vector3 delta = to - from;
            Vector3 side = Vector3.Cross(delta.normalized, Vector3.up);
            if (side.sqrMagnitude < 0.01f) side = Vector3.right;
            for (int i = 0; i < 7; i++)
            {
                float t = i / 6f;
                Vector3 point = Vector3.Lerp(from, to, t);
                if (i > 0 && i < 6) point += side * Random.Range(-0.32f, 0.32f) + Vector3.up * Random.Range(-0.2f, 0.2f);
                line.SetPosition(i, point);
            }
            Destroy(bolt, 0.16f);
            EmitBurst(to, new Color(0.45f, 0.85f, 1f), 18, 0.45f, 3f, 0.3f);
        }

        private static void EmitCone(Vector3 origin, Vector3 direction, Color color, int count,
            float range, float angle, float size, float speed)
        {
            GameObject effect = CreateParticleObject("VFX_ElementalStream", origin, color, count, size, speed, 0.65f);
            ParticleSystem ps = effect.GetComponent<ParticleSystem>();
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = angle;
            shape.radius = 0.25f;
            effect.transform.rotation = Quaternion.LookRotation(direction.sqrMagnitude > 0.001f ? direction : Vector3.forward);
            var main = ps.main;
            main.startLifetime = Mathf.Clamp(range / Mathf.Max(1f, speed), 0.35f, 1.4f);
            ps.Emit(count);
            ps.Play();
            Destroy(effect, 1.8f);
        }

        private static void EmitBurst(Vector3 position, Color color, int count, float size, float speed, float lifetime)
        {
            GameObject effect = CreateParticleObject("VFX_ElementalImpact", position, color, count, size, speed, lifetime);
            ParticleSystem ps = effect.GetComponent<ParticleSystem>();
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = Mathf.Max(0.15f, size * 0.35f);
            ps.Emit(count);
            ps.Play();
            Destroy(effect, lifetime + 0.5f);
        }

        private static void EmitRing(Vector3 position, float radius, Color color)
        {
            GameObject ring = new GameObject("VFX_ChakraShockwave");
            ring.transform.position = position;
            LineRenderer line = ring.AddComponent<LineRenderer>();
            const int segments = 48;
            line.positionCount = segments;
            line.loop = true;
            line.useWorldSpace = false;
            line.startWidth = 0.16f;
            line.endWidth = 0.035f;
            line.material = GetLineMaterial();
            line.startColor = new Color(color.r, color.g, color.b, 0.8f);
            line.endColor = new Color(color.r, color.g, color.b, 0.15f);
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                line.SetPosition(i, new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius);
            }
            ring.transform.localScale = Vector3.one * 0.08f;
            ring.AddComponent<ShockwaveFade>().Initialize(1f, 0.24f);
            Destroy(ring, 0.35f);
        }

        private static Material GetLineMaterial()
        {
            if (lineMaterial != null) return lineMaterial;
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            lineMaterial = new Material(shader);
            return lineMaterial;
        }

        private static GameObject CreateParticleObject(string objectName, Vector3 position, Color color,
            int count, float size, float speed, float lifetime)
        {
            GameObject go = new GameObject(objectName);
            go.transform.position = position;
            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = false;
            main.playOnAwake = false;
            main.duration = lifetime;
            main.startLifetime = lifetime;
            main.startSpeed = speed;
            main.startSize = size * 0.16f;
            main.startColor = color;
            main.gravityModifier = 0.12f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = ps.emission;
            emission.enabled = false;
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = GetParticleMaterial();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            return go;
        }

        private static Material GetParticleMaterial()
        {
            if (particleMaterial != null) return particleMaterial;
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            particleMaterial = new Material(shader);
            return particleMaterial;
        }
    }

    public sealed class ShockwaveFade : MonoBehaviour
    {
        private float finalScale;
        private float duration;
        private float elapsed;
        private Renderer targetRenderer;
        private Material runtimeMaterial;

        public void Initialize(float scale, float seconds)
        {
            finalScale = scale;
            duration = Mathf.Max(0.05f, seconds);
            targetRenderer = GetComponent<Renderer>();
            runtimeMaterial = targetRenderer.material;
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.localScale = new Vector3(finalScale * t, 0.035f, finalScale * t);
            if (runtimeMaterial != null)
            {
                Color color = runtimeMaterial.color;
                color.a = 0.65f * (1f - t);
                runtimeMaterial.color = color;
            }
        }

        private void OnDestroy()
        {
            if (runtimeMaterial != null) Destroy(runtimeMaterial);
        }
    }
}
