using UnityEngine;

[CreateAssetMenu(menuName = "SO/Weapon/Data", fileName = "Weapon Data")]
public class WeaponDataSO : ScriptableObject
{
    [Header("Visual")]
    public Sprite Visual;

    [Header("Damage")]
    public int Damage;

    [Header("Ammo")]
    public int MaxAmmo;
    public float ReloadTime;

    [Header("Fire")]
    public float FireRate;
    public float FireRange;
    public bool IsAuto;

    [Header("Aiming")]
    public float AimingDistance;
    public float AimingOrthoRatio = 1f;

    [Header("ETC")]
    public TrailRenderer _bulletTrailPrefab;
}
