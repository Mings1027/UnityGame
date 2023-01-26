using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKController : MonoBehaviour
{
    [SerializeField] IKMover frontLeftLeg, frontRightLeg, backLeftLeg, backRightLeg;

    private void Awake()
    {
        StartCoroutine(LegCoroutine());
    }

    private IEnumerator LegCoroutine()
    {
        while (true)
        {
            do
            {
                frontLeftLeg.CheckMove();
                backRightLeg.CheckMove();
                yield return null;
            } while (backRightLeg.isMove || frontLeftLeg.isMove);

            do
            {
                frontRightLeg.CheckMove();
                backLeftLeg.CheckMove();
                yield return null;
            } while (backLeftLeg.isMove || frontRightLeg.isMove);
        }
    }
}
