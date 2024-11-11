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

    public enum WeaponShootType
    {
        Manual,
        Automatic,
        Charge,
        Sniper
    }

    /// <summary>
    /// 무기를 관리하는 클래스
    /// </summary>
    public class WeaponController : MonoBehaviour
    {
        #region Variables
        // 무기 활성화, 비활성화
        public GameObject weaponRoot;

        public GameObject Owner { get; set; }           // 무기의 주인
        public GameObject SourcePrefab { get; set; }    // 무기를 생성한 오리지널 프리팹
        public bool IsWeaponActive { get; set; }        // 무기 활성화 여부

        private AudioSource shootAudioSource;
        public AudioClip switchWeaponSfx;

        // Shooting
        public WeaponShootType shootType;

        [SerializeField] private float maxAmmo = 8f;    // 장전할 수 있는 최대 총탄
        private float currentAmmo;                      // 현재 총탄

        [SerializeField]
        private float delayBetweenShots = 0.5f;         // 발사 간격
        private float lastTimeShot;                     // 마지막으로 발사한 순간

        // VFX, SFX
        public Transform weaponMuzzle;                  // 총구 위치
        public GameObject muzzleFlashPrefab;            // 발사 효과
        public AudioClip shootSfx;                      // 발사 소리

        // CrossHair
        public CrossHairData crossHairDefault;          // 기본, 평상시
        public CrossHairData crossHairTargetInSight;    // 적을 포착했을 때

        // 조준
        public float aimZoomRatio = 1;
        public Vector3 aimOffset = Vector3.zero;

        // 반동
        public float recoilForce = 0.5f;

        // 발사체
        public ProjectileBase projectilePrefab;

        public Vector3 MuzzleWorldVelocity { get; private set; }
        private Vector3 lastMuzzlePosition;

        // Charge: 발사 버튼을 누르고 있으면 발사체의 데미지, 속도가 일정값까지 커진다
        public float CurrentCharge { get; private set; }
        public bool IsCharging { get; private set; }
        private float ammoUseOnStartCharge = 1f;    // 차지 시작 버튼을 누르기 위해 필요한 ammo 량
        private float ammoUsageRateWhileCharging = 1f;  // 차지하고 있는 동안 소비되는 ammo 량
        private float maxChargeDuration = 2f;           // 충전 시간 Max

        public float lastChargeTriggerTimeStamp;        // 충전 시작 시간

        [SerializeField] private int bulletsPerShot = 1;        // 한번 슛하는데 발사되는 탄환의 갯수
        [SerializeField] private float bulletSpreadAngle = 0f;  // 탄환이 퍼져나가는 각도

        public float CurrentAmmoRatio => currentAmmo / maxAmmo;
        #endregion

        #region Life Cycle
        private void Start()
        {
            // 초기화
            currentAmmo = maxAmmo;
            lastTimeShot = Time.time;
            lastMuzzlePosition = weaponMuzzle.position;
        }

        private void Update()
        {
            UpdateCharge();

            // MuzzleWorldVelocity
            if (Time.deltaTime > 0f)
            {
                MuzzleWorldVelocity = (weaponMuzzle.position - lastMuzzlePosition) / Time.deltaTime;

                lastMuzzlePosition = weaponMuzzle.position;
            }
        }

        private void Awake()
        {
            shootAudioSource = GetComponent<AudioSource>();
        }
        #endregion

        #region Methods
        void UpdateCharge()
        {
            if (IsCharging)
            {
                if (CurrentCharge < 1f)
                {
                    // 현재 남아 있는 충전량
                    float chargeLeft = 1f - CurrentCharge;
                    float chargeAdd;       // 이번 프레임에 충전할 양
                    if (maxChargeDuration <= 0f)
                    {
                        chargeAdd = chargeLeft; // 한번에 완전히 충전
                    }
                    else
                    {
                        chargeAdd = (1f / maxChargeDuration) * Time.deltaTime;
                    }
                    chargeAdd = Mathf.Clamp(chargeAdd, 0f, chargeLeft); // 남아있는 충전량보다 작아야 한다

                    //chargeAdd 만큼 Ammo 소비량을 구한다
                    float ammoThisChargeRequire = chargeAdd * ammoUsageRateWhileCharging;
                    if (ammoThisChargeRequire <= currentAmmo)
                    {
                        UseAmmo(ammoThisChargeRequire);
                        CurrentCharge = Mathf.Clamp01(CurrentCharge + chargeAdd);
                    }
                }
            }
        }

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

        // Fire
        public bool HandleShootInputs(bool inputDown, bool inputHeld, bool inputUp)
        {
            switch (shootType)
            {
                case WeaponShootType.Manual:
                    if (inputDown)
                    {
                        return TryShoot();
                    }
                    break;

                case WeaponShootType.Automatic:
                    if (inputHeld)
                    {
                        return TryShoot();
                    }
                    break;

                case WeaponShootType.Charge:
                    if (inputHeld)
                    {
                        // 충전 시작
                        TryBeginCharge();
                    }
                    if (inputUp)
                    {
                        // 충전 끝
                        if (TryReleaseCharge()) return TryShoot();
                    }
                    break;

                case WeaponShootType.Sniper:
                    if (inputDown)
                    {
                        return TryShoot();
                    }
                    break;
            }

            return false;
        }
        #endregion

        // 충전 시작
        void TryBeginCharge()
        {
            if (!IsCharging && currentAmmo >= ammoUseOnStartCharge
                && (lastTimeShot + delayBetweenShots) < Time.time)
            {
                UseAmmo(ammoUseOnStartCharge);

                lastChargeTriggerTimeStamp = Time.time;
                IsCharging = true;
            }
        }

        // 충전 끝 - 발사
        bool TryReleaseCharge()
        {
            if (IsCharging)
            {
                // 슛
                HandleShoot();

                // 초기화
                CurrentCharge = 0;
                IsCharging = false;
                return true;
            }

            return false;
        }

        void UseAmmo(float amount)
        {
            currentAmmo = Mathf.Clamp(currentAmmo - amount, 0f, maxAmmo);
            lastTimeShot = Time.time;
        }

        bool TryShoot()
        {
            if (currentAmmo >= 1f && (lastTimeShot + delayBetweenShots) < Time.time)
            {
                currentAmmo -= 1f;
                Debug.Log($"CurrentAmmo: {currentAmmo}");

                HandleShoot();

                return true;
            }
            return false;
        }

        // 슛 연출
        void HandleShoot()
        {
            // Projectile 생성
            for (int i = 0; i < bulletsPerShot; i++)
            {
                Vector3 shotDir = GetShotDirectionWithinSpread(weaponMuzzle);
                ProjectileBase projectileInstance = Instantiate(projectilePrefab, weaponMuzzle.position, Quaternion.LookRotation(shotDir));
                projectileInstance.Shoot(this);
            }

            // VFX
            if (muzzleFlashPrefab)
            {
                GameObject EffectGo = Instantiate(muzzleFlashPrefab, weaponMuzzle.position, weaponMuzzle.rotation, weaponMuzzle);
                Destroy(EffectGo, 2f);
            }

            // SFX
            if (shootSfx)
                shootAudioSource.PlayOneShot(shootSfx);

            // 발사 시점
            lastTimeShot = Time.time;
        }

        Vector3 GetShotDirectionWithinSpread(Transform shootTransform)
        {
            float spreadAngleRatio = bulletSpreadAngle / 180f;
            return Vector3.Lerp(shootTransform.forward, UnityEngine.Random.insideUnitSphere, spreadAngleRatio);
        }
    }
}