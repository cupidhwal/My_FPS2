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
    /// 플레이어의 무기를 관리하는 클래스
    /// </summary>
    public class PlayerWeaponsManager : MonoBehaviour
    {
        #region Variables
        // 무기 지급
        public List<WeaponController> startingWeapons = new();

        // 무기를 장착하는 오브젝트
        public Transform weaponParentSocket;

        // 플레이어가 게임 중에 들고 다니는 무기 리스트
        private WeaponController[] weaponSlots = new WeaponController[9];

        // 무기 리스트를 관리하는 인덱스
        public int ActiveWeaponIndex { get; private set; }

        // 무기 교체
        public UnityAction<WeaponController> OnSwitchToWeapon;  // 무기 교체 시 등록된 함수 호출

        private WeaponSwitchState weaponSwitchState;    // 무기 교체 시 상태

        private PlayerInputHandler playerInputHandler;

        // 무기 교체 시 계산되는 최종 위치
        private Vector3 weaponMainLocalPosition;
        public Transform defaultWeaponPosition;
        public Transform aimingWeaponPosition;
        public Transform downWeaponPosition;

        private int weaponSwitchNewIndex;       // 새로 바뀌는 무기 인덱스
        private float weaponSwitchTimeStarted = 0f;
        [SerializeField] private float weaponSwitchDelay = 1f;

        public bool IsPointEnemy { get; private set; }
        public Camera weaponCamera;

        private PlayerCharacterController playerCharacterController;
        [SerializeField] private float defaultFov = 60f;          // 카메라 기본 FOV 계수
        [SerializeField] private float weaponFovMultiplier;       // FOV 연산 계수

        public bool IsAiming { get; private set; }                  // 무기 조준 여부
        [SerializeField] private float aimingAnimationSpeed = 10f;  // 무기 이동, Fov 연출 속도

        [SerializeField] private float bobFrequency = 10f;
        [SerializeField] private float bobSharpness = 10f;
        [SerializeField] private float defaultBobAmount = 0.05f;     // 평상시 흔들림
        [SerializeField] private float aimingBobAmount = 0.02f;      // 조준시 흔들림

        private float weaponBobFactor;          // 흔들림 계수
        private Vector3 lastCharacterPosition;  // 현재 프레임에서의 이동속도를 구하기 위한 변수
        private Vector3 weaponBobLocalPosition; // 흔들린 양 최종 계산값, 이동하지 않으면 0
        #endregion

        #region Life Cycle
        private void Start()
        {
            // 참조
            playerCharacterController = GetComponent<PlayerCharacterController>();
            playerInputHandler = GetComponent<PlayerInputHandler>();
            
            // 초기화
            ActiveWeaponIndex = -1;
            weaponSwitchState = WeaponSwitchState.Down;

            OnSwitchToWeapon += OnWeaponSwitched;

            SetFov(defaultFov);

            // 지급받은 무기 장착
            foreach (var weapon in startingWeapons)
            {
                AddWeapon(weapon);
            }
            SwitchWeapon(true);
        }

        private void Update()
        {
            WeaponController activeWeapon = GetActiveWeapon();

            IsAiming = playerInputHandler.GetAimInputHeld();

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

            // 적 포착
            IsPointEnemy = false;
            if (activeWeapon)
            {
                if (Physics.Raycast(weaponCamera.transform.position, weaponCamera.transform.forward, out RaycastHit hit, 300))
                {
                    if (hit.transform.TryGetComponent<Health>(out var health))
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
            UpdateWeaponAiming();
            UpdateWeaponSwitching();

            weaponParentSocket.localPosition = weaponMainLocalPosition + weaponBobLocalPosition;
        }
        #endregion

        #region Methods
        //
        private void SetFov(float fov)
        {
            playerCharacterController.PlayerCamera.fieldOfView = fov;
            weaponCamera.fieldOfView = fov * weaponFovMultiplier;
        }

        // 무기 조준에 따른 연출
        void UpdateWeaponAiming()
        {
            WeaponController activeWeapon = GetActiveWeapon();

            // 무기를 들고 있을 때만 조준 가능
            if (weaponSwitchState == WeaponSwitchState.Up)
            {
                if (IsAiming && activeWeapon)
                {
                    weaponMainLocalPosition = Vector3.Lerp(weaponMainLocalPosition,
                                                           aimingWeaponPosition.localPosition,
                                                           aimingAnimationSpeed * Time.deltaTime);
                    float fov = Mathf.Lerp(playerCharacterController.PlayerCamera.fieldOfView,
                                           activeWeapon.aimZoomRatio * defaultFov,
                                           aimingAnimationSpeed * Time.deltaTime);
                    SetFov(fov);
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

        // 이동에 의한 무기 흔들린 값 구하기
        void UpdateWeaponBob()
        {
            if (Time.deltaTime > 0)
            {
                // 현재 프레임에서 플레이어 이동 속도
                Vector3 velocity = (playerCharacterController.transform.position - lastCharacterPosition) / Time.deltaTime;

                float characterMovementFactor = 0f;
                if (playerCharacterController.IsGrounded)
                {
                    characterMovementFactor = Mathf.Clamp01(velocity.magnitude
                    / (playerCharacterController.MaxSpeedOnGround * playerCharacterController.SprintSpeedModifier));
                }

                // 속도에 의한 흔들림 계수
                weaponBobFactor = Mathf.Lerp(weaponBobFactor,
                                             characterMovementFactor,
                                             bobSharpness * Time.deltaTime);

                float bobAmount = (IsAiming) ? aimingBobAmount * weaponBobFactor : defaultBobAmount * weaponBobFactor;
                float frequency = bobFrequency;

                // 흔들림
                float hBobValue = Mathf.Sin(Time.time * frequency) * bobAmount * weaponBobFactor;
                float vBobValue = (Mathf.Sin(Time.time * frequency) / 2 + 0.5f) * bobAmount * weaponBobFactor;

                weaponBobLocalPosition.x = hBobValue;
                weaponBobLocalPosition.y = vBobValue;

                // 플레이어의 현재 프레임의 마지막 위치를 저장
                lastCharacterPosition = playerCharacterController.transform.position;
            }
        }

        // 상태에 따른 무기 연출
        void UpdateWeaponSwitching()
        {
            // Lerp 변수
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
                    // 현재 무기 false, 새로운 무기 true
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

            // 지연 시간 동안 무기의 위치 이동
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
            // 추가하는 무기 소지 여부 체크 - 중복 검사
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

                    weaponSlots[i] = weaponInstance;

                    return true;
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

        // 무기 바꾸기
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

                // 현재 활성화된 무기가 있는지?
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

        // 슬롯 간 거리
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
        #endregion
    }
}