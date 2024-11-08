using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    public class CrossHairManager : MonoBehaviour
    {
        #region Variables
        public Image crossHairImage;
        public Sprite nullCrossHairImage;

        private RectTransform crossHairRect;
        private CrossHairData crossHairDefault;
        private CrossHairData crossHairTarget;

        private CrossHairData crossHairCurrent;
        [SerializeField] private float crossHairLerp = 0.2f;

        private PlayerWeaponsManager player;
        private bool wasPointEnemy = false;
        #endregion

        #region Properties
        public PlayerWeaponsManager Player => player;
        #endregion

        void Start()
        {
            crossHairRect = crossHairImage.rectTransform;

            player.OnSwitchToWeapon += OnWeaponChange;
        }

        void Update()
        {
            UpdateCross();

            wasPointEnemy = player.IsPointEnemy;
        }

        private void Awake()
        {
            player = FindFirstObjectByType<PlayerWeaponsManager>();
        }

        // 적을 포착했을 때
        void UpdateCross()
        {
            if (crossHairDefault.CrossHairSprite == null) return;

            // false+true - 적을 포착한 순간
            if (/*!*/wasPointEnemy/* && player.IsPointEnemy*/)
            {
                crossHairCurrent = crossHairTarget;
            }
            // true+false - 적을 놓치는 순간
            else/* if (wasPointEnemy && !player.IsPointEnemy)*/
            {
                crossHairCurrent = crossHairDefault;
            }

            // 평상시?
            crossHairImage.overrideSprite = crossHairCurrent.CrossHairSprite;
            crossHairImage.color = Color.Lerp(crossHairImage.color,
                                              crossHairCurrent.CrossHairColor,
                                              crossHairLerp * Time.deltaTime);
            crossHairRect.sizeDelta = Mathf.Lerp(crossHairRect.sizeDelta.x,
                                                 crossHairCurrent.CrossHairSize,
                                                 crossHairLerp * Time.deltaTime) * Vector2.one;
        }

        public void OnWeaponChange(WeaponController weapon)
        {
            if (weapon)
            {
                crossHairImage.enabled = true;
                crossHairDefault = weapon.crossHairDefault;
                crossHairTarget = weapon.crossHairTargetInSight;

                crossHairImage.overrideSprite = weapon.crossHairDefault.CrossHairSprite;
                /*crossHairRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, weapon.crossHairDefault.CrossHairSize);
                crossHairRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, weapon.crossHairDefault.CrossHairSize);*/
            }
            else
            {
                if (nullCrossHairImage)
                    crossHairImage.overrideSprite = nullCrossHairImage;
                else crossHairImage.enabled = false;
            }
        }
    }
}