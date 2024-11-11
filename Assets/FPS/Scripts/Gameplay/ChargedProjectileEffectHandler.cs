using UnityEngine;
using Unity.FPS.Game;

namespace Unity.FPS.Gameplay
{
    /// <summary>
    /// 충전용 발사체를 발사할 때 충전량에 따라 발사체의 스케일 결정
    /// </summary>
    public class ChargedProjectileEffectHandler : MonoBehaviour
    {
        #region Variables
        private ProjectileBase projectileBase;
        public GameObject chargeObject;
        public MinMaxVector scale;
        #endregion

        #region Life Cycle
        private void OnEnable()
        {
            projectileBase = GetComponent<ProjectileBase>();
            projectileBase.OnShoot += OnShoot;
        }
        #endregion

        void OnShoot()
        {
            chargeObject.transform.localScale = scale.GetVectorFromRatio(projectileBase.InitialCharge);
        }
    }
}