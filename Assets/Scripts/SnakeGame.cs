
using System.Collections.Generic;
using UnityEngine;

public class SnakeGame : MonoBehaviour
{
    [Header("Game settings")]
    [SerializeField] private int width = 20;
    [SerializeField] private int height = 20;
    [SerializeField] private float stepSeconds = 0.14f;

    private readonly List<Vector2Int> snake = new List<Vector2Int>();
    private Vector2Int direction = Vector2Int.right;
    private Vector2Int queuedDirection = Vector2Int.right;
    private Vector2Int food;

    private float timer;
    private int score;
    private bool started;
    private bool gameOver;
    private bool won;

    private Sprite squareSprite;
    private readonly List<GameObject> snakeObjects = new List<GameObject>();
    private GameObject foodObject;
    private Camera gameCamera;

    private Texture2D panelTexture;
    private Texture2D overlayTexture;
    private Texture2D greenTexture;
    private Texture2D greenHoverTexture;

    private GUIStyle titleStyle;
    private GUIStyle subtitleStyle;
    private GUIStyle scoreStyle;
    private GUIStyle buttonStyle;
    private GUIStyle instructionStyle;

    private void Start()
    {
        CreateSquareSprite();
        SetupCamera();
        ResetGame();
    }

    private void Update()
    {
        if (!started || gameOver || won)
        {
            if (Input.GetKeyDown(KeyCode.Space) ||
                Input.GetKeyDown(KeyCode.Return))
            {
                StartGame();
            }

            return;
        }

        ReadInput();

        timer += Time.deltaTime;

        if (timer >= stepSeconds)
        {
            timer = 0f;
            StepGame();
            RenderSnake();
        }
    }

    private void ReadInput()
    {
        Vector2Int candidate = queuedDirection;

        if (Input.GetKeyDown(KeyCode.UpArrow) ||
            Input.GetKeyDown(KeyCode.W))
        {
            candidate = Vector2Int.up;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) ||
                 Input.GetKeyDown(KeyCode.S))
        {
            candidate = Vector2Int.down;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) ||
                 Input.GetKeyDown(KeyCode.A))
        {
            candidate = Vector2Int.left;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) ||
                 Input.GetKeyDown(KeyCode.D))
        {
            candidate = Vector2Int.right;
        }

        if (candidate != -direction)
        {
            queuedDirection = candidate;
        }
    }

    private void StepGame()
    {
        direction = queuedDirection;

        Vector2Int next = snake[0] + direction;
        bool eating = next == food;

        // Colisão com as paredes.
        if (next.x < 0 || next.x >= width ||
            next.y < 0 || next.y >= height)
        {
            EndGame();
            return;
        }

        // Colisão com o próprio corpo.
        int bodyCheckCount = eating ? snake.Count : snake.Count - 1;

        for (int i = 0; i < bodyCheckCount; i++)
        {
            if (snake[i] == next)
            {
                EndGame();
                return;
            }
        }

        snake.Insert(0, next);

        if (eating)
        {
            score += 10;

            // Vitória: a cobra preencheu todas as casas.
            if (snake.Count >= width * height)
            {
                won = true;
                started = false;
                gameOver = false;

                if (foodObject != null)
                {
                    foodObject.SetActive(false);
                }

                RenderSnake();
                return;
            }

            PlaceFood();
        }
        else
        {
            snake.RemoveAt(snake.Count - 1);
        }
    }

    private void StartGame()
    {
        ResetGame();
        started = true;
        gameOver = false;
        won = false;
    }

    private void ResetGame()
    {
        score = 0;
        direction = Vector2Int.right;
        queuedDirection = Vector2Int.right;
        timer = 0f;

        started = false;
        gameOver = false;
        won = false;

        snake.Clear();

        snake.Add(new Vector2Int(width / 2, height / 2));
        snake.Add(new Vector2Int(width / 2 - 1, height / 2));

        PlaceFood();
        RenderSnake();
    }

    private void EndGame()
    {
        gameOver = true;
        started = false;
    }

    private void PlaceFood()
    {
        List<Vector2Int> available = new List<Vector2Int>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int cell = new Vector2Int(x, y);

                if (!snake.Contains(cell))
                {
                    available.Add(cell);
                }
            }
        }

        if (available.Count == 0)
        {
            won = true;
            started = false;
            gameOver = false;
            return;
        }

        food = available[Random.Range(0, available.Count)];

        if (foodObject == null)
        {
            foodObject = CreateSquare(
                "Food",
                new Color(1f, 0.25f, 0.3f),
                1.05f
            );
        }

        foodObject.transform.position = CellToWorld(food);
        foodObject.SetActive(true);
    }

    private void RenderSnake()
    {
        while (snakeObjects.Count < snake.Count)
        {
            GameObject segment = CreateSquare(
                "Snake Segment",
                new Color(0.25f, 0.9f, 0.45f),
                0.92f
            );

            snakeObjects.Add(segment);
        }

        for (int i = 0; i < snakeObjects.Count; i++)
        {
            bool visible = i < snake.Count;
            snakeObjects[i].SetActive(visible);

            if (visible)
            {
                snakeObjects[i].transform.position = CellToWorld(snake[i]);

                SpriteRenderer renderer =
                    snakeObjects[i].GetComponent<SpriteRenderer>();

                renderer.color = i == 0
                    ? new Color(0.1f, 0.75f, 0.3f)
                    : new Color(0.25f, 0.9f, 0.45f);
            }
        }
    }

    private GameObject CreateSquare(
        string objectName,
        Color color,
        float scale)
    {
        GameObject go = new GameObject(objectName);

        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = squareSprite;
        renderer.color = color;

        go.transform.localScale = new Vector3(scale, scale, 1f);

        return go;
    }

    private Vector3 CellToWorld(Vector2Int cell)
    {
        return new Vector3(
            cell.x - width / 2f + 0.5f,
            cell.y - height / 2f + 0.5f,
            0f
        );
    }

    private void CreateSquareSprite()
    {
        Texture2D texture = new Texture2D(
            1, 1, TextureFormat.RGBA32, false
        );

        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        squareSprite = Sprite.Create(
            texture,
            new Rect(0, 0, 1, 1),
            new Vector2(0.5f, 0.5f),
            1f
        );
    }

    private void SetupCamera()
    {
        gameCamera = Camera.main;

        if (gameCamera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            gameCamera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
        }

        gameCamera.orthographic = true;
        gameCamera.orthographicSize = height / 2f + 1.8f;
        gameCamera.transform.position = new Vector3(0f, 0f, -10f);
        gameCamera.backgroundColor = new Color(0.055f, 0.07f, 0.09f);
        gameCamera.clearFlags = CameraClearFlags.SolidColor;
    }

    private void OnGUI()
    {
        EnsureUIStyles();

        float scale = Mathf.Clamp(Screen.height / 720f, 0.75f, 1.5f);
        float centerX = Screen.width / 2f;

        // Pontuação.
        GUI.Label(
            new Rect(
                centerX - 180 * scale,
                20 * scale,
                360 * scale,
                45 * scale
            ),
            "S C O R E     " + score,
            scoreStyle
        );

        // Durante o jogo, mostra somente as instruções.
        if (started && !gameOver && !won)
        {
            GUI.Label(
                new Rect(
                    0,
                    Screen.height - 42 * scale,
                    Screen.width,
                    30 * scale
                ),
                "USE AS SETAS OU WASD PARA JOGAR",
                instructionStyle
            );

            return;
        }

        // Fundo escurecido.
        GUI.DrawTexture(
            new Rect(0, 0, Screen.width, Screen.height),
            overlayTexture
        );

        // Painel central.
        float panelWidth = Mathf.Min(500 * scale, Screen.width - 32);
        float panelHeight = 370 * scale;
        float panelX = centerX - panelWidth / 2f;
        float panelY = (Screen.height - panelHeight) / 2f;

        GUI.DrawTexture(
            new Rect(panelX, panelY, panelWidth, panelHeight),
            panelTexture
        );

        // Faixa verde superior.
        GUI.DrawTexture(
            new Rect(panelX, panelY, panelWidth, 5 * scale),
            greenTexture
        );

        GUI.Label(
            new Rect(
                panelX + 15 * scale,
                panelY + 32 * scale,
                panelWidth - 30 * scale,
                65 * scale
            ),
            "SNAKE 2D",
            titleStyle
        );

        GUI.Label(
            new Rect(
                panelX + 15 * scale,
                panelY + 95 * scale,
                panelWidth - 30 * scale,
                30 * scale
            ),
            "C L A S S I C   A R C A D E",
            subtitleStyle
        );

        // Tela de vitória.
        if (won)
        {
            GUI.Label(
                new Rect(
                    panelX + 20 * scale,
                    panelY + 145 * scale,
                    panelWidth - 40 * scale,
                    35 * scale
                ),
                "VOCE VENCEU!",
                victoryStyle()
            );

            GUI.Label(
                new Rect(
                    panelX + 20 * scale,
                    panelY + 180 * scale,
                    panelWidth - 40 * scale,
                    30 * scale
                ),
                "Voce preencheu todo o tabuleiro!",
                instructionStyle
            );

            GUI.Label(
                new Rect(
                    panelX + 20 * scale,
                    panelY + 205 * scale,
                    panelWidth - 40 * scale,
                    25 * scale
                ),
                "Pontuacao final: " + score,
                scoreStyle
            );
        }
        // Tela de derrota.
        else if (gameOver)
        {
            GUI.Label(
                new Rect(
                    panelX + 20 * scale,
                    panelY + 145 * scale,
                    panelWidth - 40 * scale,
                    35 * scale
                ),
                "FIM DE JOGO",
                scoreStyle
            );

            GUI.Label(
                new Rect(
                    panelX + 20 * scale,
                    panelY + 180 * scale,
                    panelWidth - 40 * scale,
                    30 * scale
                ),
                "Pontuacao final: " + score,
                instructionStyle
            );
        }
        else
        {
            GUI.Label(
                new Rect(
                    panelX + 20 * scale,
                    panelY + 155 * scale,
                    panelWidth - 40 * scale,
                    40 * scale
                ),
                "Prepare-se para jogar!",
                instructionStyle
            );
        }

        // Botão principal.
        string buttonText = (gameOver || won)
            ? "JOGAR NOVAMENTE"
            : "COMEÇAR A JOGAR";

        if (GUI.Button(
            new Rect(
                panelX + 55 * scale,
                panelY + 240 * scale,
                panelWidth - 110 * scale,
                55 * scale
            ),
            buttonText,
            buttonStyle))
        {
            StartGame();
        }

        GUI.Label(
            new Rect(
                panelX + 15 * scale,
                panelY + 310 * scale,
                panelWidth - 30 * scale,
                25 * scale
            ),
            "SETAS / WASD  •  COMA E CRESCA",
            subtitleStyle
        );

        GUI.Label(
            new Rect(
                panelX + 15 * scale,
                panelY + 337 * scale,
                panelWidth - 30 * scale,
                22 * scale
            ),
            "NAO BATA NAS PAREDES NEM EM VOCE!",
            instructionStyle
        );
    }

    private GUIStyle victoryStyle()
    {
        GUIStyle style = new GUIStyle(scoreStyle);
        style.normal.textColor = new Color(0.25f, 1f, 0.52f);
        style.fontSize = Mathf.Clamp(Screen.height / 28, 22, 34);
        return style;
    }

    private void EnsureUIStyles()
    {
        if (panelTexture != null)
        {
            return;
        }

        panelTexture = MakeTexture(new Color(0.055f, 0.075f, 0.10f));
        overlayTexture = MakeTexture(
            new Color(0.015f, 0.02f, 0.035f, 0.88f)
        );
        greenTexture = MakeTexture(new Color(0.20f, 1f, 0.48f));
        greenHoverTexture = MakeTexture(new Color(0.35f, 1f, 0.60f));

        titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.fontSize = Mathf.Clamp(Screen.height / 10, 34, 68);
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = new Color(0.25f, 1f, 0.52f);

        subtitleStyle = new GUIStyle(GUI.skin.label);
        subtitleStyle.alignment = TextAnchor.MiddleCenter;
        subtitleStyle.fontSize = Mathf.Clamp(Screen.height / 48, 12, 19);
        subtitleStyle.fontStyle = FontStyle.Bold;
        subtitleStyle.normal.textColor = new Color(0.45f, 0.85f, 0.68f);

        scoreStyle = new GUIStyle(GUI.skin.label);
        scoreStyle.alignment = TextAnchor.MiddleCenter;
        scoreStyle.fontSize = Mathf.Clamp(Screen.height / 32, 18, 28);
        scoreStyle.fontStyle = FontStyle.Bold;
        scoreStyle.normal.textColor = Color.white;

        instructionStyle = new GUIStyle(GUI.skin.label);
        instructionStyle.alignment = TextAnchor.MiddleCenter;
        instructionStyle.fontSize = Mathf.Clamp(Screen.height / 55, 11, 16);
        instructionStyle.normal.textColor = new Color(0.70f, 0.77f, 0.82f);

        buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.alignment = TextAnchor.MiddleCenter;
        buttonStyle.fontSize = Mathf.Clamp(Screen.height / 38, 16, 23);
        buttonStyle.fontStyle = FontStyle.Bold;
        buttonStyle.normal.background = greenTexture;
        buttonStyle.hover.background = greenHoverTexture;
        buttonStyle.active.background = greenHoverTexture;
        buttonStyle.normal.textColor = new Color(0.025f, 0.06f, 0.04f);
        buttonStyle.hover.textColor = Color.black;
        buttonStyle.active.textColor = Color.black;
    }

    private Texture2D MakeTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();

        return texture;
    }
}
