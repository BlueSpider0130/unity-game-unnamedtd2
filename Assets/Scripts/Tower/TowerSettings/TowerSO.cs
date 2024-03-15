using System;
using UnityEngine;

namespace TowerManagement
{
    [CreateAssetMenu(menuName = "ScriptableObjects/New Tower", fileName = "New Tower", order = 0)]
    public class TowerSO : ScriptableObject
    {
        [Serializable]
        public class UpgradeOption 
        {
            [Header("Next Upgrade Settings")]
            public Sprite nextUpgradeIcon;
            public float nextUpgradeCost;
            [TextArea(2, 5)] public string nextUpgradeDesc;
        }

        [Serializable]
        public class TowerLevel
        {
            [Header("Level Settings")]
            public string overrideName;
            public float sellCost;
            public Sprite towerIcon;
            public GameObject towerPrefab;

            public UpgradeOption[] upgradeOptions;
        }

        [SerializeField] private Sprite icon;
        [SerializeField] private Color bgColor;
        [SerializeField] private Color brColor;
        [SerializeField] private string towerName;
        [SerializeField] private float cost;
        [TextArea(5, 10)][SerializeField] private string towerDescription;

        [SerializeField] private TowerLevel[] towerLevels;

        public Sprite Icon => icon;
        public Color BGColor => bgColor;
        public Color BRColor => brColor;
        public string TowerName => towerName;
        public float Cost => cost;
        public string TowerDescription => towerDescription;
        public TowerLevel[] TowerLevels => towerLevels;

        private void OnValidate()
        {
            for (int i = 0; i < towerLevels.Length; i++)
            {
                if (towerLevels[i].overrideName == "")
                {
                    towerLevels[i].overrideName = towerName;
                }

                if (towerLevels[i].towerIcon == null)
                {
                    towerLevels[i].towerIcon = icon;
                }
            }
        }
    }
}