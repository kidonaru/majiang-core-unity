using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Majiang
{
    /// <summary>
    /// 純手牌の各牌の枚数
    /// </summary>
    public class Bingpai
    {
        public int _ = 0;
        public int[] m = new int[10];
        public int[] p = new int[10];
        public int[] s = new int[10];
        public int[] z = new int[8];

        public int[] this[char key]
        {
            get
            {
                switch (key)
                {
                    case 'm': return this.m;
                    case 'p': return this.p;
                    case 's': return this.s;
                    case 'z': return this.z;
                    default: return null;
                }
            }
        }

        /// <summary>
        /// 表示用の牌一覧を取得 (赤牌置き換え済み)
        /// </summary>
        /// <returns></returns>
        public List<string> display_pai()
        {
            List<string> pai_list = new List<string>();
            foreach (char s in "mpsz")
            {
                int[] bingpai = this[s];
                for (int n = 1; n < bingpai.Length; n++)
                {
                    int i = 0;

                    // 赤牌の置き換え
                    if (n == 5 && bingpai[0] > 0)
                    {
                        for (; i < bingpai[0]; i++)
                        {
                            pai_list.Add($"{s}0");
                        }
                    }

                    for (; i < bingpai[n]; i++)
                    {
                        pai_list.Add($"{s}{n}");
                    }
                }
            }

            return pai_list;
        }
    }

    /// <summary>
    /// 手牌
    /// </summary>
    public class Shoupai : EntityBase
    {
        /// <summary>
        /// 純手牌の各牌の枚数を示す。
        /// 添字0は赤牌の枚数。
        /// </summary>
        public Bingpai _bingpai;
        /// <summary>
        /// 副露牌を示す 面子 の配列。
        /// 副露した順に配列する。
        /// 暗槓子も含むので _fulou.length == 0 がメンゼンを示す訳ではないことに注意。
        /// </summary>
        public List<string> _fulou = new List<string>();
        /// <summary>
        /// 手牌が打牌可能な場合、最後にツモしてきた 牌 あるいは最後に副露した 面子。
        /// 打牌可能でない場合は null。
        /// </summary>
        public string _zimo;
        /// <summary>
        /// リーチ後に true になる。
        /// </summary>
        public bool _lizhi;

        /// <summary>
        /// p が 牌 として正しければそのまま返す。
        /// 正しくなければ null を返す。 _ は正しいと見なさない。
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static string valid_pai(string p)
        {
            if (Regex.IsMatch(p, @"^(?:[mps]\d|z[1-7])_?\*?[\+\=\-]?$"))
            {
                return p;
            }
            return null;
        }

        /// <summary>
        /// m が 面子 として正しければ正規化して返す。
        /// 正しくなければ null を返す。
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static string valid_mianzi(string m)
        {
            if (Regex.IsMatch(m, @"^z.*[089]")) return null;
            string h = m.Replace("0", "5");
            if (Regex.IsMatch(h, @"^[mpsz](\d)\1\1[\+\=\-]\1?$"))
            {
                return Regex.Replace(m, @"([mps])05", match => match.Groups[1].Value + "50");
            }
            else if (Regex.IsMatch(h, @"^[mpsz](\d)\1\1\1[\+\=\-]?$"))
            {
                return m[0] + string.Join("", Regex.Matches(m, @"\d(?![\+\=\-])").Cast<Match>().Select(match => match.Value).OrderByDescending(x => x))
                    + (Regex.Match(m, @"\d[\+\=\-]$").Value ?? "");
            }
            else if (Regex.IsMatch(h, @"^[mps]\d+\-\d*$"))
            {
                Match hongpai = Regex.Match(m, @"0");
                List<string> nn = Regex.Matches(h, @"\d").Cast<Match>().Select(match => match.Value).OrderBy(x => x).ToList();
                if (nn.Count != 3) return null;
                if (int.Parse(nn[0]) + 1 != int.Parse(nn[1]) || int.Parse(nn[1]) + 1 != int.Parse(nn[2])) return null;
                h = h[0] + string.Join("", Regex.Matches(h, @"\d[\+\=\-]?").Cast<Match>().Select(match => match.Value).OrderBy(x => x));
                return hongpai.Success ? h.Replace("5", "0") : h;
            }
            return null;
        }

        /// <summary>
        /// qipai (配牌)からインスタンスを生成する。
        /// qipai の要素数は13でなくてもよい。
        /// </summary>
        /// <param name="qipai"></param>
        /// <exception cref="Exception"></exception>
        public Shoupai(List<string> qipai = null)
        {
            this._bingpai = new Bingpai();
            this._fulou = new List<string>();
            this._zimo = null;
            this._lizhi = false;

            if (qipai == null) return;

            foreach (string p in qipai)
            {
                if (p == "_")
                {
                    this._bingpai._++;
                    continue;
                }
                if (Shoupai.valid_pai(p) == null)
                {
                    throw new Exception(p);
                }
                char s = p[0];
                int n = int.Parse(p[1].ToString());
                if (this._bingpai[s][n] == 4)
                {
                    throw new Exception($"{this}, {p}");
                }
                this._bingpai[s][n]++;
                if (s != 'z' && n == 0) this._bingpai[s][5]++;
            }
        }

        /// <summary>
        /// paistr からインスタンスを生成する。
        /// 手牌が14枚を超える牌姿の場合、超える分が純手牌(副露面子以外の打牌可能な手牌のこと)から取り除かれる。
        /// </summary>
        /// <param name="paistr"></param>
        /// <returns></returns>
        public static Shoupai fromString(string paistr = "")
        {
            List<string> fulou = paistr.SplitJS(',').ToList();
            string bingpai = fulou.Shift();

            List<string> qipai = bingpai.MatchJS(@"_") ?? new List<string>();
            foreach (string suitstr in bingpai.MatchJS(@"[mpsz]\d+") ?? new List<string>())
            {
                char s = suitstr[0];
                foreach (string n in suitstr.MatchJS(@"\d"))
                {
                    if (s == 'z' && (n[0] < '1' || '7' < n[0])) continue;
                    qipai.Add(s.ToString() + n);
                }
            }
            qipai = qipai.Take(14 - fulou.Count(x => !string.IsNullOrEmpty(x)) * 3).ToList();
            string zimo = (qipai.Count - 2) % 3 == 0 ? qipai.Last() : null;
            Shoupai shoupai = new Shoupai(qipai);

            string last = null;
            foreach (string _m in fulou)
            {
                if (string.IsNullOrEmpty(_m))
                {
                    shoupai._zimo = last;
                    break;
                }
                var m = Shoupai.valid_mianzi(_m);
                if (!string.IsNullOrEmpty(m))
                {
                    shoupai._fulou.Add(m);
                    last = m;
                }
            }

            shoupai._zimo = shoupai._zimo ?? zimo ?? null;
            shoupai._lizhi = bingpai.EndsWith("*");

            return shoupai;
        }

        /// <summary>
        /// 牌姿 に変換する。
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string paistr = new string('_', this._bingpai._ + (this._zimo == "_" ? -1 : 0));

            foreach (char s in "mpsz")
            {
                string suitstr = s.ToString();
                int[] bingpai = this._bingpai[s];
                int n_hongpai = s == 'z' ? 0 : bingpai[0];
                for (int n = 1; n < bingpai.Length; n++)
                {
                    int n_pai = bingpai[n];
                    if (this._zimo != null)
                    {
                        if ($"{s}{n}" == this._zimo) { n_pai--; }
                        if (n == 5 && s + "0" == this._zimo) { n_pai--; n_hongpai--; }
                    }
                    for (int i = 0; i < n_pai; i++)
                    {
                        if (n == 5 && n_hongpai > 0) { suitstr += "0"; n_hongpai--; }
                        else { suitstr += n.ToString(); }
                    }
                }
                if (suitstr.Length > 1) paistr += suitstr;
            }
            if (this._zimo != null && this._zimo.Length <= 2) paistr += this._zimo;
            if (this._lizhi) paistr += "*";

            foreach (string m in this._fulou)
            {
                paistr += "," + m;
            }
            if (this._zimo != null && this._zimo.Length > 2) paistr += ",";

            return paistr;
        }

        /// <summary>
        /// 複製する。
        /// </summary>
        /// <returns></returns>
        public Shoupai clone()
        {
            Shoupai shoupai = new Shoupai();

            shoupai._bingpai = new Bingpai
            {
                _ = this._bingpai._,
                m = this._bingpai.m.ToArray(),
                p = this._bingpai.p.ToArray(),
                s = this._bingpai.s.ToArray(),
                z = this._bingpai.z.ToArray(),
            };
            shoupai._fulou = new List<string>(this._fulou);
            shoupai._zimo = this._zimo;
            shoupai._lizhi = this._lizhi;

            return shoupai;
        }

        /// <summary>
        /// 複製する。 (EntityBase互換)
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            return this.clone();
        }

        /// <summary>
        /// paistr で手牌を置き換える。
        /// </summary>
        /// <param name="paistr"></param>
        /// <returns></returns>
        public Shoupai _fromString(string paistr = "")
        {
            var shoupai = Shoupai.fromString(paistr);
            this._bingpai = new Bingpai
            {
                _ = shoupai._bingpai._,
                m = shoupai._bingpai.m.ToArray(),
                p = shoupai._bingpai.p.ToArray(),
                s = shoupai._bingpai.s.ToArray(),
                z = shoupai._bingpai.z.ToArray(),
            };
            this._fulou = new List<string>(shoupai._fulou);
            this._zimo = shoupai._zimo;
            this._lizhi = shoupai._lizhi;

            return this;
        }

        /// <summary>
        /// 牌の数を減らす
        /// </summary>
        /// <param name="s"></param>
        /// <param name="n"></param>
        /// <exception cref="Exception"></exception>
        public void decrease(char s, int n)
        {
            int[] bingpai = this._bingpai[s];
            if (bingpai[n] == 0 || n == 5 && bingpai[0] == bingpai[5])
            {
                if (this._bingpai._ == 0)
                {
                    throw new Exception($"{this},{s}{n}");
                }
                this._bingpai._--;
            }
            else
            {
                bingpai[n]--;
                if (n == 0) bingpai[5]--;
            }
        }

        /// <summary>
        /// p をツモる。
        /// check が真の場合、多牌となるツモは例外を発生する。
        /// </summary>
        /// <param name="p"></param>
        /// <param name="check"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public Shoupai zimo(string p = "", bool check = true)
        {
            if (check && this._zimo != null) throw new Exception($"{this}, {p}");
            if (p == "_")
            {
                this._bingpai._++;
                this._zimo = p;
            }
            else
            {
                if (Shoupai.valid_pai(p) == null) throw new Exception(p);
                char s = p[0];
                int n = int.Parse(p[1].ToString());
                int[] bingpai = this._bingpai[s];
                if (bingpai[n] == 4) throw new Exception($"{this}, {p}");
                bingpai[n]++;
                if (n == 0)
                {
                    if (bingpai[5] == 4) throw new Exception($"{this}, {p}");
                    bingpai[5]++;
                }
                this._zimo = $"{s}{n}";
            }
            return this;
        }

        /// <summary>
        /// p を打牌する。
        /// 手牌にない牌あるいは _ の打牌は例外を発生する。
        /// check が真の場合、少牌となる打牌も例外を発生する。
        /// リーチ後の手出しはチェックしない。
        /// </summary>
        /// <param name="p"></param>
        /// <param name="check"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public Shoupai dapai(string p = "", bool check = true)
        {
            if (check && this._zimo == null)
            {
                throw new Exception($"{this}, {p}");
            }
            if (Shoupai.valid_pai(p) == null)
            {
                throw new Exception(p);
            }
            char s = p[0];
            int n = int.Parse(p[1].ToString());
            this.decrease(s, n);
            this._zimo = null;
            if (p.EndsWith("*")) this._lizhi = true;
            return this;
        }

        /// <summary>
        /// m で副露する。
        /// 手牌にない構成での副露は例外を発生する。
        /// check が真の場合、多牌となる副露も例外を発生する。
        /// リーチ後の副露はチェックしない。
        /// </summary>
        /// <param name="m"></param>
        /// <param name="check"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public Shoupai fulou(string m, bool check = true)
        {
            if (check && this._zimo != null) throw new Exception($"{this}, {m}");
            if (m != Shoupai.valid_mianzi(m)) throw new Exception(m);
            if (Regex.IsMatch(m, @"\d{4}$")) throw new Exception($"{this}, {m}");
            if (Regex.IsMatch(m, @"\d{3}[\+\=\-]\d$")) throw new Exception($"{this}, {m}");
            char s = m[0];
            foreach (var n in m.MatchJS(@"\d(?![\+\=\-])"))
            {
                this.decrease(s, int.Parse(n));
            }
            this._fulou.Add(m);
            if (!Regex.IsMatch(m, @"\d{4}")) this._zimo = m;
            return this;
        }

        /// <summary>
        /// m で暗槓もしくは加槓する。
        /// 手牌にない構成での槓は例外を発生する。
        /// check が真の場合、少牌となる槓も例外を発生する。
        /// リーチ後の槓の正当性はチェックしない。
        /// </summary>
        /// <param name="m"></param>
        /// <param name="check"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public Shoupai gang(string m, bool check = true)
        {
            if (check && this._zimo == null) throw new Exception($"{this}, {m}");
            if (check && this._zimo.Length > 2) throw new Exception($"{this}, {m}");
            if (m != Shoupai.valid_mianzi(m)) throw new Exception(m);
            char s = m[0];
            if (Regex.IsMatch(m, @"\d{4}$"))
            {
                foreach (var n in m.MatchJS(@"\d"))
                {
                    this.decrease(s, int.Parse(n));
                }
                this._fulou.Add(m);
            }
            else if (Regex.IsMatch(m, @"\d{3}[\+\=\-]\d$"))
            {
                string m1 = m.Substring(0, 5);
                int i = this._fulou.FindIndex(m2 => m1 == m2);
                if (i < 0) throw new Exception($"{this}, {m}");
                this._fulou[i] = m;
                this.decrease(s, int.Parse(m.Substring(m.Length - 1)));
            }
            else throw new Exception($"{this}, {m}");
            this._zimo = null;
            return this;
        }

        /// <summary>
        /// メンゼンの場合、true を返す。
        /// </summary>
        public bool menqian
        {
            get
            {
                return this._fulou.Count(m => Regex.IsMatch(m, @"[\+\=\-]")) == 0;
            }
        }

        /// <summary>
        /// リーチ後は true を返す。
        /// </summary>
        public bool lizhi
        {
            get => this._lizhi;
        }

        /// <summary>
        /// 打牌可能な牌の一覧を返す。
        /// 赤牌およびツモ切りは別の牌として区別する。
        /// リーチ後はツモ切りのみ返す。
        /// check が真の場合、喰い替えとなる打牌は含まない。
        /// 打牌すると少牌となる場合は null を返す。
        /// </summary>
        /// <param name="check"></param>
        /// <returns></returns>
        public List<string> get_dapai(bool check = true)
        {
            if (this._zimo == null) return null;

            Dictionary<string, bool> deny = new Dictionary<string, bool>();
            if (check && this._zimo.Length > 2)
            {
                string m = this._zimo;
                char s = m[0];
                int n = Regex.IsMatch(m, @"\d(?=[\+\=\-])") ? int.Parse(Regex.Match(m, @"\d(?=[\+\=\-])").Value) : 0;
                if (n == 0) n = 5;
                deny[$"{s}{n}"] = true;
                if (!Regex.IsMatch(m.Replace("0", "5"), @"^[mpsz](\d)\1\1"))
                {
                    if (n < 7 && Regex.IsMatch(m, @"^[mps]\d\-\d\d$")) deny[s.ToString() + (n + 3).ToString()] = true;
                    if (3 < n && Regex.IsMatch(m, @"^[mps]\d\d\d\-$")) deny[s.ToString() + (n - 3).ToString()] = true;
                }
            }

            List<string> dapai = new List<string>();
            if (!this._lizhi)
            {
                foreach (char s in "mpsz")
                {
                    int[] bingpai = this._bingpai[s];
                    for (int n = 1; n < bingpai.Length; n++)
                    {
                        if (bingpai[n] == 0) continue;
                        var p = $"{s}{n}";
                        if (deny.ContainsKey(p)) continue;
                        if (p == this._zimo && bingpai[n] == 1) continue;
                        if (s == 'z' || n != 5) dapai.Add(p);
                        else
                        {
                            if (bingpai[0] > 0 && s.ToString() + "0" != this._zimo || bingpai[0] > 1) dapai.Add(s.ToString() + "0");
                            if (bingpai[0] < bingpai[5]) dapai.Add(p);
                        }
                    }
                }
            }
            if (this._zimo.Length == 2) dapai.Add(this._zimo + "_");
            return dapai;
        }

        /// <summary>
        /// p でチー可能な面子の一覧を返す。
        /// 赤牌のありなしは別の面子として区別する。
        /// リーチ後は空配列を返す。
        /// check が真の場合、喰い替えが必ず起きる面子は含まない。
        /// チーすると多牌になる場合は null を返す。
        /// </summary>
        /// <param name="p"></param>
        /// <param name="check"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public List<string> get_chi_mianzi(string p, bool check = true)
        {
            if (this._zimo != null) return null;
            if (Shoupai.valid_pai(p) == null) throw new Exception(p);

            List<string> mianzi = new List<string>();
            char s = p[0];
            int n = Util.get_pai_rank(p);
            string d = Regex.Match(p, @"[\+\=\-]$").Value;
            if (d.IsNullOrEmpty()) throw new Exception(p);
            if (s == 'z' || d != "-") return mianzi;
            if (this._lizhi) return mianzi;

            int[] bingpai = this._bingpai[s];
            if (3 <= n && bingpai[n - 2] > 0 && bingpai[n - 1] > 0)
            {
                if (!check || (3 < n ? bingpai[n - 3] : 0) + bingpai[n] < 14 - (this._fulou.Count + 1) * 3)
                {
                    if (n - 2 == 5 && bingpai[0] > 0) mianzi.Add(s.ToString() + "067-");
                    if (n - 1 == 5 && bingpai[0] > 0) mianzi.Add(s.ToString() + "406-");
                    if (n - 2 != 5 && n - 1 != 5 || bingpai[0] < bingpai[5]) mianzi.Add(s.ToString() + (n - 2).ToString() + (n - 1).ToString() + p[1].ToString() + d);
                }
            }
            if (2 <= n && n <= 8 && bingpai[n - 1] > 0 && bingpai[n + 1] > 0)
            {
                if (!check || bingpai[n] < 14 - (this._fulou.Count + 1) * 3)
                {
                    if (n - 1 == 5 && bingpai[0] > 0) mianzi.Add(s.ToString() + "06-7");
                    if (n + 1 == 5 && bingpai[0] > 0) mianzi.Add(s.ToString() + "34-0");
                    if (n - 1 != 5 && n + 1 != 5 || bingpai[0] < bingpai[5]) mianzi.Add(s.ToString() + (n - 1).ToString() + p[1].ToString() + d + (n + 1).ToString());
                }
            }
            if (n <= 7 && bingpai[n + 1] > 0 && bingpai[n + 2] > 0)
            {
                if (!check || bingpai[n] + (n < 7 ? bingpai[n + 3] : 0) < 14 - (this._fulou.Count + 1) * 3)
                {
                    if (n + 1 == 5 && bingpai[0] > 0) mianzi.Add(s.ToString() + "4-06");
                    if (n + 2 == 5 && bingpai[0] > 0) mianzi.Add(s.ToString() + "3-40");
                    if (n + 1 != 5 && n + 2 != 5 || bingpai[0] < bingpai[5]) mianzi.Add(s.ToString() + p[1].ToString() + d + (n + 1).ToString() + (n + 2).ToString());
                }
            }
            return mianzi;
        }

        /// <summary>
        /// p でポン可能な面子の一覧を返す。
        /// 赤牌のありなしは別の面子として区別する。
        /// リーチ後は空配列を返す。
        /// ポンすると多牌になる場合は null を返す。
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public List<string> get_peng_mianzi(string p)
        {
            if (this._zimo != null) return null;
            if (Shoupai.valid_pai(p) == null) throw new Exception(p);

            List<string> mianzi = new List<string>();
            char s = p[0];
            int n = Util.get_pai_rank(p);
            string d = Regex.Match(p, @"[\+\=\-]$").Value;
            if (d.IsNullOrEmpty()) throw new Exception(p);
            if (this._lizhi) return mianzi;

            int[] bingpai = this._bingpai[s];
            if (bingpai[n] >= 2)
            {
                if (n == 5 && bingpai[0] >= 2) mianzi.Add(s.ToString() + "00" + p[1].ToString() + d);
                if (n == 5 && bingpai[0] >= 1 && bingpai[5] - bingpai[0] >= 1) mianzi.Add(s.ToString() + "50" + p[1].ToString() + d);
                if (n != 5 || bingpai[5] - bingpai[0] >= 2) mianzi.Add($"{s}{n}" + n.ToString() + p[1].ToString() + d);
            }
            return mianzi;
        }

        /// <summary>
        /// p が指定された場合、それで大明槓可能な面子の一覧を返す。
        /// リーチ後は空配列を返す。
        /// p が指定されない場合は加槓あるいは暗槓可能な面子の一覧を返す。
        /// リーチ後は送り槓は含まない。
        /// カンすると少牌あるいは多牌になる場合は null を返す。
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public List<string> get_gang_mianzi(string p = null)
        {
            List<string> mianzi = new List<string>();
            if (p != null)
            {
                if (this._zimo != null) return null;
                if (Shoupai.valid_pai(p) == null) throw new Exception(p);

                char s = p[0];
                int n = Util.get_pai_rank(p);
                string d = Regex.Match(p, @"[\+\=\-]$").Value;
                if (d.IsNullOrEmpty()) throw new Exception(p);
                if (this._lizhi) return mianzi;

                int[] bingpai = this._bingpai[s];
                if (bingpai[n] == 3)
                {
                    if (n == 5) mianzi = new List<string> { s.ToString() + new string('5', 3 - bingpai[0]) + new string('0', bingpai[0]) + p[1].ToString() + d };
                    else mianzi = new List<string> { s.ToString() + new string(n.ToString()[0], 4) + d };
                }
            }
            else
            {
                if (this._zimo == null) return null;
                if (this._zimo.Length > 2) return null;
                p = this._zimo.Replace("0", "5");

                foreach (char s in "mpsz")
                {
                    int[] bingpai = this._bingpai[s];
                    for (int n = 1; n < bingpai.Length; n++)
                    {
                        if (bingpai[n] == 0) continue;
                        if (bingpai[n] == 4)
                        {
                            if (this._lizhi && $"{s}{n}" != p) continue;
                            if (n == 5) mianzi.Add(s.ToString() + new string('5', 4 - bingpai[0]) + new string('0', bingpai[0]));
                            else mianzi.Add(s.ToString() + new string(n.ToString()[0], 4));
                        }
                        else
                        {
                            if (this._lizhi) continue;
                            foreach (string m in this._fulou)
                            {
                                if (m.Replace("0", "5").Substring(0, 4) == s.ToString() + new string(n.ToString()[0], 3))
                                {
                                    if (n == 5 && bingpai[0] > 0) mianzi.Add(m + "0");
                                    else mianzi.Add(m + n.ToString());
                                }
                            }
                        }
                    }
                }
            }
            return mianzi;
        }
    }
}