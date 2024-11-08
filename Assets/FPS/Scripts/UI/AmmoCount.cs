using UnityEngine;
using UnityEngine.UI;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using TMPro;

namespace Unity.FPS.UI
{
    /// <summary>
    /// WeaponController(무기)의 Ammo 카운트
    /// </summary>
    public class AmmoCount : MonoBehaviour
    {
        #region Variables
        private PlayerWeaponsManager weaponsManager;
        private WeaponController weaponController;
        private int weaponIndex;

        // UI
        public TextMeshProUGUI weaponIndexText;
        public Image ammoFillImage;
        [SerializeField] private float ammoFillSharpness = 10f;     // 게이지 채우는 속도
        private float weaponSwitchSharpness = 10f;                  // 무기 교체 시 UI가 바뀌는 속도

        public CanvasGroup canvasGroup;
        [SerializeField] [Range(0, 1)] private float unSelectedOpacity = 0.5f;
        private Vector3 unSelectedScale = Vector3.one * 0.8f;

        // 게이지바 색 변경
        private FillBarColorChange fillBarColorChange;
        #endregion

        #region Life Cycle
        private void Update()
        {
            float currentFillRate = weaponController.CurrentAmmoRatio;
            ammoFillImage.fillAmount = Mathf.Lerp(ammoFillImage.fillAmount,
                                                  currentFillRate,
                                                  ammoFillSharpness * Time.deltaTime);

            // 액티브 무기 구분
            bool isActiveWeapon = (weaponController == weaponsManager.GetActiveWeapon());
            float currentOpacity = isActiveWeapon ? 1.0f : unSelectedOpacity;
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha,
                                           currentOpacity,
                                           weaponSwitchSharpness * Time.deltaTime);
            Vector3 currentScale = isActiveWeapon ? Vector3.one : unSelectedScale;
            transform.localScale = Vector3.Lerp(transform.localScale,
                                                currentScale,
                                                weaponSwitchSharpness * Time.deltaTime);

            // 배경색 변경
            fillBarColorChange.UpdateVisual(currentFillRate);
        }
        #endregion

        #region Methods
        // AmmoCount UI 값 초기화
        public void Initialize(WeaponController weapon, int _weaponIndex)
        {
            weaponController = weapon;
            weaponIndex = _weaponIndex;

            // 무기 인덱스
            weaponIndexText.text = (weaponIndex + 1).ToString();

            // 게이지 색 값 초기화
            fillBarColorChange = GetComponent<FillBarColorChange>();
            fillBarColorChange.Initialize(1f, 0.1f);

            // 참조
            weaponsManager = GetComponentInParent<CrossHairManager>().Player;
        }
        #endregion
    }
}