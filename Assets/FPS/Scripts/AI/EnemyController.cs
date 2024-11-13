using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.AI
{
    public class EnemyController : MonoBehaviour
    {
        #region Variables
        private Health health;
        [SerializeField]
        private GameObject deathVFXPrefab;
        public Transform deathVFXSpawnPosition;
        #endregion

        #region Life Cycle
        private void Start()
        {
            health = GetComponent<Health>();
            health.OnDamaged += OnDamaged;
            health.OnDie += OnDie;
        }
        #endregion

        #region Methods
        void OnDamaged(float damage, GameObject damageSource)
        {

        }

        void OnDie()
        {
            GameObject effectGo = Instantiate(deathVFXPrefab,
                                              deathVFXSpawnPosition.position,
                                              Quaternion.identity,
                                              deathVFXSpawnPosition);
            Destroy(effectGo, 2);
        }
        #endregion
    }
}