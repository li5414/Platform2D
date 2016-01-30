using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace druggedcode.extensions.collection
{
    public static class CollectionExtensions
    {
//        public static T PickRandom<T>(this IEnumerable<T> source)
//        {
//            return source.PickRandom(1).Single();
//        }
//        
//        public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count)
//        {
//            return source.Shuffle().Take(count);
//        }
//        
//        protected static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
//        {
//            return source.OrderBy(x => Guid.NewGuid());
//        }

        public static List<T> Shuffle<T>( this List<T> list, bool clone = true )
        {
            List<T> source;

            if( clone )
                source = new List<T>( list );
            else
                source = list;

            for( int i = 0; i < source.Count; i++ )
            {
                T temp = source[i];
                int randomIndex = UnityEngine.Random.Range(i, source.Count);
                source[i] = source[randomIndex];
                source[randomIndex] = temp;
            }

            return source;
        }

        public static T PickRandom<T>( this List<T> list )
        {
            List<T> clone = list.Shuffle();
            return (T) clone[0];
        }


        public static T[] Shuffle<T>( this T[] arr, bool clone = true )
        {
            T[] source;

            if( clone )
                source = (T[])arr.Clone();
            else
                source = arr;
            
            for( int i = 0; i < source.Length; i++ )
            {
                T temp = source[i];
                int randomIndex = UnityEngine.Random.Range(i, source.Length);
                source[i] = source[randomIndex];
                source[randomIndex] = temp;
            }
            
            return source;
        }

        public static T PickRandom<T>( this T[] arr )
        {
            T[] clone = arr.Shuffle();
            return (T) clone[0];
        }
    }
}

