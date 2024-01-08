using System;

namespace Majiang
{
    public static partial class Util
    {
        /// <summary>
        /// 牌の数字を取得 (0-9)
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static int get_pai_rank(string p)
        {
            int number = 0;
            bool success = p.Length > 1 && int.TryParse(p[1].ToString(), out number);
            return success && number > 0 ? number : 5;
        }

        /// <summary>
        /// 最小値を返す
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static double min(params double[] values)
        {
            if (values == null || values.Length == 0)
            {
                throw new ArgumentException("values must contain at least one element");
            }

            double min = values[0];
            for (int i = 1; i < values.Length; i++)
            {
                min = Math.Min(min, values[i]);
            }

            return min;
        }

        /// <summary>
        /// 最大値を返す
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static double max(params double[] values)
        {
            if (values == null || values.Length == 0)
            {
                throw new ArgumentException("values must contain at least one element");
            }

            double max = values[0];
            for (int i = 1; i < values.Length; i++)
            {
                max = Math.Max(max, values[i]);
            }

            return max;
        }
    }
}