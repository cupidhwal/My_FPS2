using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    /// <summary>
    /// ���������� ��������, ��׶���� ���� ����
    /// </summary>
    public class FillBarColorChange : MonoBehaviour
    {
        #region Variables
        public Image foregroundImage;
        public Color defaultForegroundImage;        // �������� �⺻ �÷�
        public Color flashForeGroundColorFull;      // �������� ���� ���� ���� �� Flash

        public Image backgroundImage;
        public Color defaultBackgroundColor;        // ��׶��� �⺻ �÷�
        public Color flashBackgroundColorEmpty;     // ��׶��� ���������� 0�� �� �÷���

        private float fullValue = 1f;               // �������� Ǯ�� ���� ��
        private float emptyValue = 0f;              // �������� ������ ���� ��

        private float colorChangeSharpness = 5f;    // �÷� ���� �ӵ�
        private float previousValue;                // �������� ���� ���� ������ ã�� ����
        #endregion

        #region Methods
        // �� ���� ���� �� �ʱ�ȭ
        public void Initialize(float fullValueRatio, float emptyValueRatio)
        {
            fullValue = fullValueRatio;
            emptyValue = emptyValueRatio;

            previousValue = fullValue;
        }

        public void UpdateVisual(float currentRatio)
        {
            // �������� Ǯ�� ���� ����
            if (currentRatio == fullValue && currentRatio != previousValue)
            {
                foregroundImage.color = flashForeGroundColorFull;
            }
            else if (currentRatio <= emptyValue)
            {
                backgroundImage.color = flashBackgroundColorEmpty;
            }
            else
            {
                foregroundImage.color = Color.Lerp(foregroundImage.color,
                                                   defaultForegroundImage,
                                                   colorChangeSharpness * Time.deltaTime);

                backgroundImage.color = Color.Lerp(backgroundImage.color,
                                                   defaultBackgroundColor,
                                                   colorChangeSharpness * Time.deltaTime);
            }

            previousValue = currentRatio;
        }
        #endregion
    }
}