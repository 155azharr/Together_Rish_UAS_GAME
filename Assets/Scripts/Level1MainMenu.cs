using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Level1MainMenu : MonoBehaviour
{
    private enum MenuPage
    {
        Main,
        Levels,
        Options
    }

    [SerializeField] private string gameplaySceneName = "SampleScene";
    [SerializeField] private Texture2D backgroundTexture;

    private MenuPage currentPage;
    private float masterVolume = 1f;
    private bool isFullscreen;
    private bool showControls;
    private string resolutionLabel = "Native";
    private GUIStyle eyebrowStyle;
    private GUIStyle titleStyle;
    private GUIStyle subtitleStyle;
    private GUIStyle panelStyle;
    private GUIStyle buttonStyle;
    private GUIStyle primaryButtonStyle;
    private GUIStyle smallStyle;
    private GUIStyle footerStyle;
    private Texture2D panelTexture;
    private Texture2D buttonTexture;
    private Texture2D buttonHoverTexture;
    private Texture2D primaryTexture;
    private Texture2D primaryHoverTexture;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        masterVolume = PlayerPrefs.GetFloat("level1.masterVolume", 0.85f);
        isFullscreen = Screen.fullScreen;
        AudioListener.volume = masterVolume;
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (showControls)
                showControls = false;
            else
                currentPage = MenuPage.Main;
        }
    }

    private void OnGUI()
    {
        EnsureStyles();
        DrawBackground();

        float scale = Screen.width < 760 ? 0.82f : 1f;
        float margin = Mathf.Max(22f, Screen.width * 0.055f);
        bool compact = Screen.width < 760;
        DrawBrand(margin, compact, scale);

        float panelWidth = Mathf.Min(410f * scale, Screen.width - margin * 2f);
        float panelX = compact ? (Screen.width - panelWidth) * 0.5f : Screen.width - panelWidth - margin;
        float panelHeight = currentPage == MenuPage.Options
            ? (showControls ? 560f : 500f) * scale
            : 350f * scale;
        float panelY = compact ? Mathf.Max(160f, Screen.height * 0.37f) : (Screen.height - panelHeight) * 0.5f;

        DrawMenuPanel(new Rect(panelX, panelY, panelWidth, panelHeight), scale);
        DrawFooter(margin, scale);
    }

    private void DrawBackground()
    {
        Rect screen = new Rect(0f, 0f, Screen.width, Screen.height);
        if (backgroundTexture != null)
            GUI.DrawTexture(screen, backgroundTexture, ScaleMode.ScaleAndCrop, false);
        else
            DrawFill(screen, new Color(0.08f, 0.17f, 0.29f));

        DrawFill(screen, new Color(0.025f, 0.055f, 0.1f, 0.45f));
        DrawFill(new Rect(0f, Screen.height * 0.67f, Screen.width, Screen.height * 0.33f),
            new Color(0.015f, 0.035f, 0.065f, 0.32f));
    }

    private void DrawBrand(float margin, bool compact, float scale)
    {
        float leftWidth = compact ? Screen.width - margin * 2f : Screen.width * 0.38f;
        float top = compact ? 36f : Screen.height * 0.29f;
        GUI.Label(new Rect(margin, compact ? 34f : 68f, leftWidth, 28f),
            "LEVEL 01     /     ICEBOUND", eyebrowStyle);
        float titleHeight = compact ? 106f : 110f;
        GUI.Label(new Rect(margin - 4f, top, leftWidth, titleHeight),
            "TOGETHERISH\n: MIND THE GAP", titleStyle);
        GUI.Label(new Rect(margin, top + titleHeight + 4f, leftWidth, 30f),
            "A CO-OP PLATFORM ADVENTURE", subtitleStyle);

        float lineY = top + titleHeight + 46f;
        DrawFill(new Rect(margin, lineY, 68f, 4f), new Color(0.38f, 0.9f, 0.8f));
        if (!compact)
        {
            GUI.Label(new Rect(margin, lineY + 18f, leftWidth - 24f, 58f),
                "Collect two relics. Earn stronger jumps. Reach the exit.",
                subtitleStyle);
        }
    }

    private void DrawMenuPanel(Rect panel, float scale)
    {
        DrawFill(panel, new Color(0.025f, 0.055f, 0.09f, 0.9f));
        DrawFill(new Rect(panel.x, panel.y, 4f, panel.height), new Color(0.38f, 0.9f, 0.8f));

        float padding = 28f * scale;
        float contentX = panel.x + padding;
        float contentWidth = panel.width - padding * 2f;
        float y = panel.y + 26f * scale;
        string heading = currentPage == MenuPage.Main ? "MAIN MENU" :
            currentPage == MenuPage.Levels ? "LEVEL SELECT" : "OPTIONS";
        GUI.Label(new Rect(contentX, y, contentWidth, 28f * scale), heading, eyebrowStyle);
        y += 42f * scale;

        if (currentPage == MenuPage.Main)
            DrawMainButtons(contentX, contentWidth, y, scale);
        else if (currentPage == MenuPage.Levels)
            DrawLevelSelection(contentX, contentWidth, y, scale);
        else
            DrawOptions(contentX, contentWidth, y, scale);
    }

    private void DrawMainButtons(float x, float width, float y, float scale)
    {
        if (MenuButton(new Rect(x, y, width, 56f * scale), "PLAY", true))
            LoadGame();
        y += 68f * scale;
        if (MenuButton(new Rect(x, y, width, 48f * scale), "LEVEL SELECT     01  /  01", false))
            currentPage = MenuPage.Levels;
        y += 59f * scale;
        if (MenuButton(new Rect(x, y, width, 48f * scale), "OPTIONS", false))
            currentPage = MenuPage.Options;
        y += 58f * scale;
        if (MenuButton(new Rect(x, y, width, 48f * scale), "QUIT", false))
            QuitGame();
    }

    private void DrawLevelSelection(float x, float width, float y, float scale)
    {
        Rect level = new Rect(x, y, width, 126f * scale);
        DrawFill(level, new Color(0.075f, 0.14f, 0.2f, 0.95f));
        DrawFill(new Rect(level.x, level.y, 5f * scale, level.height), new Color(0.38f, 0.9f, 0.8f));
        GUI.Label(new Rect(x + 18f * scale, y + 13f * scale, width - 36f, 24f * scale),
            "01   •   ICEBOUND", eyebrowStyle);
        GUI.Label(new Rect(x + 18f * scale, y + 43f * scale, width - 36f, 26f * scale),
            "Crystal Valley", subtitleStyle);
        GUI.Label(new Rect(x + 18f * scale, y + 75f * scale, width - 36f, 24f * scale),
            "2 checkpoints  /  2 relics  /  1 exit", smallStyle);
        y += 144f * scale;
        if (MenuButton(new Rect(x, y, width, 54f * scale), "PLAY LEVEL 01", true))
            LoadGame();
        y += 66f * scale;
        if (MenuButton(new Rect(x, y, width, 46f * scale), "BACK", false))
            currentPage = MenuPage.Main;
    }

    private void DrawOptions(float x, float width, float y, float scale)
    {
        GUI.Label(new Rect(x, y, width, 25f * scale), "MASTER VOLUME", eyebrowStyle);
        masterVolume = GUI.HorizontalSlider(new Rect(x, y + 32f * scale, width - 68f * scale, 18f * scale),
            masterVolume, 0f, 1f);
        GUI.Label(new Rect(x + width - 58f * scale, y + 26f * scale, 58f * scale, 28f * scale),
            Mathf.RoundToInt(masterVolume * 100f) + "%", smallStyle);
        AudioListener.volume = masterVolume;

        y += 65f * scale;
        isFullscreen = GUI.Toggle(new Rect(x, y, width, 26f * scale), isFullscreen, "  FULLSCREEN");
        Screen.fullScreen = isFullscreen;

        y += 42f * scale;
        if (MenuButton(new Rect(x, y, width, 44f * scale), "RESOLUSI    " + resolutionLabel, false))
            CycleResolution();
        y += 54f * scale;

        GUI.Label(new Rect(x, y, width, 24f * scale), "CONTROLS", eyebrowStyle);
        y += 28f * scale;
        GUI.Label(new Rect(x, y, width, 82f * scale),
            "A / D  atau  ← / →     Bergerak\nSPACE / W / ↑           Melompat\nSHIFT + lompat             Lompat jauh (3 charge)",
            smallStyle);
        y += 89f * scale;

        if (showControls)
        {
            GUI.Label(new Rect(x, y, width, 34f * scale),
                "Kumpulkan Kunci Kristal dan Relik Es. Setiap checkpoint memperkuat lompatan.",
                smallStyle);
            y += 41f * scale;
        }

        if (MenuButton(new Rect(x, y, width, 44f * scale), showControls ? "HIDE DETAILS" : "GAMEPLAY DETAILS", false))
            showControls = !showControls;
        y += 53f * scale;
        if (MenuButton(new Rect(x, y, width, 44f * scale), "SAVE & BACK", true))
        {
            PlayerPrefs.SetFloat("level1.masterVolume", masterVolume);
            PlayerPrefs.Save();
            currentPage = MenuPage.Main;
            showControls = false;
        }
    }

    private bool MenuButton(Rect rect, string text, bool primary)
    {
        return GUI.Button(rect, text, primary ? primaryButtonStyle : buttonStyle);
    }

    private void DrawFooter(float margin, float scale)
    {
        GUI.Label(new Rect(margin, Screen.height - 42f, Screen.width - margin * 2f, 24f),
            "A TWO-RELIC PLATFORM ADVENTURE", footerStyle);
    }

    private void LoadGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameplaySceneName);
    }

    private void CycleResolution()
    {
        if (Screen.width < 1500)
        {
            Screen.SetResolution(1920, 1080, isFullscreen);
            resolutionLabel = "1920 × 1080";
        }
        else
        {
            Screen.SetResolution(1280, 720, isFullscreen);
            resolutionLabel = "1280 × 720";
        }
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void DrawFill(Rect rect, Color color)
    {
        Color previous = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = previous;
    }

    private void EnsureStyles()
    {
        if (eyebrowStyle != null)
            return;

        panelTexture = MakeTexture(new Color(0.025f, 0.055f, 0.09f, 0.92f));
        buttonTexture = MakeTexture(new Color(0.08f, 0.15f, 0.21f, 0.96f));
        buttonHoverTexture = MakeTexture(new Color(0.14f, 0.26f, 0.31f, 0.98f));
        primaryTexture = MakeTexture(new Color(0.29f, 0.82f, 0.73f, 1f));
        primaryHoverTexture = MakeTexture(new Color(0.5f, 0.94f, 0.82f, 1f));

        eyebrowStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 13,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = new Color(0.46f, 0.9f, 0.82f) }
        };
        titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 32,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            wordWrap = true,
            normal = { textColor = new Color(0.96f, 0.98f, 0.97f) }
        };
        subtitleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            wordWrap = true,
            normal = { textColor = new Color(0.83f, 0.91f, 0.94f) }
        };
        panelStyle = new GUIStyle(GUI.skin.box)
        {
            normal = { background = panelTexture }
        };
        buttonStyle = CreateButtonStyle(buttonTexture, buttonHoverTexture,
            new Color(0.91f, 0.96f, 0.96f));
        primaryButtonStyle = CreateButtonStyle(primaryTexture, primaryHoverTexture,
            new Color(0.035f, 0.095f, 0.12f));
        smallStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            alignment = TextAnchor.MiddleLeft,
            wordWrap = true,
            normal = { textColor = new Color(0.72f, 0.82f, 0.87f) }
        };
        footerStyle = new GUIStyle(eyebrowStyle)
        {
            fontSize = 11,
            normal = { textColor = new Color(0.79f, 0.87f, 0.89f, 0.8f) }
        };
    }

    private GUIStyle CreateButtonStyle(Texture2D normalTexture, Texture2D hoverTexture, Color textColor)
    {
        return new GUIStyle(GUI.skin.button)
        {
            fontSize = 15,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            padding = new RectOffset(18, 14, 8, 8),
            normal = { background = normalTexture, textColor = textColor },
            hover = { background = hoverTexture, textColor = Color.white },
            active = { background = hoverTexture, textColor = Color.white },
            focused = { background = normalTexture, textColor = textColor }
        };
    }

    private Texture2D MakeTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }
}
