using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public Image hpImage;
    public Text hpText;
    public CharacterController characterController;
    public GameObject bulletObject;
    public Transform bulletsRoot;

    public float speed;
    public float rotSpeed;

    [HideInInspector] public bool inCombat;

    private PlayerStatus status;
    private float shootCooldown;
    private float currentHp;
    private float dodgeCooldown;
    private float invincibleTime;

    public void Upgrade(PlayerStatus playerStatus)
    {
        status = playerStatus;
        if (!inCombat)
        {
            currentHp = status.hp.currentValue;
        }
        speed = status.moveSpeed.currentValue;
    }

    public void Init(PlayerStatus playerStatus)
    {
        status = playerStatus;
        currentHp = status.hp.currentValue;
        shootCooldown = 0.0f;
        dodgeCooldown = 0.0f;
        invincibleTime = 0.0f;
    }

    public void Reset()
    {
        currentHp = status.hp.currentValue;
        inCombat = false;
        shootCooldown = 0.0f;
        dodgeCooldown = 0.0f;
        invincibleTime = 0.0f;
    }

    private void Update()
    {
        var direction = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

        direction = transform.rotation * direction.normalized;

        characterController.SimpleMove(speed * direction);

        var mouseDelta = Input.GetAxisRaw("Mouse X");

        transform.Rotate(Vector3.up, Time.deltaTime * rotSpeed * mouseDelta);

        if (shootCooldown > 0)
        {
            shootCooldown -= Time.deltaTime;
        }

        if (inCombat && Input.GetMouseButton(0) && shootCooldown <= 0)
        {
            var transform1 = transform;
            var position = transform1.position;
            var rotation = transform1.rotation;

            shootCooldown = status.attackInterval.currentValue;

            if (status.doubleBarrelUnlocked)
            {
                var lFirePoint = position + rotation * new Vector3(-0.25f, 0, 0.6f);
                var rFirePoint = position + rotation * new Vector3(0.25f, 0, 0.6f);
                for (var i = 0; i < status.bulletCount.currentValue; i++)
                {
                    var rot = Quaternion.Euler(0, Random.Range(-15, 15), 0) * transform.rotation;
                    FireBullet(lFirePoint, rot);
                }
                for (var i = 0; i < status.bulletCount.currentValue; i++)
                {
                    var rot = Quaternion.Euler(0, Random.Range(-15, 15), 0) * transform.rotation;
                    FireBullet(rFirePoint, rot);
                }
            }
            else
            {
                var firePoint = position + rotation * Vector3.forward * 0.6f;
                for (var i = 0; i < status.bulletCount.currentValue; i++)
                {
                    var rot = Quaternion.Euler(0, Random.Range(-15, 15), 0) * transform.rotation;
                    FireBullet(firePoint, rot);
                }
            }
        }

        if (status != null && status.dodgeUnlocked && dodgeCooldown <= 0 && Input.GetKeyDown(KeyCode.LeftShift))
        {
            characterController.Move(2.0f * direction);
            dodgeCooldown = 1.0f;
            invincibleTime = status.invincibleTimeOnDodge.currentValue;
        }

        if (dodgeCooldown > 0)
        {
            dodgeCooldown -= Time.deltaTime;
        }

        if (invincibleTime > 0)
        {
            invincibleTime -= Time.deltaTime;
        }

        if (status != null)
        {
            hpImage.fillAmount = currentHp / status.hp.currentValue;
            hpText.text = $"{currentHp:0}/{status.hp.currentValue:0}";
        }
    }

    public void SetInvincible(float time)
    {
        invincibleTime = time;
    }

    public void TakeDamage(float damage)
    {
        if (invincibleTime <= 0)
        {
            var dmg = damage * (100.0f / (100.0f + status.def.currentValue));

            currentHp -= dmg;
            if (currentHp <= 0)
            {
                GameMain.Instance.OnPlayerDied();
            }

            invincibleTime = status.invincibleTimeOnHit.currentValue;
        }
    }

    private void FireBullet(Vector3 pos, Quaternion rot)
    {
        var bullet = Instantiate(bulletObject, bulletsRoot);
        bullet.transform.position = pos;
        bullet.transform.rotation = rot;

        bullet.GetComponent<Bullet>().Init(30.0f, status.bulletLifeTime.currentValue, true);
    }

    public float CalcDamage()
    {
        if (!status.critUnlocked)
        {
            return status.damage.currentValue;
        }
        else
        {
            return status.damage.currentValue *
                   (Random.Range(0, 1) < status.critChance.currentValue ? status.critDamage.currentValue : 1);
        }
    }
}
