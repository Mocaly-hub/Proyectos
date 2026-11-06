#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Genera la escena jugable automáticamente al abrir el proyecto por primera vez.
/// También se puede reconstruir con Herramientas > Salto Islas 3D > Reconstruir nivel.
/// </summary>
[InitializeOnLoad]
public static class BuildPlatformerLevel
{
    private const string ScenePath = "Assets/Scenes/SaltoIslas.unity";
    private static Material grass, dirt, stone, sea, cyan, yellow, purple, red, white, dark, checkpoint, orange;

    static BuildPlatformerLevel()
    {
        EditorApplication.delayCall += InitializeScene;
    }

    private static void InitializeScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
        if (!File.Exists(ScenePath))
        {
            BuildScene();
            return;
        }
        if (SceneManager.GetActiveScene().path != ScenePath)
            EditorSceneManager.OpenScene(ScenePath);
    }

    [MenuItem("Herramientas/Salto Islas 3D/Reconstruir nivel")]
    public static void BuildScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("Detén el modo Play antes de reconstruir el nivel.");
            return;
        }

        Directory.CreateDirectory("Assets/Scenes");
        Directory.CreateDirectory("Assets/Materials");
        CreateMaterials();
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Scene scene = SceneManager.GetActiveScene();
        scene.name = "SaltoIslas";

        SetupEnvironment();

        GameObject systems = new GameObject("01 · Sistema del juego");
        GameFlow game = systems.AddComponent<GameFlow>();
        game.TotalCoins = 11;

        GameObject world = new GameObject("02 · Plataformas y decoracion");
        // Isla (x, altura, z): recorrido mas largo con 13 plataformas.
        Vector3[] islands = {
            new Vector3(0, 0, 0), new Vector3(0, 0, 10),
            new Vector3(3.5f, .5f, 18), new Vector3(0, 1f, 26),
            new Vector3(-3.5f, 1.3f, 34), new Vector3(0, 1.6f, 42),
            new Vector3(3f, 1.6f, 50), new Vector3(0, 1.8f, 58),
            new Vector3(-3f, 1.8f, 66), new Vector3(0, 2f, 74),
            new Vector3(3.5f, 2.2f, 82), new Vector3(0, 2.4f, 90),
            new Vector3(0, 2.4f, 99)
        };
        Material[] palette = { grass, cyan, purple, grass, cyan, purple };
        for (int i = 0; i < islands.Length; i++)
        {
            bool wide = i == 0 || i == islands.Length - 1;
            bool control = i == 3 || i == 9;
            Vector3 size = wide ? new Vector3(8f, 1f, 8f) : new Vector3(5.2f, 1f, 5.2f);
            Platform(world.transform, i == 0 ? "Inicio" : (i == islands.Length - 1 ? "Meta" : "Isla " + i),
                islands[i], size, control ? grass : palette[i % palette.Length]);
            // Faroles decorativos, sin banderas ni postes de checkpoint.
            if (i > 0 && i < islands.Length - 1 && i % 3 == 1)
            {
                Vector3 pos = islands[i] + new Vector3(-1.8f, 1.10f, -1.0f);
                GameObject lamp = Shape(PrimitiveType.Cylinder, "Faro decorativo " + i, pos, new Vector3(.16f, .44f, .16f), dark, world.transform);
                StripCollider(lamp);
                GameObject glow = Shape(PrimitiveType.Sphere, "Luz", pos + Vector3.up * .47f, Vector3.one * .42f, yellow, world.transform);
                StripCollider(glow);
            }
        }

        GameObject items = new GameObject("03 · Monedas, checkpoints y peligros");
        int coinCount = 0;
        for (int i = 1; i < islands.Length - 1; i++)
        {
            Coin(items.transform, islands[i] + Vector3.up * 1.73f);
            coinCount++;
        }
        game.TotalCoins = coinCount;
        // Los checkpoints ahora son placas luminosas en el suelo, NO banderas.
        Checkpoint(items.transform, islands[3], islands[3] + Vector3.up * 1.55f);
        Checkpoint(items.transform, islands[9], islands[9] + Vector3.up * 1.55f);
        foreach (int index in new int[] { 4, 5, 8, 10 })
            Hazard(items.transform, islands[index] + new Vector3(0, 1.55f, 1.15f), 1.10f);

        GameObject goal = new GameObject("04 · Portal de victoria");
        Vector3 goalCenter = islands[islands.Length - 1] + new Vector3(0, 2f, 1.9f);
        Shape(PrimitiveType.Cube, "Pilar izquierdo", goalCenter + new Vector3(-1.45f, 0, 0), new Vector3(.45f, 3.5f, .55f), yellow, goal.transform);
        Shape(PrimitiveType.Cube, "Pilar derecho", goalCenter + new Vector3(1.45f, 0, 0), new Vector3(.45f, 3.5f, .55f), yellow, goal.transform);
        Shape(PrimitiveType.Cube, "Travesaño", goalCenter + Vector3.up * 1.85f, new Vector3(3.25f, .5f, .6f), yellow, goal.transform);
        GameObject portal = Shape(PrimitiveType.Cube, "Zona de meta · toca para ganar", goalCenter, new Vector3(2.35f, 2.7f, .2f), orange, goal.transform);
        portal.GetComponent<BoxCollider>().isTrigger = true;
        portal.AddComponent<LevelTrigger>().kind = LevelTrigger.TriggerKind.Finish;

        // La cápsula es solamente física; el personaje visible sale del FBX Idle.
        GameObject actor = new GameObject("05 · PERSONAJE (controlable)");
        actor.transform.position = new Vector3(0, 1.55f, 0);
        CharacterController cc = actor.AddComponent<CharacterController>();
        cc.height = 2f;
        cc.radius = .43f;
        cc.center = Vector3.zero;
        cc.stepOffset = .3f;
        cc.slopeLimit = 45f;
        PlayerPlatformer motor = actor.AddComponent<PlayerPlatformer>();
        AttachAnimatedModel(actor, motor);

        GameObject cameraGo = new GameObject("06 · Camara de seguimiento");
        cameraGo.tag = "MainCamera";
        Camera cam = cameraGo.AddComponent<Camera>();
        cam.fieldOfView = 60;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(.57f, .76f, .94f);
        cam.nearClipPlane = .1f;
        cam.farClipPlane = 260;
        cameraGo.AddComponent<AudioListener>();
        CameraChase chase = cameraGo.AddComponent<CameraChase>();
        chase.target = actor.transform;
        cameraGo.transform.position = actor.transform.position + chase.offset;
        cameraGo.transform.LookAt(actor.transform.position + Vector3.up * .6f);

        GameObject sunlight = new GameObject("07 · Sol suave");
        Light sun = sunlight.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.color = new Color(1f, .95f, .82f);
        sun.intensity = 1.5f;
        sunlight.transform.rotation = Quaternion.Euler(45f, -32f, 0);

        // Escena jugable con el modelo y las cuatro animaciones FBX incluidas.
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorSceneManager.OpenScene(ScenePath);
        Selection.activeGameObject = actor;
        Debug.Log("Salto Islas 3D listo. Presiona Play. Controles: WASD/flechas, espacio y R.");
    }

    private static void AttachAnimatedModel(GameObject actor, PlayerPlatformer motor)
    {
        const string prefix = "Assets/Animations/";
        const string modelPath = prefix + "Ch03_nonPBR.fbx";
        // El personaje completo (con malla y UV) es el origen visual.
        // Las otras cuatro importaciones solo suministran los movimientos humanoides.
        string[] names = { "Ch03_nonPBR.fbx", "Idle.fbx", "Run.fbx", "Running Jump.fbx", "Floating.fbx" };
        foreach (string n in names)
        {
            ModelImporter importer = AssetImporter.GetAtPath(prefix + n) as ModelImporter;
            if (importer == null) continue;
            if (importer.animationType != ModelImporterAnimationType.Human ||
                importer.avatarSetup != ModelImporterAvatarSetup.CreateFromThisModel)
            {
                importer.animationType = ModelImporterAnimationType.Human;
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                importer.SaveAndReimport();
            }
        }
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        if (prefab == null)
        {
            Debug.LogError("No se encontró el modelo original Ch03_nonPBR.fbx.");
            return;
        }
        GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        model.name = "Personaje ORIGINAL con textura y animaciones";
        model.transform.SetParent(actor.transform, false);
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;
        model.transform.localScale = Vector3.one;

        // Material independiente con la textura de color extraída del FBX.
        // Así no depende de si Unity puede leer los materiales internos de Mixamo.
        Material characterMaterial = CreateCharacterMaterial();
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
        {
            Material[] slots = renderer.sharedMaterials;
            if (slots.Length == 0) slots = new Material[1];
            for (int i = 0; i < slots.Length; i++) slots[i] = characterMaterial;
            renderer.sharedMaterials = slots;
        }
        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            foreach (Renderer r in renderers) bounds.Encapsulate(r.bounds);
            float scale = Mathf.Clamp(1.8f / Mathf.Max(.01f, bounds.size.y), .001f, 10f);
            model.transform.localScale = Vector3.one * scale;
            renderers = model.GetComponentsInChildren<Renderer>(true);
            bounds = renderers[0].bounds;
            foreach (Renderer r in renderers) bounds.Encapsulate(r.bounds);
            model.transform.position += Vector3.up * (actor.transform.position.y - 1f - bounds.min.y);
        }
        Animator animator = model.GetComponent<Animator>();
        if (animator == null) animator = model.AddComponent<Animator>();
        animator.applyRootMotion = false;
        AnimatorController controller = MakeAnimatorController();
        if (controller != null) animator.runtimeAnimatorController = controller;
        PlayerAnimationDriver driver = actor.AddComponent<PlayerAnimationDriver>();
        driver.animator = animator;
        driver.motor = motor;
    }

    private static Material CreateCharacterMaterial()
    {
        const string path = "Assets/Materials/Personaje_Original_Texturizado.mat";
        const string texturePath = "Assets/Textures/Ch03_1001_Diffuse.png";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        Texture2D diffuse = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        if (material == null)
        {
            Shader shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Diffuse");
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }
        material.color = Color.white;
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", Color.white);
        if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", diffuse);
        if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", diffuse);
        material.mainTexture = diffuse;
        if (diffuse == null) Debug.LogError("No aparece Ch03_1001_Diffuse.png: revisa Assets/Textures.");
        EditorUtility.SetDirty(material);
        return material;
    }

    private static AnimatorController MakeAnimatorController()
    {
        const string path = "Assets/Animations/Personaje.controller";
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if (controller != null) AssetDatabase.DeleteAsset(path);
        controller = AnimatorController.CreateAnimatorControllerAtPath(path);
        var machine = controller.layers[0].stateMachine;
        string[] paths = { "Idle.fbx", "Run.fbx", "Running Jump.fbx", "Floating.fbx" };
        string[] states = { "Idle", "Run", "Jump", "Fall" };
        for (int i = 0; i < paths.Length; i++)
        {
            string asset = "Assets/Animations/" + paths[i];
            AnimationClip clip = null;
            foreach (Object obj in AssetDatabase.LoadAllAssetsAtPath(asset))
            {
                AnimationClip candidate = obj as AnimationClip;
                if (candidate != null && !candidate.name.StartsWith("__preview__") && candidate.length > .01f)
                {
                    clip = candidate;
                    break;
                }
            }
            if (clip == null) Debug.LogWarning("No se encontró clip en " + asset);
            var state = machine.AddState(states[i]);
            state.motion = clip;
            if (i == 0) machine.defaultState = state;
        }
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        return controller;
    }

    private static void SetupEnvironment()
    {
        RenderSettings.skybox = null;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(.7f, .78f, .94f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(.60f, .78f, .94f);
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 60f;
        RenderSettings.fogEndDistance = 150f;

    }

    private static void CreateMaterials()
    {
        grass = Mat("01_Pradera", new Color(.27f, .83f, .62f));
        dirt = Mat("02_Tierra", new Color(.27f, .34f, .52f));
        stone = Mat("03_Piedra", new Color(.28f, .36f, .47f));
        sea = Mat("04_Vacio", new Color(.53f, .76f, .92f));
        cyan = Mat("05_Cian", new Color(.23f, .72f, .91f));
        yellow = Mat("06_Dorado", new Color(1f, .78f, .22f));
        purple = Mat("07_Morado", new Color(.68f, .50f, .96f));
        red = Mat("08_Peligro", new Color(1f, .29f, .39f));
        white = Mat("09_Personaje", new Color(.98f, .98f, .96f));
        dark = Mat("10_Oscuro", new Color(.12f, .20f, .31f));
        checkpoint = Mat("11_Checkpoint", new Color(.40f, 1f, .80f));
        orange = Mat("12_Portal", new Color(1f, .53f, .19f));
    }

    private static Material Mat(string name, Color color)
    {
        string path = "Assets/Materials/" + name + ".mat";
        Material found = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (found != null) return found;
        Shader shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Diffuse");
        Material material = new Material(shader);
        material.color = color;
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static GameObject Shape(PrimitiveType type, string name, Vector3 position, Vector3 scale, Material material, Transform parent)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name;
        if (parent != null) go.transform.SetParent(parent, true);
        go.transform.position = position;
        go.transform.localScale = scale;
        Renderer r = go.GetComponent<Renderer>();
        if (r != null) r.sharedMaterial = material;
        return go;
    }

    private static void StripCollider(GameObject go)
    {
        Collider c = go.GetComponent<Collider>();
        if (c != null) Object.DestroyImmediate(c);
    }

    private static void Platform(Transform parent, string name, Vector3 center, Vector3 size, Material top)
    {
        GameObject baseObject = Shape(PrimitiveType.Cube, name, center, size, dirt, parent);
        Shape(PrimitiveType.Cube, name + " · superficie", center + Vector3.up * (size.y * .5f + .11f), new Vector3(size.x, .22f, size.z), top, parent);
        GameObject undersurface = Shape(PrimitiveType.Cube, name + " · base inferior", center - Vector3.up * .75f, new Vector3(size.x * .83f, .57f, size.z * .83f), stone, parent);
        StripCollider(undersurface);
    }

    private static void Accent(Transform parent, Vector3 center, Vector3 size, Material material)
    {
        GameObject go = Shape(PrimitiveType.Cube, "Franja de color", center, size, material, parent);
        StripCollider(go);
    }

    private static void Coin(Transform parent, Vector3 position)
    {
        GameObject coin = new GameObject("Moneda · recoger");
        coin.transform.SetParent(parent, true);
        coin.transform.position = position;
        GameObject visual = Shape(PrimitiveType.Cylinder, "Disco dorado", position, new Vector3(.48f, .13f, .48f), yellow, coin.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        StripCollider(visual);
        SphereCollider trigger = coin.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = .85f;
        coin.AddComponent<LevelTrigger>().kind = LevelTrigger.TriggerKind.Coin;
        coin.AddComponent<FloatAndSpin>();
    }

    private static void Checkpoint(Transform parent, Vector3 position, Vector3 respawn)
    {
        GameObject cp = new GameObject("Checkpoint · placa en el suelo (sin bandera)");
        cp.transform.SetParent(parent, true);
        cp.transform.position = position + Vector3.up * .68f;
        GameObject pad = Shape(PrimitiveType.Cylinder, "Placa luminosa", cp.transform.position, new Vector3(1.3f, .07f, 1.3f), checkpoint, cp.transform);
        StripCollider(pad);
        BoxCollider col = cp.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.center = Vector3.up * .8f;
        col.size = new Vector3(2.25f, 1.8f, 2.25f);
        LevelTrigger trigger = cp.AddComponent<LevelTrigger>();
        trigger.kind = LevelTrigger.TriggerKind.Checkpoint;
        trigger.checkpointPosition = respawn;
    }

    private static void Hazard(Transform parent, Vector3 position, float distance)
    {
        GameObject hazard = Shape(PrimitiveType.Cube, "Obstaculo rojo · evitalo", position, new Vector3(.8f, .85f, .8f), red, parent);
        hazard.transform.rotation = Quaternion.Euler(0, 30, 0);
        hazard.GetComponent<BoxCollider>().isTrigger = true;
        hazard.AddComponent<LevelTrigger>().kind = LevelTrigger.TriggerKind.Hazard;
        HazardMover move = hazard.AddComponent<HazardMover>();
        move.distance = distance;
    }
}
#endif
