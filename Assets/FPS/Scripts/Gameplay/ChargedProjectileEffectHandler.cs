using UnityEngine;
using Unity.FPS.Game;

namespace Unity.FPS.Gameplay
{
    /// <summary>
    /// ������ �߻�ü�� �߻��� �� �������� ���� �߻�ü�� ������ ����
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