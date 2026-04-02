using UnityEngine;
using UnityEngine.UI;

public class CloudFlow : MonoBehaviour
{
    [SerializeField] private float speed = 50f; // pixels per second feel
    [SerializeField] private Vector2 direction = Vector2.right;

    private RawImage img;

    void Awake()
    {
        img = GetComponent<RawImage>();
    }

    void Update()
    {
        if (!img) return;

        Rect r = img.uvRect;
        r.position += direction.normalized * speed * Time.deltaTime * 0.001f;
        r.position = new Vector2(r.x % 1f, r.y % 1f);
        img.uvRect = r;
    }
}