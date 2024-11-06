using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    /// �������� �Դ� �浹ü�� �����Ǿ� �������� �����ϴ� Ŭ����
    /// </summary>
    public class Damagable : MonoBehaviour
    {
        #region Variables
        private Health health;
        [SerializeField] private float damageMultiplier = 1f;           // ������ ���
        [SerializeField] private float sensibilityToSelfDamage = 0.5f;  // ���� ������ ���
        #endregion

        #region Life Cycle
        private void Awake()
        {
            health = GetComponent<Health>();
            if (health == null) health = GetComponentInParent<Health>();
        }
        #endregion

        #region Methods
        public void InflictDamage(float damage, bool isExplosionDamage, GameObject damageSource)
        {
            if (health == null) return;

            // totalDamage: ���� ������
            var totalDamage = damage;

            // ���� ������ üũ - ���� �������� �� damageMultiplier�� ������� �ʴ´�
            if (!isExplosionDamage)
                totalDamage *= damageMultiplier;

            // �ڽ��� ���� ���������
            if (health.gameObject == damageSource)
                totalDamage *= sensibilityToSelfDamage;

            // ������ ������
            health.TakeDamage(totalDamage, damageSource);
        }
        #endregion
    }
}