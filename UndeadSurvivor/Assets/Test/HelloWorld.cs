using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class HelloWorld : MonoBehaviour
{
}

public class Person
{
    public virtual string SayHi()
    {
        return "Hi";
    }
}

public class Korean : Person
{
    public override string SayHi()
    {
        return "안녕";
    }
}

public class Canadian : Person
{
    public override string SayHi()
    {
        return "Hello";
    }
}