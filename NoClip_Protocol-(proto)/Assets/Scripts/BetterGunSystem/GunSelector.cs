using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class GunSelector : MonoBehaviour 
{
    [SerializeField] private GunType Gun;
    [SerializeField] private Transform GunParent;
    [SerializeField] private List<GunScriptableObject> Guns;
    //[SerializeField] private PlayerIK InverseKinematics;

    [Space]
    [Header("Runtime Filled")]
    public GunRuntime ActiveGun;

    private void Start()
    {
        GunScriptableObject gun = Guns.Find(gun => gun.Type == Gun);

        if (gun == null) {
            Debug.LogError($"No GunScriptableObject found for GunType: {gun}");
            return;
        }

        ActiveGun = gun.Spawn (GunParent, this);
    }
}