using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public bool moving;
    public float moveRange = 0.5f;
    public float moveSpeed = 1;

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

    void OnCollisionEnter(Collision other)
    {
        if (hit || Time.timeScale == 0) return;
        if (!other.gameObject.CompareTag("Player")) return;

        hit = true;
        //Destroy(other.gameObject);

        GameObject.FindWithTag("GameOverPanel").transform.GetChild(0).gameObject.SetActive(true);

        Time.timeScale = 0;
    }
}