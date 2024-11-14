using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.AI
{
    /// <summary>
    /// Patrol Waypoints�� �����ϴ� Ŭ����
    /// </summary>
    public class PatrolPath : MonoBehaviour
    {
        #region Variables
        public List<Transform> wayPoints = new();

        // this Path�� Patrol�ϴ� enemy ����Ʈ
        public List<EnemyController> enemiesToAssign = new();
        #endregion

        #region Life Cycle
        private void Start()
        {
            // ��ϵ� enemy���� ��Ʈ���� �н�(this) ����
            foreach (var enemy in enemiesToAssign)
            {
                enemy.PatrolPath = this;
            }
        }
        #endregion

        #region Methods
        // Ư��(enemy) ��ġ�κ��� ������ WayPoint���� �Ÿ� ���ϱ�
        public float GetDistanceToWayPoint(Vector3 origin, int wayPointIndex)
        {
            if (wayPointIndex < 0 ||
                wayPointIndex >= wayPoints.Count ||
                wayPoints[wayPointIndex] == null)
                return -1f;

            return (wayPoints[wayPointIndex].position - origin).magnitude;
        }

        // index�� ������ WayPoint�� ��ġ ��ȯ
        public Vector3 GetPositionOfWayPoint(int wayPointIndex)
        {
            if (wayPointIndex < 0 ||
                wayPointIndex >= wayPoints.Count ||
                wayPoints[wayPointIndex] == null)
                return Vector3.zero;

            return wayPoints[wayPointIndex].position;
        }
        #endregion

        #region Utilities
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < wayPoints.Count; i++)
            {
                int nextIndex = i + 1;
                if (nextIndex >= wayPoints.Count)
                    nextIndex -= wayPoints.Count;

                Gizmos.DrawLine(wayPoints[i].position, wayPoints[nextIndex].position);

                Gizmos.DrawSphere(wayPoints[i].position, 0.1f);
            }
        }
        #endregion
    }
}