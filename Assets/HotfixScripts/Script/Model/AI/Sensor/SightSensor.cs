using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AI
{
    public class SightSensor : Sensor
    {
        [SerializeField]
        float range = 1.0f;
        [SerializeField]
        float angle = 60;
        [SerializeField]
        LayerMask layers;
        Collider[] triggers;
        private float time = 1f;
        private float timer = 0;

        private void Awake()
        {
            triggers = new Collider[20];
        }
        public void Update()
        {
            if(timer > time)
            {
                timer = 0;
                Check();
            }
            else
            {
                time += Time.deltaTime;
            }
        }
        void Check()
        {
            GetAllObject();
            switch (shapeType)
            {
                case ShapeType.Circle:
                    for (int i = 0; i < triggers.Length; i++)
                    {
                        var go = triggers[i];
                        Vector3 rayDirection = transform.position - go.transform.position;
                        rayDirection.y = 0;

                        if (Physics.Raycast(go.transform.position + new Vector3(0, 1, 0), rayDirection, out RaycastHit hit, range))
                        {
                            if (hit.collider.gameObject == this.gameObject)
                            {
                                //do something;
                            }
                        }
                        
                    }
                    break;
                case ShapeType.Rect:
                    break;
                case ShapeType.Sector:
                    for (int i = 0; i < triggers.Length; i++)
                    {
                        var go = triggers[i];
                        Vector3 rayDirection = transform.position - go.transform.position;
                        rayDirection.y = 0;
                        if (Vector3.Angle(rayDirection, go.transform.forward) < angle)
                        {
                            if (Physics.Raycast(go.transform.position + new Vector3(0, 1, 0), rayDirection, out RaycastHit hit, range))
                            {
                                if (hit.collider.gameObject == this.gameObject)
                                {
                                    //do something;
                                }
                            }
                        }
                    }
                    break;
            }
        }
        private void GetAllObject()
        {
            Physics.OverlapSphereNonAlloc(transform.position,range, triggers, layers);
        }
        public override void Notify(Trigger trigger)
        {
            Debug.Log("¿´µ½ÁË£º"+trigger.name);
        }
    }
}