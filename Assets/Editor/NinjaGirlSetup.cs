using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public static class NinjaGirlSetup
{
    private const string SpriteFolder = "Assets/charakter/ninjagirlnew/png";
    private const string AnimationFolder = "Assets/Animations/NinjaGirl";
    private const string BackgroundPath = "Assets/Backgrounds/Level1_Icebound_Dusk.png";
    private const string PrefabFolder = "Assets/Prefabs";
    private const string PrefabPath = PrefabFolder + "/NinjaGirlPlayer.prefab";
    private const string ControllerPath = AnimationFolder + "/NinjaGirl.controller";
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";
    private const string SetupRequestPath = "Temp/NinjaGirlSetup.request";
    private const string BackgroundRequestPath = "Temp/Level1Background.request";
    private const float FramesPerSecond = 12f;

    private static string SetupRequestFullPath =>
        Path.Combine(Directory.GetParent(Application.dataPath).FullName, SetupRequestPath);
    private static string BackgroundRequestFullPath =>
        Path.Combine(Directory.GetParent(Application.dataPath).FullName, BackgroundRequestPath);

    [InitializeOnLoadMethod]
    private static void RunRequestedSetup()
    {
        EditorApplication.delayCall += TryRunRequestedSetup;
    }

    private static void TryRunRequestedSetup()
    {
        string requestPath = SetupRequestFullPath;
        if (!File.Exists(requestPath))
            return;

        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            return;
        }

        File.Delete(requestPath);
        SetupNinjaGirlPlayer(false);
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredEditMode)
            return;

        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        EditorApplication.delayCall += TryRunRequestedSetup;
    }

    [InitializeOnLoadMethod]
    private static void RunRequestedBackgroundSetup()
    {
        EditorApplication.delayCall += TryRunRequestedBackgroundSetup;
    }

    private static void TryRunRequestedBackgroundSetup()
    {
        string requestPath = BackgroundRequestFullPath;
        if (!File.Exists(requestPath))
            return;

        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.playModeStateChanged -= OnBackgroundSetupPlayModeChanged;
            EditorApplication.playModeStateChanged += OnBackgroundSetupPlayModeChanged;
            return;
        }

        File.Delete(requestPath);
        InstallBackgroundOnly(false);
    }

    private static void OnBackgroundSetupPlayModeChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredEditMode)
            return;

        EditorApplication.playModeStateChanged -= OnBackgroundSetupPlayModeChanged;
        EditorApplication.delayCall += TryRunRequestedBackgroundSetup;
    }

    [MenuItem("Tools/Background/Install Level 1 Background")]
    private static void InstallBackgroundFromMenu()
    {
        InstallBackgroundOnly(true);
    }

    private static void InstallBackgroundOnly(bool showDialogs)
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != ScenePath)
        {
            const string message = "Buka Assets/Scenes/SampleScene.unity untuk memasang background Level 1.";
            if (showDialogs)
                EditorUtility.DisplayDialog("Open Level 1", message, "OK");
            else
                Debug.LogError(message);
            return;
        }

        Tilemap tilemap = FindTilemap(scene);
        if (tilemap == null)
        {
            Debug.LogError("Tilemap level 1 tidak ditemukan.");
            return;
        }

        SetupBackground(scene, tilemap);
        EditorSceneManager.MarkSceneDirty(scene);
        AssetDatabase.SaveAssets();
        if (showDialogs)
            EditorUtility.DisplayDialog("Background ready", "Background panorama Level 1 sudah dipasang.", "OK");
        else
            Debug.Log("Level 1 background installed. Save SampleScene to keep the change.");
    }

    [MenuItem("Tools/Characters/Create Ninja Girl Player")]
    private static void CreateNinjaGirlPlayer()
    {
        SetupNinjaGirlPlayer(true);
    }

    private static void SetupNinjaGirlPlayer(bool showDialogs)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            if (showDialogs)
                EditorUtility.DisplayDialog("Exit Play Mode", "Hentikan Play Mode sebelum memasang checkpoint dan rintangan.", "OK");
            return;
        }

        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != ScenePath)
        {
            const string message = "Buka Assets/Scenes/SampleScene.unity, lalu jalankan Tools > Characters > Create Ninja Girl Player.";
            if (showDialogs)
                EditorUtility.DisplayDialog("Open gameplay scene", message, "OK");
            else
                Debug.LogError(message);
            return;
        }

        if (showDialogs && File.Exists(PrefabPath) && !EditorUtility.DisplayDialog(
                "Replace generated character?",
                "Prefab Ninja Girl dan animasinya akan dibuat ulang. Lanjutkan?",
                "Replace",
                "Cancel"))
            return;

        EnsureFolder("Assets", "Animations");
        EnsureFolder("Assets/Animations", "NinjaGirl");
        EnsureFolder("Assets", "Prefabs");

        Sprite[] idleFrames = ImportFrames("Idle__*.png");
        Sprite[] runFrames = ImportFrames("Run__*.png");
        Sprite[] jumpFrames = ImportFrames("Jump__*.png");
        if (idleFrames.Length == 0 || runFrames.Length == 0 || jumpFrames.Length == 0)
        {
            const string message = "Idle, Run, dan Jump PNG harus tersedia di " + SpriteFolder + ".";
            if (showDialogs)
                EditorUtility.DisplayDialog("Character frames missing", message, "OK");
            else
                Debug.LogError(message);
            return;
        }

        AnimationClip idleClip = CreateClip("NinjaGirl_Idle.anim", idleFrames, true);
        AnimationClip runClip = CreateClip("NinjaGirl_Run.anim", runFrames, true);
        AnimationClip jumpClip = CreateClip("NinjaGirl_Jump.anim", jumpFrames, false);
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath) != null)
            AssetDatabase.DeleteAsset(ControllerPath);
        AnimatorController controller = CreateController(idleClip, runClip, jumpClip);
        GameObject prefab = CreatePrefab(idleFrames[0], controller);
        AddTilemapColliders(scene);
        Tilemap tilemap = FindTilemap(scene);
        if (tilemap == null)
        {
            Debug.LogError("SampleScene tidak memiliki Tilemap untuk level platformer.");
            return;
        }

        PlatformRun[] platforms = FindPlatformRuns(tilemap);
        PlatformRun startPlatform = platforms.Where(platform =>
                platform.minX <= 2 && platform.maxX >= 18 && platform.Length >= 12)
            .OrderBy(platform => platform.minX).FirstOrDefault();
        PlatformRun firstCheckpointPlatform = platforms.Where(platform =>
                platform.minX >= 4 && platform.minX <= 20 && platform.Length >= 6)
            .OrderByDescending(platform => platform.Length).FirstOrDefault();
        PlatformRun secondCheckpointPlatform = platforms.Where(platform =>
                platform.minX >= 25 && platform.minX <= 42 && platform.Length >= 6)
            .OrderByDescending(platform => platform.Length).FirstOrDefault();
        PlatformRun finishPlatform = platforms.Where(platform => platform.maxX >= 48)
            .OrderByDescending(platform => platform.maxX).FirstOrDefault();

        if (startPlatform.Length == 0 || firstCheckpointPlatform.Length == 0 ||
            secondCheckpointPlatform.Length == 0 || finishPlatform.Length == 0)
        {
            Debug.LogError("Platform start, checkpoint, atau finish tidak ditemukan pada Tilemap.");
            return;
        }

        Vector3 startPosition = GetPlatformPosition(tilemap, startPlatform, startPlatform.minX);
        startPosition.x -= 6f;
        GameObject playerObject = AddPlayerToScene(scene, prefab, idleFrames[0], controller, startPosition);
        InstallLevelFlow(scene, tilemap, playerObject.GetComponent<NinjaGirlController>(),
            startPosition, firstCheckpointPlatform, secondCheckpointPlatform, finishPlatform);
        SetupBackground(scene, tilemap);

        EditorSceneManager.MarkSceneDirty(scene);
        AssetDatabase.SaveAssets();
        if (showDialogs)
            EditorUtility.DisplayDialog("Ninja Girl ready", "Prefab dan animasi dibuat. Simpan scene untuk menyimpan karakter di SampleScene.", "OK");
        else
            Debug.Log("Ninja Girl prefab and animations created. Save SampleScene to keep the character in the scene.");
    }

    private static Sprite[] ImportFrames(string pattern)
    {
        string[] paths = Directory.GetFiles(SpriteFolder, pattern)
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();

        foreach (string path in paths)
        {
            string assetPath = path.Replace('\\', '/');
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
                continue;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 500f;
            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = new Vector2(0.5f, 0f);
            importer.SetTextureSettings(settings);
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        return paths
            .Select(path => AssetDatabase.LoadAssetAtPath<Sprite>(path.Replace('\\', '/')))
            .Where(sprite => sprite != null)
            .ToArray();
    }

    private static AnimationClip CreateClip(string fileName, Sprite[] frames, bool loop)
    {
        string path = AnimationFolder + "/" + fileName;
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (clip == null)
            clip = new AnimationClip();

        clip.frameRate = FramesPerSecond;
        ObjectReferenceKeyframe[] keys = frames.Select((sprite, index) => new ObjectReferenceKeyframe
        {
            time = index / FramesPerSecond,
            value = sprite
        }).ToArray();
        EditorCurveBinding binding = EditorCurveBinding.PPtrCurve(
            string.Empty, typeof(SpriteRenderer), "m_Sprite");
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        if (AssetDatabase.LoadAssetAtPath<AnimationClip>(path) == null)
            AssetDatabase.CreateAsset(clip, path);
        EditorUtility.SetDirty(clip);
        return clip;
    }

    private static AnimatorController CreateController(AnimationClip idleClip, AnimationClip runClip, AnimationClip jumpClip)
    {
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("Grounded", AnimatorControllerParameterType.Bool);

        AnimatorState idle = controller.layers[0].stateMachine.AddState("Idle");
        AnimatorState run = controller.layers[0].stateMachine.AddState("Run");
        AnimatorState jump = controller.layers[0].stateMachine.AddState("Jump");
        controller.layers[0].stateMachine.defaultState = idle;
        idle.motion = idleClip;
        run.motion = runClip;
        jump.motion = jumpClip;

        AddTransition(idle, run, "Speed", AnimatorConditionMode.Greater, 0.01f);
        AddTransition(run, idle, "Speed", AnimatorConditionMode.Less, 0.01f);
        AddTransition(idle, jump, "Grounded", AnimatorConditionMode.IfNot, 0f);
        AddTransition(run, jump, "Grounded", AnimatorConditionMode.IfNot, 0f);
        AddTransition(jump, idle, "Grounded", AnimatorConditionMode.If, 0f);
        EditorUtility.SetDirty(controller);
        return controller;
    }

    private static void AddTransition(AnimatorState from, AnimatorState to, string parameter,
        AnimatorConditionMode mode, float threshold)
    {
        AnimatorStateTransition transition = from.AddTransition(to);
        transition.hasExitTime = false;
        transition.duration = 0f;
        transition.AddCondition(mode, threshold, parameter);
    }

    private static GameObject CreatePrefab(Sprite startingSprite, AnimatorController controller)
    {
        GameObject player = new GameObject("NinjaGirlPlayer");
        ConfigurePlayer(player, startingSprite, controller);
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(player, PrefabPath);
        UnityEngine.Object.DestroyImmediate(player);
        return prefab;
    }

    private static void ConfigurePlayer(GameObject player, Sprite startingSprite, AnimatorController controller)
    {
        player.tag = "Player";
        player.transform.localScale = Vector3.one * 1.5f;

        SpriteRenderer renderer = player.GetComponent<SpriteRenderer>();
        if (renderer == null)
            renderer = player.AddComponent<SpriteRenderer>();
        renderer.sprite = startingSprite;
        renderer.sortingOrder = 5;

        Animator animator = player.GetComponent<Animator>();
        if (animator == null)
            animator = player.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;

        Rigidbody2D body = player.GetComponent<Rigidbody2D>();
        if (body == null)
            body = player.AddComponent<Rigidbody2D>();
        body.gravityScale = 3f;
        body.freezeRotation = true;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        CapsuleCollider2D collider = player.GetComponent<CapsuleCollider2D>();
        if (collider == null)
            collider = player.AddComponent<CapsuleCollider2D>();
        collider.size = new Vector2(0.62f, 0.95f);
        collider.offset = new Vector2(0f, 0.475f);

        if (player.GetComponent<NinjaGirlController>() == null)
            player.AddComponent<NinjaGirlController>();
    }

    private static GameObject AddPlayerToScene(Scene scene, GameObject prefab, Sprite startingSprite,
        AnimatorController controller, Vector3 startPosition)
    {
        GameObject instance = scene.GetRootGameObjects().FirstOrDefault(gameObject =>
            gameObject.name == "NinjaGirlPlayer" || gameObject.name == "Ninja Girl");

        if (instance == null)
        {
            instance = PrefabUtility.InstantiatePrefab(prefab, scene) as GameObject;
            Undo.RegisterCreatedObjectUndo(instance, "Add Ninja Girl Player");
        }
        else
            ConfigurePlayer(instance, startingSprite, controller);

        Undo.RecordObject(instance.transform, "Position Ninja Girl Player");
        instance.transform.position = startPosition;
        instance.transform.localScale = Vector3.one * 1.5f;
        instance.tag = "Player";
        return instance;
    }

    private struct PlatformRun
    {
        public int minX;
        public int maxX;
        public int y;
        public int Length => maxX - minX + 1;
    }

    private static Tilemap FindTilemap(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Tilemap tilemap = root.GetComponentInChildren<Tilemap>(true);
            if (tilemap != null)
                return tilemap;
        }

        return null;
    }

    private static PlatformRun[] FindPlatformRuns(Tilemap tilemap)
    {
        BoundsInt bounds = tilemap.cellBounds;
        PlatformRun[] platforms = new PlatformRun[bounds.size.x * bounds.size.y];
        int platformCount = 0;
        for (int y = bounds.yMin; y < bounds.yMax; y++)
        {
            int runStart = -1;
            for (int x = bounds.xMin; x <= bounds.xMax; x++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                bool isTopTile = x < bounds.xMax && tilemap.HasTile(cell) &&
                    !tilemap.HasTile(cell + Vector3Int.up);

                if (isTopTile)
                {
                    if (runStart < 0)
                        runStart = x;
                    continue;
                }

                int runLength = x - runStart;
                if (runStart >= 0 && runLength >= 3)
                    platforms[platformCount++] = new PlatformRun { minX = runStart, maxX = x - 1, y = y };

                runStart = -1;
            }
        }

        Array.Resize(ref platforms, platformCount);
        return platforms;
    }

    private static Vector3 GetPlatformPosition(Tilemap tilemap, PlatformRun platform, int x)
    {
        int cellX = Mathf.Clamp(x, platform.minX + 1, platform.maxX - 1);
        Vector3Int cell = new Vector3Int(cellX, platform.y, 0);
        Vector3 center = tilemap.GetCellCenterWorld(cell);
        Vector3 surface = tilemap.CellToWorld(cell + Vector3Int.up);
        return new Vector3(center.x, surface.y + 0.04f, 0f);
    }

    private static void InstallLevelFlow(Scene scene, Tilemap tilemap, NinjaGirlController player,
        Vector3 startPosition, PlatformRun firstCheckpointPlatform, PlatformRun secondCheckpointPlatform,
        PlatformRun finishPlatform)
    {
        GameObject managerObject = scene.GetRootGameObjects()
            .FirstOrDefault(gameObject => gameObject.name == "LevelFlowManager");
        if (managerObject == null)
        {
            managerObject = new GameObject("LevelFlowManager");
            Undo.RegisterCreatedObjectUndo(managerObject, "Add Level Flow Manager");
        }

        GameProgressManager manager = managerObject.GetComponent<GameProgressManager>();
        if (manager == null)
            manager = Undo.AddComponent<GameProgressManager>(managerObject);

        SerializedObject managerSettings = new SerializedObject(manager);
        managerSettings.FindProperty("player").objectReferenceValue = player;
        managerSettings.FindProperty("initialSpawn").vector3Value = startPosition;
        managerSettings.FindProperty("totalCheckpoints").intValue = 2;
        managerSettings.FindProperty("crystalKeyIcon").objectReferenceValue =
            LoadTileSprite("Assets/Tiles/key_blue_0.asset");
        managerSettings.FindProperty("iceRelicIcon").objectReferenceValue =
            LoadTileSprite("Assets/Tiles/gem_blue_0.asset");
        managerSettings.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(manager);

        GameObject markerContainer = ReplaceSceneObject(scene, "LevelMarkers", null);
        Sprite checkpointOneSprite = LoadTileSprite("Assets/Tiles/key_blue_0.asset");
        Sprite checkpointTwoSprite = LoadTileSprite("Assets/Tiles/gem_blue_0.asset");
        Sprite finishSprite = LoadTileSprite("Assets/Tiles/flag_green_a_0.asset");
        Sprite exitSprite = LoadTileSprite("Assets/Tiles/sign_exit_0.asset");

        Vector3 checkpointOnePosition = GetPlatformPosition(tilemap, firstCheckpointPlatform,
            firstCheckpointPlatform.minX + 2);
        Vector3 checkpointTwoPosition = GetPlatformPosition(tilemap, secondCheckpointPlatform,
            secondCheckpointPlatform.minX + 2);
        Vector3 finishPosition = GetPlatformPosition(tilemap, finishPlatform, finishPlatform.maxX - 1);

        CreateCheckpoint(markerContainer.transform, "Checkpoint_01", 1,
            checkpointOnePosition, checkpointOneSprite, 1);
        CreateCheckpoint(markerContainer.transform, "Checkpoint_02", 2,
            checkpointTwoPosition, checkpointTwoSprite, 2);
        CreateFinish(markerContainer.transform, finishPosition, finishSprite, exitSprite);
        InstallHazards(scene, tilemap, firstCheckpointPlatform, secondCheckpointPlatform);
    }

    private static GameObject ReplaceSceneObject(Scene scene, string name, Transform parent)
    {
        GameObject existing = parent == null
            ? scene.GetRootGameObjects().FirstOrDefault(gameObject => gameObject.name == name)
            : parent.Find(name)?.gameObject;
        if (existing != null)
            Undo.DestroyObjectImmediate(existing);

        GameObject created = new GameObject(name);
        if (parent != null)
            created.transform.SetParent(parent, false);
        else
            SceneManager.MoveGameObjectToScene(created, scene);
        Undo.RegisterCreatedObjectUndo(created, "Create " + name);
        return created;
    }

    private static Sprite LoadTileSprite(string path)
    {
        Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
        return tile != null ? tile.sprite : null;
    }

    private static void CreateCheckpoint(Transform parent, string name, int number,
        Vector3 position, Sprite objectiveSprite, int objectiveId)
    {
        GameObject checkpoint = ReplaceSceneObject(parent.gameObject.scene, name, parent);
        checkpoint.transform.position = position;
        BoxCollider2D trigger = checkpoint.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        trigger.size = new Vector2(1.2f, 1.9f);
        trigger.offset = new Vector2(0f, 0.9f);
        checkpoint.AddComponent<CheckpointTrigger>().Configure(number, position, objectiveId);
        string visualName = objectiveId == 1 ? "KunciKristal" : "RelikEs";
        AddMarkerVisual(checkpoint.transform, visualName, objectiveSprite,
            new Vector3(0f, 0.75f, 0f), 2f, 15);
    }

    private static void CreateFinish(Transform parent, Vector3 position, Sprite flagSprite, Sprite exitSprite)
    {
        GameObject finish = ReplaceSceneObject(parent.gameObject.scene, "FinishTrigger", parent);
        finish.transform.position = position;
        BoxCollider2D trigger = finish.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        trigger.size = new Vector2(1.8f, 2.2f);
        trigger.offset = new Vector2(0f, 1.05f);
        finish.AddComponent<FinishTrigger>();
        AddMarkerVisual(finish.transform, "FinishFlag", flagSprite, new Vector3(-0.25f, 0.8f, 0f), 1.5f, 15);
        AddMarkerVisual(finish.transform, "ExitSign", exitSprite, new Vector3(0.65f, 0.35f, 0f), 0.85f, 16);
    }

    private static void AddMarkerVisual(Transform parent, string name, Sprite sprite,
        Vector3 localPosition, float scale, int sortingOrder)
    {
        if (sprite == null)
            return;

        GameObject visual = new GameObject(name);
        visual.transform.SetParent(parent, false);
        visual.transform.localPosition = localPosition;
        visual.transform.localScale = Vector3.one * scale;
        SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = sortingOrder;
    }

    private static void InstallHazards(Scene scene, Tilemap tilemap,
        PlatformRun firstCheckpointPlatform, PlatformRun secondCheckpointPlatform)
    {
        GameObject hazards = ReplaceSceneObject(scene, "LevelHazards", null);
        Tile spikeTile = AssetDatabase.LoadAssetAtPath<Tile>("Assets/Tiles/spikes_0.asset");
        Sprite spikeSprite = spikeTile != null ? spikeTile.sprite : null;

        BoundsInt bounds = tilemap.cellBounds;
        for (int y = bounds.yMin; y < bounds.yMax; y++)
        {
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                TileBase tile = tilemap.GetTile(cell);
                if (tile == null || tile.name.IndexOf("spike", StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                CreateHazard(hazards.transform, "MapSpike_" + x + "_" + y,
                    tilemap.GetCellCenterWorld(cell), null, new Vector2(0.95f, 0.95f));
            }
        }

        if (spikeSprite != null)
        {
            CreateSpike(hazards.transform, tilemap, firstCheckpointPlatform,
                firstCheckpointPlatform.maxX - 3, spikeSprite);
            CreateSpike(hazards.transform, tilemap, firstCheckpointPlatform,
                firstCheckpointPlatform.maxX - 2, spikeSprite);
            CreateSpike(hazards.transform, tilemap, secondCheckpointPlatform,
                secondCheckpointPlatform.maxX - 3, spikeSprite);
            CreateSpike(hazards.transform, tilemap, secondCheckpointPlatform,
                secondCheckpointPlatform.maxX - 2, spikeSprite);
        }

        Vector3 fallCenter = tilemap.CellToWorld(new Vector3Int(
            bounds.xMin + bounds.size.x / 2, bounds.yMin - 5, 0));
        CreateHazard(hazards.transform, "FallReset", fallCenter, null,
            new Vector2(bounds.size.x + 20f, 1f));
    }

    private static void CreateSpike(Transform parent, Tilemap tilemap, PlatformRun platform,
        int x, Sprite sprite)
    {
        int cellX = Mathf.Clamp(x, platform.minX + 1, platform.maxX - 1);
        Vector3Int emptyCell = new Vector3Int(cellX, platform.y + 1, 0);
        Vector3 position = tilemap.GetCellCenterWorld(emptyCell);
        CreateHazard(parent, "Spike_" + cellX + "_" + platform.y,
            position, sprite, new Vector2(0.95f, 0.9f));
    }

    private static void CreateHazard(Transform parent, string name, Vector3 position,
        Sprite sprite, Vector2 colliderSize)
    {
        GameObject hazard = new GameObject(name);
        hazard.transform.SetParent(parent, false);
        hazard.transform.position = position;
        BoxCollider2D trigger = hazard.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        trigger.size = colliderSize;
        trigger.offset = new Vector2(0f, -0.08f);
        hazard.AddComponent<HazardTrigger>();

        if (sprite != null)
        {
            SpriteRenderer renderer = hazard.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 10;
        }
    }

    private static void SetupBackground(Scene scene, Tilemap tilemap)
    {
        TextureImporter importer = AssetImporter.GetAtPath(BackgroundPath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogError("Background PNG belum terimpor: " + BackgroundPath);
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 32f;
        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteAlignment = (int)SpriteAlignment.Center;
        importer.SetTextureSettings(settings);
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.maxTextureSize = 4096;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.SaveAndReimport();

        Sprite backgroundSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundPath);
        if (backgroundSprite == null)
        {
            Debug.LogError("Sprite background gagal dimuat: " + BackgroundPath);
            return;
        }

        Camera sceneCamera = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Camera>(true))
            .FirstOrDefault(camera => camera.CompareTag("MainCamera"));
        if (sceneCamera == null)
        {
            Debug.LogError("Main Camera tidak ditemukan untuk background level.");
            return;
        }

        BoundsInt cellBounds = tilemap.cellBounds;
        Vector3 lower = tilemap.CellToWorld(cellBounds.min);
        Vector3 upper = tilemap.CellToWorld(cellBounds.max);
        Vector3 center = (lower + upper) * 0.5f;

        GameObject background = scene.GetRootGameObjects()
            .FirstOrDefault(gameObject => gameObject.name == "Level1Background");
        if (background == null)
        {
            background = new GameObject("Level1Background");
            SceneManager.MoveGameObjectToScene(background, scene);
            Undo.RegisterCreatedObjectUndo(background, "Add Level 1 Background");
        }

        Undo.RecordObject(background.transform, "Position Level 1 Background");
        background.transform.position = new Vector3(center.x, center.y, 0f);
        background.transform.localScale = Vector3.one;

        SpriteRenderer renderer = background.GetComponent<SpriteRenderer>();
        if (renderer == null)
            renderer = Undo.AddComponent<SpriteRenderer>(background);
        renderer.sprite = backgroundSprite;
        renderer.sortingOrder = -100;

        BackgroundParallax parallax = background.GetComponent<BackgroundParallax>();
        if (parallax == null)
            parallax = Undo.AddComponent<BackgroundParallax>(background);
        parallax.Configure(sceneCamera, 0.2f);
    }

    private static void AddTilemapColliders(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Tilemap tilemap in root.GetComponentsInChildren<Tilemap>(true))
            {
                if (tilemap.GetComponent<TilemapCollider2D>() == null)
                    Undo.AddComponent<TilemapCollider2D>(tilemap.gameObject);
            }
        }
    }

    private static void EnsureFolder(string parent, string folderName)
    {
        string path = parent + "/" + folderName;
        if (!AssetDatabase.IsValidFolder(path))
            AssetDatabase.CreateFolder(parent, folderName);
    }
}
