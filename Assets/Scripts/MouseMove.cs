using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseMove : MonoBehaviour
{
    public GameManager gameManager;
    public float maxSpeed;
    public float jumpPower;
    private float curTime;
    public float coolTime = 0.5f;
    public Transform pos;
    public Vector2 boxSize;
    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    Animator anim;
    int jumpCount = 2;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        jumpCount = 0;
    }

    void Update()
    {
        //Jump -> 2단점프
        if (jumpCount > 0)
        {
            if (Input.GetButtonDown("Jump")) //&& !anim.GetBool("Jumping") = 무한 점프 막기
            {
                rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
                anim.SetBool("Jumping", true);
                jumpCount--;
            }
        }

        if (Input.GetButtonUp("Horizontal")) //버튼에서 손을 때는 경우, 속력을 줄임
        {
            rigid.velocity = new Vector2(rigid.velocity.normalized.x * 0.5f, rigid.velocity.y);
        }

        //Direction Sprite
        if (Input.GetButtonDown("Horizontal"))
            spriteRenderer.flipX = Input.GetAxisRaw("Horizontal") == -1;

        //Animation
        if (rigid.velocity.normalized.x == 0)
            anim.SetBool("Walking", false);
        else
            anim.SetBool("Walking", true);

        //Eat
        if (curTime <= 0)
        {
            if (Input.GetKey(KeyCode.Z))
            {
                Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(pos.position, boxSize, 0);
                foreach (Collider2D collider in collider2Ds)
                {
                    //Debug.Log(collider.tag);
                    if (collider.tag == "Cheese")
                    {
                        collider.GetComponent<EatCheese>().EatenCheese(1);
                    }
                }

                anim.SetBool("Eating", true);
                curTime = coolTime;
            }
            else
                anim.SetBool("Eating", false);
        }
        else
            curTime -= Time.deltaTime;
    }

    private void onDramGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(pos.position, boxSize);
    }

    void FixedUpdate()
    {
        //Move Speed
        float h = Input.GetAxisRaw("Horizontal");
        rigid.AddForce(Vector2.right * h, ForceMode2D.Impulse);

        //Max Speed
        if (rigid.velocity.x > maxSpeed)
            rigid.velocity = new Vector2(maxSpeed, rigid.velocity.y);
        else if (rigid.velocity.x < maxSpeed * (-1))
            rigid.velocity = new Vector2(maxSpeed * (-1), rigid.velocity.y);

        //Landing Platform
        if (rigid.velocity.y < 1)
        {
            Debug.DrawRay(rigid.position, Vector3.down * 3, Color.red);
            RaycastHit2D rayhit = Physics2D.Raycast(rigid.position, Vector3.down * 3, 3, LayerMask.GetMask("Platform"));

            if (rayhit.collider != null)
            {
                if (rayhit.distance < 1.5f) // player 크기의 반 -> 크기 3으로 수정해서 1.5
                {
                    //Debug.Log(rayhit.collider.name);
                    anim.SetBool("isJumping", false);
                    jumpCount = 2;
                }
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            gameManager.HealthDown();
        }
    }
}