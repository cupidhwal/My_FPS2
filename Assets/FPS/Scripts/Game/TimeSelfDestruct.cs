using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    /// TimeSelfDestruct 부착한 게임 오브젝트는 지정된 딜레이에 자살
    /// </summary>
    public class TimeSelfDestruct : MonoBehaviour
    {
        #region Variables
        public float lifeTime;
        private float spawnTime;
        #endregion

        #region Life Cycle
        private void Awake()
        {
            spawnTime = Time.time;
        }

        private void Update()
        {
            if (spawnTime + lifeTime <= Time.time)
            {
                Destroy(gameObject);
            }
        }
        #endregion
    }
}