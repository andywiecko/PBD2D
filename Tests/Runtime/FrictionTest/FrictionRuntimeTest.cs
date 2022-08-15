using System.Collections;
using System.Collections.Generic;
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
            yield return new WaitForSeconds(5f);
        }
    }
}