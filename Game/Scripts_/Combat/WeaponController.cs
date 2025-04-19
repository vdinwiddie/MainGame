using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [SerializeField] Transform rightHand = null;
    [SerializeField] Transform leftHand = null;

    public Weapon currentWeapon;

    public void EquipWeapon(Weapon weapon)
    {
        currentWeapon = weapon;
        weapon.Spawn(rightHand, leftHand);
    }
}
