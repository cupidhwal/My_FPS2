using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    /// TimeSelfDestruct ������ ���� ������Ʈ�� ������ �����̿� �ڻ�
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