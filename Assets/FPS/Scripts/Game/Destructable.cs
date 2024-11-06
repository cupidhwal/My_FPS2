using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    /// �׾��� �� Health�� ���� ������Ʋ�� ų�ϴ� Ŭ����
    /// </summary>
    public class Destructable : MonoBehaviour
    {
        #region Variables
        private Health health;
        #endregion

        #region Life Cycle
        private void Start()
        {
            health = GetComponent<Health>();
            // Health Ŭ������ �ݵ�� �ֵ��� �����ϴ� ����׷α�
            DebugUtility.HandleErrorIfNullGetComponent<Health, Destructable>(health, this, gameObject);

            // UnityAction �Լ� ���
            health.OnDamaged += OnDamaged;
            health.OnDie += OnDie;
        }
        #endregion

        #region Methods
        void OnDamaged(float damage, GameObject damageSource)
        {
            // TODO : ������ ȿ�� ����
        }

        void OnDie()
        {
            Destroy(gameObject);
        }
        #endregion
    }
}