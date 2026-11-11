using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameProgressManager : MonoBehaviour
{
    [SerializeField] private NinjaGirlController player;
    [SerializeField] private Vector3 initialSpawn;
    [SerializeField] private int totalCheckpoints = 2;
    [SerializeField] private int levelNumber = 1;
    [SerializeField] private string levelTitle = "ICEBOUND";
    [SerializeField] private string nextLevelSceneName = "Level2";
    [SerializeField] private string missionDescription =
        "Ambil Kunci Kristal, temukan Relik Es, lalu capai EXIT!";
    [SerializeField] private Sprite crystalKeyIcon;
    [SerializeField] private Sprite iceRelicIcon;

    private Vector3 respawnPosition;
    private int activeCheckpoint;
    private bool respawning;
    private bool levelComplete;
    private bool paused;
    private bool hasCrystalKey;
    private bool hasIceRelic;
    private string objectiveOneName = "KUNCI KRISTAL";
    private string objectiveTwoName = "RELIK ES";
    private float pauseVolume;
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
    private GUIStyle pauseTitleStyle;

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
        ConfigureLevelObjectives();
        activeCheckpoint = 0;
        hasCrystalKey = false;
        hasIceRelic = false;
        respawning = false;
        levelComplete = false;
        paused = false;
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

        ShowNotice("MISI LEVEL " + levelNumber, missionDescription, 5f);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void ConfigureLevelObjectives()
    {
        switch (levelNumber)
        {
            case 2:
                objectiveOneName = "SIGIL GURUN";
                objectiveTwoName = "RELIK PASIR";
                missionDescription = "Ambil Sigil Gurun, temukan Relik Pasir, lalu capai EXIT!";
                break;
            case 3:
                objectiveOneName = "MOONSTONE";
                objectiveTwoName = "RELIK BULAN";
                missionDescription = "Ambil Moonstone, temukan Relik Bulan, lalu capai EXIT!";
                break;
            default:
                objectiveOneName = "KUNCI KRISTAL";
                objectiveTwoName = "RELIK ES";
                missionDescription = "Ambil Kunci Kristal, temukan Relik Es, lalu capai EXIT!";
                break;
        }
    }

    private string GetObjectiveName(int objectiveId)
    {
        if (objectiveId == 1)
            return objectiveOneName;

        if (objectiveId == 2)
            return objectiveTwoName;

        return objectiveId == 1 ? "KUNCI KRISTAL" : "RELIK ES";
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
            string skillName = checkpointNumber == 1 ? "LOMPATAN PANJANG" : "AIR DASH";
            string itemName = string.Format("{0} DIDAPAT!", GetObjectiveName(objectiveId));
            string skillHint = checkpointNumber == 1
                ? "SHIFT + LOMPAT di tanah untuk lompat jauh."
                : "SHIFT + LOMPAT di udara untuk dash sekali sebelum mendarat.";
            ShowNotice("SKILL BARU: " + skillName,
                itemName + "  •  " + skillHint + "  " +
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
                    ? string.Format("{0} dan {1} masih dicari.", GetObjectiveName(1), GetObjectiveName(2))
                    : !hasCrystalKey ? string.Format("{0} masih dicari.", GetObjectiveName(1)) : string.Format("{0} masih dicari.", GetObjectiveName(2));
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

        ShowNotice("SELAMAT, PETUALANG!", string.Format("{0} dan {1} berhasil kamu kumpulkan!", GetObjectiveName(1), GetObjectiveName(2)), 999f);
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
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if (levelComplete && keyboard.rKey.wasPressedThisFrame)
            RestartLevel();
        else if (levelComplete && keyboard.mKey.wasPressedThisFrame)
            ReturnToMainMenu();
        else if (levelComplete && levelNumber < 3 &&
            keyboard.nKey.wasPressedThisFrame)
            LoadNextLevel();
        else if (!levelComplete && (keyboard.escapeKey.wasPressedThisFrame || keyboard.pKey.wasPressedThisFrame))
            SetPaused(!paused);
        else if (paused && keyboard.rKey.wasPressedThisFrame)
            RestartLevel();
        else if (paused && keyboard.mKey.wasPressedThisFrame)
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
            "LEVEL " + levelNumber.ToString("00") + "   /   " + levelTitle, hudStyle);
        GUI.Label(new Rect(hud.x + 18f * scale, hud.y + 34f * scale, hud.width - 30f, 25f * scale),
            "CHECKPOINT  " + activeCheckpoint + " / " + totalCheckpoints, bodyStyle);
        GUI.Label(new Rect(hud.x + 18f * scale, hud.y + 59f * scale, hud.width - 30f, 23f * scale),
            GetObjectiveName(1).ToUpperInvariant() + ": " + (hasCrystalKey ? "DIDAPAT" : "BELUM DIDAPAT"), bodyStyle);
        GUI.Label(new Rect(hud.x + 18f * scale, hud.y + 82f * scale, hud.width - 30f, 23f * scale),
            GetObjectiveName(2).ToUpperInvariant() + ": " + (hasIceRelic ? "DIDAPAT" : "BELUM DIDAPAT"), bodyStyle);
        GUI.Label(new Rect(hud.x + 18f * scale, hud.y + 105f * scale, hud.width - 30f, 23f * scale),
            player != null && player.JumpUpgradeLevel > 0
                ? "LOMPAT JAUH " + player.LongJumpCharges + "/" + player.LongJumpsPerCheckpoint +
                  "  •  DASH " + (player.AirDashUnlocked ? player.AirDashAvailable ? "SIAP" : "PAKAI" : "LOCKED")
                : "SKILL: LOCKED", bodyStyle);

        if (Time.unscaledTime < noticeUntil)
            DrawNotice(scale);

        if (levelComplete)
            DrawVictory(scale);
        else if (paused)
            DrawPauseMenu(scale);
    }

    private void DrawPauseMenu(float scale)
    {
        DrawPanel(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0.012f, 0.025f, 0.045f, 0.78f));

        float width = Mathf.Min(510f * scale, Screen.width - 32f);
        float height = 420f * scale;
        Rect panel = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
        DrawPanel(new Rect(panel.x + 7f * scale, panel.y + 8f * scale, panel.width, panel.height),
            new Color(0f, 0f, 0f, 0.32f));
        DrawPanel(panel, new Color(0.035f, 0.075f, 0.11f, 0.98f));
        DrawPanel(new Rect(panel.x, panel.y, panel.width, 5f * scale), new Color(0.38f, 0.9f, 0.8f));

        float padding = 30f * scale;
        float contentX = panel.x + padding;
        float contentWidth = panel.width - padding * 2f;
        GUI.Label(new Rect(contentX, panel.y + 22f * scale, contentWidth, 24f * scale),
            "GAME PAUSED   /   LEVEL " + levelNumber.ToString("00"), victoryKickerStyle);
        GUI.Label(new Rect(contentX, panel.y + 48f * scale, contentWidth, 48f * scale),
            "ISTIRAHAT SEJENAK", pauseTitleStyle);

        float buttonHeight = 46f * scale;
        float buttonGap = 11f * scale;
        float buttonY = panel.y + 112f * scale;
        if (GUI.Button(new Rect(contentX, buttonY, contentWidth, buttonHeight),
                "LANJUTKAN   [ESC]", victoryButtonStyle))
            SetPaused(false);
        buttonY += buttonHeight + buttonGap;
        if (GUI.Button(new Rect(contentX, buttonY, contentWidth, buttonHeight),
                "ULANGI LEVEL   [R]", victorySecondaryButtonStyle))
            RestartLevel();
        buttonY += buttonHeight + 10f * scale;

        GUI.Label(new Rect(contentX, buttonY, contentWidth, 22f * scale),
            "VOLUME   " + Mathf.RoundToInt(AudioListener.volume * 100f) + "%", bodyStyle);
        pauseVolume = GUI.HorizontalSlider(
            new Rect(contentX, buttonY + 25f * scale, contentWidth, 18f * scale),
            pauseVolume, 0f, 1f);
        AudioListener.volume = pauseVolume;
        buttonY += 54f * scale;

        GUI.Label(new Rect(contentX, buttonY, contentWidth, 36f * scale),
            "A/D atau ←/→ bergerak    •    SPACE melompat    •    SHIFT + lompat skill", centerBodyStyle);
        buttonY += 42f * scale;
        if (GUI.Button(new Rect(contentX, buttonY, contentWidth, buttonHeight),
                "KEMBALI KE MENU   [M]", victorySecondaryButtonStyle))
            ReturnToMainMenu();

        GUI.Label(new Rect(contentX, panel.y + panel.height - 25f * scale, contentWidth, 18f * scale),
            "Tekan ESC atau P untuk melanjutkan", victoryKickerStyle);
    }

    private void SetPaused(bool shouldPause)
    {
        if (levelComplete || respawning || paused == shouldPause)
            return;

        paused = shouldPause;
        if (player != null)
            player.enabled = !paused;

        if (paused)
        {
            pauseVolume = AudioListener.volume;
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
            PlayerPrefs.SetFloat("level1.masterVolume", AudioListener.volume);
            PlayerPrefs.Save();
        }
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
            "LEVEL " + levelNumber.ToString("00") + "   /   MISSION COMPLETE", victoryKickerStyle);
        GUI.Label(new Rect(contentX, panel.y + 45f * scale, contentWidth, 50f * scale),
            "SELAMAT, PETUALANG!", victoryTitleStyle);
        GUI.Label(new Rect(contentX, panel.y + 96f * scale, contentWidth, 28f * scale),
            "Kamu berhasil menaklukkan " + levelTitle + ".", victoryBodyStyle);
        DrawPanel(new Rect(contentX, panel.y + 132f * scale, contentWidth, 1f),
            new Color(0.55f, 0.71f, 0.76f, 0.38f));

        float gap = 14f * scale;
        float cardWidth = (contentWidth - gap) * 0.5f;
        float cardY = panel.y + 147f * scale;
        float cardHeight = 78f * scale;
        DrawRewardCard(new Rect(contentX, cardY, cardWidth, cardHeight),
            "OBJECTIVE 01", GetObjectiveName(1).ToUpperInvariant(), crystalKeyIcon, new Color(1f, 0.72f, 0.25f));
        DrawRewardCard(new Rect(contentX + cardWidth + gap, cardY, cardWidth, cardHeight),
            "OBJECTIVE 02", GetObjectiveName(2).ToUpperInvariant(), iceRelicIcon, new Color(0.34f, 0.82f, 1f));

        Rect skillBanner = new Rect(contentX, panel.y + 237f * scale, contentWidth, 43f * scale);
        DrawPanel(skillBanner, new Color(0.12f, 0.25f, 0.27f, 0.95f));
        DrawPanel(new Rect(skillBanner.x, skillBanner.y, 4f * scale, skillBanner.height),
            new Color(0.38f, 0.9f, 0.8f));
        GUI.Label(new Rect(skillBanner.x + 14f * scale, skillBanner.y + 4f * scale,
            skillBanner.width - 28f * scale, 34f * scale),
            player != null && player.AirDashUnlocked
                ? "SKILL DIBUKA  •  AIR DASH"
                : "SKILL DIBUKA  •  LOMPATAN PANJANG", victoryKickerStyle);

        float buttonY = panel.y + 294f * scale;
        float buttonGap = 12f * scale;
        float buttonWidth = (contentWidth - buttonGap) * 0.5f;
        if (GUI.Button(new Rect(contentX, buttonY, buttonWidth, 46f * scale),
                "MAIN LAGI   [R]", victoryButtonStyle))
            RestartLevel();
        string nextButtonLabel = levelNumber < 3
            ? "LEVEL BERIKUTNYA   [N]"
            : "SELESAI   [M]";
        if (GUI.Button(new Rect(contentX + buttonWidth + buttonGap, buttonY, buttonWidth, 46f * scale),
                nextButtonLabel, victorySecondaryButtonStyle))
        {
            if (levelNumber < 3)
                LoadNextLevel();
            else
                ReturnToMainMenu();
        }

        if (levelNumber < 3 &&
            GUI.Button(new Rect(contentX, panel.y + 348f * scale, contentWidth, 40f * scale),
                "MENU UTAMA   [M]", victorySecondaryButtonStyle))
            ReturnToMainMenu();
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
        paused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void LoadNextLevel()
    {
        paused = false;
        if (levelNumber >= 3)
        {
            ReturnToMainMenu();
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(nextLevelSceneName);
    }

    private void ReturnToMainMenu()
    {
        paused = false;
        Time.timeScale = 1f;
        PlayerPrefs.SetFloat("level1.masterVolume", AudioListener.volume);
        PlayerPrefs.Save();
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
        pauseTitleStyle = new GUIStyle(victoryTitleStyle)
        {
            fontSize = 30,
            normal = { textColor = Color.white }
        };
    }
}
