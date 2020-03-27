using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Util {

	public static void SetLayerRecursively(GameObject gameObject, int layerIndex)
    {
        if (gameObject == null)
        {
            return;
        }

        gameObject.layer = layerIndex;

        foreach (Transform child in gameObject.transform)
        {
            if (child == null)
            {
                // Skip this iteration and continue further
                continue;
            }

            SetLayerRecursively(child.gameObject, layerIndex);
        }
    }
}
