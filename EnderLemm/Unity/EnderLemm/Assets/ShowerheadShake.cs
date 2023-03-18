using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShoweheadShake : MonoBehaviour
{
    public Vector3 direction;
    bool isRotating = false;
    int dir = 1;
    // Start is called before the first frame update
    void Update()
    {
        if (!isRotating)
        {
            StartCoroutine(RotateMe(direction * (dir) *20f, 5f));

            dir = -dir;
        }
    }

    IEnumerator RotateMe(Vector3 byAngles, float inTime)
    {
        isRotating = true;
        var fromAngle = transform.rotation;
        var toAngle = Quaternion.Euler(transform.eulerAngles + byAngles);
        for (var t = 0f; t < 1; t += Time.deltaTime / inTime)
        {
            transform.rotation = Quaternion.Lerp(fromAngle, toAngle, t);
            yield return null;
        }
        isRotating = false;
    }


}
