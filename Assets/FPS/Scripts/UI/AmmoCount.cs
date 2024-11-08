using UnityEngine;
using UnityEngine.UI;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using TMPro;

namespace Unity.FPS.UI
{
    /// <summary>
    /// WeaponController(����)�� Ammo ī��Ʈ
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
        [SerializeField] private float ammoFillSharpness = 10f;     // ������ ä��� �ӵ�
        private float weaponSwitchSharpness = 10f;                  // ���� ��ü �� UI�� �ٲ�� �ӵ�

        public CanvasGroup canvasGroup;
        [SerializeField] [Range(0, 1)] private float unSelectedOpacity = 0.5f;
        private Vector3 unSelectedScale = Vector3.one * 0.8f;

        // �������� �� ����
        private FillBarColorChange fillBarColorChange;
        #endregion

        #region Life Cycle
        private void Update()
        {
            float currentFillRate = weaponController.CurrentAmmoRatio;
            ammoFillImage.fillAmount = Mathf.Lerp(ammoFillImage.fillAmount,
                                                  currentFillRate,
                                                  ammoFillSharpness * Time.deltaTime);

            // ��Ƽ�� ���� ����
            bool isActiveWeapon = (weaponController == weaponsManager.GetActiveWeapon());
            float currentOpacity = isActiveWeapon ? 1.0f : unSelectedOpacity;
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha,
                                           currentOpacity,
                                           weaponSwitchSharpness * Time.deltaTime);
            Vector3 currentScale = isActiveWeapon ? Vector3.one : unSelectedScale;
            transform.localScale = Vector3.Lerp(transform.localScale,
                                                currentScale,
                                                weaponSwitchSharpness * Time.deltaTime);

            // ���� ����
            fillBarColorChange.UpdateVisual(currentFillRate);
        }
        #endregion

        #region Methods
        // AmmoCount UI �� �ʱ�ȭ
        public void Initialize(WeaponController weapon, int _weaponIndex)
        {
            weaponController = weapon;
            weaponIndex = _weaponIndex;

            // ���� �ε���
            weaponIndexText.text = (weaponIndex + 1).ToString();

            // ������ �� �� �ʱ�ȭ
            fillBarColorChange = GetComponent<FillBarColorChange>();
            fillBarColorChange.Initialize(1f, 0.1f);

            // ����
            weaponsManager = GetComponentInParent<CrossHairManager>().Player;
        }
        #endregion
    }
}