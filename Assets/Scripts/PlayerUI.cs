using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{

    #region Fields
    [Header("UI Components:")]
    [SerializeField]
    private RectTransform thrusterFuelFill;

    [SerializeField]
    private RectTransform healthBarFill;

    [SerializeField]
    private Text ammoText;

    [SerializeField]
    private GameObject pauseMenu;

    private Player player;
    private PlayerController playerController;
    private WeaponManager weaponManager;
    #endregion

    #region MonoBehaviour
    // Use this for initialization
    void Start()
    {
        if (this.thrusterFuelFill == null)
        {
            Debug.LogError("PlayerUI: ThrusterFuelFill reference is Missing!");
        }

        if (this.healthBarFill == null)
        {
            Debug.LogError("PlayerUI: HealthBarFill reference is Missing!");
        }

        if (this.ammoText == null)
        {
            Debug.LogError("PlayerUI: AmmoText reference is Missing!");
        }

        if (this.pauseMenu == null)
        {
            Debug.LogError("PlayerUI: PauseMenu reference is Missing!");
        }

        PauseMenu.isOn = false;
    }

    // Update is called once per frame
    void Update()
    {
        float fuelAmount = this.playerController.GetThrusterFuelAmount();
        this.SetThrusterFuelAmount(fuelAmount);

        float healthPercentage = this.player.GetHealthPercentage();
        this.SetHealthAmount(healthPercentage);

        int ammoAmount = weaponManager.GetCurrentWeapon().bullets;
        this.SetAmmoAmount(ammoAmount);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            this.TogglePauseMenu();
        }
    }
    #endregion

    #region Custom_methods
    /// <summary>
    /// Sets player controller
    /// </summary>
    /// <param name="player"></param>
    public void SetPlayer(Player player)
    {
        this.player = player;
        this.playerController = this.player.GetComponent<PlayerController>();
        this.weaponManager = this.player.GetComponent<WeaponManager>();

        if (weaponManager == null)
        {
            Debug.Log("weaponManager refernce is Missing");
        }
    }

    /// <summary>
    /// Sets thruster's fuel amount to player UI
    /// </summary>
    /// <param name="fuelAmount"></param>
    private void SetThrusterFuelAmount(float fuelAmount)
    {
        this.thrusterFuelFill.localScale = new Vector3(1f, fuelAmount, 1f);
    }

    /// <summary>
    /// Sets health amount to player UI
    /// </summary>
    /// <param name="healthAmount"></param>
    private void SetHealthAmount(float healthAmount)
    {
        this.healthBarFill.localScale = new Vector3(1f, healthAmount, 1f);
    }

    /// <summary>
    /// Sets ammo amount to player UI
    /// </summary>
    /// <param name="ammoAmount"></param>
    private void SetAmmoAmount(int ammoAmount)
    {
        this.ammoText.text = ammoAmount.ToString();
    }

    /// <summary>
    /// Toggles pause menu
    /// </summary>
    public void TogglePauseMenu()
    {
        this.pauseMenu.SetActive(!this.pauseMenu.activeSelf);
        PauseMenu.isOn = pauseMenu.activeSelf;
    }
    #endregion
}
