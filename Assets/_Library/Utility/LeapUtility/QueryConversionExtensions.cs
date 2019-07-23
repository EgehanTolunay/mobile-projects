using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Khalreon.Generic;

namespace Khalreon.Extensions.Query
{
    public static class QueryConversionExtensions
    {

        /// <summary>
        /// Constructs a new Query from the given Collection.
        /// </summary>
        public static Query<T> Query<T>(this ICollection<T> collection)
        {
            return new Query<T>(collection);
        }

        /// <summary>
        /// Constructs a new Query from the given IEnumerable
        /// </summary>
        public static Query<T> Query<T>(this IEnumerable<T> enumerable)
        {
            List<T> list = LightPool<List<T>>.Spawn();
            try
            {
                list.AddRange(enumerable);
                return new Query<T>(list);
            }
            finally
            {
                list.Clear();
                LightPool<List<T>>.Recycle(list);
            }
        }

        /// <summary>
        /// Constructs a new Query from the given IEnumerator
        /// </summary>
        public static Query<T> Query<T>(this IEnumerator<T> enumerator)
        {
            List<T> list = LightPool<List<T>>.Spawn();
            try
            {
                while (enumerator.MoveNext())
                {
                    list.Add(enumerator.Current);
                }
                return new Query<T>(list);
            }
            finally
            {
                list.Clear();
                LightPool<List<T>>.Recycle(list);
            }
        }

        /// <summary>
        /// Constructs a new Query from the given two dimensional array.
        /// </summary>
        public static Query<T> Query<T>(this T[,] array)
        {
            var dst = ArrayPool<T>.Spawn(array.GetLength(0) * array.GetLength(1));
            int dstIndex = 0;
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    dst[dstIndex++] = array[i, j];
                }
            }

            return new Query<T>(dst, array.GetLength(0) * array.GetLength(1));
        }
    }



}

