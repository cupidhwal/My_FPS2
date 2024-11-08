using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.Game;

namespace Unity.FPS.Gameplay
{
    /// <summary>
    /// �߻�ü ǥ����
    /// </summary>
    public class ProjectileStandard : ProjectileBase
    {
        #region Variables
        private ProjectileBase projectileBase;
        private float maxLifeTime = 5f;

        // �̵�
        [SerializeField] private float speed = 20f;
        [SerializeField] private float gravityDown = 0f;
        public Transform root;
        public Transform tip;

        private Vector3 velocity;
        private Vector3 lastRootPosition;
        private float shotTime;

        // �浹
        private float radius = 0.01f;               // �浹�� �˻��ϴ� ��ü�� �ݰ�
        public LayerMask hittableLayers = -1;       // hit ���� Layer
        private List<Collider> ignoredColliders;    // hit ���� �� �����ϴ� �浹ü ����Ʈ

        // �浹 ����
        public GameObject impactVfxPrefab;          // Ÿ�� ȿ��
        [SerializeField] private float impactVfxLifeTime = 1f;
        private float impactVfxSpawnOffset = 0.1f;
        
        public AudioClip impactSfxClip;             // Ÿ����
        #endregion

        private void OnEnable()
        {
            projectileBase = GetComponent<ProjectileBase>();
            projectileBase.OnShoot += OnShoot;
        }

        private void Update()
        {
            // �̵�
            transform.position += velocity * Time.deltaTime;

            // �߷�
            if (gravityDown > 0f)
            {
                velocity += gravityDown * Time.deltaTime * Vector3.down;
            }

            // �浹
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

        // ��ȿ�� hit���� ����
        bool IsHitValid(RaycastHit hit)
        {
            // IgnoreHitDetection ������Ʈ�� ���� �ݶ��̴� ����
            if (hit.collider.GetComponent<IgnoreHitDetection>())
                return false;

            // ignoredColliders�� ���Ե� �ݶ��̴� ����
            if (ignoredColliders != null && ignoredColliders.Contains(hit.collider))
                return false;

            // trigger collider && Damagable ������Ʈ�� ���ٸ�
            if (hit.collider.isTrigger && hit.collider.GetComponent<Damagable>() == null)
                return false;

            return true;
        }

        // Hit ����, ������, VFX, SFX ��
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
                // �浹 ��ġ�� ���� ������Ʈ�� �����ϰ� AudioSource ������Ʈ�� �߰��ؼ� ������ Ŭ���� �÷����Ѵ�
                AudioUtility.CreateSfx(impactSfxClip, point, 1f, 3f);
            }

            // �߻�ü ų
            Destroy(gameObject);
        }

        // shoot �� ����
        new void OnShoot()
        {
            velocity = transform.forward * speed;
            transform.position += projectileBase.InheritedMuzzleVelocity * Time.deltaTime;

            lastRootPosition = root.position;

            // �浹�� ������ �ݶ��̴� ����Ʈ ���� - projectile�� �߻��ϴ� �ڽ��� �浹ü�� �����ͼ� ���
            ignoredColliders = new();
            Collider[] ownerColliders = projectileBase.Owner.GetComponentsInChildren<Collider>();
            ignoredColliders.AddRange(ownerColliders);

            // Projectile�� ���� �հ� ���ư��� ���� ����
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