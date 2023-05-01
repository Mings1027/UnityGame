using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test
{
    public virtual void AAA()
    {
        Debug.Log("Test  AAA");
        BBB();
    }

    public virtual void BBB()
    {
        Debug.Log("Test BBB");
    }
}

public class Test2 : Test
{
    public new void BBB()
    {
        Debug.Log("Test2  BBB");
    }
}

public class Test3 : Test
{
    public override void BBB()
    {
        Debug.Log("Test3   BBB");
    }
}