using System.Collections;
using UnityEngine;

public class CompletePanelView : MonoBehaviour
{
    private const float HideDelay = 2f;

    private Coroutine _hideCoroutine;

    private void OnEnable()
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        if (_hideCoroutine != null)
        {
            StopCoroutine(_hideCoroutine);
        }

        _hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private void OnDisable()
    {
        if (_hideCoroutine == null)
        {
            return;
        }

        StopCoroutine(_hideCoroutine);
        _hideCoroutine = null;
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(HideDelay);
        _hideCoroutine = null;
        gameObject.SetActive(false);
    }
}
