using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using UnityEngine;

public enum PlayerState
{
    Idle, Move
}

public class PlayerController : MonoBehaviour
{
    public Animator animator;
    public float moveSpeed;
    public Vector2 moveRangeX;
    public Vector2 moveRangeY;
    public Transform gunRoot;
    public int lv = 1;
    public int bulletQuantity { get => lv; }
    public float attackCD { get => Mathf.Clamp(1F / lv * 1.5F, 0.1F, 1F); }
    public int LV
    {
        get => lv;
        set
        {
            lv = value;
            // 因为等级导致的初始化数据
            SharedData.gameSharedData.Data.spawnInterval = 10f / lv * spawnMonsterIntervalMultiply;
            SharedData.gameSharedData.Data.spawnCount = (int)(lv * 5 * spawnMonsterQuantityMultiply);
        }
    }

    public float spawnMonsterIntervalMultiply = 1;
    public float spawnMonsterQuantityMultiply = 1;
    private PlayerState playerState;
    public PlayerState PlayerState
    {
        get { return playerState; }
        set
        {
            playerState = value;
            switch (playerState)
            {
                case PlayerState.Idle:
                    PlayAnimation("Idle");
                    break;
                case PlayerState.Move:
                    PlayAnimation("Move");
                    break;
            }
        }
    }

    private void Awake()
    {
        CheckPositionRange();
        LV = lv; // 为了初始化
    }

    private void Start()
    {
        PlayerState = PlayerState.Idle;
    }
    private void Update()
    {
        CheckAttack();

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        switch (playerState)
        {
            case PlayerState.Idle:
                if (h != 0 || v != 0) PlayerState = PlayerState.Move;
                break;
            case PlayerState.Move:
                if (h == 0 && v == 0)
                {
                    PlayerState = PlayerState.Idle;
                    return;
                }
                transform.Translate(moveSpeed * Time.deltaTime * new Vector3(h, v, 0));
                CheckPositionRange();
                if (h > 0) transform.localScale = Vector3.one;
                else if (h < 0) transform.localScale = new Vector3(-1, 1, 1);
                break;
        }
    }

    private void CheckPositionRange()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, moveRangeX.x, moveRangeX.y);
        pos.y = Mathf.Clamp(pos.y, moveRangeY.x, moveRangeY.y);
        pos.z = pos.y;
        transform.position = pos;
        SharedData.playerPos.Data = (Vector2)transform.position;
    }


    public void PlayAnimation(string animationName)
    {
        animator.CrossFadeInFixedTime(animationName, 0);
    }

    private float attackCDTimer;
    private void CheckAttack()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        gunRoot.up = (Vector2)mousePos - (Vector2)transform.position;

        attackCDTimer -= Time.deltaTime;
        if (attackCDTimer <= 0 && Input.GetMouseButton(0))
        {
            Attack();
            attackCDTimer = attackCD;
        }
    }

    private void Attack()
    {
        AudioManager.instance.PlayShootAudio();
        // 生成子弹信息
        DynamicBuffer<BulletCreateInfo> buffer = World.DefaultGameObjectInjectionWorld.EntityManager.GetBuffer<BulletCreateInfo>(SharedData.singtonEnitty.Data);
        buffer.Add(new BulletCreateInfo()
        {
            position = gunRoot.position,
            rotation = gunRoot.rotation,
        });
        float angleStep = Mathf.Clamp(360 / bulletQuantity, 0, 5F);
        for (int i = 1; i < bulletQuantity / 2; i++)
        {
            buffer.Add(new BulletCreateInfo()
            {
                position = gunRoot.position,
                rotation = gunRoot.rotation * Quaternion.Euler(0, 0, angleStep * i),
            });
            buffer.Add(new BulletCreateInfo()
            {
                position = gunRoot.position,
                rotation = gunRoot.rotation * Quaternion.Euler(0, 0, -angleStep * i),
            });
        }
    }
}
