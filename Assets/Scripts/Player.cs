using UnityEngine;
using Mirror;

public class Player : NetworkBehaviour
{
    [SerializeField] Rigidbody2D rb;
    [SerializeField] float speed = 10;
    private Vector2 axis = new Vector2();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer) return;
        axis.x = Input.GetAxis("Horizontal");
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer) return;
        MovePlayer();
    }

    private void MovePlayer()
    {
        if (Input.GetKey(KeyCode.A)){
            rb.linearVelocity = new Vector2(rb.linearVelocityX - speed ,rb.linearVelocityY);
        }
        if (Input.GetKey(KeyCode.D))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocityX + speed, rb.linearVelocityY);
        }
    }
}
