using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace andywiecko.PBD2D.Tests
{
    public class FrictionRuntimeTest : RuntimeTest
    {
        [UnityTest]
        public IEnumerator Test()
        {
            yield return LoadSceneAsync();
            yield return new WaitForSeconds(2f);
        }
    }
}