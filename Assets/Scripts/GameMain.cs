using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameMain : MonoBehaviour
{
    public Player player;
    public Boss boss;
    public GameObject door;
    public Transform bulletsRoot;

    public GameObject info;

    private PlayerStatus playerStatus;

    public GameObject mainMenu;
    public GameObject pauseMenu;
    public GameObject upgradeMenu;
    public GameObject endMenu;
    public GameObject message;

    private const string SaveName = "save";

    public static GameMain Instance { get; private set; }

    private void Start()
    {
        Instance = this;
        Time.timeScale = 0;

        mainMenu.SetActive(true);

        InitMainMenu();
        InitPauseMenu();
        InitEndMenu();
    }

    #region UI Init

    private void InitMainMenu()
    {
        mainMenu.transform.Find("NewGame").GetComponent<Button>().onClick.AddListener(StartNewGame);
        mainMenu.transform.Find("LoadGame").GetComponent<Button>().onClick.AddListener(LoadGame);
        mainMenu.transform.Find("LoadGame").GetComponent<Button>().interactable = File.Exists(Path.Combine(Application.dataPath, SaveName));
    }

    private void InitPauseMenu()
    {
        pauseMenu.transform.Find("Resume").GetComponent<Button>().onClick.AddListener(() =>
        {
            pauseMenu.SetActive(false);
            ResumeGame();
        });
        pauseMenu.transform.Find("Exit").GetComponent<Button>().onClick.AddListener(Application.Quit);
    }

    private void InitUpgradeMenu()
    {
        InitItem("hp", playerStatus.hp);
        InitItem("def", playerStatus.def);
        InitItem("moveSpeed", playerStatus.moveSpeed);
        InitItem("invincibleTimeOnHit", playerStatus.invincibleTimeOnHit);
        InitItem("damage", playerStatus.damage);
        InitItem("bulletCount", playerStatus.bulletCount);
        InitItem("attackInterval", playerStatus.attackInterval);
        InitItem("bulletLifeTime", playerStatus.bulletLifeTime);
        InitItem("invincibleTimeOnDodge", playerStatus.invincibleTimeOnDodge);
        InitItem("critChance", playerStatus.critChance, true);
        InitItem("critDamage", playerStatus.critDamage, true);

        upgradeMenu.transform.Find("dodgeUnlocked/Button").GetComponent<Button>().onClick.AddListener(() =>
        {
            if (playerStatus.rarePerks > 0)
            {
                playerStatus.rarePerks--;
                playerStatus.dodgeUnlocked = true;
            }
        });
        upgradeMenu.transform.Find("critUnlocked/Button").GetComponent<Button>().onClick.AddListener(() =>
        {
            if (playerStatus.rarePerks > 0)
            {
                playerStatus.rarePerks--;
                playerStatus.critUnlocked = true;
            }
        });
        upgradeMenu.transform.Find("doubleBarrelUnlocked/Button").GetComponent<Button>().onClick.AddListener(() =>
        {
            if (playerStatus.rarePerks >= 5)
            {
                playerStatus.rarePerks -= 5;
                playerStatus.doubleBarrelUnlocked = true;
            }
        });

        upgradeMenu.transform.Find("back").GetComponent<Button>().onClick.AddListener(() =>
        {
            upgradeMenu.SetActive(false);
            player.Upgrade(playerStatus);
            ResumeGame();
        });
    }

    private void InitEndMenu()
    {
        endMenu.transform.Find("End").GetComponent<Button>().onClick.AddListener(Application.Quit);
        endMenu.transform.Find("Continue").GetComponent<Button>().onClick.AddListener(() =>
        {
            endMenu.SetActive(false);
            ResumeGame();

            OnBossKilled();
        });
    }

    //Again I know status should be generic, but anyway just live with it
    private void InitItem(string itemName, FloatStatus status, bool percentage = false)
    {
        var text = upgradeMenu.transform.Find(itemName + "/Button/Text").GetComponent<Text>();
        text.text = percentage ? $"+{status.stepValue:P}" : $"{status.stepValue:+0.####;-0.####}";

        var button = upgradeMenu.transform.Find(itemName + "/Button").GetComponent<Button>();
        button.onClick.AddListener(() =>
        {
            if (playerStatus.rarePerks >= 1)
            {
                playerStatus.normalPerks--;
                status.currentLevel++;
            }
        });
    }

    private void InitItem(string itemName, IntStatus status)
    {
        var text = upgradeMenu.transform.Find(itemName + "/Button/Text").GetComponent<Text>();
        text.text = $"{status.stepValue:+0.####;-0.####}";

        var button = upgradeMenu.transform.Find(itemName + "/Button").GetComponent<Button>();
        button.onClick.AddListener(() =>
        {
            if (playerStatus.rarePerks >= 1)
            {
                playerStatus.normalPerks--;
                status.currentLevel++;
            }
        });
    }

    #endregion

    private void StartNewGame()
    {
        playerStatus = CreateDefaultPlayerStatus();
        StartGame();
    }

    private void LoadGame()
    {
        playerStatus = JsonUtility.FromJson<PlayerStatus>(File.ReadAllText(Path.Combine(Application.dataPath, SaveName)));
        StartGame();
    }

    private void StartGame()
    {
        mainMenu.SetActive(false);

        InitUpgradeMenu();

        player.Init(playerStatus);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1;
    }

    private void PauseGame()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0;
    }

    private void ResumeGame()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1;
    }

    public void OnEnterArena()
    {
        player.inCombat = true;
        door.SetActive(true);

        boss.Init(playerStatus.totalLevel);
        boss.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (playerStatus != null)
        {
            UpdateUpgradeMenu();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseMenu.activeSelf)
            {
                pauseMenu.SetActive(false);
                ResumeGame();
            }
            else if (upgradeMenu.activeSelf)
            {
                upgradeMenu.SetActive(false);
                player.Upgrade(playerStatus);
                ResumeGame();
            }
            else
            {
                PauseGame();
                pauseMenu.SetActive(true);
            }
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            if (playerStatus != null && !pauseMenu.activeSelf && !upgradeMenu.activeSelf)
            {
                PauseGame();
                upgradeMenu.SetActive(true);
            }
        }
    }

    private void ResetGame()
    {
        door.SetActive(false);

        boss.Stop();
        boss.gameObject.SetActive(false);
        boss.transform.position = new Vector3(0, 0.5f, 117);

        player.Reset();
        player.characterController.enabled = false;
        player.transform.position = new Vector3(0, 0.5f, 0);
        player.transform.rotation = Quaternion.identity;
        player.characterController.enabled = true;

        foreach (Transform bullet in bulletsRoot)
        {
            Destroy(bullet.gameObject);
        }
    }

    public void OnPlayerDied()
    {
        ShowInfo("You Died!");
        message.SetActive(true);
        ResetGame();
    }

    public void OnBossTrapped()
    {
        PauseGame();
        endMenu.SetActive(true);
    }

    public void OnBossKilled()
    {
        var msg = "Boss defeated!\nYou received 1 normal perk!\nPress C to use perks.";
        playerStatus.totalLevel++;
        playerStatus.normalPerks++;
        if (Random.Range(0.0f, 1.0f) <= 0.1f)
        {
            msg = "Boss defeated!\nYou received 1 normal perk and 1 rare perk!\nPress C to use perks.";
            playerStatus.rarePerks++;
        }

        ShowInfo(msg);
        message.SetActive(true);
        ResetGame();
    }

    private void ShowInfo(string msg)
    {
        info.GetComponent<Text>().text = msg;
        info.SetActive(true);
        LeanTween.delayedCall(info, 3.0f, () =>
        {
            LeanTween.textColor(info.GetComponent<RectTransform>(), new Color(1, 0, 0, 0), 1).setOnComplete(() =>
            {
                info.SetActive(false);
                info.GetComponent<Text>().color = Color.red;
            });
        });
    }

    #region UI Update

    //Its quite hard to not repeat yourself in a hush...
    private void UpdateUpgradeMenu()
    {
        UpdateItem("hp", playerStatus.hp);
        UpdateItem("def", playerStatus.def);
        UpdateItem("moveSpeed", playerStatus.moveSpeed);
        UpdateItem("invincibleTimeOnHit", playerStatus.invincibleTimeOnHit);
        UpdateItem("damage", playerStatus.damage);
        UpdateItem("bulletCount", playerStatus.bulletCount);
        UpdateItem("attackInterval", playerStatus.attackInterval);
        UpdateItem("bulletLifeTime", playerStatus.bulletLifeTime);
        UpdateItem("invincibleTimeOnDodge", playerStatus.invincibleTimeOnDodge);
        UpdateItem("critChance", playerStatus.critChance, true);
        UpdateItem("critDamage", playerStatus.critDamage, true);

        upgradeMenu.transform.Find("dodgeUnlocked/Button").GetComponent<Button>().interactable = !playerStatus.dodgeUnlocked;
        upgradeMenu.transform.Find("dodgeUnlocked/Button/Text").GetComponent<Text>().text =
            playerStatus.dodgeUnlocked ? "Unlocked" : "Unlock!";
        upgradeMenu.transform.Find("critUnlocked/Button").GetComponent<Button>().interactable = !playerStatus.critUnlocked;
        upgradeMenu.transform.Find("critUnlocked/Button/Text").GetComponent<Text>().text = playerStatus.critUnlocked ? "Unlocked" : "Unlock!";
        upgradeMenu.transform.Find("doubleBarrelUnlocked/Button").GetComponent<Button>().interactable = !playerStatus.doubleBarrelUnlocked;
        upgradeMenu.transform.Find("doubleBarrelUnlocked/Button/Text").GetComponent<Text>().text = 
            playerStatus.doubleBarrelUnlocked ? "Unlocked!" : "Unlock (5)";

        upgradeMenu.transform.Find("NormalPerks").GetComponent<Text>().text = $"Normal Perks: {playerStatus.normalPerks}";
        upgradeMenu.transform.Find("RarePerks").GetComponent<Text>().text = $"Rare Perks: {playerStatus.rarePerks}";
    }

    private void UpdateItem(string itemName, FloatStatus status, bool percentage = false)
    {
        var text = upgradeMenu.transform.Find(itemName + "/value").GetComponent<Text>();
        text.text = percentage ? $"{status.currentValue:P}" : $"{status.currentValue}";

        var button = upgradeMenu.transform.Find(itemName + "/Button").GetComponent<Button>();
        button.interactable = status.maxLevel == -1 || status.currentLevel < status.maxLevel;
    }

    private void UpdateItem(string itemName, IntStatus status)
    {
        var text = upgradeMenu.transform.Find(itemName + "/value").GetComponent<Text>();
        text.text = $"{status.currentValue}";

        var button = upgradeMenu.transform.Find(itemName + "/Button").GetComponent<Button>();
        button.interactable = status.maxLevel == -1 || status.currentLevel < status.maxLevel;
    }

    #endregion

    private void OnDestroy()
    {
        if (playerStatus != null)
        {
            File.WriteAllText(Path.Combine(Application.dataPath, SaveName), JsonUtility.ToJson(playerStatus));
        }
    }

    private PlayerStatus CreateDefaultPlayerStatus()
    {
        return new PlayerStatus
        {
            hp = new FloatStatus
            {
                baseValue = 100,
                stepValue = 10,
                currentLevel = 0,
                maxLevel = 10
            },
            def = new FloatStatus
            {
                baseValue = 0,
                stepValue = 10,
                currentLevel = 0,
                maxLevel = -1
            },
            moveSpeed = new FloatStatus
            {
                baseValue = 7.5f,
                stepValue = 0.1f,
                currentLevel = 0,
                maxLevel = 10
            },
            invincibleTimeOnHit = new FloatStatus
            {
                baseValue = 0.2f,
                stepValue = 0.01f,
                currentLevel = 0,
                maxLevel = 10
            },
            damage = new FloatStatus
            {
                baseValue = 10,
                stepValue = 1,
                currentLevel = 0,
                maxLevel = -1
            },
            bulletCount = new IntStatus
            {
                baseValue = 5,
                stepValue = 1,
                currentLevel = 0,
                maxLevel = 10
            },
            attackInterval = new FloatStatus
            {
                baseValue = 1.5f,
                stepValue = -0.05f,
                currentLevel = 0,
                maxLevel = 10
            },
            bulletLifeTime = new FloatStatus
            {
                baseValue = 0.5f,
                stepValue = 0.05f,
                currentLevel = 0,
                maxLevel = 10
            },
            dodgeUnlocked = false,
            invincibleTimeOnDodge = new FloatStatus
            {
                baseValue = 0.1f,
                stepValue = 0.01f,
                currentLevel = 0,
                maxLevel = 10
            },
            critUnlocked = false,
            critChance = new FloatStatus
            {
                baseValue = 0,
                stepValue = 0.05f,
                currentLevel = 0,
                maxLevel = 20
            },
            critDamage = new FloatStatus
            {
                baseValue = 1.50f,
                stepValue = 0.05f,
                currentLevel = 0,
                maxLevel = -1
            },
            doubleBarrelUnlocked = false,
            normalPerks = 0,
            rarePerks = 0
        };
    }
}
