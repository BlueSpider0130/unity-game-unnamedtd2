using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnscriptedLogic.Raycast;
using UnscriptedLogic.MathUtils;
using System.Collections.Generic;
using DG.Tweening.Core;
using DG.Tweening;
using UnscriptedLogic.Currency;

public class InspectWindow : MonoBehaviour
{
    [Header("Inspect Window")]
    [SerializeField] private Transform openPos;
    [SerializeField] private Transform closePos;
    [SerializeField] private float tweenTime = 0.25f;
    [SerializeField] private Ease easeType = Ease.InOutSine;
    [SerializeField] private LayerMask unitLayer;
    [SerializeField] private LayerMask towerLayer;

    [Header("Avatar Section")]
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI nameTMP;
    [SerializeField] private Image levelSlider;
    [SerializeField] private TextMeshProUGUI levelTMP;

    [Header("Stats")]
    [SerializeField] private GameObject statPrefab;
    [SerializeField] private Transform statParent;

    [Header("Ability Section")]
    [SerializeField] private GameObject abilityButtonPrefab;
    [SerializeField] private Transform abilityParent;

    private List<AbilityButton> abilityButtons;

    [Header("Upgrade Section")]
    [SerializeField] private GameObject upgradeButtonPrefab;
    [SerializeField] private Transform upgradeParent;

    private List<Button> upgradeButtons;

    //Others
    private InputManager inputManager;
    private AbilityManager abilityManager;
    private bool isOpen = true;
    private Vector2 mousePos;
    private bool isOverUI;

    //Inspected tower components
    private GameObject inspectedObject;
    private TowerBase inspectedTower;
    private TowerSO inspectedTowerSO;
    private TowerUpgradeHandler inspectedTUH;
    private AbilityHandler inspectedAbilityHandler;
    private TowerLevelHandler inspectedLevelHandler;

    private void Start()
    {
        Hide();

        abilityManager = AbilityManager.instance;

        inputManager = InputManager.instance;
        inputManager.OnMouseDown += Inspect;
    }

    private void Inspect(Vector2 mousePos, bool isOverUI)
    {
        this.mousePos = mousePos;
        this.isOverUI = isOverUI;

        abilityManager = AbilityManager.instance;

        if (isOverUI) return;

        if (RaycastLogic.FromMousePos3D(Camera.main, out RaycastHit unitHit))
        {
            IInspectable towerInspectable = unitHit.collider.gameObject.GetComponent<IInspectable>();
            if (towerInspectable != null)
            {
                if (towerInspectable as TowerBase)
                {
                    inspectedTower = towerInspectable as TowerBase;
                    inspectedTowerSO = TowerDefenseManager.instance.AllTowerList.GetSOFromTower(inspectedTower);
                    inspectedTUH = inspectedTower.GetComponent<TowerUpgradeHandler>();
                    inspectedAbilityHandler = inspectedTower.GetComponent<AbilityHandler>();
                    inspectedLevelHandler = inspectedTower.GetComponent<TowerLevelHandler>();

                    RefreshTowerWindow();

                    inspectedObject = unitHit.collider.gameObject;

                    Show();
                    return;
                }
            }

            IInspectable unitInspectable = unitHit.collider.gameObject.GetComponent<IInspectable>();
            if (unitInspectable != null)
            {
                if (unitInspectable as UnitBase)
                {
                    UnitBase unit = unitInspectable as UnitBase;
                    DisplayInspectedAvatar(unit.DisplayName, unit.DisplayIcon);

                    inspectedObject = unitHit.collider.gameObject;

                    Clear(abilityParent);

                    UnitAbilityHandler abilityHandler = inspectedObject.GetComponent<UnitAbilityHandler>();
                    if (abilityHandler != null)
                    {
                        List<UnitAbility> abilities = new List<UnitAbility>(abilityHandler.Abilities);

                        for (int i = 0; i < abilities.Count; i++)
                        {
                            GameObject abilityButton = Instantiate(abilityButtonPrefab, abilityParent);
                            AbilityButton buttonScript = abilityButton.GetComponent<AbilityButton>();
                            buttonScript.Initialize(abilityManager.GetUnitAbilityInfoByAbility(abilities[i]), abilities[i]);
                        }
                    }

                    Show();
                    return;
                }
            }

            IInspectable debriInspectable = unitHit.collider.gameObject.GetComponent<IInspectable>();
            if (debriInspectable != null)
            {
                if (debriInspectable as InspectableDebri)
                {
                    InspectableDebri debri = debriInspectable as InspectableDebri;
                    DisplayInspectedAvatar(debri.DisplayName, debri.Icon);

                    inspectedObject = unitHit.collider.gameObject;

                    Show();
                    return;
                }
            }
        }

        Hide();
    }

    #region Tower Functions

    private void RefreshTowerWindow()
    {
        DisplayInspectedAvatar(inspectedTowerSO.TowerName, inspectedTowerSO.IconSpr, true);
        DisplayTowerStat(inspectedTower);
        DisplayUpgradeButtons(inspectedTowerSO, inspectedTUH.UpgradesChosen.ToArray(), inspectedTUH);
        DisplayTowerAbilities();
    }

    private void DisplayInspectedAvatar(string displayName, Sprite icon, bool showLevel = false)
    {
        this.icon.sprite = icon;
        nameTMP.text = displayName;

        if (showLevel)
            DisplayTowerLevel();
    }

    private void DisplayTowerLevel()
    {
        SyncLevel(null, new CurrencyEventArgs());
        inspectedLevelHandler.ExperienceHandler.OnModified += SyncLevel;
    }

    private void DisplayTowerStat(TowerBase tower)
    {
        float damage = tower.Damage;
        float range = tower.Range;
        float rate = 60f / tower.ReloadTime / 60f;

        Clear(statParent);

        GameObject attStat = Instantiate(statPrefab, statParent);
        attStat.GetComponent<StatView>().Initialized("DMG", damage.ToString());

        GameObject rangeStat = Instantiate(statPrefab, statParent);
        rangeStat.GetComponent<StatView>().Initialized("RNG", range.ToString());

        GameObject rateStat = Instantiate(statPrefab, statParent);
        rateStat.GetComponent<StatView>().Initialized("RATE", $"{Math.Round(rate, 2)}/s");

        tower.DamageHandler.OnModified += StatRefreshWindow;
        tower.RangeHandler.OnModified += StatRefreshWindow;
        tower.ReloadTimeHandler.OnModified += StatRefreshWindow;
    }

    private void DisplayTowerAbilities()
    {
        Clear(abilityParent);

        List<Ability> abilities = new List<Ability>(inspectedAbilityHandler.Abilities);

        for (int i = 0; i < abilities.Count; i++)
        {
            GameObject abilityButton = Instantiate(abilityButtonPrefab, abilityParent);
            AbilityButton buttonScript = abilityButton.GetComponent<AbilityButton>();

            buttonScript.Initialize(abilityManager.GetAbilityInfoByAbility(abilities[i]), abilities[i], inspectedLevelHandler);

            int index = i;
            buttonScript.LevelUpButton.onClick.AddListener(() =>
            {
                RefreshTowerWindow();
            });
        }
    }

    private void DisplayUpgradeButtons(TowerSO towerSO, int[] upgradeHistory, TowerUpgradeHandler upgradeHandler)
    {
        Clear(upgradeParent);

        if (upgradeHistory.Length == towerSO.TowerLevels.Length)
        {
            //All levels completed
            return;
        }

        // Create the upgrade buttons
        int levelIndex = upgradeHistory.Length;
        UpgradeOption[] towerUpgrades = towerSO.GetUpgradesAtIndex(levelIndex);

        if (towerUpgrades.Length == 0)
        {
            //All upgrades completed
            //fullyUpgraded.SetActive(true);
            //InitPathView(towerSO, levelIndex, upgradeHistory);
            return;
        }

        upgradeButtons = new List<Button>();
        for (int i = 0; i < towerUpgrades.Length; i++)
        {
            //GameObject upgradeButton = LevelManagement.PullObject(upgradeButtonPrefab, Vector3.zero, Quaternion.identity, true, upgradeSection.transform);
            GameObject upgradeButton = Instantiate(upgradeButtonPrefab, upgradeParent);
            upgradeButton.transform.localScale = Vector3.one;

            UpgradeButton upgradeButtonScript = upgradeButton.GetComponent<UpgradeButton>();
            upgradeButtonScript.Initalize(towerUpgrades[i]);
            upgradeButtons.Add(upgradeButtonScript.UpgradeBtn);

            int index = i;
            upgradeButtonScript.UpgradeBtn.onClick.AddListener(() =>
            {
                float cost = towerUpgrades[index].Cost;
                if (!TowerDefenseManager.instance.CashSystem.HasEnough(cost)) return;

                TowerDefenseManager.instance.CashSystem.Modify(ModifyType.Subtract, cost);

                upgradeHandler.UpgradeTower(index);
                RefreshTowerWindow();
            });
        }
    }

    private void SyncLevel(object sender, CurrencyEventArgs e)
    {
        levelSlider.fillAmount = inspectedLevelHandler.ExperienceHandler.Current / inspectedLevelHandler.CurrentExperienceLevelNeeded.amount;
        levelTMP.text = $"{inspectedLevelHandler.Level + 1}";
    }

    private void StatRefreshWindow(object sender, CurrencyEventArgs e)
    {
        DisplayTowerStat(inspectedTower);
        UnsubscribeTowerStatEvents(inspectedTower);
    }

    private void UnsubscribeTowerStatEvents(TowerBase tower)
    {
        tower.DamageHandler.OnModified -= StatRefreshWindow;
        tower.RangeHandler.OnModified -= StatRefreshWindow;
        tower.ReloadTimeHandler.OnModified -= StatRefreshWindow;
    }

    #endregion

    private void Clear(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }

    private void Show() 
    {
        if (isOpen) return;

        transform.DOMove(openPos.position, tweenTime).SetEase(easeType);
        //gameObject.transform.position = openPos.position;
        //LeanTween.move(gameObject, openPos.position, tweenTime).setEase(easeType).setOnComplete(() => transform.position = openPos.position); 
        isOpen = true;
    }
    
    private void Hide()
    {
        if (!isOpen) return;

        transform.DOMove(closePos.position, tweenTime).SetEase(easeType);
        Clear(statParent);

        isOpen = false;
    }
}
