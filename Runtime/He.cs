using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Majiang
{
    /// <summary>
    /// 捨て牌
    /// </summary>
    public class He : EntityBase
    {
        /// <summary>
        /// 捨てられた 牌 を表す配列。
        /// </summary>
        public List<string> _pai = new List<string>();
        /// <summary>
        /// 特定の 牌 が捨て牌にあるか判定するためのキャッシュ。
        /// </summary>
        public HashSet<string> _find = new HashSet<string>();

        /// <summary>
        /// p を捨て牌に追加する。
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public He dapai(string p)
        {
            if (Shoupai.valid_pai(p) == null) throw new Exception(p);
            this._pai.Add(Regex.Replace(p, "[\\+\\=\\-]$", ""));
            var key = p[0].ToString() + Util.get_pai_rank(p).ToString();
            this._find.Add(key);
            return this;
        }

        /// <summary>
        /// m で副露された状態にする。
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public He fulou(string m)
        {
            if (Shoupai.valid_mianzi(m) == null) throw new Exception(m);
            string p = m[0].ToString() + Regex.Match(m, @"\d(?=[\+\=\-])").Value;
            string d = Regex.Match(m, @"[\+\=\-]").Value;
            if (d.IsNullOrEmpty()) throw new Exception(m);
            if (this._pai[this._pai.Count - 1].Substring(0, 2) != p) throw new Exception(m);
            this._pai[this._pai.Count - 1] += d;
            return this;
        }

        /// <summary>
        /// p が捨て牌にあるとき true を返す。
        /// 手出し/ツモ切り、赤牌か否かは無視し、フリテンとなるか否かの観点で判定する。
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool find(string p)
        {
            var key = p[0].ToString() + Util.get_pai_rank(p).ToString();;
            return this._find.Contains(key);
        }

        public override object Clone()
        {
            return new He
            {
                _pai = _pai.Concat(),
                // _findは不要
            };
        }
    }
}