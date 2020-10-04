using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Boss : MonoBehaviour
{
    public Transform player;
    public Transform hpBar;
    private Image hpImage;
    private Text hpText;

    public GameObject bulletObject;
    public GameObject bulletObject2;
    public Transform bulletsRoot;

    public NavMeshAgent navMeshAgent;

    private float baseSpeed = 3.5f;
    private float baseHpMax = 500.0f;
    private float baseAtk = 10.0f;
    private float baseAttackInterval = 1.0f;
    private float baseSpecialInterval = 5.0f;
    private int baseCrossCount = 4;
    private float currentSpeed;
    private float hpMax;
    private float currentAtk;
    private float currentHp;
    private float currentAttackInterval;
    private float currentSpecialInterval;
    private int currentCrossCount;

    private int specialBulletFireCount;

    private float normalAttackCooldown;
    private float specialAttackCooldown;

    public void Start()
    {
        hpImage = hpBar.Find("Image").GetComponent<Image>();
        hpText = hpBar.Find("Text").GetComponent<Text>();
    }

    public void Init(int level)
    {
        currentSpeed = baseSpeed + Mathf.Clamp(level, 0, 35) * 0.1f;
        navMeshAgent.speed = currentSpeed;

        hpMax = baseHpMax * Mathf.Pow(1.05f, level);
        currentHp = hpMax;
        currentAtk = baseAtk * Mathf.Pow(1.05f, level);
        currentAttackInterval = baseAttackInterval - Mathf.Clamp(level, 0, 50) * 0.01f;
        currentSpecialInterval = baseSpecialInterval - Mathf.Clamp(level, 0, 50) * 0.02f;
        currentCrossCount = baseCrossCount + Mathf.Clamp(level, 0, 2);

        hpBar.gameObject.SetActive(true);

        specialBulletFireCount = 6;
        specialAttackCooldown = currentSpecialInterval;
    }

    public void Stop()
    {
        hpBar.gameObject.SetActive(false);
    }

    private void Update()
    {
        hpImage.fillAmount = currentHp / hpMax;
        hpText.text = $"{currentHp:0}/{hpMax:0}";

        navMeshAgent.SetDestination(player.position);

        if (normalAttackCooldown <= 0)
        {
            FireBullet(transform.position, transform.rotation);
            normalAttackCooldown = currentAttackInterval;
        }

        if (normalAttackCooldown > 0)
        {
            normalAttackCooldown -= Time.deltaTime;
        }

        if (specialAttackCooldown <= 0)
        {
            if (specialBulletFireCount == 0)
            {
                specialBulletFireCount = 6;
                FireSpecialBullet();
            }
            else
            {
                var rand = Random.Range(0.0f, 1.0f);

                if (rand < 0.15f)
                {
                    FireSpecialBullet();
                }
                else if (rand < 0.45f)
                {
                    specialBulletFireCount--;
                    StartCoroutine(FireCrossBullets());
                }
                else
                {
                    specialBulletFireCount--;
                    FireALotBullets();
                }
            }

            specialAttackCooldown = currentSpecialInterval;
        }

        if (specialAttackCooldown > 0)
        {
            specialAttackCooldown -= Time.deltaTime;
        }
    }

    public void TakeDamage(float damage)
    {
        currentHp -= damage;
        if (currentHp <= 0)
        {
            GameMain.Instance.OnBossKilled();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        var otherPlayer = other.transform.GetComponent<Player>();
        if (otherPlayer != null)
        {
            otherPlayer.TakeDamage(currentAtk);
        }
    }

    private void FireBullet(Vector3 pos, Quaternion rot)
    {
        var bullet = Instantiate(bulletObject, bulletsRoot);
        bullet.transform.position = pos;
        bullet.transform.rotation = rot;

        bullet.GetComponent<Bullet>().Init(5.0f, 10.0f, false);
    }

    private void FireSpecialBullet()
    {
        var position = transform.position;
        var rotation = transform.rotation;
        var firePoint = position + rotation * Vector3.forward * 1.0f;
        Instantiate(bulletObject2, firePoint, rotation, bulletsRoot);
    }

    private void FireALotBullets()
    {
        for (var i = 0; i < 50; i++)
        {
            var rot = Quaternion.Euler(0, Random.Range(-45, 45), 0) * transform.rotation;
            FireBullet(transform.position, rot);
        }
    }

    private IEnumerator FireCrossBullets()
    {
        var baseRot = transform.rotation;
        var sign = Random.Range(-1.0f, 1.0f) > 0 ? 1 : -1;

        for (var i = 0; i < 60; i++)
        {
            for (var j = 0; j < currentCrossCount; j++)
            {
                var rot = baseRot * Quaternion.Euler(0, 360.0f / currentCrossCount * j, 0);
                FireBullet(transform.position, rot);
            }

            baseRot *= Quaternion.Euler(0, 6 * sign, 0);
            yield return new WaitForSeconds(0.05f);
        }
    }

    public float CalcDamage()
    {
        return currentAtk;
    }
}