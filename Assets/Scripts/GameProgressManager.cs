using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameProgressManager : MonoBehaviour
{
    [SerializeField] private NinjaGirlController player;
    [SerializeField] private Vector3 initialSpawn;
    [SerializeField] private int totalCheckpoints = 2;
    [SerializeField] private Sprite crystalKeyIcon;
    [SerializeField] private Sprite iceRelicIcon;

    private Vector3 respawnPosition;
    private int activeCheckpoint;
    private bool respawning;
    private bool levelComplete;
    private bool hasCrystalKey;
    private bool hasIceRelic;
    private float levelCompleteAt;
    private string noticeTitle;
    private string noticeBody;
    private float noticeUntil;
    private GUIStyle hudStyle;
    private GUIStyle headingStyle;
    private GUIStyle bodyStyle;
    private GUIStyle titleStyle;
    private GUIStyle centerBodyStyle;
    private GUIStyle victoryTitleStyle;
    private GUIStyle victoryKickerStyle;
    private GUIStyle victoryBodyStyle;
    private GUIStyle rewardTitleStyle;
    private GUIStyle victoryButtonStyle;
    private GUIStyle victorySecondaryButtonStyle;

    public static GameProgressManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        if (player == null)
            player = FindAnyObjectByType<NinjaGirlController>();
    }

    private void Start()
    {
        activeCheckpoint = 0;
        hasCrystalKey = false;
        hasIceRelic = false;
        respawning = false;
        levelComplete = false;
        Time.timeScale = 1f;

        if (player != null)
        {
            player.ResetJumpUpgrades();
            if (initialSpawn == Vector3.zero)
                initialSpawn = player.transform.position;

            respawnPosition = initialSpawn;
            player.transform.position = initialSpawn;
            player.enabled = true;

            Rigidbody2D body = player.GetComponent<Rigidbody2D>();
            if (body != null)
            {
                body.simulated = true;
                body.linearVelocity = Vector2.zero;
            }
        }
        else
        {
            respawnPosition = initialSpawn;
        }

        ShowNotice("MISI LEVEL 1", "Ambil Kunci Kristal, temukan Relik Es, lalu capai EXIT!", 5f);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void ActivateCheckpoint(int checkpointNumber, Vector3 position, int objectiveId)
    {
        if (levelComplete)
            return;

        bool checkpointUpdated = checkpointNumber > activeCheckpoint;
        if (checkpointUpdated)
        {
            activeCheckpoint = checkpointNumber;
            respawnPosition = position;
        }

        bool objectiveCollected = CollectObjective(objectiveId);
        if (objectiveCollected)
        {
            player?.UnlockLongJump(checkpointNumber);
            string skillName = checkpointNumber == 1 ? "LOMPATAN PANJANG I" : "LOMPATAN PANJANG II";
            string itemName = objectiveId == 1 ? "KUNCI KRISTAL DIDAPAT!" : "RELIK ES DIDAPAT!";
            ShowNotice("SKILL BARU: " + skillName,
                itemName + "  •  SHIFT + LOMPAT untuk lompat jauh (3x/checkpoint).  " +
                ObjectiveCount() + "/2 misi.", 4.5f);
        }
        else if (checkpointUpdated)
        {
            ShowNotice("CHECKPOINT " + checkpointNumber + " AKTIF", "Titik respawn baru telah disimpan.", 3f);
        }
    }

    private bool CollectObjective(int objectiveId)
    {
        if (objectiveId == 1 && !hasCrystalKey)
        {
            hasCrystalKey = true;
            return true;
        }

        if (objectiveId == 2 && !hasIceRelic)
        {
            hasIceRelic = true;
            return true;
        }

        return false;
    }

    private int ObjectiveCount()
    {
        return (hasCrystalKey ? 1 : 0) + (hasIceRelic ? 1 : 0);
    }

    public void HitHazard()
    {
        if (!respawning && !levelComplete && player != null)
            StartCoroutine(RespawnAfterDeath());
    }

    public void CompleteLevel()
    {
        if (levelComplete || !hasCrystalKey || !hasIceRelic)
        {
            if (!levelComplete)
            {
                string missing = !hasCrystalKey && !hasIceRelic
                    ? "Kunci Kristal dan Relik Es masih dicari."
                    : !hasCrystalKey ? "Kunci Kristal masih dicari." : "Relik Es masih dicari.";
                ShowNotice("MISI BELUM LENGKAP", missing + " Kembali ke EXIT setelah lengkap.", 4f);
            }
            return;
        }

        levelComplete = true;
        levelCompleteAt = Time.unscaledTime;
        if (player != null)
        {
            player.enabled = false;
            Rigidbody2D body = player.GetComponent<Rigidbody2D>();
            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
                body.simulated = false;
            }
        }

        ShowNotice("SELAMAT, PETUALANG!", "Kunci Kristal dan Relik Es berhasil kamu kumpulkan!", 999f);
    }

    private IEnumerator RespawnAfterDeath()
    {
        respawning = true;
        ShowNotice("TERKENA DURI!", "Kembali ke checkpoint...", 1f);

        Rigidbody2D body = player.GetComponent<Rigidbody2D>();
        if (body != null)
        {
            body.linearVelocity = Vector2.zero;
            body.simulated = false;
        }
        player.enabled = false;

        yield return new WaitForSecondsRealtime(0.7f);

        player.transform.position = respawnPosition;
        if (body != null)
        {
            body.simulated = true;
            body.linearVelocity = Vector2.zero;
        }
        player.enabled = true;
        respawning = false;
        ShowNotice("KEMBALI BERMAIN", "Checkpoint " + activeCheckpoint + " / " + totalCheckpoints, 2f);
    }

    private void Update()
    {
        if (levelComplete && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            RestartLevel();
        else if (levelComplete && Keyboard.current != null && Keyboard.current.mKey.wasPressedThisFrame)
            ReturnToMainMenu();
    }

    private void OnGUI()
    {
        EnsureStyles();
        float scale = Mathf.Clamp(Screen.width / 1000f, 0.72f, 1.25f);
        float hudWidth = Mathf.Min(340f * scale, Screen.width - 28f);
        Rect hud = new Rect(18f, 18f, hudWidth, 144f * scale);
        DrawPanel(hud, new Color(0.035f, 0.075f, 0.11f, 0.88f));
        DrawPanel(new Rect(hud.x, hud.y, 5f * scale, hud.height), new Color(0.32f, 0.9f, 0.66f, 1f));
        GUI.Label(new Rect(hud.x + 18f * scale, hud.y + 9f * scale, hud.width - 30f, 24f * scale),
            "LEVEL 01   /   PLATFORMER", hudStyle);
        GUI.Label(new Rect(hud.x + 18f * scale, hud.y + 34f * scale, hud.width - 30f, 25f * scale),
            "CHECKPOINT  " + activeCheckpoint + " / " + totalCheckpoints, bodyStyle);
        GUI.Label(new Rect(hud.x + 18f * scale, hud.y + 59f * scale, hud.width - 30f, 23f * scale),
            "KUNCI KRISTAL: " + (hasCrystalKey ? "DIDAPAT" : "BELUM DIDAPAT"), bodyStyle);
        GUI.Label(new Rect(hud.x + 18f * scale, hud.y + 82f * scale, hud.width - 30f, 23f * scale),
            "RELIK ES: " + (hasIceRelic ? "DIDAPAT" : "BELUM DIDAPAT"), bodyStyle);
        GUI.Label(new Rect(hud.x + 18f * scale, hud.y + 105f * scale, hud.width - 30f, 23f * scale),
            player != null && player.JumpUpgradeLevel > 0
                ? "LONG JUMP " + player.JumpUpgradeLevel + "    " + player.LongJumpCharges + "/" + player.LongJumpsPerCheckpoint
                : "LONG JUMP: LOCKED", bodyStyle);

        if (Time.unscaledTime < noticeUntil)
            DrawNotice(scale);

        if (levelComplete)
            DrawVictory(scale);
    }

    private void DrawNotice(float scale)
    {
        float width = Mathf.Min(520f * scale, Screen.width - 32f);
        float height = 78f * scale;
        Rect notice = new Rect((Screen.width - width) * 0.5f, 174f, width, height);
        DrawPanel(notice, new Color(0.035f, 0.075f, 0.11f, 0.93f));
        DrawPanel(new Rect(notice.x, notice.y + notice.height - 4f * scale, notice.width, 4f * scale),
            new Color(0.32f, 0.9f, 0.66f, 1f));
        GUI.Label(new Rect(notice.x + 12f, notice.y + 10f * scale, notice.width - 24f, 28f * scale),
            noticeTitle, headingStyle);
        GUI.Label(new Rect(notice.x + 12f, notice.y + 39f * scale, notice.width - 24f, 28f * scale),
            noticeBody, centerBodyStyle);
    }

    private void DrawVictory(float scale)
    {
        DrawPanel(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0.015f, 0.035f, 0.06f, 0.84f));
        DrawVictoryConfetti(scale);

        float width = Mathf.Min(720f * scale, Screen.width - 28f);
        float height = Mathf.Min(430f * scale, Screen.height - 24f);
        Rect panel = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
        DrawPanel(new Rect(panel.x + 7f * scale, panel.y + 8f * scale, panel.width, panel.height),
            new Color(0f, 0f, 0f, 0.3f));
        DrawPanel(panel, new Color(0.035f, 0.075f, 0.11f, 0.98f));
        DrawPanel(new Rect(panel.x, panel.y, panel.width, 6f * scale), new Color(0.96f, 0.69f, 0.3f));

        float padding = 28f * scale;
        float contentX = panel.x + padding;
        float contentWidth = panel.width - padding * 2f;
        GUI.Label(new Rect(contentX, panel.y + 20f * scale, contentWidth, 23f * scale),
            "LEVEL 01   /   MISSION COMPLETE", victoryKickerStyle);
        GUI.Label(new Rect(contentX, panel.y + 45f * scale, contentWidth, 50f * scale),
            "SELAMAT, PETUALANG!", victoryTitleStyle);
        GUI.Label(new Rect(contentX, panel.y + 96f * scale, contentWidth, 28f * scale),
            "Kamu berhasil menaklukkan Lembah Kristal.", victoryBodyStyle);
        DrawPanel(new Rect(contentX, panel.y + 132f * scale, contentWidth, 1f),
            new Color(0.55f, 0.71f, 0.76f, 0.38f));

        float gap = 14f * scale;
        float cardWidth = (contentWidth - gap) * 0.5f;
        float cardY = panel.y + 147f * scale;
        float cardHeight = 78f * scale;
        DrawRewardCard(new Rect(contentX, cardY, cardWidth, cardHeight),
            "RELIC 01", "KUNCI KRISTAL", crystalKeyIcon, new Color(1f, 0.72f, 0.25f));
        DrawRewardCard(new Rect(contentX + cardWidth + gap, cardY, cardWidth, cardHeight),
            "RELIC 02", "RELIK ES", iceRelicIcon, new Color(0.34f, 0.82f, 1f));

        Rect skillBanner = new Rect(contentX, panel.y + 237f * scale, contentWidth, 43f * scale);
        DrawPanel(skillBanner, new Color(0.12f, 0.25f, 0.27f, 0.95f));
        DrawPanel(new Rect(skillBanner.x, skillBanner.y, 4f * scale, skillBanner.height),
            new Color(0.38f, 0.9f, 0.8f));
        GUI.Label(new Rect(skillBanner.x + 14f * scale, skillBanner.y + 4f * scale,
            skillBanner.width - 28f * scale, 34f * scale),
            "SKILL DIBUKA  •  LOMPATAN PANJANG II", victoryKickerStyle);

        float buttonY = panel.y + 294f * scale;
        float buttonGap = 12f * scale;
        float buttonWidth = (contentWidth - buttonGap) * 0.5f;
        if (GUI.Button(new Rect(contentX, buttonY, buttonWidth, 46f * scale),
                "MAIN LAGI   [R]", victoryButtonStyle))
            RestartLevel();
        if (GUI.Button(new Rect(contentX + buttonWidth + buttonGap, buttonY, buttonWidth, 46f * scale),
                "MENU UTAMA   [M]", victorySecondaryButtonStyle))
            ReturnToMainMenu();

        GUI.Label(new Rect(contentX, panel.y + 354f * scale, contentWidth, 20f * scale),
            "TERIMA KASIH SUDAH BERMAIN", victoryKickerStyle);
    }

    private void DrawRewardCard(Rect rect, string category, string title, Sprite icon, Color accent)
    {
        DrawPanel(rect, new Color(0.075f, 0.14f, 0.19f, 0.98f));
        DrawPanel(new Rect(rect.x, rect.y, 4f, rect.height), accent);
        Rect iconBox = new Rect(rect.x + 12f, rect.y + 10f, rect.height - 20f, rect.height - 20f);
        DrawPanel(iconBox, new Color(accent.r, accent.g, accent.b, 0.16f));

        if (icon != null && icon.texture != null)
        {
            Rect uv = new Rect(icon.rect.x / icon.texture.width, icon.rect.y / icon.texture.height,
                icon.rect.width / icon.texture.width, icon.rect.height / icon.texture.height);
            GUI.DrawTextureWithTexCoords(iconBox, icon.texture, uv, true);
        }
        else
        {
            GUI.Label(iconBox, "✦", victoryTitleStyle);
        }

        float textX = iconBox.xMax + 12f;
        float textWidth = rect.xMax - textX - 10f;
        GUI.Label(new Rect(textX, rect.y + 9f, textWidth, 18f), category, victoryKickerStyle);
        GUI.Label(new Rect(textX, rect.y + 29f, textWidth, 28f), title, rewardTitleStyle);
        GUI.Label(new Rect(textX, rect.y + 56f, textWidth, 16f), "OBJECTIVE CLEARED", victoryBodyStyle);
    }

    private void DrawVictoryConfetti(float scale)
    {
        float elapsed = Time.unscaledTime - levelCompleteAt;
        Color[] colors =
        {
            new Color(0.38f, 0.9f, 0.8f, 0.85f),
            new Color(1f, 0.72f, 0.25f, 0.9f),
            new Color(0.34f, 0.82f, 1f, 0.85f),
            new Color(0.94f, 0.97f, 0.93f, 0.75f)
        };

        for (int i = 0; i < 28; i++)
        {
            float x = Mathf.Repeat(i * 127f + elapsed * (24f + i % 4 * 7f), Screen.width);
            float y = Mathf.Repeat(i * 71f + elapsed * (58f + i % 3 * 11f), Screen.height);
            float size = (3f + i % 4) * scale;
            DrawPanel(new Rect(x, y, size, size * (i % 2 == 0 ? 1f : 1.8f)), colors[i % colors.Length]);
        }
    }

    private void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu");
    }

    private void ShowNotice(string title, string body, float duration)
    {
        noticeTitle = title;
        noticeBody = body;
        noticeUntil = Time.unscaledTime + duration;
    }

    private void DrawPanel(Rect rect, Color color)
    {
        Color previous = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = previous;
    }

    private Texture2D MakeSolidTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }

    private void EnsureStyles()
    {
        if (hudStyle != null)
            return;

        hudStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = new Color(0.58f, 0.94f, 0.75f) }
        };
        headingStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 15,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color(0.58f, 0.94f, 0.75f) }
        };
        bodyStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = new Color(0.92f, 0.96f, 1f) }
        };
        centerBodyStyle = new GUIStyle(bodyStyle)
        {
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true
        };
        titleStyle = new GUIStyle(headingStyle)
        {
            fontSize = 28,
            normal = { textColor = new Color(0.98f, 0.75f, 0.35f) }
        };
        victoryTitleStyle = new GUIStyle(headingStyle)
        {
            fontSize = 38,
            normal = { textColor = new Color(1f, 0.79f, 0.42f) }
        };
        victoryKickerStyle = new GUIStyle(headingStyle)
        {
            fontSize = 12,
            normal = { textColor = new Color(0.52f, 0.9f, 0.82f) }
        };
        victoryBodyStyle = new GUIStyle(bodyStyle)
        {
            alignment = TextAnchor.MiddleLeft,
            wordWrap = true,
            normal = { textColor = new Color(0.77f, 0.86f, 0.91f) }
        };
        rewardTitleStyle = new GUIStyle(headingStyle)
        {
            fontSize = 16,
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = Color.white }
        };
        victoryButtonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color(0.04f, 0.1f, 0.12f), background = MakeSolidTexture(new Color(0.38f, 0.9f, 0.8f)) },
            hover = { textColor = Color.black, background = MakeSolidTexture(new Color(0.58f, 0.98f, 0.84f)) },
            active = { textColor = Color.black, background = MakeSolidTexture(new Color(0.32f, 0.78f, 0.7f)) }
        };
        victorySecondaryButtonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color(0.89f, 0.94f, 0.96f), background = MakeSolidTexture(new Color(0.1f, 0.18f, 0.24f)) },
            hover = { textColor = Color.white, background = MakeSolidTexture(new Color(0.17f, 0.29f, 0.34f)) },
            active = { textColor = Color.white, background = MakeSolidTexture(new Color(0.13f, 0.24f, 0.3f)) }
        };
    }
}
