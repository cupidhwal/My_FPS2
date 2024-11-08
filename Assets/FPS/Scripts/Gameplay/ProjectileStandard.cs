using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.Game;

namespace Unity.FPS.Gameplay
{
    /// <summary>
    /// 발사체 표준형
    /// </summary>
    public class ProjectileStandard : ProjectileBase
    {
        #region Variables
        private ProjectileBase projectileBase;
        private float maxLifeTime = 5f;

        // 이동
        [SerializeField] private float speed = 20f;
        [SerializeField] private float gravityDown = 0f;
        public Transform root;
        public Transform tip;

        private Vector3 velocity;
        private Vector3 lastRootPosition;
        private float shotTime;

        // 충돌
        private float radius = 0.01f;               // 충돌을 검사하는 구체의 반경
        public LayerMask hittableLayers = -1;       // hit 가능 Layer
        private List<Collider> ignoredColliders;    // hit 판정 시 무시하는 충돌체 리스트

        // 충돌 연출
        public GameObject impactVfxPrefab;          // 타격 효과
        [SerializeField] private float impactVfxLifeTime = 1f;
        private float impactVfxSpawnOffset = 0.1f;
        
        public AudioClip impactSfxClip;             // 타격음
        #endregion

        private void OnEnable()
        {
            projectileBase = GetComponent<ProjectileBase>();
            projectileBase.OnShoot += OnShoot;
        }

        private void Update()
        {
            // 이동
            transform.position += velocity * Time.deltaTime;

            // 중력
            if (gravityDown > 0f)
            {
                velocity += gravityDown * Time.deltaTime * Vector3.down;
            }

            // 충돌
            RaycastHit raycastHit = new()
            {
                distance = Mathf.Infinity
            };
            RaycastHit closestHit = raycastHit;
            bool foundHit = false;

            // SphereCast
            Vector3 displacementSinceLastFrame = tip.position - lastRootPosition;
            RaycastHit[] hits = Physics.SphereCastAll(lastRootPosition,
                                                      radius,
                                                      displacementSinceLastFrame.normalized,
                                                      displacementSinceLastFrame.magnitude,
                                                      hittableLayers,
                                                      QueryTriggerInteraction.Collide);
            
            foreach (var hit in hits)
            {
                if (IsHitValid(hit) && hit.distance < closestHit.distance)
                {
                    closestHit = hit;
                    foundHit = true;
                }
            }

            if (foundHit)
            {
                if (closestHit.distance <= 0f)
                {
                    closestHit.point = root.position;
                    closestHit.normal = -transform.forward;
                }

                OnHit(closestHit.point, closestHit.normal, closestHit.collider);
            }

            lastRootPosition = root.position;
        }

        // 유효한 hit인지 판정
        bool IsHitValid(RaycastHit hit)
        {
            // IgnoreHitDetection 컴포넌트를 가진 콜라이더 무시
            if (hit.collider.GetComponent<IgnoreHitDetection>())
                return false;

            // ignoredColliders에 포함된 콜라이더 무시
            if (ignoredColliders != null && ignoredColliders.Contains(hit.collider))
                return false;

            // trigger collider && Damagable 컴포넌트가 없다면
            if (hit.collider.isTrigger && hit.collider.GetComponent<Damagable>() == null)
                return false;

            return true;
        }

        // Hit 구현, 데미지, VFX, SFX 등
        void OnHit(Vector3 point, Vector3 normal, Collider collider)
        {
            // VFX
            if (impactVfxPrefab)
            {
                GameObject impactGo = Instantiate(impactVfxPrefab,
                                                  point + (normal * impactVfxSpawnOffset),
                                                  Quaternion.LookRotation(normal));
                if (impactVfxLifeTime > 0)
                    Destroy(impactGo, impactVfxLifeTime);
            }

            // SFX
            if (impactSfxClip)
            {
                // 충돌 위치에 게임 오브젝트를 생성하고 AudioSource 컴포넌트를 추가해서 지정된 클립을 플레이한다
                AudioUtility.CreateSfx(impactSfxClip, point, 1f, 3f);
            }

            // 발사체 킬
            Destroy(gameObject);
        }

        // shoot 값 설정
        new void OnShoot()
        {
            velocity = transform.forward * speed;
            transform.position += projectileBase.InheritedMuzzleVelocity * Time.deltaTime;

            lastRootPosition = root.position;

            // 충돌을 무시할 콜라이더 리스트 생성 - projectile을 발사하는 자신의 충돌체를 가져와서 등록
            ignoredColliders = new();
            Collider[] ownerColliders = projectileBase.Owner.GetComponentsInChildren<Collider>();
            ignoredColliders.AddRange(ownerColliders);

            // Projectile이 벽을 뚫고 날아가는 버그 수정
            PlayerWeaponsManager weaponsManager = projectileBase.Owner.GetComponent<PlayerWeaponsManager>();
            if (weaponsManager)
            {
                Vector3 cameraToMuzzle = projectileBase.InitialPosition - weaponsManager.weaponCamera.transform.position;
                if (Physics.Raycast(weaponsManager.weaponCamera.transform.position,
                                    cameraToMuzzle.normalized,
                                    out RaycastHit hit,
                                    cameraToMuzzle.magnitude,
                                    hittableLayers,
                                    QueryTriggerInteraction.Collide))
                {
                    if (IsHitValid(hit))
                        OnHit(hit.point, hit.normal, hit.collider);
                }
            }
        }
    }
}