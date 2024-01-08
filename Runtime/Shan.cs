using System;
using System.Collections.Generic;
using System.Linq;

namespace Majiang
{
    /// <summary>
    /// 牌山インターフェース
    /// </summary>
    public interface IShan
    {
        string zimo();
        string gangzimo();
        Shan kaigang();
        Shan close();

        int paishu { get; }
        List<string> baopai { get; }
        List<string> fubaopai { get; }
    }

    /// <summary>
    /// 牌山
    /// </summary>
    public class Shan : EntityBase, IShan
    {
        /// <summary>
        /// インスタンス生成時に指定された ルール。
        /// </summary>
        public Rule _rule { get; set; }
        /// <summary>
        /// 牌山中の牌を表す 牌 の配列。
        /// 初期状態では添字 0〜13 が王牌となり、0〜3 がリンシャン牌、4〜8 がドラ表示牌、9〜13 が裏ドラ表示牌として順に使用される。
        /// ツモは常に最後尾から取られる。
        /// </summary>
        public List<string> _pai { get; set; }
        /// <summary>
        /// ドラ表示牌の配列。
        /// </summary>
        public List<string> _baopai { get; set; }
        /// <summary>
        /// 裏ドラ表示牌の配列。
        /// </summary>
        public List<string> _fubaopai { get; set; }
        /// <summary>
        /// 開槓可能なとき true になる。
        /// </summary>
        public bool _weikaigang { get; set; }
        /// <summary>
        /// 牌山固定後に true になる。
        /// </summary>
        public bool _closed { get; set; }

        /// <summary>
        /// ドラ表示牌が p の場合のドラを返す。
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string zhenbaopai(string p)
        {
            if (Shoupai.valid_pai(p) == null) throw new Exception(p);
            char s = p[0];
            int n = Util.get_pai_rank(p);
            return s == 'z' ? (n < 5 ? s + (n % 4 + 1).ToString() : s + ((n - 4) % 3 + 5).ToString())
                            : s + (n % 9 + 1).ToString();
        }

        /// <summary>
        /// インスタンスを生成する。
        /// 赤牌の枚数、カンドラ、裏ドラ、カン裏は rule にしたがう。
        /// </summary>
        /// <param name="rule"></param>
        public Shan(Rule rule)
        {
            this._rule = rule;
            var hongpai = rule.赤牌;

            List<string> pai = new List<string>();
            foreach (char s in "mpsz")
            {
                for (int n = 1; n <= (s == 'z' ? 7 : 9); n++)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (n == 5 && i < hongpai[s]) pai.Add(s.ToString() + 0);
                        else pai.Add(s.ToString() + n);
                    }
                }
            }

            this._pai = new List<string>();
            Random rand = new Random();
            while (pai.Count > 0)
            {
                this._pai.Add(pai.RemoveAtJS(rand.Next(0, pai.Count)));
            }

            this._baopai = new List<string> { this._pai[4] };
            this._fubaopai = rule.裏ドラあり ? new List<string> { this._pai[9] } : null;
            this._weikaigang = false;
            this._closed = false;
        }

        /// <summary>
        /// コンストラクタ
        /// 牌山を作成しない、Clone時に使用する
        /// </summary>
        public Shan()
        {
        }

        /// <summary>
        /// 次のツモ牌を返す。
        /// 牌山固定後に呼び出された場合は例外を発生する。
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public string zimo()
        {
            if (this._closed) throw new Exception(this.ToString());
            if (this.paishu == 0) throw new Exception(this.ToString());
            if (this._weikaigang) throw new Exception(this.ToString());
            return this._pai.RemoveAtJS(this._pai.Count - 1);
        }

        /// <summary>
        /// リンシャン牌からの次のツモ牌を返す。
        /// 牌山固定後に呼び出された場合は例外を発生する。
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public string gangzimo()
        {
            if (this._closed) throw new Exception(this.ToString());
            if (this.paishu == 0) throw new Exception(this.ToString());
            if (this._weikaigang) throw new Exception(this.ToString());
            if (this._baopai.Count == 5) throw new Exception(this.ToString());
            this._weikaigang = this._rule.カンドラあり;
            if (!this._weikaigang) this._baopai.Add("");
            return this._pai.RemoveAtJS(0);
        }

        /// <summary>
        /// カンドラを増やす。
        /// カンヅモより前に呼び出された場合は例外を発生する。
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public Shan kaigang()
        {
            if (this._closed) throw new Exception(this.ToString());
            if (!this._weikaigang) throw new Exception(this.ToString());
            this._baopai.Add(this._pai[4]);
            if (this._fubaopai != null && this._rule.カン裏あり)
                this._fubaopai.Add(this._pai[9]);
            this._weikaigang = false;
            return this;
        }

        /// <summary>
        /// 牌山を固定する。
        /// </summary>
        /// <returns></returns>
        public Shan close() { this._closed = true; return this; }

        /// <summary>
        /// ツモ可能な残り牌数を返す。
        /// </summary>
        public int paishu => this._pai.Count - 14;

        /// <summary>
        /// ドラ表示牌の配列を返す。
        /// </summary>
        public List<string> baopai { get { return this._baopai.Where(x => !string.IsNullOrEmpty(x)).ToList(); } }

        /// <summary>
        /// 牌山固定前は null を返す。
        /// 牌山固定後は裏ドラ表示牌の配列を返す。
        /// </summary>
        public List<string> fubaopai =>
            !this._closed ? null
            : this._fubaopai != null ? new List<string>(this._fubaopai)
            : null;

        public override object Clone()
        {
            return new Shan
            {
                _rule = this._rule, // 変更しないのでそのままコピー
                _pai = this._pai.Clone(),
                _baopai = this._baopai.Clone(),
                _fubaopai = this._fubaopai.Clone(),
                _weikaigang = this._weikaigang,
                _closed = this._closed,
            };
        }
    }
}
