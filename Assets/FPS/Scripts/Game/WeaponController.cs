using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    /// ���⸦ �����ϴ� Ŭ����
    /// </summary>
    public class WeaponController : MonoBehaviour
    {
        #region Variables
        // ���� Ȱ��ȭ, ��Ȱ��ȭ
        public GameObject weaponRoot;

        public bool IsWeaponActive { get; set; }        // ���� Ȱ��ȭ ����
        public GameObject Owner { get; set; }           // ������ ����
        public GameObject SourcePrefab { get; set; }    // ���⸦ ������ �������� ������

        private AudioSource shootAudioSource;
        public AudioClip switchWeaponSfx;
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

            // this ����� ����
            if (show)
            {
                // ���� ���� ȿ���� �÷���
                shootAudioSource.PlayOneShot(switchWeaponSfx);
            }
            IsWeaponActive = show;
        }
        #endregion
    }
}