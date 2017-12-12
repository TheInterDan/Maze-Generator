using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {

    public float hSpeed=1;
    public float vSpeed=1;

    public int vMax = 40;
    public int vMin = -10;

    Transform pivotV;
    Vector2 mPos;

    void Update()
    {

        //forzar la actualización de la posición del ratón cuando el botón es pulsado
        if (Input.GetMouseButtonDown(0))
        {
            pivotV = transform.GetChild(0);
            mPos = Input.mousePosition;
        }

        //ratón pulsado
        if (Input.GetMouseButton(0))
        {
            //mover
            Vector2 movimiento = (Vector2)Input.mousePosition - mPos;
            transform.Rotate(0, movimiento.x * hSpeed, 0, Space.World);
            pivotV.Rotate(-movimiento.y * vSpeed, 0, 0, Space.Self);

            //calcular límite
            float vRotation = pivotV.rotation.eulerAngles.x;
            if (vRotation > vMax && vRotation < 360 + vMin)
            {
                if (Mathf.Abs(vMax - vRotation) < Mathf.Abs(360 - vMin - vRotation)) vRotation = vMax;
                else vRotation = vMin;
            }

            //aplicar límite
            pivotV.rotation = Quaternion.Euler
                (vRotation,
                transform.rotation.eulerAngles.y,
                0);
        }

        //actualizar la posición del ratón
        mPos = Input.mousePosition;

    }
}
