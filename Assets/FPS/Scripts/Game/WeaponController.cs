using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    /// 조준경을 관리하는 데이터
    /// </summary>
    [System.Serializable]
    public struct CrossHairData
    {
        public Sprite CrossHairSprite;
        public Color CrossHairColor;
        public float CrossHairSize;
    }

    /// <summary>
    /// 무기를 관리하는 클래스
    /// </summary>
    public class WeaponController : MonoBehaviour
    {
        #region Variables
        // 무기 활성화, 비활성화
        public GameObject weaponRoot;

        public bool IsWeaponActive { get; set; }        // 무기 활성화 여부
        public GameObject Owner { get; set; }           // 무기의 주인
        public GameObject SourcePrefab { get; set; }    // 무기를 생성한 오리지널 프리팹

        private AudioSource shootAudioSource;
        public AudioClip switchWeaponSfx;

        // CrossHair
        public CrossHairData crossHairDefault;          // 기본, 평상시
        public CrossHairData crossHairTargetInSight;    // 적을 포착했을 때

        public float aimZoomRatio = 1;
        public Vector3 aimOffset = Vector3.zero;
        #endregion

        #region Life Cycle
        private void Awake()
        {
            shootAudioSource = GetComponent<AudioSource>();
        }
        #endregion

        #region Methods
        public void ShowWeapon(bool show)
        {
            weaponRoot.SetActive(show);

            // this 무기로 변경
            if (show)
            {
                // 무기 변경 효과음 플레이
                shootAudioSource.PlayOneShot(switchWeaponSfx);
            }
            IsWeaponActive = show;
        }
        #endregion
    }
}