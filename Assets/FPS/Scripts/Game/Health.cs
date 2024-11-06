using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Game
{
    /// <summary>
    /// ü���� �����ϴ� Ŭ����
    /// </summary>
    public class Health : MonoBehaviour
    {
        #region Variables
        public float CurrentHealth { get; private set; }    // ���� HP
        [SerializeField] private float maxHealth = 100f;    // �ִ� HP
        private bool isDeath = false;                       // ���� üũ

        public UnityAction<float, GameObject> OnDamaged;    // �ǰ� ȿ��
        public UnityAction OnDie;                           // ���� ȿ��
        public UnityAction<float> OnHeal;                   // ȸ�� ȿ��

        // ���� ��ȣ ����
        [SerializeField] private float criticalHealthRatio = 0.3f;

        public bool Invincible { get; private set; }
        #endregion

        #region Life Cycle
        private void Start()
        {
            // �ʱ�ȭ
            CurrentHealth = maxHealth;
            Invincible = false;
        }
        #endregion

        #region Methods
        // �� ������ �Ⱦ� ���� ���� üũ
        public bool CanPickUp() => CurrentHealth < maxHealth;

        // UI HP ������ ��
        public float GetRatio() => CurrentHealth / maxHealth;

        // ���� üũ
        public bool IsCritical() => GetRatio() < criticalHealthRatio;

        // ��
        public void HealHealth(float heal)
        {
            if (isDeath || !CanPickUp()) return;

            float beforeHealth = CurrentHealth;
            CurrentHealth += heal;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0, maxHealth);

            // Real Damage ������
            float realHeal = CurrentHealth - beforeHealth;
            if (realHeal > 0)
            {
                // �� ȿ�� ����
                OnHeal?.Invoke(heal);
            }
        }

        // damageSource: �������� �ִ� ��ü
        public void TakeDamage(float damage, GameObject damageSource)
        {
            // ���� üũ
            if (Invincible || isDeath) return;

            float beforeHealth = CurrentHealth;
            CurrentHealth -= damage;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0, maxHealth);

            // Real Damage ������
            float realDamage = beforeHealth - CurrentHealth;
            if (realDamage > 0)
            {
                // ������ ȿ�� ����
                OnDamaged?.Invoke(realDamage, damageSource);
            }

            HandleDeath();
        }

        private void HandleDeath()
        {
            if (isDeath) return;

            if (CurrentHealth <= 0)
            {
                isDeath = true;
                OnDie?.Invoke();
            }
        }
        #endregion
    }
}