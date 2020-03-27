using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwitcher : MonoBehaviour
{
    [SerializeField]
    private WeaponManager weaponManager;
    [SerializeField]
    private int currentWeaponIndex = 0;
    private int previousWeaponIndex = 0;
    private List<WeaponGraphics> weapons;
    private int weaponsCount;

    // Start is called before the first frame update
    void OnEnable()
    {
        weapons = weaponManager.GetAllWeaponsGraphics();
        weaponsCount = weapons.Count;
        SelectWeapon();
    }

    // Update is called once per frame
    void Update()
    {
        if (!weaponManager.isReloading)
        {
            HandlePlayerInput();
        }
    }

    private void HandlePlayerInput()
    {
        previousWeaponIndex = currentWeaponIndex;

        // Mouse ScrollWheel Input
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            if (currentWeaponIndex >= weaponsCount - 1)
            {
                currentWeaponIndex = 0;
            }
            else
            {
                currentWeaponIndex++;
            }
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            if (currentWeaponIndex <= 0)
            {
                currentWeaponIndex = weaponsCount - 1;
            }
            else
            {
                currentWeaponIndex--;
            }
        }

        // Keyboard Numerical Input
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentWeaponIndex = 0;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) && weaponsCount <= 2)
        {
            currentWeaponIndex = 1;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3) && weaponsCount <= 3)
        {
            currentWeaponIndex = 2;
        }
        if (Input.GetKeyDown(KeyCode.Alpha4) && weaponsCount <= 4)
        {
            currentWeaponIndex = 3;
        }

        if (previousWeaponIndex != currentWeaponIndex)
        {
            SelectWeapon();
        }
    }

    private void SelectWeapon()
    {
        for (int i = 0; i < weaponsCount; i++)
        {
            if (i == currentWeaponIndex)
            {
                weapons[i].gameObject.SetActive(true);
                weaponManager.EquipSelectedWeapon(i, weapons[i].gameObject);
            }
            else
            {
                weapons[i].gameObject.SetActive(false);
            }
        }
    }
}
