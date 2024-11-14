using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Unity.FPS.AI
{
    /// <summary>
    /// ������ ������: ��Ƽ���� ���� ����
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
        public Material bodyMaterial;                           // ������ ȿ���� ��ü�� ��Ƽ����
        [GradientUsage(true)]
        public Gradient OnHitBodyGradient;                      // ������ ȿ���� �÷� �׶��̼����� ǥ��
        private List<RendererIndexData> bodyRenderer = new();   // bodyMaterial�� ������ �ִ� ������ ����Ʈ
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
            // ����
            Agent = GetComponent<NavMeshAgent>();
            health = GetComponent<Health>();

            health.OnDamaged += OnDamaged;
            health.OnDie += OnDie;

            // body Material�� ������ �ִ� ������ ���� ����Ʈ �����
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
            // ������ ȿ��
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
                // ��ϵ� �ǰ� �޼��� ȣ��
                Damaged?.Invoke();

                // �������� �� �ð�
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

        // Patrol�� ��ȿ����? Patrol�� ��������?
        private bool IsPathValid()
        {
            return PatrolPath && PatrolPath.wayPoints.Count > 0;
        }

        // ���� ����� WayPoint ã��
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

        // ��ǥ ������ ��ġ �� ������
        public Vector3 GetDestinationOnPath()
        {
            if (IsPathValid() == false)
                return this.transform.position;
            return PatrolPath.GetPositionOfWayPoint(pathDestinationIndex);
        }

        // ��ǥ ���� ���� - Nav �ý��� ����
        public void SetNavDestination(Vector3 destination)
        {
            if (Agent)
                Agent.SetDestination(destination);
        }

        // ���� ���� �� ���� ��ǥ���� ����
        public void UpdatePathDestination(bool inverseOrder = false)
        {
            if (IsPathValid() == false) return;

            // ���� ����
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