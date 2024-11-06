using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Game
{
    /// <summary>
    /// 체력을 관리하는 클래스
    /// </summary>
    public class Health : MonoBehaviour
    {
        #region Variables
        public float CurrentHealth { get; private set; }    // 현재 HP
        [SerializeField] private float maxHealth = 100f;    // 최대 HP
        private bool isDeath = false;                       // 죽음 체크

        public UnityAction<float, GameObject> OnDamaged;    // 피격 효과
        public UnityAction OnDie;                           // 죽음 효과
        public UnityAction<float> OnHeal;                   // 회복 효과

        // 위험 신호 기준
        [SerializeField] private float criticalHealthRatio = 0.3f;

        public bool Invincible { get; private set; }
        #endregion

        #region Life Cycle
        private void Start()
        {
            // 초기화
            CurrentHealth = maxHealth;
            Invincible = false;
        }
        #endregion

        #region Methods
        // 힐 아이템 픽업 가능 여부 체크
        public bool CanPickUp() => CurrentHealth < maxHealth;

        // UI HP 게이지 값
        public float GetRatio() => CurrentHealth / maxHealth;

        // 위험 체크
        public bool IsCritical() => GetRatio() < criticalHealthRatio;

        // 힐
        public void HealHealth(float heal)
        {
            if (isDeath || !CanPickUp()) return;

            float beforeHealth = CurrentHealth;
            CurrentHealth += heal;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0, maxHealth);

            // Real Damage 데미지
            float realHeal = CurrentHealth - beforeHealth;
            if (realHeal > 0)
            {
                // 힐 효과 구현
                OnHeal?.Invoke(heal);
            }
        }

        // damageSource: 데미지를 주는 주체
        public void TakeDamage(float damage, GameObject damageSource)
        {
            // 무적 체크
            if (Invincible || isDeath) return;

            float beforeHealth = CurrentHealth;
            CurrentHealth -= damage;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0, maxHealth);

            // Real Damage 데미지
            float realDamage = beforeHealth - CurrentHealth;
            if (realDamage > 0)
            {
                // 데미지 효과 구현
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