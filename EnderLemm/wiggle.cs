using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EnderLemm
{
    public  class wiggle:MonoBehaviour
    {
        public Vector3 direction;
        public Quaternion orig;
        public bool isRotating = false;
        int dir = 1;

        void Awake()
        {
            orig = transform.rotation;
            isRotating = false;
        }
        // Start is called before the first frame update
        void Start()
        {
            isRotating = false;
        }
        void Update()
        {
            if (!isRotating)
            {
                StartCoroutine(RotateMe(direction * (dir) * 10f, 1f));
            }
        }

        void OnDisable()
        {
            transform.rotation=orig;
            isRotating = false;
        }

        IEnumerator RotateMe(Vector3 byAngles, float inTime)
        {
            isRotating = true;
            var fromAngle = orig;
            var toAngle = Quaternion.Euler(transform.eulerAngles + byAngles);
            for (var t = 0f; t < 1; t += Time.deltaTime / inTime)
            {
                transform.rotation = Quaternion.Lerp(fromAngle, toAngle, t);
                yield return null;
            }
            yield return new WaitForSeconds(0.1f);
            gameObject.SetActive(false);
            isRotating = false;
        }

    }
}
