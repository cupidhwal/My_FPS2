using Unity.FPS.Gameplay;
using Unity.FPS.UI;
using UnityEngine;

namespace Unity.FPS.Game
{
    public class WeaponHUDManager : MonoBehaviour
    {
        #region Variables
        public RectTransform ammoPanel;         // ammoCountUI 부모 오브젝트
        public GameObject ammoCountPrefab;      // ammoCountUI 프리팹

        private CrossHairManager crossHairManager;
        private PlayerWeaponsManager weaponsManager;
        #endregion

        #region Life Cycle
        private void Start()
        {
            crossHairManager = GetComponent<CrossHairManager>();
            weaponsManager = crossHairManager.Player;

            weaponsManager.OnAddedWeapon += AddWeapon;
            weaponsManager.OnRemoveWeapon += RemoveWeapon;
        }
        #endregion

        #region Methods
        void AddWeapon(WeaponController newWeapon, int weaponIndex)
        {
            GameObject ammoCountGo = Instantiate(ammoCountPrefab, ammoPanel);
            AmmoCount ammoCount = ammoCountGo.GetComponent<AmmoCount>();
            ammoCount.Initialize(newWeapon, weaponIndex);
        }

        void RemoveWeapon(WeaponController oldWeapon, int weaponIndex)
        {
            Destroy(oldWeapon);
        }

        void SwitchWeapon(WeaponController weapon)
        {
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(ammoPanel);
        }
        #endregion
    }
}