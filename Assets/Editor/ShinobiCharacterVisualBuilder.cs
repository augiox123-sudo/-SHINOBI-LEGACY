using UnityEditor;
using UnityEngine;

namespace ShinobiLegacy.Editor
{
    /// <summary>Builds a lightweight, original shinobi visual from reusable primitive meshes.</summary>
    public static class ShinobiCharacterVisualBuilder
    {
        private static Material cloth;
        private static Material darkCloth;
        private static Material armor;
        private static Material skin;
        private static Material metal;
        private static Material accent;

        private static Material MakeMaterial(string name, Color color, float metallic = 0f)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            Material material = new Material(shader);
            material.name = name;
            material.color = color;
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
            return material;
        }

        private static void EnsureMaterials()
        {
            if (cloth != null) return;
            cloth = MakeMaterial("SL Shinobi - Indigo Cloth", new Color(0.075f, 0.105f, 0.19f));
            darkCloth = MakeMaterial("SL Shinobi - Dark Wrap", new Color(0.025f, 0.035f, 0.055f));
            armor = MakeMaterial("SL Shinobi - Blue Steel", new Color(0.12f, 0.22f, 0.31f), 0.45f);
            skin = MakeMaterial("SL Shinobi - Skin", new Color(0.68f, 0.48f, 0.34f));
            metal = MakeMaterial("SL Shinobi - Blade", new Color(0.62f, 0.72f, 0.79f), 0.8f);
            accent = MakeMaterial("SL Shinobi - Azure Accent", new Color(0.12f, 0.65f, 0.88f), 0.25f);
        }

        public static void BuildPlayer(Transform parent)
        {
            EnsureMaterials();
            Transform rig = CreateRoot(parent, "VisualRig_ShadowShinobi");
            Part(rig, "Torso", PrimitiveType.Capsule, new Vector3(0f, 0.05f, 0f), new Vector3(0.58f, 0.72f, 0.34f), cloth);
            Part(rig, "ChestArmor", PrimitiveType.Cube, new Vector3(0f, 0.19f, 0.205f), new Vector3(0.48f, 0.38f, 0.12f), armor);
            Part(rig, "ShoulderLeft", PrimitiveType.Sphere, new Vector3(-0.39f, 0.37f, 0f), new Vector3(0.29f, 0.23f, 0.31f), armor);
            Part(rig, "ShoulderRight", PrimitiveType.Sphere, new Vector3(0.39f, 0.37f, 0f), new Vector3(0.29f, 0.23f, 0.31f), armor);
            Part(rig, "Head", PrimitiveType.Sphere, new Vector3(0f, 0.78f, 0f), new Vector3(0.39f, 0.43f, 0.36f), skin);
            Part(rig, "Headband", PrimitiveType.Cube, new Vector3(0f, 0.91f, 0.02f), new Vector3(0.42f, 0.09f, 0.38f), darkCloth);
            Part(rig, "FaceMask", PrimitiveType.Cube, new Vector3(0f, 0.68f, 0.245f), new Vector3(0.32f, 0.19f, 0.12f), darkCloth);
            Part(rig, "HeadbandEmblem", PrimitiveType.Cube, new Vector3(0f, 0.92f, 0.22f), new Vector3(0.12f, 0.055f, 0.025f), accent);
            Part(rig, "WaistWrap", PrimitiveType.Cube, new Vector3(0f, -0.34f, 0f), new Vector3(0.58f, 0.17f, 0.39f), darkCloth);
            Part(rig, "Belt", PrimitiveType.Cube, new Vector3(0f, -0.29f, 0.205f), new Vector3(0.48f, 0.07f, 0.035f), accent);
            Part(rig, "ArmLeft", PrimitiveType.Capsule, new Vector3(-0.43f, -0.02f, 0f), new Vector3(0.19f, 0.48f, 0.19f), cloth).localRotation = Quaternion.Euler(0f, 0f, -12f);
            Part(rig, "ArmRight", PrimitiveType.Capsule, new Vector3(0.43f, -0.02f, 0f), new Vector3(0.19f, 0.48f, 0.19f), cloth).localRotation = Quaternion.Euler(0f, 0f, 12f);
            Part(rig, "ForearmGuardLeft", PrimitiveType.Cube, new Vector3(-0.48f, -0.2f, 0.08f), new Vector3(0.22f, 0.22f, 0.24f), armor);
            Part(rig, "ForearmGuardRight", PrimitiveType.Cube, new Vector3(0.48f, -0.2f, 0.08f), new Vector3(0.22f, 0.22f, 0.24f), armor);
            Part(rig, "LegLeft", PrimitiveType.Capsule, new Vector3(-0.18f, -0.76f, 0f), new Vector3(0.22f, 0.48f, 0.23f), darkCloth);
            Part(rig, "LegRight", PrimitiveType.Capsule, new Vector3(0.18f, -0.76f, 0f), new Vector3(0.22f, 0.48f, 0.23f), darkCloth);
            Part(rig, "ShinGuardLeft", PrimitiveType.Cube, new Vector3(-0.18f, -0.78f, 0.12f), new Vector3(0.19f, 0.28f, 0.09f), armor);
            Part(rig, "ShinGuardRight", PrimitiveType.Cube, new Vector3(0.18f, -0.78f, 0.12f), new Vector3(0.19f, 0.28f, 0.09f), armor);
            Part(rig, "SandalsLeft", PrimitiveType.Cube, new Vector3(-0.18f, -1.02f, 0.07f), new Vector3(0.25f, 0.09f, 0.34f), skin);
            Part(rig, "SandalsRight", PrimitiveType.Cube, new Vector3(0.18f, -1.02f, 0.07f), new Vector3(0.25f, 0.09f, 0.34f), skin);
            Transform sword = Part(rig, "SheathedKatana", PrimitiveType.Cube, new Vector3(0.34f, -0.18f, -0.2f), new Vector3(0.075f, 0.9f, 0.08f), metal);
            sword.localRotation = Quaternion.Euler(0f, 0f, -28f);
            Transform handle = Part(rig, "KatanaHandle", PrimitiveType.Cube, new Vector3(0.56f, 0.2f, -0.2f), new Vector3(0.09f, 0.25f, 0.09f), darkCloth);
            handle.localRotation = Quaternion.Euler(0f, 0f, -28f);
        }

        public static void BuildEnemy(Transform parent, bool boss = false)
        {
            EnsureMaterials();
            Transform rig = CreateRoot(parent, boss ? "VisualRig_StoneGuardian" : "VisualRig_Rogue");
            float scale = boss ? 1.35f : 1f;
            Material body = boss ? armor : darkCloth;
            Part(rig, "Torso", PrimitiveType.Capsule, new Vector3(0f, 0.05f * scale, 0f), new Vector3(0.66f, 0.82f, 0.43f) * scale, body);
            Part(rig, "Head", PrimitiveType.Sphere, new Vector3(0f, 0.82f * scale, 0f), new Vector3(0.4f, 0.42f, 0.38f) * scale, boss ? metal : skin);
            Part(rig, "Mask", PrimitiveType.Cube, new Vector3(0f, 0.7f * scale, 0.23f * scale), new Vector3(0.34f, 0.17f, 0.1f) * scale, darkCloth);
            Part(rig, "ShoulderLeft", PrimitiveType.Sphere, new Vector3(-0.43f * scale, 0.38f * scale, 0f), new Vector3(0.32f, 0.28f, 0.34f) * scale, armor);
            Part(rig, "ShoulderRight", PrimitiveType.Sphere, new Vector3(0.43f * scale, 0.38f * scale, 0f), new Vector3(0.32f, 0.28f, 0.34f) * scale, armor);
            Part(rig, "LegLeft", PrimitiveType.Capsule, new Vector3(-0.2f * scale, -0.68f * scale, 0f), new Vector3(0.23f, 0.5f, 0.24f) * scale, body);
            Part(rig, "LegRight", PrimitiveType.Capsule, new Vector3(0.2f * scale, -0.68f * scale, 0f), new Vector3(0.23f, 0.5f, 0.24f) * scale, body);
            Part(rig, "Weapon", PrimitiveType.Cube, new Vector3(0.55f * scale, -0.05f * scale, 0.08f), new Vector3(0.08f, 0.95f, 0.08f) * scale, metal).localRotation = Quaternion.Euler(0f, 0f, -25f);
        }

        private static Transform CreateRoot(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing != null) Object.DestroyImmediate(existing.gameObject);
            GameObject root = new GameObject(name);
            root.transform.SetParent(parent, false);
            return root.transform;
        }

        private static Transform Part(Transform parent, string name, PrimitiveType type, Vector3 position, Vector3 scale, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            Renderer renderer = part.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            Collider collider = part.GetComponent<Collider>();
            if (collider != null) Object.DestroyImmediate(collider);
            return part.transform;
        }
    }
}