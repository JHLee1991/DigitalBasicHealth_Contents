using UnityEngine;

public class MultiDisplayManager : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log($"[MultiDisplayManager] 감지된 디스플레이 개수: {Display.displays.Length}");
        // Display 1(index 0)은 기본적으로 활성화되어 있다.
        for (int i = 1; i < Display.displays.Length; i++)
        {
            Display.displays[i].Activate();
            Debug.Log($"[MultiDisplayManager] Display {i + 1} 활성화 완료");
        }
    }
}
