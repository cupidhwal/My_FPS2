using UnityEngine;
using Unity.FPS.Game;

namespace Unity.FPS.Gameplay
{
    public class ChargedWeaponEffectHandler : MonoBehaviour
    {
        #region Variables
        public GameObject chargingObject;           // �����ϴ� �߻�ü
        public GameObject spinningFrame;            // �߻�ü�� ���ΰ� �ִ� ������
        public GameObject discOrbitParticlePrefab;  // �߻�ü�� ���ΰ� �ִ� ����Ʈ
        public MinMaxVector scale;                  // �߻�ü�� ũ��

        // VFX
        [SerializeField] private Vector3 offset;
        public Transform parentTransform;
        public MinMaxFloat orbitY;                  // ����Ʈ ������
        public MinMaxVector radius;                 // ����Ʈ ������
        public MinMaxFloat spinningSpeed;           // ȸ�� ������

        // SFX
        public AudioClip chargeSound;
        public AudioClip loopChargeWeaponSfx;

        private float fadeLoopDuration = 0.5f;
        [SerializeField] public bool useProceduralPitchOnLoop;

        public float maxProceduralPitchValue = 2.0f;

        private AudioSource audioSource;
        private AudioSource audioSourceLoop;

        //
        public GameObject ParticleInstance { get; private set; }
        private ParticleSystem discOrbitParticle;
        private ParticleSystem.VelocityOverLifetimeModule velocityOverLifetimeModule;

        private WeaponController weaponController;
        private float lastChargeTriggerTimeStamp;
        private float endChargeTime;
        private float chargeRatio;                  // ���� ������
        #endregion

        #region Life Cycle
        private void Update()
        {
            if (ParticleInstance == null)
                SpawnParticleSystem();

            discOrbitParticle.gameObject.SetActive(weaponController.IsWeaponActive);
            chargeRatio = weaponController.CurrentCharge;

            // disc, frame
            chargingObject.transform.localScale = scale.GetVectorFromRatio(chargeRatio);
            if (spinningFrame)
                spinningFrame.transform.localRotation *= Quaternion.Euler(0,
                                                                          spinningSpeed.GetValueFromRatio(chargeRatio) * Time.deltaTime,
                                                                          0);

            // particle
            velocityOverLifetimeModule.orbitalY = orbitY.GetValueFromRatio(chargeRatio);
            discOrbitParticle.transform.localScale = radius.GetVectorFromRatio(chargeRatio);

            // SFX
            if (chargeRatio > 0f)
            {
                if (audioSourceLoop.isPlaying == false &&
                    weaponController.lastChargeTriggerTimeStamp > lastChargeTriggerTimeStamp)
                {
                    lastChargeTriggerTimeStamp = weaponController.lastChargeTriggerTimeStamp;
                    if (useProceduralPitchOnLoop == false)
                    {
                        endChargeTime = Time.time + chargeSound.length;
                        audioSource.Play();
                    }
                    audioSourceLoop.Play();
                }

                if (useProceduralPitchOnLoop == false)  // �� ���� ���� ���̵� ȿ���� ���� ǥ��
                {
                    float volumeRatio = Mathf.Clamp01((endChargeTime - Time.time - fadeLoopDuration) / fadeLoopDuration);
                    audioSource.volume = volumeRatio;
                    audioSourceLoop.volume = 1f - volumeRatio;
                }
                else  // ���� ������ ����ӵ��� ���� ǥ��
                {
                    audioSourceLoop.pitch = Mathf.Lerp(1.0f, maxProceduralPitchValue, chargeRatio);
                }
            }
            else
            {
                audioSource.Stop();
                audioSourceLoop.Stop();
            }
        }

        private void Awake()
        {
            // ChargeSound Play
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = chargeSound;
            audioSource.playOnAwake = false;

            // LoopChargeWeaponSfx Play
            audioSourceLoop = gameObject.AddComponent<AudioSource>();
            audioSourceLoop.clip = loopChargeWeaponSfx;
            audioSourceLoop.playOnAwake = false;
            audioSourceLoop.loop = true;
        }
        #endregion

        #region Methods
        void SpawnParticleSystem()
        {
            ParticleInstance = Instantiate(discOrbitParticlePrefab,
                                           parentTransform != null ? parentTransform : transform);
            ParticleInstance.transform.localPosition += offset;

            FindReference();
        }

        void FindReference()
        {
            discOrbitParticle = ParticleInstance.GetComponent<ParticleSystem>();
            velocityOverLifetimeModule = discOrbitParticle.velocityOverLifetime;

            weaponController = GetComponent<WeaponController>();
        }
        #endregion
    }
}