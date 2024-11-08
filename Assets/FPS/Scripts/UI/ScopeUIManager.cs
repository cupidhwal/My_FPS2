using Unity.FPS.Gameplay;
using UnityEngine;

namespace Unity.FPS.UI
{
    public class ScopeUIManager : MonoBehaviour
    {
        #region Variables
        private PlayerWeaponsManager player;
        private CrossHairManager crossHairManager;
        public GameObject scopeUI;
        #endregion

        #region Life Cycle
        private void Start()
        {
            crossHairManager = GetComponent<CrossHairManager>();
            player = crossHairManager.Player;

            player.OnScopedWeapon += OnScope;
            player.OffScopedWeapon += OffScope;
        }

        private void OnDisable()
        {
            player.OnScopedWeapon -= OnScope;
            player.OffScopedWeapon -= OffScope;
        }
        #endregion

        #region Methods
        public void OnScope()
        {
            scopeUI.SetActive(true);
        }

        public void OffScope()
        {
            scopeUI.SetActive(false);
        }
        #endregion
    }
}