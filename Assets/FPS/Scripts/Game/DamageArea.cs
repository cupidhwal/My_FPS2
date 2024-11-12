using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    /// ���� ���� �ȿ� �ִ� �ݶ��̴� ������Ʈ ������ �ֱ�
    /// </summary>
    public class DamageArea : MonoBehaviour
    {
        #region Variables
        [SerializeField] private float areaOfEffectDistance = 10f;
        [SerializeField] private AnimationCurve damageRatioOverDistance;
        #endregion

        public void InflictDamageArea(float damage,
                                      Vector3 center,
                                      LayerMask layers,
                                      QueryTriggerInteraction interaction,
                                      GameObject owner)
        {
            Dictionary<Health, Damagable> uniqueDamagedHealth = new();

            Collider[] affectedColliders = Physics.OverlapSphere(center, areaOfEffectDistance, layers, interaction);
            foreach (Collider collider in affectedColliders)
            {
                if (collider.TryGetComponent<Damagable>(out var damagable))
                {
                    Health health = damagable.GetComponent<Health>();

                    if (health != null && !uniqueDamagedHealth.ContainsKey(health))
                    {
                        uniqueDamagedHealth.Add(health, damagable);
                    }
                }
            }

            Debug.Log(uniqueDamagedHealth.Count);

            // ������ �ֱ�
            foreach (var uniqueDamagable in uniqueDamagedHealth.Values)
            {
                float distance = Vector3.Distance(uniqueDamagable.transform.position, center);
                float curveDamage = damage * damageRatioOverDistance.Evaluate(distance / areaOfEffectDistance);
                Debug.Log($"curveDamage: {curveDamage}");
                uniqueDamagable.InflictDamage(curveDamage, true, owner);
            }
        }
    }
}