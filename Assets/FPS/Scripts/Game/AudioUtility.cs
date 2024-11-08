using Unity.VisualScripting;
using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    /// 사운드 플레이 관련 기능 구현
    /// </summary>
    public class AudioUtility : MonoBehaviour
    {
        // 지정된 위치에 사운드 효과 프리팹을 생성하는 정적 메서드
        // 클립 사운드 플레이가 끝나면 자동으로 제거 - TimeSelfDestruction
        public static void CreateSfx(AudioClip clip, Vector3 position, float spartialBlend, float rollOffMinDistance = 1f)
        {
            GameObject impactSfxInstance = new();
            impactSfxInstance.transform.position = position;
            
            // 오디오클립 재생
            AudioSource source = impactSfxInstance.AddComponent<AudioSource>();
            source.clip = clip;
            source.spatialBlend = spartialBlend;
            source.minDistance = rollOffMinDistance;
            source.Play();

            // 오브젝트 셀프 킬
            TimeSelfDestruct timeSelfDestruct = impactSfxInstance.AddComponent<TimeSelfDestruct>();
            timeSelfDestruct.lifeTime = clip.length;
        }
    }
}