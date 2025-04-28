using BeachHero;
using System.Collections;
using UnityEngine;

public class CharacterHero : MonoBehaviour
{
    public float waitTime;

    private void OnEnable()
    {
        StartCoroutine(CountdownCoroutine());
    }

    IEnumerator CountdownCoroutine()
    {
        yield return new WaitForSeconds(waitTime);
        PoolManager.Instance.ReturnToPool(this);
        Debug.Log("Returned to pool.");
    }
}
