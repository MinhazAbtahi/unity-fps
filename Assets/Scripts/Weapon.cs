using UnityEngine;

public enum WeaponType
{
    Automatic,
    Burst
}

public enum DamageType
{
    Single,
    Area
}

[System.Serializable]
public class Weapon
{
    public string name = "Weapon";
    public WeaponType weaponType = WeaponType.Automatic;
    public DamageType damageType = DamageType.Single;
    public int damage = 10;
    public int maxBullets = 20;
    public int bullets;
    public float reloadTime = 1f;
    public float range = 100f;
    public float fireRate = 0f;
    public GameObject weaponGraphics;

    public Weapon()
    {
        bullets = maxBullets;
    }
}
