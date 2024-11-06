using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    /// 죽었을 때 Health를 가진 오브젝틀르 킬하는 클래스
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
            // Health 클래스를 반드시 넣도록 유도하는 디버그로그
            DebugUtility.HandleErrorIfNullGetComponent<Health, Destructable>(health, this, gameObject);

            // UnityAction 함수 등록
            health.OnDamaged += OnDamaged;
            health.OnDie += OnDie;
        }
        #endregion

        #region Methods
        void OnDamaged(float damage, GameObject damageSource)
        {
            // TODO : 데미지 효과 구현
        }

        void OnDie()
        {
            Destroy(gameObject);
        }
        #endregion
    }
}