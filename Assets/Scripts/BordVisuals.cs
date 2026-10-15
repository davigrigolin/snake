
using UnityEngine;

public class BoardVisuals : MonoBehaviour
{
    public int width = 20;
    public int height = 20;
    public float thickness = 0.12f;

    public Color borderColor = new Color(0.2f, 0.9f, 0.55f);

    private Sprite squareSprite;

    void Start()
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        squareSprite = Sprite.Create(
            texture,
            new Rect(0, 0, 1, 1),
            new Vector2(0.5f, 0.5f),
            1f
        );

        CreateBorder("Top", 0, height / 2f,
            width, thickness);

        CreateBorder("Bottom", 0, -height / 2f,
            width, thickness);

        CreateBorder("Left", -width / 2f, 0,
            thickness, height);

        CreateBorder("Right", width / 2f, 0,
            thickness, height);
    }

    void CreateBorder(
        string objectName,
        float x,
        float y,
        float w,
        float h)
    {
        GameObject border = new GameObject(objectName);
        border.transform.SetParent(transform);
        border.transform.position = new Vector3(x, y, 0);
        border.transform.localScale = new Vector3(w, h, 1);

        SpriteRenderer renderer =
            border.AddComponent<SpriteRenderer>();

        renderer.sprite = squareSprite;
        renderer.color = borderColor;
        renderer.sortingOrder = 5;
    }
}
