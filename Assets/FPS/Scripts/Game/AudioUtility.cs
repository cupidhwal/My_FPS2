using Unity.VisualScripting;
using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    /// ���� �÷��� ���� ��� ����
    /// </summary>
    public class AudioUtility : MonoBehaviour
    {
        // ������ ��ġ�� ���� ȿ�� �������� �����ϴ� ���� �޼���
        // Ŭ�� ���� �÷��̰� ������ �ڵ����� ���� - TimeSelfDestruction
        public static void CreateSfx(AudioClip clip, Vector3 position, float spartialBlend, float rollOffMinDistance = 1f)
        {
            GameObject impactSfxInstance = new();
            impactSfxInstance.transform.position = position;
            
            // �����Ŭ�� ���
            AudioSource source = impactSfxInstance.AddComponent<AudioSource>();
            source.clip = clip;
            source.spatialBlend = spartialBlend;
            source.minDistance = rollOffMinDistance;
            source.Play();

            // ������Ʈ ���� ų
            TimeSelfDestruct timeSelfDestruct = impactSfxInstance.AddComponent<TimeSelfDestruct>();
            timeSelfDestruct.lifeTime = clip.length;
        }
    }
}