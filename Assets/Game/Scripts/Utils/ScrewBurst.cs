using System.Collections.Generic;
using UnityEngine;

public class ScrewBurst : MonoBehaviour
{
    private static ScrewBurst instance;

    public static ScrewBurst Instance => instance;

    [Header("Screw")]
    [SerializeField] private Sprite screwSprite;
    [SerializeField] private float scale = 0.7f;
    [SerializeField] private int sortingOrder = 1;
    [SerializeField] private float depth = 0.2f;

    [Header("Motion")]
    [SerializeField] private float upSpeed = 3.2f;
    [SerializeField] private float sideSpeed = 0.9f;
    [SerializeField] private float speedVariation = 0.25f;
    [SerializeField] private float spinSpeed = 120f;
    [SerializeField] private float gravity = 3.6f;
    [SerializeField] private float lifetime = 2.2f;
    [SerializeField] private float fadeTime = 0.4f;

    [Header("Golden Screw")]
    [Range(0f, 1f)] [SerializeField] private float goldenChance = 0.04f;
    [SerializeField] private int goldenReward = 250;
    [SerializeField] private Color goldenColor = new Color(1f, 0.83f, 0.25f, 1f);
    [SerializeField] private float goldenScale = 1.25f;
    [SerializeField] private int goldenSortingOrder = 3;
    [SerializeField] private float clickPadding = 1.3f;
    [SerializeField] private float collectTime = 0.18f;
    [SerializeField] private MoneyManager moneyManager;

    private readonly List<Screw> screws = new List<Screw>();
    private Sprite centeredSprite;
    private Vector2 goldenHalfSize;

    private class Screw
    {
        public Transform transform;
        public SpriteRenderer renderer;
        public Vector2 velocity;
        public float spin;
        public float age;
        public bool golden;
        public bool collected;
    }

    private void Awake()
    {
        instance = this;
        centeredSprite = CreateCenteredSprite();

        if (centeredSprite != null)
        {
            goldenHalfSize = (Vector2)centeredSprite.bounds.extents * clickPadding;
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }

        if (centeredSprite != null)
        {
            Destroy(centeredSprite);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryCollectGolden();
        }

        for (int i = screws.Count - 1; i >= 0; i--)
        {
            Screw screw = screws[i];

            if (screw.collected)
            {
                UpdateCollected(screw, i);
                continue;
            }

            screw.age += Time.deltaTime;
            screw.velocity += Vector2.down * (gravity * Time.deltaTime);
            screw.transform.position += (Vector3)(screw.velocity * Time.deltaTime);
            screw.transform.Rotate(0f, 0f, screw.spin * Time.deltaTime);

            float remaining = lifetime - screw.age;

            if (remaining <= 0f)
            {
                DestroyScrew(i);
                continue;
            }

            if (remaining < fadeTime)
            {
                SetAlpha(screw, remaining / fadeTime);
            }
        }
    }

    public void Spawn(Vector3 position)
    {
        if (centeredSprite == null)
        {
            return;
        }

        bool golden = Random.value < goldenChance;

        var piece = new GameObject(golden ? "GoldenScrew" : "Screw");
        piece.transform.position = new Vector3(position.x, position.y, depth);
        piece.transform.localScale = Vector3.one * scale * (golden ? goldenScale : 1f);
        piece.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

        var spriteRenderer = piece.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = centeredSprite;
        spriteRenderer.sortingOrder = golden ? goldenSortingOrder : sortingOrder;
        spriteRenderer.color = golden ? goldenColor : Color.white;

        float horizontal = Random.Range(-sideSpeed, sideSpeed);
        float vertical = upSpeed * Random.Range(1f - speedVariation, 1f + speedVariation);

        screws.Add(new Screw
        {
            transform = piece.transform,
            renderer = spriteRenderer,
            velocity = new Vector2(horizontal, vertical),
            spin = Random.Range(-spinSpeed, spinSpeed),
            age = 0f,
            golden = golden,
            collected = false
        });
    }

    private void TryCollectGolden()
    {
        Camera[] cameras = Camera.allCameras;

        for (int i = 0; i < screws.Count; i++)
        {
            Screw screw = screws[i];

            if (!screw.golden || screw.collected)
            {
                continue;
            }

            for (int c = 0; c < cameras.Length; c++)
            {
                Camera camera = cameras[c];

                if (camera == null)
                {
                    continue;
                }

                Vector3 world = camera.ScreenToWorldPoint(Input.mousePosition);
                Vector3 local = screw.transform.InverseTransformPoint(world);

                if (Mathf.Abs(local.x) <= goldenHalfSize.x && Mathf.Abs(local.y) <= goldenHalfSize.y)
                {
                    Collect(screw);
                    return;
                }
            }
        }
    }

    private void Collect(Screw screw)
    {
        if (screw.collected)
        {
            return;
        }

        screw.collected = true;
        screw.age = 0f;
        screw.velocity = Vector2.zero;

        MoneyManager manager = ResolveMoneyManager();

        if (manager != null)
        {
            manager.AddFlatMoney(goldenReward);
        }
    }

    private void UpdateCollected(Screw screw, int index)
    {
        screw.age += Time.deltaTime;

        float progress = Mathf.Clamp01(screw.age / Mathf.Max(collectTime, 0.0001f));
        screw.transform.localScale = Vector3.one * scale * goldenScale * (1f + progress * 0.8f);
        SetAlpha(screw, 1f - progress);

        if (progress >= 1f)
        {
            DestroyScrew(index);
        }
    }

    private void DestroyScrew(int index)
    {
        Destroy(screws[index].transform.gameObject);
        screws.RemoveAt(index);
    }

    private static void SetAlpha(Screw screw, float alpha)
    {
        Color color = screw.renderer.color;
        color.a = Mathf.Clamp01(alpha);
        screw.renderer.color = color;
    }

    private MoneyManager ResolveMoneyManager()
    {
        if (moneyManager == null)
        {
            moneyManager = FindFirstObjectByType<MoneyManager>();
        }

        return moneyManager;
    }

    private Sprite CreateCenteredSprite()
    {
        if (screwSprite == null)
        {
            return null;
        }

        return Sprite.Create(screwSprite.texture, screwSprite.textureRect, new Vector2(0.5f, 0.5f), screwSprite.pixelsPerUnit);
    }
}
