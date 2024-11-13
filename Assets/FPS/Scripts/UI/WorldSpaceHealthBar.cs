using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    public class WorldSpaceHealthBar : MonoBehaviour
    {
        #region Variables
        [SerializeField]
        private bool hideFullHealthBar = true;

        public Health health;
        public Image healthBar;
        public GameObject healthBarPivot;
        #endregion

        #region Life Cycle
        private void Update()
        {
            ShowHealth();
        }
        #endregion

        #region Methods
        private void ShowHealth()
        {
            healthBarPivot.SetActive(!hideFullHealthBar || health.GetRatio() != 1);

            healthBarPivot.transform.localRotation = Quaternion.LookRotation(healthBarPivot.transform.position - Camera.main.transform.position);
            healthBar.fillAmount = health.GetRatio();
        }
        #endregion
    }
}