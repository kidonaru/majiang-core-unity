using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Majiang
{
    public static class ListExtensions
    {
        public static T Pop<T>(this List<T> list)
        {
            if (list.Count > 0)
            {
                T result = list[list.Count - 1];
                list.RemoveAt(list.Count - 1);
                return result;
            }
            else
            {
                throw new InvalidOperationException("Cannot pop from an empty list.");
            }
        }

        public static T Shift<T>(this List<T> list)
        {
            if (list.Count == 0) return default(T);

            var firstElement = list[0];
            list.RemoveAt(0);
            return firstElement;
        }

        public static T RemoveAtJS<T>(this List<T> list, int index)
        {
            try
            {
                T removedElement = list[index];
                list.RemoveAt(index);
                return removedElement;
            }
            catch (ArgumentOutOfRangeException e)
            {
                Debug.LogError(e);
                return default(T);
            }
        }

        public static List<T> Concat<T>(this List<T> list1)
        {
            if (list1 == null) return null;

            var result = new List<T>(list1);
            return result;
        }

        public static List<T> Concat<T>(this List<T> list1, List<T> list2)
        {
            if (list1 == null) return null;

            var result = new List<T>(list1);
            result.AddRange(list2);
            return result;
        }

        public static List<T> Clone<T>(this List<T> list) where T : ICloneable
        {
            var clone = new List<T>(list.Count);
            foreach (var item in list)
            {
                clone.Add((T)item.Clone());
            }
            return clone;
        }

        public static string JoinJS<T>(this IEnumerable<T> source, string separator = ",")
        {
            if (source == null) return "";
            return string.Join(separator, source);
        }

        public static List<T> Reversed<T>(this List<T> list)
        {
            var reversedList = new List<T>(list);
            reversedList.Reverse();
            return reversedList;
        }

        public static bool SafeSequenceEqual<T>(this List<T> list1, List<T> list2)
        {
            if (list1 == null && list2 == null)
                return true;

            if (list1 == null || list2 == null)
                return false;

            return list1.SequenceEqual(list2);
        }
    }

    public static class ArrayExtensions
    {
        public static void Clear<T>(this T[] array)
        {
            Array.Clear(array, 0, array.Length);
        }
    }

    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static string[] SplitJS(this string str, char separator)
        {
            if (string.IsNullOrEmpty(str))
            {
                return new string[] { "" };
            }
            else
            {
                return str.Split(separator);
            }
        }

        public static List<string> MatchJS(this string input, string pattern)
        {
            var matches = Regex.Matches(input, pattern);
            var result = new List<string>(matches.Count);
            foreach (Match match in matches)
            {
                result.Add(match.Value);
            }
            return result;
        }
    }
}
