using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed;
    public float lifeTime;
    public bool fromPlayer;

    private void Update()
    {
        transform.Translate(speed * Time.deltaTime * Vector3.forward);

        lifeTime -= Time.deltaTime;

        if (lifeTime <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (fromPlayer && other.GetComponent<Boss>() != null)
        {
            GameMain.Instance.boss.TakeDamage(GameMain.Instance.player.CalcDamage());
            Destroy(gameObject);
        }
        else if(!fromPlayer && other.GetComponent<Player>() != null)
        {
            GameMain.Instance.player.TakeDamage(GameMain.Instance.boss.CalcDamage());
            Destroy(gameObject);
        }
    }

    public void Init(float speed, float lifeTime, bool fromPlayer)
    {
        this.speed = speed;
        this.lifeTime = lifeTime;
        this.fromPlayer = fromPlayer;
    }
}
