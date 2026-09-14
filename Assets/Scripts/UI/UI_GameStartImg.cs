using DG.Tweening;
using System.Collections;
using UnityEngine;

public class UI_GameStartImg : MonoBehaviour
{
    private void OnEnable()
    {
        DOTween.Kill(transform);
        transform.DOPunchScale(Vector3.one * 0.5f, 1f);
        StopAllCoroutines();
        StartCoroutine(SetDisableAfter3Sec_Cor());
    }

    private IEnumerator SetDisableAfter3Sec_Cor()
    {
        yield return CoroutineManager.GetWaitForSec(3);
        gameObject.SetActive(false);
    }
}
