using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Khalreon.Generic
{
    public static class Type<T>
    {
        public static readonly bool isValueType;

        static Type()
        {
            isValueType = typeof(T).IsValueType;
        }
    }
}

