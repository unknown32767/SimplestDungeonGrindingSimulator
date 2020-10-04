using System;

//Should be Status<T> but u know JsonUtility sucks at generics...
[Serializable]
public class FloatStatus
{
    public float baseValue;
    public float stepValue;

    public int maxLevel;
    public int currentLevel;

    public float currentValue => baseValue + currentLevel * stepValue;
}

[Serializable]
public class IntStatus
{
    public int baseValue;
    public int stepValue;

    public int maxLevel;
    public int currentLevel;

    public int currentValue => baseValue + currentLevel * stepValue;
}

[Serializable]
public class PlayerStatus
{
    public FloatStatus hp;
    public FloatStatus def;
    public FloatStatus moveSpeed;
    public FloatStatus invincibleTimeOnHit;

    public FloatStatus damage;
    public IntStatus bulletCount;
    public FloatStatus attackInterval;
    public FloatStatus bulletLifeTime;

    public bool dodgeUnlocked;
    public FloatStatus invincibleTimeOnDodge;

    public bool critUnlocked;
    public FloatStatus critChance;
    public FloatStatus critDamage;

    public bool doubleBarrelUnlocked;

    public int normalPerks;
    public int rarePerks;

    public int totalLevel;
}