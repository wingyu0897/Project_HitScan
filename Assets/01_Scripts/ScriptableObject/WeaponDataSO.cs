using UnityEngine;

[CreateAssetMenu(menuName = "SO/Weapon/Data", fileName = "Weapon Data")]
public class WeaponDataSO : ScriptableObject
{
    [Header("Visual")]
    public Sprite Visual;
    public string Name;

    [Header("Damage")]
    public int Damage;

    [Header("Ammo")]
    public int MaxAmmo;
    public float ReloadTime;

    [Header("Fire")]
    public float FireRate;
    public float FireRange;
    public bool IsAuto;

    [Header("Recoil")]
    [Range(0, 180)]
    public float RecoilAngle;
    public float RecoilDistance;

    [Header("Aiming")]
    public float AimingDistance;
    public float AimingOrthoRatio = 1f;
    [Range(0, 1f)]
    public float AimingSlowdown = 0.5f;

    [Header("ETC")]
    public TrailRenderer _bulletTrailPrefab;

    [Header("Audio")]
    public AudioClip FireClip;
    public AudioClip ReloadClip;
    public AudioClip EmptyClip;
}
