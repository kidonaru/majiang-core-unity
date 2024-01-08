using System;
using System.Collections.Generic;

namespace Majiang
{
    /// <summary>
    /// 聴牌数を計算するためのユーティリティ
    /// </summary>
    public static partial class Util
    {
        /// <summary>
        /// 聴牌数を計算します
        /// </summary>
        /// <param name="m"></param>
        /// <param name="d"></param>
        /// <param name="g"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        private static int _xiangting(int m, int d, int g, bool j)
        {
            int n = j ? 4 : 5;
            if (m > 4) { d += m - 4; m = 4; }
            if (m + d > 4) { g += m + d - 4; d = 4 - m; }
            if (m + d + g > n) { g = n - m - d; }
            if (j) d++;
            return 13 - m * 3 - d * 2 - g;
        }

        public class MianziResult : EntityBase
        {
            /// <summary>
            /// 面子の数、対子の数、孤立牌の数 (面子の数が優先)
            /// </summary>
            public List<int> a;
            /// <summary>
            /// 面子の数、対子の数、孤立牌の数　(対子の数が優先)
            /// </summary>
            public List<int> b;

            /// <summary>
            /// 各要素に対してactionを適用します
            /// </summary>
            /// <param name="action"></param>
            public void apply_action(Action<List<int>> action)
            {
                action(a);
                action(b);
            }

            public override string ToString()
            {
                return $"MianziResult(a: {a.JoinJS()} b: {b.JoinJS()})";
            }
        }

        /// <summary>
        /// 手牌から対子と孤立牌の数を計算
        /// </summary>
        /// <param name="bingpai"></param>
        /// <returns></returns>
        public static MianziResult dazi(int[] bingpai)
        {
            int n_pai = 0, n_dazi = 0, n_guli = 0;

            for (int n = 1; n <= 9; n++)
            {
                n_pai += bingpai[n];
                if (n <= 7 && bingpai[n + 1] == 0 && bingpai[n + 2] == 0)
                {
                    n_dazi += n_pai >> 1;
                    n_guli += n_pai % 2;
                    n_pai = 0;
                }
            }
            n_dazi += n_pai >> 1;
            n_guli += n_pai % 2;

            return new MianziResult {
                a = new List<int> { 0, n_dazi, n_guli },
                b = new List<int> { 0, n_dazi, n_guli }
            };
        }

        /// <summary>
        /// 手牌から面子、対子、孤立牌の数を計算
        /// 再帰的に呼び出され、面子を優先的に探した結果、対子を優先した結果を返します
        /// </summary>
        /// <param name="bingpai"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static MianziResult mianzi(int[] bingpai, int n = 1)
        {
            if (n > 9) return dazi(bingpai);

            MianziResult max = mianzi(bingpai, n + 1);

            if (n <= 7 && bingpai[n] > 0 && bingpai[n + 1] > 0 && bingpai[n + 2] > 0)
            {
                bingpai[n]--; bingpai[n + 1]--; bingpai[n + 2]--;
                MianziResult r = mianzi(bingpai, n);
                bingpai[n]++; bingpai[n + 1]++; bingpai[n + 2]++;
                r.a[0]++; r.b[0]++;
                if (r.a[2] < max.a[2]
                    || r.a[2] == max.a[2] && r.a[1] < max.a[1]) max.a = r.a;
                if (r.b[0] > max.b[0]
                    || r.b[0] == max.b[0] && r.b[1] > max.b[1]) max.b = r.b;
            }

            if (bingpai[n] >= 3)
            {
                bingpai[n] -= 3;
                MianziResult r = mianzi(bingpai, n);
                bingpai[n] += 3;
                r.a[0]++; r.b[0]++;
                if (r.a[2] < max.a[2]
                    || r.a[2] == max.a[2] && r.a[1] < max.a[1]) max.a = r.a;
                if (r.b[0] > max.b[0]
                    || r.b[0] == max.b[0] && r.b[1] > max.b[1]) max.b = r.b;
            }

            return max;
        }

        /// <summary>
        /// 全ての色の牌について面子、対子、孤立牌の数を計算し、最小の聴牌数を返します。
        /// </summary>
        /// <param name="shoupai"></param>
        /// <param name="jiangpai"></param>
        /// <returns></returns>
        public static int mianzi_all(Shoupai shoupai, bool jiangpai)
        {
            var r_m = mianzi(shoupai._bingpai.m);
            var r_p = mianzi(shoupai._bingpai.p);
            var r_s = mianzi(shoupai._bingpai.s);

            List<int> z = new List<int> { 0, 0, 0 };
            for (int n = 1; n <= 7; n++)
            {
                if (shoupai._bingpai.z[n] >= 3) z[0]++;
                else if (shoupai._bingpai.z[n] == 2) z[1]++;
                else if (shoupai._bingpai.z[n] == 1) z[2]++;
            }

            int n_fulou = shoupai._fulou.Count;
            int min = 13;
            int[] x = new int[] { 0, 0, 0 };

            r_m.apply_action(m => {
                r_p.apply_action(p => {
                    r_s.apply_action(s => {
                        x[0] = n_fulou; x[1] = 0; x[2] = 0;
                        for (int i = 0; i < 3; i++)
                        {
                            x[i] += m[i] + p[i] + s[i] + z[i];
                        }
                        int n_xiangting = _xiangting(x[0], x[1], x[2], jiangpai);
                        if (n_xiangting < min) min = n_xiangting;
                    });
                });
            });

            return min;
        }

        /// <summary>
        /// shoupai の一般手(七対子形、国士無双形以外)としてのシャンテン数を返す。
        /// </summary>
        /// <param name="shoupai"></param>
        /// <returns></returns>
        public static double xiangting_yiban(Shoupai shoupai)
        {
            int min = mianzi_all(shoupai, false);

            foreach (char s in "mpsz")
            {
                int[] bingpai = shoupai._bingpai[s];
                for (int n = 1; n < bingpai.Length; n++)
                {
                    if (bingpai[n] >= 2)
                    {
                        bingpai[n] -= 2;
                        int n_xiangting = mianzi_all(shoupai, true);
                        bingpai[n] += 2;
                        if (n_xiangting < min) min = n_xiangting;
                    }
                }
            }
            if (min == -1 && !string.IsNullOrEmpty(shoupai._zimo) && shoupai._zimo.Length > 2) return 0;

            return min;
        }

        /// <summary>
        /// shoupai の国士無双形としてのシャンテン数を返す。
        /// </summary>
        /// <param name="shoupai"></param>
        /// <returns></returns>
        public static double xiangting_guoshi(Shoupai shoupai)
        {
            if (shoupai._fulou.Count > 0) return double.PositiveInfinity;

            int n_yaojiu = 0;
            int n_duizi = 0;

            foreach (char s in "mpsz")
            {
                int[] bingpai = shoupai._bingpai[s];
                List<int> nn = (s == 'z') ? new List<int> { 1, 2, 3, 4, 5, 6, 7 } : new List<int> { 1, 9 };
                foreach (int n in nn)
                {
                    if (bingpai[n] >= 1) n_yaojiu++;
                    if (bingpai[n] >= 2) n_duizi++;
                }
            }

            return n_duizi > 0 ? 12 - n_yaojiu : 13 - n_yaojiu;
        }

        /// <summary>
        /// shoupai の七対子形としてのシャンテン数を返す。
        /// </summary>
        /// <param name="shoupai"></param>
        /// <returns></returns>
        public static double xiangting_qidui(Shoupai shoupai)
        {
            if (shoupai._fulou.Count > 0) return double.PositiveInfinity;

            int n_duizi = 0;
            int n_guli = 0;

            foreach (char s in "mpsz")
            {
                int[] bingpai = shoupai._bingpai[s];
                for (int n = 1; n < bingpai.Length; n++)
                {
                    if (bingpai[n] >= 2) n_duizi++;
                    else if (bingpai[n] == 1) n_guli++;
                }
            }

            if (n_duizi > 7) n_duizi = 7;
            if (n_duizi + n_guli > 7) n_guli = 7 - n_duizi;

            return 13 - n_duizi * 2 - n_guli;
        }

        /// <summary>
        /// shoupai のシャンテン数を返す。
        /// </summary>
        /// <param name="shoupai"></param>
        /// <returns></returns>
        public static double xiangting(Shoupai shoupai)
        {
            return Util.min(
                xiangting_yiban(shoupai),
                xiangting_guoshi(shoupai),
                xiangting_qidui(shoupai)
            );
        }

        /// <summary>
        /// shoupai に1枚加えるとシャンテン数の進む 牌 の配列を返す。
        /// f_xiangting で指定された関数をシャンテン数計算の際に使用する。
        /// 返り値には赤牌は含まない。
        /// shoupai がツモると多牌になる場合は null を返す。
        /// </summary>
        /// <param name="shoupai"></param>
        /// <param name="f_xiangting"></param>
        /// <returns></returns>
        public static List<string> tingpai(Shoupai shoupai, Func<Shoupai, double> f_xiangting = null)
        {
            f_xiangting = f_xiangting ?? xiangting;

            if (shoupai._zimo != null) return null;

            List<string> pai = new List<string>();
            double n_xiangting = f_xiangting(shoupai);
            foreach (char s in "mpsz")
            {
                int[] bingpai = shoupai._bingpai[s];
                for (int n = 1; n < bingpai.Length; n++)
                {
                    if (bingpai[n] >= 4) continue;
                    bingpai[n]++;
                    if (f_xiangting(shoupai) < n_xiangting) pai.Add(s.ToString() + n);
                    bingpai[n]--;
                }
            }
            return pai;
        }
    }

}