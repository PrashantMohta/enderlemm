using Satchel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EnderLemm
{
    public  class rotate :MonoBehaviour
    {
        public Rigidbody2D rb2d;
        Vector3 destination;
        bool isRotating = false;
        int dir = 1;
        // Start is called before the first frame update
        void Update()
        {
            if (!isRotating)
            {
                //StartCoroutine(RotateMe(new Vector3(0f,0f,5f) * (dir) * 30f, 5f));
                if(rb2d == null) {
                    isRotating = true;
                    rb2d = gameObject.AddComponent<Rigidbody2D>();
                    rb2d.gravityScale = 0f;
                    destination = HeroController.instance.gameObject.transform.position;
                    StartCoroutine(MoveToPlayer());
                }
                dir = -dir;
            }
        }
        IEnumerator MoveToPlayer()
        {
           
            Vector2 force = destination - transform.position;
            force += new Vector2(UnityEngine.Random.Range(-0.01f, 0.01f), UnityEngine.Random.Range(-0.01f, 0.01f));
            yield return rb2d.moveTowards(force * 0.5f, -1f, 0.3f);
            yield return null;
            yield return rb2d.moveTowards(-force * 30f, -20f, 1f);
            gameObject.SetActive(false);
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
}
