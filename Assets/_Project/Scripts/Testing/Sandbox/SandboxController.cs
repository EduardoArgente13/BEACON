using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using BEACON.Combat;
using BEACON.Combat.Core;
using BEACON.Core.Health;
using BEACON.Core.Stats;
using BEACON.Player;

/// <summary>
/// Runtime sandbox tools: weapon swap, enemy spawn, god mode, infinite resources,
/// slow-mo and debug overlays. Designed to drop into any scene for rapid testing.
/// </summary>
public class SandboxController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CombatController combatController;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private ResonanceController resonanceController;
    [SerializeField] private Health playerHealth;
    [SerializeField] private HitboxController hitboxController;

    [Header("Weapon Loadout")]
    [Tooltip("Order matches hotkeys 1-4")]
    [SerializeField] private HybridWeaponData[] weapons;

    [Header("Enemy Spawner")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform spawnOrigin;
    [SerializeField] private float spawnOffset = 3f;

    [Header("Debug Flags")]
    [SerializeField] private bool showHUD = true;
    [SerializeField] private bool godMode = false;
    [SerializeField] private bool infiniteResonance = false;
    [SerializeField] private bool infiniteDash = false;
    [SerializeField, Range(0.1f, 2f)] private float slowMoScale = 1f;

    private int currentWeaponIndex;
    private float damageDealt;
    private float comboCounter;
    private readonly List<GameObject> spawnedEnemies = new List<GameObject>();

    private void Awake()
    {
        if (combatController == null) combatController = FindFirstObjectByType<CombatController>();
        if (playerController == null) playerController = FindFirstObjectByType<PlayerController>();
        if (resonanceController == null) resonanceController = FindFirstObjectByType<ResonanceController>();
        if (playerHealth == null) playerHealth = FindFirstObjectByType<Health>();
        if (hitboxController == null && combatController != null)
        {
            var field = typeof(CombatController).GetField("hitboxController", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            hitboxController = field != null ? field.GetValue(combatController) as HitboxController : null;
        }

        if (hitboxController != null)
        {
            hitboxController.OnHitConfirmed.AddListener(OnHitConfirmed);
        }
    }

    private void Start()
    {
        EquipWeapon(currentWeaponIndex);
    }

    private void Update()
    {
        HandleHotkeys();
        ApplyCheats();
    }

    private void HandleHotkeys()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.f1Key.wasPressedThisFrame) showHUD = !showHUD;
        if (kb.f2Key.wasPressedThisFrame) godMode = !godMode;
        if (kb.f3Key.wasPressedThisFrame) infiniteResonance = !infiniteResonance;
        if (kb.f4Key.wasPressedThisFrame) ToggleSlowMo();
        if (kb.f5Key.wasPressedThisFrame) SpawnEnemyAtCursor();
        if (kb.f6Key.wasPressedThisFrame) ClearEnemies();
        if (kb.f7Key.wasPressedThisFrame) ToggleHitboxes();
        if (kb.f8Key.wasPressedThisFrame) ResetPlayer();

        if (kb.digit1Key.wasPressedThisFrame) EquipWeapon(0);
        if (kb.digit2Key.wasPressedThisFrame) EquipWeapon(1);
        if (kb.digit3Key.wasPressedThisFrame) EquipWeapon(2);
        if (kb.digit4Key.wasPressedThisFrame) EquipWeapon(3);
    }

    private void ApplyCheats()
    {
        if (infiniteResonance && resonanceController != null)
        {
            resonanceController.AddResonance(resonanceController.MaxResonance);
        }

        if (infiniteDash && playerController != null)
        {
            // Reset air dashes every frame to mimic infinite dashes.
            playerController.ResetAirDashes();
        }

        if (godMode && playerHealth != null && playerController != null)
        {
            playerHealth.Heal(playerHealth.MaxHealth);
            playerController.StartIFrames(0.2f);
        }
    }

    private void ToggleSlowMo()
    {
        if (Mathf.Approximately(Time.timeScale, 1f))
        {
            Time.timeScale = slowMoScale;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }
        else
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
        }
    }

    private void ToggleHitboxes()
    {
        if (hitboxController != null)
        {
            var field = typeof(HitboxController).GetField("showDebugGizmos", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (field != null)
            {
                bool current = (bool)field.GetValue(hitboxController);
                field.SetValue(hitboxController, !current);
            }
        }
    }

    private void EquipWeapon(int index)
    {
        if (weapons == null || weapons.Length == 0) return;
        index = Mathf.Clamp(index, 0, weapons.Length - 1);
        if (combatController != null && weapons[index] != null)
        {
            combatController.EquipWeapon(weapons[index]);
            currentWeaponIndex = index;
        }
    }

    private void SpawnEnemyAtCursor()
    {
        if (enemyPrefab == null || Camera.main == null) return;

        Vector3 mouseWorld;
        if (Mouse.current != null)
        {
            mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        }
        else
        {
            mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        mouseWorld.z = 0;

        SpawnEnemy(mouseWorld);
    }

    public void SpawnEnemy(Vector3 position)
    {
        var go = Instantiate(enemyPrefab, position, Quaternion.identity);
        spawnedEnemies.Add(go);
    }

    public void SpawnEnemyInFrontOfPlayer(int count = 1)
    {
        if (playerController == null || enemyPrefab == null) return;

        Vector3 origin = playerController.transform.position;
        Vector3 dir = playerController.IsFacingRight ? Vector3.right : Vector3.left;

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = origin + dir * (spawnOffset + i * 1.5f);
            SpawnEnemy(pos);
        }
    }

    public void ClearEnemies()
    {
        foreach (var e in spawnedEnemies)
        {
            if (e != null) Destroy(e);
        }
        spawnedEnemies.Clear();
    }

    private void ResetPlayer()
    {
        if (playerController == null) return;
        playerController.transform.position = Vector3.zero;
        if (resonanceController != null) resonanceController.AddResonance(resonanceController.MaxResonance);
        if (playerHealth != null) playerHealth.Heal(playerHealth.MaxHealth);
    }

    private void OnHitConfirmed(GameObject target, Vector2 point, float damage)
    {
        damageDealt += damage;
        comboCounter += 1;
    }

    private void OnGUI()
    {
        if (!showHUD) return;

        GUILayout.BeginArea(new Rect(10, 10, 320, 260), GUI.skin.box);
        GUILayout.Label($"Weapon: {(weapons != null && weapons.Length > 0 && weapons[currentWeaponIndex] != null ? weapons[currentWeaponIndex].weaponName : "None")}");
        if (resonanceController != null)
        {
            GUILayout.Label($"Resonance: {resonanceController.CurrentResonance:F0}/{resonanceController.MaxResonance:F0}");
        }
        GUILayout.Label($"Damage Dealt: {damageDealt:F0}");
        GUILayout.Label($"Hits (session): {comboCounter:F0}");
        GUILayout.Space(6);

        GUILayout.Label("Cheats:");
        godMode = GUILayout.Toggle(godMode, "God Mode (F2)");
        infiniteResonance = GUILayout.Toggle(infiniteResonance, "Infinite Resonance (F3)");
        infiniteDash = GUILayout.Toggle(infiniteDash, "Infinite Dash");

        GUILayout.Space(6);
        GUILayout.Label("SlowMo (F4 toggles):");
        slowMoScale = GUILayout.HorizontalSlider(slowMoScale, 0.1f, 2f);
        GUILayout.Label($"{slowMoScale:F2}x");
        GUILayout.Space(6);

        GUILayout.Label("Spawner:");
        if (GUILayout.Button("Spawn at Cursor (F5)")) SpawnEnemyAtCursor();
        if (GUILayout.Button("Spawn in Front (x3)")) SpawnEnemyInFrontOfPlayer(3);
        if (GUILayout.Button("Clear Enemies (F6)")) ClearEnemies();
        GUILayout.EndArea();
    }
}
