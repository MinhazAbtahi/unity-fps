using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class WeaponManager : MonoBehaviour
{
    [SerializeField]
    private List<Weapon> weaponsList;
    private List<WeaponGraphics> weaponsGraphicsList;
    private Weapon currentWeapon;
    private WeaponGraphics currentWeaponGraphics;
    private WeaponSoundFx currentWeaponSoundFx;
    private WeaponSwitcher weaponSwitcher;

    [SerializeField]
    private string weaponLayerName = "WeaponLayer";

    [SerializeField]
    private Transform weaponHolder;

    public bool isReloading = false;
    private AudioSource audioSource;

    // Use this for initialization
    void Start()
    {
        weaponSwitcher = GetComponentInChildren<WeaponSwitcher>();
        weaponsGraphicsList = new List<WeaponGraphics>();
        int weaponCount = weaponsList.Count;
        for (int i = 0; i < weaponCount; i++)
        {
            this.LoadWeapons(weaponsList[i]);
        }
        weaponSwitcher.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void LoadWeapons(Weapon selectedWeapon)
    {
        GameObject currentWeaponClone = Instantiate(selectedWeapon.weaponGraphics, weaponHolder.position, weaponHolder.rotation) as GameObject;
        currentWeaponClone.transform.SetParent(this.weaponHolder);
        EquipWeapon(selectedWeapon, currentWeaponClone);
        weaponsGraphicsList.Add(currentWeaponClone.GetComponent<WeaponGraphics>());
    }

    /// <summary>
    /// Equip Selected Weapon
    /// </summary>
    /// <param name="selectedWeaponIndex"></param>
    public void EquipSelectedWeapon(int selectedWeaponIndex, GameObject selectedWeaponGraphics)
    {
        EquipWeapon(weaponsList[selectedWeaponIndex], selectedWeaponGraphics);
    }

    /// <summary>
    /// Equip Current Weapon
    /// </summary>
    /// <param name="selectedWeapon"></param>
    private void EquipWeapon(Weapon selectedWeapon, GameObject selectedWeaponClone)
    {
        this.currentWeapon = selectedWeapon;

        // Weapon graphics
        this.currentWeaponGraphics = selectedWeaponClone.GetComponent<WeaponGraphics>();
        if (!currentWeaponGraphics)
        {
            Debug.LogError("WeapponGraphics component is missing on the Weapon : " + selectedWeaponClone.gameObject.name);
        }

        // Weapon SoundFx
        this.currentWeaponSoundFx = selectedWeaponClone.GetComponent<WeaponSoundFx>();
        if (!currentWeaponSoundFx)
        {
            Debug.LogError("WeapponSoundFx component is missing on the Weapon : " + selectedWeaponClone.gameObject.name);
        }

        // Set weaponLayer for Local Player
        //if (isLocalPlayer)
        //{
            int layerIndex = LayerMask.NameToLayer(this.weaponLayerName);
            Util.SetLayerRecursively(selectedWeaponClone, layerIndex);
        //}
    }

    /// <summary>
    /// Returns selected Weapon as currentWeapon
    /// </summary>
    /// <returns></returns>
    public Weapon GetCurrentWeapon()
    {
        return this.currentWeapon;
    }

    /// <summary>
    /// Returns all the Weapons
    /// </summary>
    /// <returns></returns>
    public List<Weapon> GetAllWeapons()
    {
        return this.weaponsList;
    }

    /// <summary>
    /// Returns selected WeaponGraphics as currentWeaponGraphics
    /// </summary>
    /// <returns></returns>
    public WeaponGraphics GetCurrentWeaponGraphics()
    {
        return this.currentWeaponGraphics;
    }

    /// <summary>
    /// Returns all the WeaponsGraphics
    /// </summary>
    /// <returns></returns>
    public List<WeaponGraphics> GetAllWeaponsGraphics()
    {
        return this.weaponsGraphicsList;
    }

    /// <summary>
    /// Returns selected WeaponSoundFx as currentWeaponSoundFx
    /// </summary>
    /// <returns></returns>
    public WeaponSoundFx GetCurrentWeaponSoundFx()
    {
        return this.currentWeaponSoundFx;
    }

    public void Reload()
    {
        if (isReloading)
        {
            return;
        }

        StartCoroutine(ReloadCoroutine());
    }

    IEnumerator ReloadCoroutine()
    {
        this.isReloading = true;

        CmdOnReload();
        yield return new WaitForSeconds(this.currentWeapon.reloadTime + .25f);

        this.currentWeaponSoundFx.PlaySoundFx(currentWeaponSoundFx.ReloadSoundFx, 1f);
        this.currentWeapon.bullets = this.currentWeapon.maxBullets;

        this.isReloading = false;
    }

    private void CmdOnReload()
    {
        RpcOnReload();
    }

    private void RpcOnReload()
    {
        Animator weaponAnimator = this.currentWeaponGraphics.GetComponent<Animator>();
        if (weaponAnimator)
        {
            weaponAnimator.SetTrigger("Reload");
        }
    }

}
