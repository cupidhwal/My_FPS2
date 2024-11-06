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

        // ���� ����Ʈ�� �����ϴ� �ε���
        public int ActiveWeaponIndex { get; private set; }

        // ���� ��ü
        public UnityAction<WeaponController> OnSwitchToWeapon;  // ���� ��ü �� ��ϵ� �Լ� ȣ��

        private WeaponSwitchState weaponSwitchState;    // ���� ��ü �� ����

        private PlayerInputHandler playerInputHandler;

        // ���� ��ü �� ���Ǵ� ���� ��ġ
        private Vector3 weaponMainLocalPosition;
        public Transform defaultWeaponPosition;
        public Transform downWeaponPosition;

        private int weaponSwitchNewIndex;       // ���� �ٲ�� ���� �ε���
        private float weaponSwitchTimeStarted = 0f;
        [SerializeField] private float weaponSwitchDelay = 1f;
        #endregion

        private void Start()
        {
            // �ʱ�ȭ
            playerInputHandler = GetComponent<PlayerInputHandler>();
            ActiveWeaponIndex = -1;
            weaponSwitchState = WeaponSwitchState.Down;

            OnSwitchToWeapon += OnWeaponSwitched;

            // ���޹��� ���� ����
            foreach (var weapon in startingWeapons)
            {
                AddWeapon(weapon);
            }
            SwitchWeapon(true);
        }

        private void Update()
        {
            if (weaponSwitchState == WeaponSwitchState.Up ||
                weaponSwitchState == WeaponSwitchState.Down)
            {
                int switchWeaponInput = playerInputHandler.GetSwitchWeaponInput();
                if (switchWeaponInput != 0)
                {
                    bool switchUp = switchWeaponInput > 0;
                    SwitchWeapon(switchUp);
                }
            }
        }

        private void LateUpdate()
        {
            UpdateWeaponSwitching();

            weaponParentSocket.localPosition = weaponMainLocalPosition;
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
            else
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
                    weaponMainLocalPosition = downWeaponPosition.position;
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
    }
}