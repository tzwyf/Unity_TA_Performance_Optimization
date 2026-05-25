using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TA.VFX
{
    public class VFXGenerator : EditorWindow
    {
        private const string MenuPath = "TA Tools/Generate Particle Effects";
        private const string InstallMenuPath = "TA Tools/Install Showcase Particle Effects";
        private const string InstallOfficeLightFogMenuPath = "TA Tools/Install Office Light Fog";
        private const string TextureDir = "Assets/Art/Textures/Particles";
        private const string MaterialDir = "Assets/Art/Materials/Particles";
        private const string GeneratedMeshDir = "Assets/Art/Models/Generated";
        private const string PrefabDir = "Assets/Prefabs/Particles/VFX";
        private const string ShowcaseScenePath = "Assets/Scenes/Showcase_Scene.unity";

        [MenuItem(MenuPath)]
        public static void ShowWindow()
        {
            GetWindow<VFXGenerator>("VFX Generator");
        }

        [MenuItem(InstallMenuPath)]
        public static void GenerateAllAndInstallInShowcaseScene()
        {
            GenerateAll();
            InstallShowcaseInstances();
        }

        [MenuItem(InstallOfficeLightFogMenuPath)]
        public static void GenerateAllAndInstallOfficeLightFog()
        {
            GenerateAll();
            InstallOfficeLightFog();
        }

        private void OnGUI()
        {
            GUILayout.Label("TA Performance Optimization - VFX Generator", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("Generate Particle Prefabs", GUILayout.Height(36)))
            {
                GenerateAll();
            }

            if (GUILayout.Button("Generate And Install Into Showcase Scene", GUILayout.Height(36)))
            {
                GenerateAllAndInstallInShowcaseScene();
            }

            if (GUILayout.Button("Generate And Install Office Light Fog", GUILayout.Height(36)))
            {
                GenerateAllAndInstallOfficeLightFog();
            }

            GUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "Generates:\n" +
                "- T_SoftParticle.png and T_ElectricSpark.png\n" +
                "- Dust, spark, coffee steam, and office light fog materials\n" +
                "- VFX_DustAir, VFX_ElectricSparks, VFX_CoffeeSteam, and VFX_OfficeLightFog prefabs\n\n" +
                "Office Light Fog is a heavier but scene-appropriate indoor lighting haze effect.",
                MessageType.Info);
        }

        public static void GenerateAll()
        {
            EnsureDirectories();

            Texture2D softTex = GenerateSoftParticleTexture();
            string softTexPath = $"{TextureDir}/T_SoftParticle.png";
            SaveTextureAsPNG(softTex, softTexPath);
            ConfigureTextureImporter(softTexPath);
            Texture2D importedSoftTex = AssetDatabase.LoadAssetAtPath<Texture2D>(softTexPath);

            Texture2D sparkTex = GenerateSparkTexture();
            string sparkTexPath = $"{TextureDir}/T_ElectricSpark.png";
            SaveTextureAsPNG(sparkTex, sparkTexPath);
            ConfigureTextureImporter(sparkTexPath);
            Texture2D importedSparkTex = AssetDatabase.LoadAssetAtPath<Texture2D>(sparkTexPath);

            Material dustMaterial = CreateParticleMaterial(
                $"{MaterialDir}/M_DustAir.mat",
                importedSoftTex,
                new Color(0.78f, 0.82f, 0.86f, 0.36f),
                additive: false);

            Material sparkMaterial = CreateParticleMaterial(
                $"{MaterialDir}/M_ElectricSparks.mat",
                importedSparkTex,
                new Color(1.7f, 2.2f, 2.6f, 1f),
                additive: true);

            Material sparkTrailMaterial = CreateParticleMaterial(
                $"{MaterialDir}/M_ElectricSparks_Trail.mat",
                importedSparkTex,
                new Color(1.4f, 1.9f, 2.4f, 0.85f),
                additive: true);

            Material steamMaterial = CreateParticleMaterial(
                $"{MaterialDir}/M_CoffeeSteam.mat",
                importedSoftTex,
                new Color(0.84f, 0.8f, 0.74f, 0.2f),
                additive: false);

            Material officeLightFogMaterial = CreateParticleMaterial(
                $"{MaterialDir}/M_OfficeLightFog_Haze.mat",
                importedSoftTex,
                new Color(0.66f, 0.72f, 0.78f, 0.26f),
                additive: false);

            Material officeLightBeamMaterial = CreateParticleMaterial(
                $"{MaterialDir}/M_OfficeLightFog_Beam.mat",
                importedSoftTex,
                new Color(0.78f, 0.84f, 0.9f, 0.18f),
                additive: false);

            GenerateDustAirEffect(dustMaterial);
            GenerateElectricSparkEffect(sparkMaterial, sparkTrailMaterial);
            GenerateCoffeeSteamEffect(steamMaterial);
            GenerateOfficeLightFogEffect(officeLightFogMaterial, officeLightBeamMaterial);
            DeleteObsoleteGeneratedEffects();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Generated particle VFX prefabs in {PrefabDir}");
        }

        private static void InstallShowcaseInstances()
        {
            Scene scene = EditorSceneManager.OpenScene(ShowcaseScenePath, OpenSceneMode.Single);

            GameObject existing = GameObject.Find("VFX_RoomAtmosphere");
            if (existing != null)
            {
                DestroyImmediate(existing);
            }

            GameObject root = new GameObject("VFX_RoomAtmosphere");
            root.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            GameObject dustPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/VFX_DustAir.prefab");
            GameObject sparkPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/VFX_ElectricSparks.prefab");
            GameObject steamPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/VFX_CoffeeSteam.prefab");
            GameObject fogPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/VFX_OfficeLightFog.prefab");

            if (dustPrefab == null || sparkPrefab == null || steamPrefab == null || fogPrefab == null)
            {
                Debug.LogError("Particle prefabs are missing. Run GenerateAll before installing showcase instances.");
                return;
            }

            GameObject dust = (GameObject)PrefabUtility.InstantiatePrefab(dustPrefab, scene);
            dust.name = "VFX_DustAir_ShowcaseVolume";
            dust.transform.SetParent(root.transform);
            dust.transform.SetLocalPositionAndRotation(new Vector3(2.6f, 2.7f, 39.7f), Quaternion.identity);
            dust.transform.localScale = Vector3.one;

            CreateSparkInstance(sparkPrefab, root.transform, "VFX_ElectricSparks_ServerRack_R", new Vector3(7.58f, 2.05f, 48.25f), new Vector3(0f, -90f, 0f));
            CreateSparkInstance(sparkPrefab, root.transform, "VFX_ElectricSparks_ServerRack_L", new Vector3(-1.55f, 2.12f, 48.3f), new Vector3(0f, 90f, 0f));
            CreateSparkInstance(sparkPrefab, root.transform, "VFX_ElectricSparks_Panel", new Vector3(4.7f, 1.55f, 30.85f), new Vector3(0f, 20f, 0f));
            CreateSparkInstance(steamPrefab, root.transform, "VFX_CoffeeSteam_GlassCup", new Vector3(7.67f, 1.2f, 46.72f), Vector3.zero);

            Vector3[] lightPositions = new[]
            {
                new Vector3(0.5177f, 3.99f, 31.74f),
                new Vector3(6.0902f, 3.99f, 31.74f),
                new Vector3(5.9052f, 3.99f, 37.0f),
                new Vector3(0.5177f, 3.99f, 37.0f),
                new Vector3(4.0702f, 3.99f, 42.52f),
                new Vector3(-1.9298f, 3.99f, 42.52f),
                new Vector3(4.0702f, 3.99f, 47.84f),
                new Vector3(-1.9298f, 3.99f, 47.84f)
            };

            for (int i = 0; i < lightPositions.Length; i++)
            {
                GameObject fog = (GameObject)PrefabUtility.InstantiatePrefab(fogPrefab, scene);
                fog.name = $"VFX_OfficeLightFog_Light_{i + 1}";
                fog.transform.SetParent(root.transform);
                fog.transform.SetPositionAndRotation(lightPositions[i], Quaternion.identity);
                fog.transform.localScale = Vector3.one;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"Installed indoor dust, electric spark, coffee steam, and office light fog instances into {ShowcaseScenePath}");
        }

        private static void InstallOfficeLightFog()
        {
            Scene scene = EditorSceneManager.OpenScene(ShowcaseScenePath, OpenSceneMode.Single);

            GameObject existing = GameObject.Find("VFX_OfficeLightFogRoot");
            if (existing != null)
            {
                DestroyImmediate(existing);
            }

            GameObject obsoleteStress = GameObject.Find("VFX_ParticleBombardment_StressRoot");
            if (obsoleteStress != null)
            {
                DestroyImmediate(obsoleteStress);
            }

            GameObject obsoleteOffice = GameObject.Find("VFX_ScifiOfficeAtmosphereRoot");
            if (obsoleteOffice != null)
            {
                DestroyImmediate(obsoleteOffice);
            }

            GameObject fogPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/VFX_OfficeLightFog.prefab");
            if (fogPrefab == null)
            {
                Debug.LogError("Office light fog prefab is missing. Run GenerateAll before installing it.");
                return;
            }

            GameObject root = new GameObject("VFX_OfficeLightFogRoot");
            root.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            Vector3[] lightPositions = new[]
            {
                new Vector3(0.5177f, 3.99f, 31.74f),
                new Vector3(6.0902f, 3.99f, 31.74f),
                new Vector3(5.9052f, 3.99f, 37.0f),
                new Vector3(0.5177f, 3.99f, 37.0f),
                new Vector3(4.0702f, 3.99f, 42.52f),
                new Vector3(-1.9298f, 3.99f, 42.52f),
                new Vector3(4.0702f, 3.99f, 47.84f),
                new Vector3(-1.9298f, 3.99f, 47.84f)
            };

            for (int i = 0; i < lightPositions.Length; i++)
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(fogPrefab, scene);
                instance.name = $"VFX_OfficeLightFog_Light_{i + 1}";
                instance.transform.SetParent(root.transform);
                instance.transform.SetPositionAndRotation(lightPositions[i], Quaternion.identity);
                instance.transform.localScale = Vector3.one;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"Installed office light fog for 8 ceiling lights into {ShowcaseScenePath}");
        }

        private static void CreateSparkInstance(GameObject prefab, Transform parent, string name, Vector3 position, Vector3 eulerAngles)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent.gameObject.scene);
            instance.name = name;
            instance.transform.SetParent(parent);
            instance.transform.SetLocalPositionAndRotation(position, Quaternion.Euler(eulerAngles));
            instance.transform.localScale = Vector3.one;
        }

        private static void EnsureDirectories()
        {
            if (!Directory.Exists(TextureDir))
            {
                Directory.CreateDirectory(TextureDir);
            }

            if (!Directory.Exists(MaterialDir))
            {
                Directory.CreateDirectory(MaterialDir);
            }

            if (!Directory.Exists(GeneratedMeshDir))
            {
                Directory.CreateDirectory(GeneratedMeshDir);
            }

            if (!Directory.Exists(PrefabDir))
            {
                Directory.CreateDirectory(PrefabDir);
            }
        }

        private static Texture2D GenerateSoftParticleTexture()
        {
            const int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];
            float center = (size - 1) * 0.5f;
            float radius = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - center) / radius;
                    float dy = (y - center) / radius;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = Mathf.Clamp01(1f - dist);
                    alpha = Mathf.SmoothStep(0f, 1f, alpha);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateSparkTexture()
        {
            const int width = 256;
            const int height = 64;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[width * height];
            float centerY = (height - 1) * 0.5f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float u = Mathf.Abs((x / (float)(width - 1)) * 2f - 1f);
                    float v = Mathf.Abs((y - centerY) / centerY);
                    float core = Mathf.Pow(Mathf.Clamp01(1f - v), 10f);
                    float lengthFade = Mathf.Pow(Mathf.Clamp01(1f - u), 0.55f);
                    float sideForks = Mathf.Clamp01(Mathf.Sin((x * 0.17f) + (y * 0.63f)) * 0.5f + 0.5f) * Mathf.Pow(1f - v, 4f);
                    float alpha = Mathf.Clamp01((core * 0.9f + sideForks * 0.22f) * lengthFade);
                    pixels[y * width + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        private static void SaveTextureAsPNG(Texture2D tex, string path)
        {
            File.WriteAllBytes(path, tex.EncodeToPNG());
            AssetDatabase.ImportAsset(path);
        }

        private static void ConfigureTextureImporter(string path)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            importer.textureType = TextureImporterType.Default;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
        }

        private static Material CreateParticleMaterial(string path, Texture2D texture, Color baseColor, bool additive)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            else
            {
                material.shader = shader;
            }

            material.name = Path.GetFileNameWithoutExtension(path);
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", additive ? 1f : 0f);
            material.SetInt("_SrcBlend", additive ? (int)UnityEngine.Rendering.BlendMode.One : (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", additive ? (int)UnityEngine.Rendering.BlendMode.One : (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;

            if (material.HasProperty("_BaseMap"))
            {
                material.SetTexture("_BaseMap", texture);
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", baseColor);
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static void GenerateDustAirEffect(Material material)
        {
            GameObject root = new GameObject("VFX_DustAir");

            ParticleSystem ambient = CreateParticleSystem(root.transform, "Ambient_Dust_Motes", material);
            ConfigureAmbientDust(ambient);

            ParticleSystem shafts = CreateParticleSystem(root.transform, "Light_Shaft_Motes", material);
            ConfigureLightShaftDust(shafts);

            ParticleSystem haze = CreateParticleSystem(root.transform, "Soft_Room_Haze", material);
            ConfigureSoftRoomHaze(haze);

            SavePrefab(root, $"{PrefabDir}/VFX_DustAir.prefab");
        }

        private static void GenerateElectricSparkEffect(Material material, Material trailMaterial)
        {
            GameObject root = new GameObject("VFX_ElectricSparks");

            ParticleSystem arcs = CreateParticleSystem(root.transform, "Arc_Streaks", material);
            ConfigureArcStreaks(arcs, trailMaterial);

            ParticleSystem hotPoints = CreateParticleSystem(root.transform, "Hot_Spark_Points", material);
            ConfigureHotSparkPoints(hotPoints);

            ParticleSystem glow = CreateParticleSystem(root.transform, "Blue_Flash_Glow", material);
            ConfigureBlueFlashGlow(glow);

            SavePrefab(root, $"{PrefabDir}/VFX_ElectricSparks.prefab");
        }

        private static void GenerateCoffeeSteamEffect(Material material)
        {
            GameObject root = new GameObject("VFX_CoffeeSteam");

            ParticleSystem wisps = CreateParticleSystem(root.transform, "Steam_Wisps", material);
            ConfigureSteamWisps(wisps);

            ParticleSystem softPuffs = CreateParticleSystem(root.transform, "Steam_Soft_Puffs", material);
            ConfigureSteamSoftPuffs(softPuffs);

            SavePrefab(root, $"{PrefabDir}/VFX_CoffeeSteam.prefab");
        }

        private static void GenerateOfficeLightFogEffect(Material hazeMaterial, Material beamMaterial)
        {
            GameObject root = new GameObject("VFX_OfficeLightFog");
            Mesh beamMesh = CreateOrUpdateLightFogBeamMesh();

            ParticleSystem ceilingBeam = CreateParticleSystem(root.transform, "Ceiling_Light_Volume", beamMaterial);
            ConfigureOfficeLightBeamVolume(ceilingBeam, beamMesh);

            ParticleSystem lightDust = CreateParticleSystem(root.transform, "Lit_Dust_In_Beams", hazeMaterial);
            ConfigureOfficeLitDust(lightDust);

            ParticleSystem localFog = CreateParticleSystem(root.transform, "Local_Soft_Light_Fog", hazeMaterial);
            ConfigureLocalSoftFog(localFog);

            SavePrefab(root, $"{PrefabDir}/VFX_OfficeLightFog.prefab");
        }

        private static ParticleSystem CreateParticleSystem(Transform parent, string name, Material material)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            go.transform.localScale = Vector3.one;

            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.sortMode = ParticleSystemSortMode.Distance;

            return ps;
        }

        private static void ConfigureAmbientDust(ParticleSystem ps)
        {
            var main = ps.main;
            main.duration = 12f;
            main.loop = true;
            main.prewarm = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(8f, 16f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.012f, 0.05f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.045f, 0.18f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = new Color(0.78f, 0.83f, 0.88f, 0.42f);
            main.gravityModifier = 0f;
            main.maxParticles = 420;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.scalingMode = ParticleSystemScalingMode.Local;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 34f;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(14f, 4.5f, 22f);
            shape.position = new Vector3(0f, 0.15f, 0f);
            shape.randomPositionAmount = 0.35f;

            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            velocity.x = new ParticleSystem.MinMaxCurve(-0.035f, 0.035f);
            velocity.y = new ParticleSystem.MinMaxCurve(-0.008f, 0.03f);
            velocity.z = new ParticleSystem.MinMaxCurve(-0.035f, 0.035f);

            var noise = ps.noise;
            noise.enabled = true;
            noise.quality = ParticleSystemNoiseQuality.Low;
            noise.strength = 0.18f;
            noise.frequency = 0.24f;
            noise.scrollSpeed = 0.05f;
            noise.octaveCount = 1;

            var size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, CreateFadeSizeCurve(0.5f));

            var color = ps.colorOverLifetime;
            color.enabled = true;
            color.color = CreateDustGradient(0.42f);

            var rotation = ps.rotationOverLifetime;
            rotation.enabled = true;
            rotation.z = new ParticleSystem.MinMaxCurve(-0.08f, 0.08f);

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingOrder = 0;
        }

        private static void ConfigureLightShaftDust(ParticleSystem ps)
        {
            var main = ps.main;
            main.duration = 8f;
            main.loop = true;
            main.prewarm = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(5f, 10f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.02f, 0.08f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.065f, 0.26f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = new Color(0.84f, 0.9f, 0.96f, 0.55f);
            main.gravityModifier = 0f;
            main.maxParticles = 220;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.scalingMode = ParticleSystemScalingMode.Local;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 18f;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(4.8f, 3.6f, 18f);
            shape.position = new Vector3(1.2f, 0.35f, -0.6f);
            shape.rotation = new Vector3(0f, 12f, 0f);
            shape.randomPositionAmount = 0.2f;

            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            velocity.x = new ParticleSystem.MinMaxCurve(-0.02f, 0.055f);
            velocity.y = new ParticleSystem.MinMaxCurve(0.005f, 0.05f);
            velocity.z = new ParticleSystem.MinMaxCurve(-0.025f, 0.025f);

            var noise = ps.noise;
            noise.enabled = true;
            noise.quality = ParticleSystemNoiseQuality.Low;
            noise.strength = 0.24f;
            noise.frequency = 0.36f;
            noise.scrollSpeed = 0.08f;
            noise.octaveCount = 1;

            var size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, CreateFadeSizeCurve(0.48f));

            var color = ps.colorOverLifetime;
            color.enabled = true;
            color.color = CreateDustGradient(0.62f);

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingOrder = 1;
        }

        private static void ConfigureSoftRoomHaze(ParticleSystem ps)
        {
            var main = ps.main;
            main.duration = 14f;
            main.loop = true;
            main.prewarm = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(10f, 18f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.006f, 0.02f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.35f, 0.9f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = new Color(0.66f, 0.72f, 0.78f, 0.08f);
            main.gravityModifier = 0f;
            main.maxParticles = 72;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.scalingMode = ParticleSystemScalingMode.Local;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 5f;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(14f, 4.8f, 22f);
            shape.position = new Vector3(0f, 0.25f, 0f);
            shape.randomPositionAmount = 0.45f;

            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            velocity.x = new ParticleSystem.MinMaxCurve(-0.018f, 0.018f);
            velocity.y = new ParticleSystem.MinMaxCurve(-0.004f, 0.018f);
            velocity.z = new ParticleSystem.MinMaxCurve(-0.018f, 0.018f);

            var noise = ps.noise;
            noise.enabled = true;
            noise.quality = ParticleSystemNoiseQuality.Low;
            noise.strength = 0.09f;
            noise.frequency = 0.18f;
            noise.scrollSpeed = 0.035f;
            noise.octaveCount = 1;

            var size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, CreateFadeSizeCurve(0.7f));

            var color = ps.colorOverLifetime;
            color.enabled = true;
            color.color = CreateDustGradient(0.08f);

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingOrder = -1;
        }

        private static void ConfigureArcStreaks(ParticleSystem ps, Material trailMaterial)
        {
            var main = ps.main;
            main.duration = 1.25f;
            main.loop = true;
            main.prewarm = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.055f, 0.16f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(4f, 10f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.035f, 0.12f);
            main.startColor = new Color(0.68f, 0.9f, 1f, 1f);
            main.gravityModifier = new ParticleSystem.MinMaxCurve(0f, 0.2f);
            main.maxParticles = 36;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.scalingMode = ParticleSystemScalingMode.Local;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[]
            {
                new ParticleSystem.Burst(0.04f, 4, 8, 1, 0.01f),
                new ParticleSystem.Burst(0.62f, 2, 5, 1, 0.01f),
                new ParticleSystem.Burst(1.03f, 3, 7, 1, 0.01f)
            });

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 27f;
            shape.radius = 0.025f;
            shape.length = 0.08f;
            shape.randomDirectionAmount = 0.28f;

            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            velocity.x = new ParticleSystem.MinMaxCurve(-1.3f, 1.3f);
            velocity.y = new ParticleSystem.MinMaxCurve(-0.8f, 1.4f);
            velocity.z = new ParticleSystem.MinMaxCurve(0.3f, 2.2f);

            var size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, CreateSparkSizeCurve());

            var color = ps.colorOverLifetime;
            color.enabled = true;
            color.color = CreateSparkGradient();

            var trails = ps.trails;
            trails.enabled = true;
            trails.mode = ParticleSystemTrailMode.PerParticle;
            trails.ratio = 0.75f;
            trails.lifetime = new ParticleSystem.MinMaxCurve(0.045f, 0.11f);
            trails.minVertexDistance = 0.015f;
            trails.textureMode = ParticleSystemTrailTextureMode.Stretch;
            trails.worldSpace = false;
            trails.dieWithParticles = true;
            trails.widthOverTrail = new ParticleSystem.MinMaxCurve(1f, CreateTrailWidthCurve());
            trails.colorOverTrail = CreateTrailGradient();

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.lengthScale = 3.8f;
            renderer.velocityScale = 0.12f;
            renderer.sortingOrder = 3;
            renderer.trailMaterial = trailMaterial;
        }

        private static void ConfigureHotSparkPoints(ParticleSystem ps)
        {
            var main = ps.main;
            main.duration = 1.25f;
            main.loop = true;
            main.prewarm = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.08f, 0.24f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.8f, 2.4f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.025f, 0.065f);
            main.startColor = new Color(1f, 0.96f, 0.72f, 1f);
            main.gravityModifier = new ParticleSystem.MinMaxCurve(0.2f, 0.55f);
            main.maxParticles = 28;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.scalingMode = ParticleSystemScalingMode.Local;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[]
            {
                new ParticleSystem.Burst(0.08f, 2, 5, 1, 0.01f),
                new ParticleSystem.Burst(0.65f, 1, 3, 1, 0.01f),
                new ParticleSystem.Burst(1.06f, 2, 4, 1, 0.01f)
            });

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.045f;
            shape.randomDirectionAmount = 0.75f;

            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            velocity.x = new ParticleSystem.MinMaxCurve(-1.2f, 1.2f);
            velocity.y = new ParticleSystem.MinMaxCurve(-0.7f, 0.9f);
            velocity.z = new ParticleSystem.MinMaxCurve(-0.6f, 1.2f);

            var size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, CreateSparkSizeCurve());

            var color = ps.colorOverLifetime;
            color.enabled = true;
            color.color = CreateHotPointGradient();

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingOrder = 4;
        }

        private static void ConfigureBlueFlashGlow(ParticleSystem ps)
        {
            var main = ps.main;
            main.duration = 1.25f;
            main.loop = true;
            main.prewarm = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.06f, 0.13f);
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.18f, 0.34f);
            main.startColor = new Color(0.34f, 0.72f, 1f, 0.55f);
            main.gravityModifier = 0f;
            main.maxParticles = 8;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.scalingMode = ParticleSystemScalingMode.Local;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[]
            {
                new ParticleSystem.Burst(0.04f, 1, 2, 1, 0.01f),
                new ParticleSystem.Burst(0.62f, 1, 1, 1, 0.01f),
                new ParticleSystem.Burst(1.03f, 1, 2, 1, 0.01f)
            });

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.02f;

            var size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, CreateFlashSizeCurve());

            var color = ps.colorOverLifetime;
            color.enabled = true;
            color.color = CreateFlashGradient();

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingOrder = 2;
        }

        private static void ConfigureSteamWisps(ParticleSystem ps)
        {
            var main = ps.main;
            main.duration = 6f;
            main.loop = true;
            main.prewarm = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(2.4f, 4.8f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.18f, 0.38f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.16f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = new Color(0.92f, 0.88f, 0.8f, 0.16f);
            main.gravityModifier = -0.035f;
            main.maxParticles = 42;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.scalingMode = ParticleSystemScalingMode.Local;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 8f;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 7f;
            shape.radius = 0.055f;
            shape.length = 0.06f;
            shape.position = new Vector3(0f, 0.02f, 0f);
            shape.randomPositionAmount = 0.14f;
            shape.randomDirectionAmount = 0.18f;

            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            velocity.x = new ParticleSystem.MinMaxCurve(-0.045f, 0.045f);
            velocity.y = new ParticleSystem.MinMaxCurve(0.16f, 0.32f);
            velocity.z = new ParticleSystem.MinMaxCurve(-0.045f, 0.045f);

            var noise = ps.noise;
            noise.enabled = true;
            noise.quality = ParticleSystemNoiseQuality.Low;
            noise.strength = 0.18f;
            noise.frequency = 0.82f;
            noise.scrollSpeed = 0.18f;
            noise.octaveCount = 2;

            var size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, CreateSteamWispSizeCurve());

            var color = ps.colorOverLifetime;
            color.enabled = true;
            color.color = CreateSteamGradient(0.18f);

            var rotation = ps.rotationOverLifetime;
            rotation.enabled = true;
            rotation.z = new ParticleSystem.MinMaxCurve(-0.18f, 0.18f);

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingOrder = 5;
        }

        private static void ConfigureSteamSoftPuffs(ParticleSystem ps)
        {
            var main = ps.main;
            main.duration = 7f;
            main.loop = true;
            main.prewarm = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(3.2f, 5.8f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.08f, 0.2f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.3f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = new Color(0.95f, 0.9f, 0.82f, 0.08f);
            main.gravityModifier = -0.02f;
            main.maxParticles = 24;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.scalingMode = ParticleSystemScalingMode.Local;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 3.5f;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.08f;
            shape.position = new Vector3(0f, 0.1f, 0f);
            shape.randomPositionAmount = 0.35f;

            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            velocity.x = new ParticleSystem.MinMaxCurve(-0.03f, 0.03f);
            velocity.y = new ParticleSystem.MinMaxCurve(0.08f, 0.18f);
            velocity.z = new ParticleSystem.MinMaxCurve(-0.03f, 0.03f);

            var noise = ps.noise;
            noise.enabled = true;
            noise.quality = ParticleSystemNoiseQuality.Low;
            noise.strength = 0.12f;
            noise.frequency = 0.55f;
            noise.scrollSpeed = 0.11f;
            noise.octaveCount = 1;

            var size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, CreateSteamPuffSizeCurve());

            var color = ps.colorOverLifetime;
            color.enabled = true;
            color.color = CreateSteamGradient(0.08f);

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingOrder = 4;
        }

        private static void ConfigureOfficeLightFogBase(ParticleSystem ps)
        {
            var main = ps.main;
            main.duration = 14f;
            main.loop = true;
            main.prewarm = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(9f, 18f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.008f, 0.035f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.28f, 0.85f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = new Color(0.58f, 0.64f, 0.7f, 0.16f);
            main.gravityModifier = 0f;
            main.maxParticles = 360;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.scalingMode = ParticleSystemScalingMode.Local;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 24f;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(13.5f, 4.7f, 21f);
            shape.position = new Vector3(0f, 0.05f, 0f);
            shape.randomPositionAmount = 0.5f;

            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            velocity.x = new ParticleSystem.MinMaxCurve(-0.018f, 0.018f);
            velocity.y = new ParticleSystem.MinMaxCurve(-0.004f, 0.022f);
            velocity.z = new ParticleSystem.MinMaxCurve(-0.018f, 0.018f);

            var noise = ps.noise;
            noise.enabled = true;
            noise.quality = ParticleSystemNoiseQuality.Low;
            noise.strength = 0.11f;
            noise.frequency = 0.18f;
            noise.scrollSpeed = 0.04f;
            noise.octaveCount = 1;

            var size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, CreateOfficeLightFogSizeCurve());

            var color = ps.colorOverLifetime;
            color.enabled = true;
            color.color = CreateOfficeLightFogGradient(0.14f);

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingOrder = -2;
        }

        private static void ConfigureOfficeLightBeamVolume(ParticleSystem ps, Mesh mesh)
        {
            ps.transform.localPosition = new Vector3(0f, -0.6f, 0f);

            var main = ps.main;
            main.duration = 10f;
            main.loop = true;
            main.prewarm = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(5.5f, 10f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.01f, 0.035f);
            main.startSize = new ParticleSystem.MinMaxCurve(1.6f, 3.4f);
            main.startRotation = new ParticleSystem.MinMaxCurve(-0.08f, 0.08f);
            main.startColor = new Color(0.72f, 0.78f, 0.84f, 0.22f);
            main.gravityModifier = 0f;
            main.maxParticles = 140;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.scalingMode = ParticleSystemScalingMode.Local;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 11f;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(2.2f, 3.2f, 2.2f);
            shape.randomPositionAmount = 0.25f;

            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            velocity.x = new ParticleSystem.MinMaxCurve(-0.012f, 0.012f);
            velocity.y = new ParticleSystem.MinMaxCurve(-0.01f, 0.016f);
            velocity.z = new ParticleSystem.MinMaxCurve(-0.018f, 0.018f);

            var noise = ps.noise;
            noise.enabled = true;
            noise.quality = ParticleSystemNoiseQuality.Medium;
            noise.strength = 0.11f;
            noise.frequency = 0.22f;
            noise.scrollSpeed = 0.035f;
            noise.octaveCount = 2;

            var size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, CreateLightBeamSheetSizeCurve());

            var color = ps.colorOverLifetime;
            color.enabled = true;
            color.color = CreateOfficeLightBeamGradient(0.18f);

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Mesh;
            renderer.mesh = mesh;
            renderer.sortingOrder = -1;
        }

        private static void ConfigureOfficeStripLightFog(ParticleSystem ps, Vector3 localPosition, Vector3 localEulerAngles, int variant)
        {
            ps.transform.SetLocalPositionAndRotation(localPosition, Quaternion.Euler(localEulerAngles));

            var main = ps.main;
            main.duration = 9f;
            main.loop = true;
            main.prewarm = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(4f, 8f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.05f, 0.16f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.25f, 0.72f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = new Color(0.62f, 0.7f, 0.78f, 0.14f);
            main.gravityModifier = 0f;
            main.maxParticles = 120;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.scalingMode = ParticleSystemScalingMode.Local;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 13f;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(0.75f, 2.8f, 8.6f);
            shape.randomPositionAmount = 0.35f;

            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            velocity.x = new ParticleSystem.MinMaxCurve(-0.025f, 0.025f);
            velocity.y = new ParticleSystem.MinMaxCurve(-0.006f, 0.026f);
            velocity.z = new ParticleSystem.MinMaxCurve(0.02f, 0.14f);

            var noise = ps.noise;
            noise.enabled = true;
            noise.quality = ParticleSystemNoiseQuality.Low;
            noise.strength = 0.17f;
            noise.frequency = 0.42f + variant * 0.05f;
            noise.scrollSpeed = 0.075f;
            noise.octaveCount = 1;

            var size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, CreateOfficeLightFogSizeCurve());

            var color = ps.colorOverLifetime;
            color.enabled = true;
            color.color = CreateOfficeLightFogGradient(0.16f);

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingOrder = 2 + variant;
        }

        private static void ConfigureOfficeLitDust(ParticleSystem ps)
        {
            ps.transform.localPosition = new Vector3(0f, -1.8f, 0f);

            var main = ps.main;
            main.duration = 10f;
            main.loop = true;
            main.prewarm = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(5f, 10f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.012f, 0.08f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.045f, 0.22f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = new Color(0.82f, 0.86f, 0.88f, 0.58f);
            main.gravityModifier = 0f;
            main.maxParticles = 680;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.scalingMode = ParticleSystemScalingMode.Local;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 62f;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(3.2f, 4.2f, 3.2f);
            shape.position = new Vector3(0f, 0f, 0f);
            shape.randomPositionAmount = 0.4f;

            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            velocity.x = new ParticleSystem.MinMaxCurve(-0.055f, 0.055f);
            velocity.y = new ParticleSystem.MinMaxCurve(-0.008f, 0.055f);
            velocity.z = new ParticleSystem.MinMaxCurve(-0.055f, 0.055f);

            var noise = ps.noise;
            noise.enabled = true;
            noise.quality = ParticleSystemNoiseQuality.Medium;
            noise.strength = 0.28f;
            noise.frequency = 0.38f;
            noise.scrollSpeed = 0.09f;
            noise.octaveCount = 2;

            var size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, CreateFadeSizeCurve(0.5f));

            var color = ps.colorOverLifetime;
            color.enabled = true;
            color.color = CreateOfficeLightFogGradient(0.52f);

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingOrder = 4;
        }

        private static void ConfigureOfficeForegroundFog(ParticleSystem ps)
        {
            ps.transform.localPosition = new Vector3(0f, -0.25f, -3.5f);

            var main = ps.main;
            main.duration = 12f;
            main.loop = true;
            main.prewarm = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(7f, 13f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.006f, 0.025f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.7f, 1.65f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = new Color(0.58f, 0.64f, 0.7f, 0.07f);
            main.gravityModifier = 0f;
            main.maxParticles = 80;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.scalingMode = ParticleSystemScalingMode.Local;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 6f;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(10f, 2.6f, 8f);
            shape.randomPositionAmount = 0.4f;

            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            velocity.x = new ParticleSystem.MinMaxCurve(-0.012f, 0.012f);
            velocity.y = new ParticleSystem.MinMaxCurve(-0.002f, 0.014f);
            velocity.z = new ParticleSystem.MinMaxCurve(-0.012f, 0.012f);

            var noise = ps.noise;
            noise.enabled = true;
            noise.quality = ParticleSystemNoiseQuality.Low;
            noise.strength = 0.07f;
            noise.frequency = 0.16f;
            noise.scrollSpeed = 0.03f;
            noise.octaveCount = 1;

            var color = ps.colorOverLifetime;
            color.enabled = true;
            color.color = CreateOfficeLightFogGradient(0.07f);

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingOrder = -3;
        }

        private static void ConfigureLocalSoftFog(ParticleSystem ps)
        {
            ps.transform.localPosition = new Vector3(0f, -1.2f, 0f);

            var main = ps.main;
            main.duration = 12f;
            main.loop = true;
            main.prewarm = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(6f, 12f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.006f, 0.025f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.65f, 1.45f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = new Color(0.58f, 0.64f, 0.7f, 0.14f);
            main.gravityModifier = 0f;
            main.maxParticles = 100;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.scalingMode = ParticleSystemScalingMode.Local;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 8f;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(4.5f, 5f, 4.5f);
            shape.randomPositionAmount = 0.45f;

            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            velocity.x = new ParticleSystem.MinMaxCurve(-0.015f, 0.015f);
            velocity.y = new ParticleSystem.MinMaxCurve(-0.003f, 0.018f);
            velocity.z = new ParticleSystem.MinMaxCurve(-0.015f, 0.015f);

            var noise = ps.noise;
            noise.enabled = true;
            noise.quality = ParticleSystemNoiseQuality.Low;
            noise.strength = 0.09f;
            noise.frequency = 0.16f;
            noise.scrollSpeed = 0.035f;
            noise.octaveCount = 1;

            var color = ps.colorOverLifetime;
            color.enabled = true;
            color.color = CreateOfficeLightFogGradient(0.12f);

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingOrder = -2;
        }

        private static AnimationCurve CreateFadeSizeCurve(float start)
        {
            return new AnimationCurve(
                new Keyframe(0f, start),
                new Keyframe(0.18f, 1f),
                new Keyframe(0.82f, 1f),
                new Keyframe(1f, 0.15f));
        }

        private static AnimationCurve CreateSparkSizeCurve()
        {
            return new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(0.25f, 0.72f),
                new Keyframe(1f, 0f));
        }

        private static AnimationCurve CreateFlashSizeCurve()
        {
            return new AnimationCurve(
                new Keyframe(0f, 0.35f),
                new Keyframe(0.25f, 1f),
                new Keyframe(1f, 0f));
        }

        private static AnimationCurve CreateTrailWidthCurve()
        {
            return new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(0.65f, 0.42f),
                new Keyframe(1f, 0f));
        }

        private static AnimationCurve CreateSteamWispSizeCurve()
        {
            return new AnimationCurve(
                new Keyframe(0f, 0.18f),
                new Keyframe(0.25f, 0.74f),
                new Keyframe(0.78f, 1.15f),
                new Keyframe(1f, 1.45f));
        }

        private static AnimationCurve CreateSteamPuffSizeCurve()
        {
            return new AnimationCurve(
                new Keyframe(0f, 0.3f),
                new Keyframe(0.32f, 0.9f),
                new Keyframe(0.72f, 1.25f),
                new Keyframe(1f, 1.6f));
        }

        private static AnimationCurve CreateOfficeLightFogSizeCurve()
        {
            return new AnimationCurve(
                new Keyframe(0f, 0.35f),
                new Keyframe(0.28f, 1f),
                new Keyframe(0.75f, 1.08f),
                new Keyframe(1f, 0.42f));
        }

        private static AnimationCurve CreateLightBeamSheetSizeCurve()
        {
            return new AnimationCurve(
                new Keyframe(0f, 0.55f),
                new Keyframe(0.22f, 1f),
                new Keyframe(0.78f, 1.05f),
                new Keyframe(1f, 0.35f));
        }

        private static Gradient CreateDustGradient(float peakAlpha)
        {
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(0.76f, 0.82f, 0.9f), 0f),
                    new GradientColorKey(new Color(0.94f, 0.96f, 1f), 0.45f),
                    new GradientColorKey(new Color(0.7f, 0.76f, 0.82f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(peakAlpha, 0.22f),
                    new GradientAlphaKey(peakAlpha * 0.72f, 0.72f),
                    new GradientAlphaKey(0f, 1f)
                });
            return gradient;
        }

        private static Gradient CreateSteamGradient(float peakAlpha)
        {
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(0.72f, 0.66f, 0.58f), 0f),
                    new GradientColorKey(new Color(0.96f, 0.92f, 0.86f), 0.38f),
                    new GradientColorKey(new Color(0.86f, 0.9f, 0.94f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(peakAlpha, 0.18f),
                    new GradientAlphaKey(peakAlpha * 0.7f, 0.55f),
                    new GradientAlphaKey(0f, 1f)
                });
            return gradient;
        }

        private static Gradient CreateSparkGradient()
        {
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(new Color(0.42f, 0.82f, 1f), 0.28f),
                    new GradientColorKey(new Color(0.08f, 0.28f, 0.9f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.92f, 0.2f),
                    new GradientAlphaKey(0.18f, 0.72f),
                    new GradientAlphaKey(0f, 1f)
                });
            return gradient;
        }

        private static Gradient CreateHotPointGradient()
        {
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(new Color(1f, 0.82f, 0.35f), 0.3f),
                    new GradientColorKey(new Color(0.18f, 0.52f, 1f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.74f, 0.35f),
                    new GradientAlphaKey(0f, 1f)
                });
            return gradient;
        }

        private static Gradient CreateFlashGradient()
        {
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(0.65f, 0.92f, 1f), 0f),
                    new GradientColorKey(new Color(0.17f, 0.48f, 1f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0.42f, 0f),
                    new GradientAlphaKey(0.18f, 0.35f),
                    new GradientAlphaKey(0f, 1f)
                });
            return gradient;
        }

        private static Gradient CreateTrailGradient()
        {
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(new Color(0.35f, 0.78f, 1f), 0.45f),
                    new GradientColorKey(new Color(0.05f, 0.16f, 0.8f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0.85f, 0f),
                    new GradientAlphaKey(0.48f, 0.55f),
                    new GradientAlphaKey(0f, 1f)
                });
            return gradient;
        }

        private static Gradient CreateOfficeLightFogGradient(float peakAlpha)
        {
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(0.55f, 0.62f, 0.7f), 0f),
                    new GradientColorKey(new Color(0.72f, 0.8f, 0.88f), 0.4f),
                    new GradientColorKey(new Color(0.58f, 0.66f, 0.74f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(peakAlpha, 0.22f),
                    new GradientAlphaKey(peakAlpha * 0.7f, 0.68f),
                    new GradientAlphaKey(0f, 1f)
                });
            return gradient;
        }

        private static Gradient CreateOfficeLightBeamGradient(float peakAlpha)
        {
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(0.65f, 0.74f, 0.82f), 0f),
                    new GradientColorKey(new Color(0.82f, 0.88f, 0.94f), 0.45f),
                    new GradientColorKey(new Color(0.6f, 0.7f, 0.8f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(peakAlpha, 0.18f),
                    new GradientAlphaKey(peakAlpha * 0.6f, 0.72f),
                    new GradientAlphaKey(0f, 1f)
                });
            return gradient;
        }

        private static Mesh CreateOrUpdateLightFogBeamMesh()
        {
            const string meshPath = GeneratedMeshDir + "/M_OfficeLightFog_BeamCone.asset";
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            if (mesh == null)
            {
                mesh = new Mesh();
                AssetDatabase.CreateAsset(mesh, meshPath);
            }

            Vector3[] vertices = new Vector3[5];
            vertices[0] = new Vector3(0f, 0.5f, 0f);
            vertices[1] = new Vector3(-0.5f, -0.5f, -0.5f);
            vertices[2] = new Vector3(0.5f, -0.5f, -0.5f);
            vertices[3] = new Vector3(0.5f, -0.5f, 0.5f);
            vertices[4] = new Vector3(-0.5f, -0.5f, 0.5f);

            Vector2[] uvs = new Vector2[5];
            uvs[0] = new Vector2(0.5f, 1f);
            uvs[1] = new Vector2(0f, 0f);
            uvs[2] = new Vector2(1f, 0f);
            uvs[3] = new Vector2(1f, 1f);
            uvs[4] = new Vector2(0f, 1f);

            int[] triangles = new int[]
            {
                0, 2, 1,
                0, 3, 2,
                0, 4, 3,
                0, 1, 4
            };

            mesh.Clear();
            mesh.name = "M_OfficeLightFog_BeamCone";
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            EditorUtility.SetDirty(mesh);
            return mesh;
        }

        private static void DeleteObsoleteGeneratedEffects()
        {
            string[] obsoleteAssets = new[]
            {
                $"{PrefabDir}/VFX_ParticleBombardment.prefab",
                $"{PrefabDir}/VFX_ScifiOfficeAtmosphere.prefab",
                $"{MaterialDir}/M_ParticleBombardment.mat",
                $"{MaterialDir}/M_ScifiOfficeAmbient.mat",
                $"{TextureDir}/T_ParticleBombardment.png",
                $"{TextureDir}/T_ScifiOfficeAmbient.png"
            };

            foreach (string path in obsoleteAssets)
            {
                if (AssetDatabase.LoadAssetAtPath<Object>(path) != null || File.Exists(path))
                {
                    AssetDatabase.DeleteAsset(path);
                }
            }
        }

        private static void SavePrefab(GameObject root, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(root, path);
            DestroyImmediate(root);
        }
    }
}
