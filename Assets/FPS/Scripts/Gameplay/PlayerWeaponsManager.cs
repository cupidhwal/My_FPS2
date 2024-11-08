using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Unity.FPS.Game;

namespace Unity.FPS.Gameplay
{
    public enum WeaponSwitchState
    {
        Up,
        Down,
        PutDownPrevious,
        PutUpNew
    }

    /// <summary>
    /// �÷��̾��� ���⸦ �����ϴ� Ŭ����
    /// </summary>
    public class PlayerWeaponsManager : MonoBehaviour
    {
        #region Variables
        // ���� ����
        public List<WeaponController> startingWeapons = new();

        // ���⸦ �����ϴ� ������Ʈ
        public Transform weaponParentSocket;

        // �÷��̾ ���� �߿� ��� �ٴϴ� ���� ����Ʈ
        private WeaponController[] weaponSlots = new WeaponController[9];

        private WeaponController activeWeapon;

        // ���� ����Ʈ�� �����ϴ� �ε���
        public int ActiveWeaponIndex { get; private set; }

        // ���� ��ü
        public UnityAction<WeaponController> OnSwitchToWeapon;      // ���� ��ü �� ��ϵ� �Լ� ȣ��
        public UnityAction<WeaponController, int> OnAddedWeapon;    // ���⸦ �߰��� ������ ��ϵ� �Լ� ȣ��
        public UnityAction<WeaponController, int> OnRemoveWeapon;   // ������ ���⸦ ������ ������ ��ϵ� �Լ� ȣ��

        private WeaponSwitchState weaponSwitchState;    // ���� ��ü �� ����

        private PlayerInputHandler playerInputHandler;

        // ���� ��ü �� ���Ǵ� ���� ��ġ
        private Vector3 weaponMainLocalPosition;
        public Transform defaultWeaponPosition;
        public Transform aimingWeaponPosition;
        public Transform downWeaponPosition;

        private int weaponSwitchNewIndex;       // ���� �ٲ�� ���� �ε���
        private float weaponSwitchTimeStarted = 0f;
        [SerializeField] private float weaponSwitchDelay = 1f;

        public bool IsPointEnemy { get; private set; }
        public Camera weaponCamera;

        private PlayerCharacterController playerCharacterController;
        [SerializeField] private float defaultFov = 60f;          // ī�޶� �⺻ FOV ���
        [SerializeField] private float weaponFovMultiplier;       // FOV ���� ���

        public bool IsAiming { get; private set; }                  // ���� ���� ����
        [SerializeField] private float aimingAnimationSpeed = 10f;  // ���� �̵�, Fov ���� �ӵ�

        [SerializeField] private float bobFrequency = 10f;
        [SerializeField] private float bobSharpness = 10f;
        [SerializeField] private float defaultBobAmount = 0.05f;     // ���� ��鸲
        [SerializeField] private float aimingBobAmount = 0.02f;      // ���ؽ� ��鸲

        private float weaponBobFactor;          // ��鸲 ���
        private Vector3 lastCharacterPosition;  // ���� �����ӿ����� �̵��ӵ��� ���ϱ� ���� ����
        private Vector3 weaponBobLocalPosition; // ��鸰 �� ���� ��갪, �̵����� ������ 0

        // �ݵ�
        [SerializeField] private float recoilSharpness = 50f;   // �ݵ� �ӵ�
        [SerializeField] private float maxRecoilDistance = 0.5f;// �ݵ� �� �ڷ� �и��� �ִ� �Ÿ�
        private float recoilRepositionSharpness = 10f;          // ���ڸ��� ���ư��� �ӵ�
        private Vector3 accumulateRecoil;                       // �ݵ� �� �ڷ� �и��� ����

        private Vector3 weaponRecoilLocalPosition;              // �ݵ� �� �̵��� ���� ����, �ݵ� �� ���ڸ��� ���ƿ��� 0

        private bool isScopeOn = false;
        [SerializeField] private float distanceOnScope = 0.1f;

        public UnityAction OnScopedWeapon;                      // ���� ��� ���� �� ��ϵ� �Լ� ȣ��
        public UnityAction OffScopedWeapon;                     // ���� ��� ���� �� ��ϵ� �Լ� ȣ��
        #endregion

        #region Life Cycle
        private void Start()
        {
            // ����
            playerCharacterController = GetComponent<PlayerCharacterController>();
            playerInputHandler = GetComponent<PlayerInputHandler>();
            
            // �ʱ�ȭ
            ActiveWeaponIndex = -1;
            weaponSwitchState = WeaponSwitchState.Down;
            OnSwitchToWeapon += OnWeaponSwitched;
            OnScopedWeapon += OnScope;
            OffScopedWeapon += OffScope;
            SetFov(defaultFov);

            // ���޹��� ���� ����
            foreach (var weapon in startingWeapons)
            {
                AddWeapon(weapon);
            }
            SwitchWeapon(true);
        }

        private void Update()
        {
            activeWeapon = GetActiveWeapon();

            IsAiming = playerInputHandler.GetAimInputHeld();

            if (weaponSwitchState == WeaponSwitchState.Up)
            {
                // ���� �Է°� ó��
                IsAiming = playerInputHandler.GetAimInputHeld();

                // ���� ��� ó��
                if (activeWeapon.shootType == WeaponShootType.Sniper)
                {
                    if (playerInputHandler.GetAimInputDown())
                    {
                        // ���� ��� ����
                        isScopeOn = true;
                        //OnScopedWeapon?.Invoke();
                    }
                    else if (playerInputHandler.GetAimInputUp())
                    {
                        // ���� ��� ����
                        OffScopedWeapon?.Invoke();
                    }
                }

                // �� ó��
                bool isFire = 
                activeWeapon.HandleShootInputs(playerInputHandler.GetFireInputDown(),
                                               playerInputHandler.GetFireInputHeld(),
                                               playerInputHandler.GetFireInputUp());

                if (isFire)
                {
                    // �ݵ� ȿ��
                    accumulateRecoil += Vector3.back * activeWeapon.recoilForce;
                    accumulateRecoil = Vector3.ClampMagnitude(accumulateRecoil, maxRecoilDistance);
                }
            }

            if (!IsAiming &&
                (weaponSwitchState == WeaponSwitchState.Up ||
                weaponSwitchState == WeaponSwitchState.Down))
            {
                int switchWeaponInput = playerInputHandler.GetSwitchWeaponInput();
                if (switchWeaponInput != 0)
                {
                    bool switchUp = switchWeaponInput > 0;
                    SwitchWeapon(switchUp);
                }
            }

            // �� ����
            IsPointEnemy = false;
            if (activeWeapon)
            {
                if (Physics.Raycast(weaponCamera.transform.position, weaponCamera.transform.forward, out RaycastHit hit, 300f))
                {
                    if (hit.transform.TryGetComponent<Health>(out var _))
                    {
                        IsPointEnemy = true;
                    }
                    else IsPointEnemy = false;
                }
            }
        }

        private void LateUpdate()
        {
            UpdateWeaponBob();
            UpdateWeaponRecoil();
            UpdateWeaponAiming();
            UpdateWeaponSwitching();

            weaponParentSocket.localPosition = weaponMainLocalPosition + 
                                               weaponBobLocalPosition + 
                                               weaponRecoilLocalPosition + 
                                               activeWeapon.aimOffset;
        }
        #endregion

        #region Methods
        void UpdateWeaponRecoil()
        {
            if (weaponRecoilLocalPosition.z >= accumulateRecoil.z * 0.99f)
            {
                weaponRecoilLocalPosition = Vector3.Lerp(weaponRecoilLocalPosition,
                                                         accumulateRecoil,
                                                         recoilSharpness * Time.deltaTime);
            }
            else
            {
                weaponRecoilLocalPosition = Vector3.Lerp(weaponRecoilLocalPosition,
                                                         Vector3.zero,
                                                         recoilRepositionSharpness * Time.deltaTime);
                accumulateRecoil = weaponRecoilLocalPosition;
            }
        }

        //
        private void SetFov(float fov)
        {
            playerCharacterController.PlayerCamera.fieldOfView = fov;
            weaponCamera.fieldOfView = fov * weaponFovMultiplier;
        }

        // ���� ���ؿ� ���� ����
        void UpdateWeaponAiming()
        {
            activeWeapon = GetActiveWeapon();

            // ���⸦ ��� ���� ���� ���� ����
            if (weaponSwitchState == WeaponSwitchState.Up)
            {
                if (IsAiming && activeWeapon)
                {
                    weaponMainLocalPosition = Vector3.Lerp(weaponMainLocalPosition,
                                                           aimingWeaponPosition.localPosition,
                                                           aimingAnimationSpeed * Time.deltaTime);

                    // ���� ��� ����
                    if (isScopeOn)
                    {
                        // ������ ��ġ�� �Ǹ� �׶� ����ī�޶� ��Ȱ��ȭ
                        float dist = Vector3.Distance(weaponMainLocalPosition, aimingWeaponPosition.localPosition);
                        if (dist < distanceOnScope)
                        {
                            OnScopedWeapon?.Invoke();
                            isScopeOn = false;
                        }
                    }
                    else
                    {
                        float fov = Mathf.Lerp(playerCharacterController.PlayerCamera.fieldOfView,
                                               activeWeapon.aimZoomRatio * defaultFov,
                                               aimingAnimationSpeed * Time.deltaTime);
                        SetFov(fov);
                    }
                }
                else
                {
                    weaponMainLocalPosition = Vector3.Lerp(weaponMainLocalPosition,
                                                           defaultWeaponPosition.localPosition,
                                                           aimingAnimationSpeed * Time.deltaTime);
                    float fov = Mathf.Lerp(playerCharacterController.PlayerCamera.fieldOfView,
                                           defaultFov,
                                           aimingAnimationSpeed * Time.deltaTime);
                    SetFov(fov);
                }
            }
        }

        // �̵��� ���� ���� ��鸰 �� ���ϱ�
        void UpdateWeaponBob()
        {
            if (Time.deltaTime > 0)
            {
                // ���� �����ӿ��� �÷��̾� �̵� �ӵ�
                Vector3 velocity = (playerCharacterController.transform.position - lastCharacterPosition) / Time.deltaTime;

                float characterMovementFactor = 0f;
                if (playerCharacterController.IsGrounded)
                {
                    characterMovementFactor = Mathf.Clamp01(velocity.magnitude
                    / (playerCharacterController.MaxSpeedOnGround * playerCharacterController.SprintSpeedModifier));
                }

                // �ӵ��� ���� ��鸲 ���
                weaponBobFactor = Mathf.Lerp(weaponBobFactor,
                                             characterMovementFactor,
                                             bobSharpness * Time.deltaTime);

                float bobAmount = (IsAiming) ? aimingBobAmount * weaponBobFactor : defaultBobAmount * weaponBobFactor;
                float frequency = bobFrequency;

                // ��鸲
                float hBobValue = Mathf.Sin(Time.time * frequency) * bobAmount * weaponBobFactor;
                float vBobValue = (Mathf.Sin(Time.time * frequency) / 2 + 0.5f) * bobAmount * weaponBobFactor;

                weaponBobLocalPosition.x = hBobValue;
                weaponBobLocalPosition.y = vBobValue;

                // �÷��̾��� ���� �������� ������ ��ġ�� ����
                lastCharacterPosition = playerCharacterController.transform.position;
            }
        }

        // ���¿� ���� ���� ����
        void UpdateWeaponSwitching()
        {
            // Lerp ����
            float switchingTimeFactor;
            if (weaponSwitchDelay == 0f)
            {
                switchingTimeFactor = 1f;
            }
            else
            {
                switchingTimeFactor = Mathf.Clamp01((Time.time - weaponSwitchTimeStarted) / weaponSwitchDelay);
            }

            if (switchingTimeFactor >= 1f)
            {
                if (weaponSwitchState == WeaponSwitchState.PutDownPrevious)
                {
                    // ���� ���� false, ���ο� ���� true
                    WeaponController oldWeapon = GetActiveWeapon();
                    if (oldWeapon != null)
                    {
                        oldWeapon.ShowWeapon(false);
                    }

                    ActiveWeaponIndex = weaponSwitchNewIndex;
                    WeaponController newWeapon = GetActiveWeapon();
                    OnSwitchToWeapon?.Invoke(newWeapon);

                    switchingTimeFactor = 0f;

                    if (newWeapon != null)
                    {
                        weaponSwitchTimeStarted = Time.time;
                        weaponSwitchState = WeaponSwitchState.PutUpNew;
                    }
                    else
                    {
                        weaponSwitchState = WeaponSwitchState.Down;
                    }
                }
                else if (weaponSwitchState == WeaponSwitchState.PutUpNew)
                {
                    weaponSwitchState = WeaponSwitchState.Up;
                }
            }

            // ���� �ð� ���� ������ ��ġ �̵�
            if (weaponSwitchState == WeaponSwitchState.PutDownPrevious)
            {
                weaponMainLocalPosition = Vector3.Lerp(defaultWeaponPosition.localPosition, downWeaponPosition.localPosition, switchingTimeFactor);
            }
            else if (weaponSwitchState == WeaponSwitchState.PutUpNew)
            {
                weaponMainLocalPosition = Vector3.Lerp(downWeaponPosition.localPosition, defaultWeaponPosition.localPosition, switchingTimeFactor);
            }
        }

        public bool AddWeapon(WeaponController weaponPrefab)
        {
            // �߰��ϴ� ���� ���� ���� üũ - �ߺ� �˻�
            if (HasWeapon(weaponPrefab) != null)
            {
                return false;
            }

            for (int i = 0; i < weaponSlots.Length; i++)
            {
                if (weaponSlots[i] == null)
                {
                    WeaponController weaponInstance = Instantiate(weaponPrefab, weaponParentSocket);
                    weaponInstance.transform.localPosition = Vector3.zero;
                    weaponInstance.transform.localRotation = Quaternion.identity;

                    weaponInstance.Owner = this.gameObject;
                    weaponInstance.SourcePrefab = weaponPrefab.gameObject;
                    weaponInstance.ShowWeapon(false);

                    // ���� ����
                    OnAddedWeapon?.Invoke(weaponInstance, i);

                    weaponSlots[i] = weaponInstance;

                    return true;
                }
            }

            return false;
        }

        // weaponSlots�� ������ ���� ����
        public bool RemoveWeapon(WeaponController oldWeapon)
        {
            for (int i = 0; i < weaponSlots.Length; i++)
            {
                // ���� ���� ã�Ƽ� ����
                if (weaponSlots[i] == oldWeapon)
                {
                    // ����
                    weaponSlots[i] = null;
                    OnRemoveWeapon?.Invoke(oldWeapon, i);
                    Destroy(oldWeapon.gameObject);

                    // ���� ������ ���Ⱑ Active�̸�
                    if (i == ActiveWeaponIndex)
                    {
                        SwitchWeapon(true);
                    }
                }
            }

            return false;
        }

        private WeaponController HasWeapon(WeaponController weaponPrefab)
        {
            for (int i = 0; i < weaponSlots.Length; i++)
            {
                if (weaponSlots[i] != null && weaponSlots[i].SourcePrefab == weaponPrefab)
                    return weaponSlots[i];
            }

            return null;
        }

        public WeaponController GetActiveWeapon()
        {
            return GetWeaponAtSlotIndex(ActiveWeaponIndex);
        }

        public WeaponController GetWeaponAtSlotIndex(int index)
        {
            if (index >= 0 && index < weaponSlots.Length)
            {
                return weaponSlots[index];
            }

            return null;
        }

        // ���� �ٲٱ�
        public void SwitchWeapon(bool ascendingOrder)
        {
            int newWeaponIndex = -1;
            int closestSlotDistance = weaponSlots.Length;
            for (int i = 0; i < weaponSlots.Length; i++)
            {
                if (i != ActiveWeaponIndex && GetWeaponAtSlotIndex(i) != null)
                {
                    int distanceToActiveIndex = GetDistanceBetweenWeaponSlot(ActiveWeaponIndex, i, ascendingOrder);
                    if (distanceToActiveIndex < closestSlotDistance)
                    {
                        closestSlotDistance = distanceToActiveIndex;
                        newWeaponIndex = i;
                    }
                }
            }

            SwitchToWeaponIndex(newWeaponIndex);
        }

        private void SwitchToWeaponIndex(int newWeaponIndex)
        {
            if (newWeaponIndex >= 0 && newWeaponIndex != ActiveWeaponIndex)
            {
                weaponSwitchNewIndex = newWeaponIndex;
                weaponSwitchTimeStarted = Time.time;

                // ���� Ȱ��ȭ�� ���Ⱑ �ִ���?
                if (GetActiveWeapon() == null)
                {
                    weaponMainLocalPosition = downWeaponPosition.localPosition;
                    weaponSwitchState = WeaponSwitchState.PutUpNew;
                    ActiveWeaponIndex = newWeaponIndex;

                    WeaponController weaponController = GetWeaponAtSlotIndex(newWeaponIndex);
                    OnSwitchToWeapon?.Invoke(weaponController);
                }
                else
                {
                    weaponSwitchState = WeaponSwitchState.PutDownPrevious;
                }
            }
        }

        // ���� �� �Ÿ�
        private int GetDistanceBetweenWeaponSlot(int fromSlotIndex, int toSlotIndex, bool ascendingOrder)
        {
            int distanceBetweenSlots = 0;
            if (ascendingOrder)
            {
                distanceBetweenSlots = toSlotIndex - fromSlotIndex;
            }
            else
            {
                distanceBetweenSlots = fromSlotIndex - toSlotIndex;
            }

            if (distanceBetweenSlots < 0)
            {
                distanceBetweenSlots = distanceBetweenSlots + weaponSlots.Length;
            }

            return distanceBetweenSlots;
        }

        void OnWeaponSwitched(WeaponController newWeapon)
        {
            if (newWeapon != null)
            {
                newWeapon.ShowWeapon(true);
            }
        }

        void OnScope()
        {
            weaponCamera.enabled = false;
        }

        void OffScope()
        {
            weaponCamera.enabled = true;
        }
        #endregion
    }
}