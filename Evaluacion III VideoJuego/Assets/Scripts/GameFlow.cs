
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlow : MonoBehaviour
{
    public static GameFlow Instance { get; private set; }

    public Vector3 RespawnPosition { get; private set; } = new Vector3(0f, 1.55f, 0f);
    public bool HasWon { get; private set; }
    public int Coins { get; private set; }
    public int TotalCoins = 11;

    private string message = "";
    private float messageUntil = 4f;

    private GUIStyle titleStyle;
    private GUIStyle normalStyle;
    private GUIStyle middleStyle;
    private Texture2D panelTexture;

    void Awake()
    {
        Instance = this;
    }

    public void CollectCoin()
    {
        Coins++;
        ShowMessage("¡Moneda conseguida!", 1.4f);
    }

    public void SetCheckpoint(Vector3 position)
    {
        RespawnPosition = position;
        ShowMessage("¡Punto de control activado!", 2.2f);
    }

    public void Win()
    {
        HasWon = true;
        ShowMessage("¡Nivel completado!", 999f);
    }

    public void ShowMessage(string text, float seconds)
    {
        message = text;
        messageUntil = Time.time + seconds;
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void PrepareStyles()
    {
        if (panelTexture != null) return;

        panelTexture = new Texture2D(1, 1);
        panelTexture.SetPixel(
            0, 0,
            new Color(0.05f, 0.09f, 0.18f, 0.78f)
        );
        panelTexture.Apply();

        titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 24,
            fontStyle = FontStyle.Bold
        };
        titleStyle.normal.textColor = Color.white;

        normalStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16
        };
        normalStyle.normal.textColor = Color.white;

        middleStyle = new GUIStyle(titleStyle)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 30
        };
    }

    void OnGUI()
    {
        PrepareStyles();

        float scale = Mathf.Min(
            Screen.width / 1100f,
            Screen.height / 700f
        );

        scale = Mathf.Clamp(scale, 0.7f, 1.6f);

        GUI.matrix = Matrix4x4.TRS(
            Vector3.zero,
            Quaternion.identity,
            Vector3.one * scale
        );

        float width = Screen.width / scale;
        float height = Screen.height / scale;

        // Panel principal
        GUI.DrawTexture(
            new Rect(18, 18, 335, 106),
            panelTexture
        );

        GUI.Label(
            new Rect(33, 27, 320, 35),
            "SALTO ISLAS 3D",
            titleStyle
        );

        GUI.Label(
            new Rect(33, 68, 315, 30),
            "Monedas: " + Coins + " / " + TotalCoins,
            normalStyle
        );

        GUI.Label(
            new Rect(33, 98, 310, 25),
            "WASD / Flechas  |  Espacio",
            normalStyle
        );

        // Mensajes temporales
        if (Time.time < messageUntil && !string.IsNullOrEmpty(message))
        {
            GUI.DrawTexture(
                new Rect(width / 2f - 240, 18, 480, 58),
                panelTexture
            );

            GUI.Label(
                new Rect(width / 2f - 230, 24, 460, 47),
                message,
                new GUIStyle(titleStyle)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 22
                }
            );
        }

        // Pantalla de victoria
        if (HasWon)
        {
            GUI.DrawTexture(
                new Rect(
                    width / 2f - 270,
                    height / 2f - 110,
                    540,
                    210
                ),
                panelTexture
            );

            GUI.Label(
                new Rect(
                    width / 2f - 250,
                    height / 2f - 80,
                    500,
                    60
                ),
                "¡GANASTE!",
                middleStyle
            );

            GUI.Label(
                new Rect(
                    width / 2f - 250,
                    height / 2f - 20,
                    500,
                    32
                ),
                "Recogiste " + Coins + " de " + TotalCoins + " monedas.",
                new GUIStyle(normalStyle)
                {
                    alignment = TextAnchor.MiddleCenter
                }
            );
        }
    }

    void OnDestroy()
    {
        if (panelTexture != null)
        {
            Destroy(panelTexture);
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }
}
