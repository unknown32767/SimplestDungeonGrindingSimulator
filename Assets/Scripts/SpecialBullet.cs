using UnityEngine;

public class SpecialBullet : MonoBehaviour
{
    public float speed;
    public float lifeTime = 10.0f;

    private void Update()
    {
        transform.LookAt(GameMain.Instance.player.transform);

        transform.Translate(speed * Time.deltaTime * Vector3.forward);

        lifeTime -= Time.deltaTime;

        if (lifeTime <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Boss>() != null)
        {
            if (lifeTime < 9.5f)
            {
                GameMain.Instance.OnBossTrapped();
                Destroy(gameObject);
            }
        }
        else if (other.GetComponent<Player>() != null)
        {
            var boss = GameMain.Instance.boss;
            var player = GameMain.Instance.player;

            var oldBossPosition = boss.transform.position;
            var oldPlayerPosition = player.transform.position;

            player.SetInvincible(0.1f);
            boss.transform.position = oldPlayerPosition;

            player.characterController.enabled = false;
            player.transform.position = oldBossPosition;
            player.characterController.enabled = true;

            player.transform.LookAt(boss.transform, Vector3.up);

            var eulerAngles = player.transform.eulerAngles;
            eulerAngles.x = 0;
            eulerAngles.z = 0;
            player.transform.eulerAngles = eulerAngles;

            Destroy(gameObject);
        }
    }
}