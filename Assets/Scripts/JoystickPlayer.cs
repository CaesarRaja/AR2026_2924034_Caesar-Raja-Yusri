using UnityEngine;

public class JoystickPlayer : MonoBehaviour
{
    public float speed = 5f;
    public float radius = 120f;            // radius lingkaran luar (piksel)
    public string targetName = "Target";
    public string judul = "AR Praktikum P05/P07";
    public string pesanSukses = "Target tersentuh. Praktikum berhasil.";

    Rigidbody rb;
    Texture2D circle;
    GUIStyle style;
    Vector2 center;                        // pusat joystick (origin kiri-bawah)
    Vector2 input;
    bool dragging;
    bool success;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // buat tekstur lingkaran sendiri
        int size = 128;
        circle = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float r = size / 2f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(r, r));
                circle.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(r - d)));
            }
        circle.Apply();
    }

    void Update()
    {
        center = new Vector2(Screen.width / 2f, radius * 1.8f);

        // ambil posisi sentuhan (HP) atau mouse (editor)
        bool pressed = false;
        Vector2 pos = Vector2.zero;
        if (Input.touchCount > 0) { pos = Input.GetTouch(0).position; pressed = true; }
        else if (Input.GetMouseButton(0)) { pos = Input.mousePosition; pressed = true; }

        Vector2 d = pos - center;
        if (pressed && (dragging || d.magnitude < radius * 1.5f))
        {
            dragging = true;
            input = Vector2.ClampMagnitude(d / radius, 1f);
        }
        else
        {
            dragging = false;
            input = Vector2.zero;
        }
    }

    void FixedUpdate()
    {
        // arah gerak mengikuti arah kamera
        Transform cam = Camera.main.transform;
        Vector3 fwd = cam.forward; fwd.y = 0; fwd.Normalize();
        Vector3 right = cam.right; right.y = 0; right.Normalize();
        Vector3 dir = fwd * input.y + right * input.x;

        if (rb != null)
            rb.linearVelocity = new Vector3(dir.x * speed, rb.linearVelocity.y, dir.z * speed);
        else
            transform.position += dir * speed * Time.fixedDeltaTime;
    }

    void OnCollisionEnter(Collision c) { Cek(c.gameObject); }
    void OnTriggerEnter(Collider o) { Cek(o.gameObject); }

    void Cek(GameObject g)
    {
        if (g.name == targetName)
        {
            success = true;
            Debug.Log("Target tersentuh. Praktikum berhasil.");
        }
        else if (g.name.StartsWith("Obstacle"))
        {
            Debug.Log("Obstacle tersentuh: " + g.name);
        }
    }

    void OnGUI()
    {
        if (style == null)
        {
            style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleCenter;
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = Color.white;
            style.wordWrap = true;
        }
        style.fontSize = Mathf.RoundToInt(Screen.height * 0.022f);

        // ---- Notifikasi (kotak gelap di atas) ----
        Rect box = new Rect(20, 40, Screen.width - 40, Screen.height * 0.09f);
        GUI.color = new Color(0.15f, 0.19f, 0.27f, 0.95f);
        GUI.DrawTexture(box, Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Label(box, success ? judul + "\n" + pesanSukses : judul, style);

        // ---- Joystick ----
        float y = Screen.height - center.y;   // GUI origin kiri-atas
        GUI.color = new Color(1, 1, 1, 0.3f);
        GUI.DrawTexture(new Rect(center.x - radius, y - radius, radius * 2, radius * 2), circle);

        float k = radius * 0.6f;              // ukuran knob
        Vector2 knob = center + input * radius;
        GUI.color = new Color(1, 1, 1, 0.7f);
        GUI.DrawTexture(new Rect(knob.x - k, Screen.height - knob.y - k, k * 2, k * 2), circle);
        GUI.color = Color.white;
    }
}