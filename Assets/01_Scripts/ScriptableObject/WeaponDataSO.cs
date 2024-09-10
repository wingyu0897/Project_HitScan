using UnityEngine;

[CreateAssetMenu(menuName = "SO/Weapon/Data", fileName = "Weapon Data")]
public class WeaponDataSO : ScriptableObject
{
    public int Damage;
    public int MaxAmmo;
    public float FireRate;
    public float FireRange;
    public float ReloadTime;
    public bool IsAuto;

    public TrailRenderer _bulletTrailPrefab;
}
