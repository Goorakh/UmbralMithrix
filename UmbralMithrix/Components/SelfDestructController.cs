using System.Collections;
using UnityEngine;

namespace UmbralMithrix
{
    public class SelfDestructController : MonoBehaviour
    {
        private void Start()
        {
            StartCoroutine(SelfDestruct());
        }

        private IEnumerator SelfDestruct()
        {
            yield return new WaitForSeconds(ModConfig.CrushingLeap.Value);
            Destroy(gameObject);
        }
    }
}