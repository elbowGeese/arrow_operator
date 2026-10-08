using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public bool moving;
    public float moveRange = 0.5f;
    public float moveSpeed = 1;
    public GameObject gameOverPanel;

    Vector3 startPos;
    float moveTime;
    bool hit;

    void Start()
    {
        startPos = transform.position;
        moveTime = Random.Range(0f, 10f);
    }

    void Update()
    {
        if (!moving) return;

        moveTime += Time.deltaTime * moveSpeed;

        float x = Mathf.Sin(moveTime) * moveRange;
        float y = Mathf.Sin(moveTime * 1.3f) * moveRange;

        transform.position = startPos + new Vector3(x, y, 0);
    }

    void OnTriggerEnter(Collider other)
    {
        HandlePlayerContact(other);
    }

    // Recheck overlapping players so an expired shield cannot leave them invulnerable.
    void OnTriggerStay(Collider other)
    {
        HandlePlayerContact(other);
    }

    void HandlePlayerContact(Collider other)
    {
        if (hit || Time.timeScale == 0) return;
        if (!other.CompareTag("Player")) return;

        var aids = other.GetComponentInParent<ArrowOperator.Jeff.MovementAidController>();
        if (aids != null && aids.TryProtect(GetComponent<Collider>())) return;

        hit = true;
        Destroy(other.gameObject);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Time.timeScale = 0;
    }
}
