using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    /// ���ذ��� �����ϴ� ������
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
    /// ���⸦ �����ϴ� Ŭ����
    /// </summary>
    public class WeaponController : MonoBehaviour
    {
        #region Variables
        // ���� Ȱ��ȭ, ��Ȱ��ȭ
        public GameObject weaponRoot;

        public GameObject Owner { get; set; }           // ������ ����
        public GameObject SourcePrefab { get; set; }    // ���⸦ ������ �������� ������
        public bool IsWeaponActive { get; set; }        // ���� Ȱ��ȭ ����

        private AudioSource shootAudioSource;
        public AudioClip switchWeaponSfx;

        // Shooting
        public WeaponShootType shootType;

        [SerializeField] private float maxAmmo = 8f;    // ������ �� �ִ� �ִ� ��ź
        private float currentAmmo;                      // ���� ��ź

        [SerializeField]
        private float delayBetweenShots = 0.5f;         // �߻� ����
        private float lastTimeShot;                     // ���������� �߻��� ����

        // VFX, SFX
        public Transform weaponMuzzle;                  // �ѱ� ��ġ
        public GameObject muzzleFlashPrefab;            // �߻� ȿ��
        public AudioClip shootSfx;                      // �߻� �Ҹ�

        // CrossHair
        public CrossHairData crossHairDefault;          // �⺻, ����
        public CrossHairData crossHairTargetInSight;    // ���� �������� ��

        // ����
        public float aimZoomRatio = 1;
        public Vector3 aimOffset = Vector3.zero;

        // �ݵ�
        public float recoilForce = 0.5f;

        // �߻�ü
        public ProjectileBase projectilePrefab;

        public Vector3 MuzzleWorldVelocity { get; private set; }
        private Vector3 lastMuzzlePosition;
        public float CurrentCharge { get; private set; }

        [SerializeField] private int bulletsPerShot = 1;        // �ѹ� ���ϴµ� �߻�Ǵ� źȯ�� ����
        [SerializeField] private float bulletSpreadAngle = 0f;  // źȯ�� ���������� ����

        public float CurrentAmmoRatio => currentAmmo / maxAmmo;
        #endregion

        #region Life Cycle
        private void Start()
        {
            // �ʱ�ȭ
            currentAmmo = maxAmmo;
            lastTimeShot = Time.time;
            lastMuzzlePosition = weaponMuzzle.position;
        }

        private void Update()
        {
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
        public void ShowWeapon(bool show)
        {
            weaponRoot.SetActive(show);

            // this ����� ����
            if (show)
            {
                // ���� ���� ȿ���� �÷���
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
                    if (inputUp)
                    {
                        return TryShoot();
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

        // �� ����
        void HandleShoot()
        {
            // Projectile ����
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

            // �߻� ����
            lastTimeShot = Time.time;
        }

        Vector3 GetShotDirectionWithinSpread(Transform shootTransform)
        {
            float spreadAngleRatio = bulletSpreadAngle / 180f;
            return Vector3.Lerp(shootTransform.forward, UnityEngine.Random.insideUnitSphere, spreadAngleRatio);
        }
    }
}