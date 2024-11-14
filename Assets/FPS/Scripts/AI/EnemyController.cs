using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Unity.FPS.AI
{
    /// <summary>
    /// 렌더러 데이터: 머티리얼 정보 저장
    /// </summary>
    [System.Serializable]
    public struct RendererIndexData
    {
        public Renderer renderer;
        public int materialIndex;

        public RendererIndexData(Renderer _renderer, int index)
        {
            renderer = _renderer;
            materialIndex = index;
        }
    }

    public class EnemyController : MonoBehaviour
    {
        #region Variables
        private Health health;

        // Death
        public GameObject deathVFXPrefab;
        public Transform deathVFXSpawnPosition;

        // Damage
        public UnityAction Damaged;

        // SFX
        public AudioClip damageSFX;

        // VFX
        public Material bodyMaterial;                           // 데미지 효과로 교체될 머티리얼
        [GradientUsage(true)]
        public Gradient OnHitBodyGradient;                      // 데미지 효과를 컬러 그라데이션으로 표현
        private List<RendererIndexData> bodyRenderer = new();   // bodyMaterial을 가지고 있는 렌더러 리스트
        MaterialPropertyBlock bodyFlashMaterialPropertyBlock;

        [SerializeField] private float flashOnHitDuration = 0.5f;
        
        float lastTimeDamaged = float.NegativeInfinity;
        bool wasDamagedThisFrame = false;

        // Patrol
        public NavMeshAgent Agent { get; private set; }
        public PatrolPath PatrolPath { get; set; }
        private int pathDestinationIndex;
        private float pathReachingRadius = 1.0f;
        #endregion

        #region Life Cycle
        private void Start()
        {
            // 참조
            Agent = GetComponent<NavMeshAgent>();
            health = GetComponent<Health>();

            health.OnDamaged += OnDamaged;
            health.OnDie += OnDie;

            // body Material을 가지고 있는 렌더러 정보 리스트 만들기
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in renderers)
            {
                for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    if (renderer.sharedMaterials[i] == bodyMaterial)
                    {
                        bodyRenderer.Add(new RendererIndexData(renderer, i));
                    }
                }
            }

            //
            bodyFlashMaterialPropertyBlock = new();
        }

        private void Update()
        {
            // 데미지 효과
            Color currentColor = OnHitBodyGradient.Evaluate((Time.time - lastTimeDamaged)/flashOnHitDuration);
            bodyFlashMaterialPropertyBlock.SetColor("_EmissionColor", currentColor);
            foreach (var data in bodyRenderer)
            {
                data.renderer.SetPropertyBlock(bodyFlashMaterialPropertyBlock, data.materialIndex);
            }

            //
            wasDamagedThisFrame = false;
        }
        #endregion

        #region Methods
        void OnDamaged(float damage, GameObject damageSource)
        {
            if (damageSource && damageSource.GetComponent<EnemyController>() == null)
            {
                // 등록된 피격 메서드 호출
                Damaged?.Invoke();

                // 데미지를 준 시간
                lastTimeDamaged = Time.time;

                // SFX
                if (damageSFX && wasDamagedThisFrame == false)
                    AudioUtility.CreateSfx(damageSFX, this.transform.position, 0f);
                wasDamagedThisFrame = true;
            }
        }

        void OnDie()
        {
            GameObject effectGo = Instantiate(deathVFXPrefab,
                                              deathVFXSpawnPosition.position,
                                              Quaternion.identity);
            Destroy(effectGo, 2);
        }

        // Patrol이 유효한지? Patrol이 가능한지?
        private bool IsPathValid()
        {
            return PatrolPath && PatrolPath.wayPoints.Count > 0;
        }

        // 가장 가까운 WayPoint 찾기
        private void SetPathDestinationToClosestWayPoint()
        {
            if (IsPathValid() == false)
            {
                pathDestinationIndex = 0;
                return;
            }

            int closestWayPointIndex = 0;

            for (int i = 0; i < PatrolPath.wayPoints.Count; i++)
            {
                float distance = PatrolPath.GetDistanceToWayPoint(transform.position, i);
                float closestDistance = PatrolPath.GetDistanceToWayPoint(transform.position, closestWayPointIndex);
                if (distance < closestDistance)
                {
                    closestWayPointIndex = i;
                }
            }
            pathDestinationIndex = closestWayPointIndex;
        }

        // 목표 지점의 위치 값 얻어오기
        public Vector3 GetDestinationOnPath()
        {
            if (IsPathValid() == false)
                return this.transform.position;
            return PatrolPath.GetPositionOfWayPoint(pathDestinationIndex);
        }

        // 목표 지점 설정 - Nav 시스템 설정
        public void SetNavDestination(Vector3 destination)
        {
            if (Agent)
                Agent.SetDestination(destination);
        }

        // 도착 판정 후 다음 목표지점 설정
        public void UpdatePathDestination(bool inverseOrder = false)
        {
            if (IsPathValid() == false) return;

            // 도착 판정
            float distance = Vector3.Distance(transform.position, GetDestinationOnPath());
            if (distance <= pathReachingRadius)
            {
                pathDestinationIndex = (inverseOrder) ? pathDestinationIndex - 1 : pathDestinationIndex + 1;
                if (pathDestinationIndex < 0)
                    pathDestinationIndex += PatrolPath.wayPoints.Count;
                if (pathDestinationIndex >= PatrolPath.wayPoints.Count)
                    pathDestinationIndex -= PatrolPath.wayPoints.Count;
            }
        }
        #endregion
    }
}