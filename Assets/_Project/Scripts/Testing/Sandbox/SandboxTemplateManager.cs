using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Builds three sandbox templates (Easy/Medium/Hard) at runtime and provides teleport,
/// enemy spawn per zone, and quick reset controls. Designed to work alongside SandboxController.
/// </summary>
public class SandboxTemplateManager : MonoBehaviour
{
    [System.Serializable]
    private class PlatformSpec
    {
        public Vector2 position;
        public Vector2 size = new Vector2(6, 1);
        public bool moving = false;
        public Vector2 moveOffset = new Vector2(4, 0);
        public float moveSpeed = 1.5f;
    }

    [System.Serializable]
    private class TemplateConfig
    {
        public string name;
        public Vector2 origin;
        public Vector2 playerSpawn;
        public List<PlatformSpec> platforms = new List<PlatformSpec>();
        public int enemiesToSpawn = 3;
    }

    [Header("Prefabs")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Material platformMaterial;
    [SerializeField] private Sprite platformSprite;

    [Header("Layout")]
    [SerializeField] private Vector2 easyOrigin = new Vector2(0, 0);
    [SerializeField] private Vector2 mediumOrigin = new Vector2(60, 0);
    [SerializeField] private Vector2 hardOrigin = new Vector2(120, 0);

    [Header("Settings")]
    [SerializeField] private float platformDepth = 0f;
    [SerializeField] private Color easyColor = new Color(0.2f, 0.8f, 0.2f, 1f);
    [SerializeField] private Color mediumColor = new Color(0.2f, 0.4f, 0.9f, 1f);
    [SerializeField] private Color hardColor = new Color(0.9f, 0.2f, 0.2f, 1f);

    private readonly List<TemplateConfig> templates = new List<TemplateConfig>();
    private int currentTemplateIndex = 0;
    private Transform player;

    private readonly List<GameObject> spawnedPlatforms = new List<GameObject>();
    private readonly List<GameObject> spawnedEnemies = new List<GameObject>();

    private void Awake()
    {
        player = FindFirstObjectByType<BEACON.Player.PlayerController>()?.transform;

        templates.Add(BuildEasyTemplate());
        templates.Add(BuildMediumTemplate());
        templates.Add(BuildHardTemplate());
    }

    private void Start()
    {
        BuildPlatforms();
        TeleportToTemplate(0);
    }

    private TemplateConfig BuildEasyTemplate()
    {
        var t = new TemplateConfig
        {
            name = "Easy - Fundamentals",
            origin = easyOrigin,
            playerSpawn = easyOrigin + new Vector2(0, 2),
            enemiesToSpawn = 3
        };

        t.platforms.Add(new PlatformSpec { position = t.origin + new Vector2(0, 0), size = new Vector2(14, 2) });
        t.platforms.Add(new PlatformSpec { position = t.origin + new Vector2(8, 3), size = new Vector2(6, 1) });
        t.platforms.Add(new PlatformSpec { position = t.origin + new Vector2(16, 5), size = new Vector2(5, 1) });
        t.platforms.Add(new PlatformSpec { position = t.origin + new Vector2(25, 2), size = new Vector2(4, 1) }); // coyote/buffer gap
        t.platforms.Add(new PlatformSpec { position = t.origin + new Vector2(32, 0), size = new Vector2(12, 2) });

        return t;
    }

    private TemplateConfig BuildMediumTemplate()
    {
        var t = new TemplateConfig
        {
            name = "Medium - Mechanics Integration",
            origin = mediumOrigin,
            playerSpawn = mediumOrigin + new Vector2(0, 3),
            enemiesToSpawn = 6
        };

        t.platforms.Add(new PlatformSpec { position = t.origin + new Vector2(0, 0), size = new Vector2(16, 2) });
        t.platforms.Add(new PlatformSpec { position = t.origin + new Vector2(10, 4), size = new Vector2(5, 1), moving = true, moveOffset = new Vector2(0, 3), moveSpeed = 1.2f });
        t.platforms.Add(new PlatformSpec { position = t.origin + new Vector2(20, 6), size = new Vector2(6, 1) });
        t.platforms.Add(new PlatformSpec { position = t.origin + new Vector2(30, 3), size = new Vector2(4, 1) });
        t.platforms.Add(new PlatformSpec { position = t.origin + new Vector2(38, 7), size = new Vector2(5, 1) });
        t.platforms.Add(new PlatformSpec { position = t.origin + new Vector2(48, 2), size = new Vector2(12, 2) });

        return t;
    }

    private TemplateConfig BuildHardTemplate()
    {
        var t = new TemplateConfig
        {
            name = "Hard - Advanced Mastery",
            origin = hardOrigin,
            playerSpawn = hardOrigin + new Vector2(0, 4),
            enemiesToSpawn = 12
        };

        t.platforms.Add(new PlatformSpec { position = t.origin + new Vector2(0, 0), size = new Vector2(18, 2) });
        t.platforms.Add(new PlatformSpec { position = t.origin + new Vector2(10, 6), size = new Vector2(4, 1), moving = true, moveOffset = new Vector2(8, 0), moveSpeed = 1.8f });
        t.platforms.Add(new PlatformSpec { position = t.origin + new Vector2(24, 10), size = new Vector2(3, 1) });
        t.platforms.Add(new PlatformSpec { position = t.origin + new Vector2(30, 5), size = new Vector2(3, 1) });
        t.platforms.Add(new PlatformSpec { position = t.origin + new Vector2(36, 12), size = new Vector2(3, 1) });
        t.platforms.Add(new PlatformSpec { position = t.origin + new Vector2(48, 3), size = new Vector2(14, 2) });

        return t;
    }

    private void BuildPlatforms()
    {
        ClearPlatforms();

        foreach (var template in templates)
        {
            Color tint = easyColor;
            if (template == templates[1]) tint = mediumColor;
            if (template == templates[2]) tint = hardColor;

            foreach (var p in template.platforms)
            {
                var go = CreatePlatform(p, tint);
                spawnedPlatforms.Add(go);
            }
        }
    }

    private GameObject CreatePlatform(PlatformSpec spec, Color tint)
    {
        var go = new GameObject("Platform");
        go.transform.position = new Vector3(spec.position.x, spec.position.y, platformDepth);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.color = tint;
        sr.drawMode = SpriteDrawMode.Sliced;
        sr.size = spec.size;
        // Ensure a visible sprite; fallback to white texture if none provided.
        if (platformSprite != null)
        {
            sr.sprite = platformSprite;
        }
        else
        {
            sr.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height), new Vector2(0.5f, 0.5f), 100f);
        }
        if (platformMaterial != null) sr.material = platformMaterial;

        var bc = go.AddComponent<BoxCollider2D>();
        bc.size = spec.size;
        bc.offset = Vector2.zero;

        if (spec.moving)
        {
            var mover = go.AddComponent<SandboxMovingPlatform>();
            mover.offset = spec.moveOffset;
            mover.speed = spec.moveSpeed;
        }
        return go;
    }

    private void ClearPlatforms()
    {
        foreach (var p in spawnedPlatforms)
        {
            if (p != null) Destroy(p);
        }
        spawnedPlatforms.Clear();
    }

    private void SpawnTemplateEnemies(int index)
    {
        if (enemyPrefab == null) return;
        if (index < 0 || index >= templates.Count) return;

        var template = templates[index];
        ClearTemplateEnemies();

        for (int i = 0; i < template.enemiesToSpawn; i++)
        {
            Vector2 pos = template.origin + new Vector2(6 + i * 2f, 4f);
            var e = Instantiate(enemyPrefab, pos, Quaternion.identity);
            spawnedEnemies.Add(e);
        }
    }

    private void ClearTemplateEnemies()
    {
        foreach (var e in spawnedEnemies)
        {
            if (e != null) Destroy(e);
        }
        spawnedEnemies.Clear();
    }

    public void TeleportToTemplate(int index)
    {
        if (player == null) return;
        index = Mathf.Clamp(index, 0, templates.Count - 1);
        currentTemplateIndex = index;
        var template = templates[index];

        player.position = new Vector3(template.playerSpawn.x, template.playerSpawn.y, 0);
        ClearTemplateEnemies();
        SpawnTemplateEnemies(index);
    }

    public void ResetCurrentTemplate()
    {
        TeleportToTemplate(currentTemplateIndex);
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(Screen.width - 220, 10, 210, 200), GUI.skin.box);
        GUILayout.Label("Sandbox Templates");
        if (GUILayout.Button("Easy (1)")) TeleportToTemplate(0);
        if (GUILayout.Button("Medium (2)")) TeleportToTemplate(1);
        if (GUILayout.Button("Hard (3)")) TeleportToTemplate(2);
        if (GUILayout.Button("Reset Current")) ResetCurrentTemplate();
        if (GUILayout.Button("Reload Scene")) SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        GUILayout.EndArea();
    }
}
