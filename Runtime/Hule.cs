using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Majiang
{
    /// <summary>
    /// 和了役
    /// </summary>
    [Serializable]
    public class Yaku : EntityBase
    {
        /// <summary>
        /// 役名
        /// </summary>
        public string name;
        /// <summary>
        /// 役の翻数
        /// 役満の場合 fanshu は数字ではなく、和了役それぞれの役満複合数分の * となる。
        /// </summary>
        public string fanshu;
        /// <summary>
        /// 責任者
        /// 役満のパオがあった場合は baojia に責任者を設定する。
        /// </summary>
        public string baojia;

        public override string ToString()
        {
            return $"Yaku(name={name}, fanshu={fanshu}, baojia={baojia})";
        }
    }

    /// <summary>
    /// 状況役
    /// </summary>
    [Serializable]
    public class Hupai
    {
        /// <summary>
        /// 0: リーチなし、1: リーチ、2: ダブルリーチ。
        /// </summary>
        public int lizhi;
        /// <summary>
        /// 一発のとき true。
        /// </summary>
        public bool yifa;
        /// <summary>
        /// 槍槓のとき true。
        /// </summary>
        public bool qianggang;
        /// <summary>
        /// 嶺上開花のとき true。
        /// </summary>
        public bool lingshang;
        /// <summary>
        /// 0: ハイテイなし、1: ハイテイツモ、2: ハイテイロン。
        /// </summary>
        public int haidi;
        /// <summary>
        /// 0: 天和/地和なし、1: 天和、2: 地和。
        /// </summary>
        public int tianhu;
    }

    /// <summary>
    /// 供託
    /// </summary>
    [Serializable]
    public class Jicun
    {
        /// <summary>
        /// 積み棒の本数。
        /// </summary>
        public int changbang;
        /// <summary>
        /// リーチ棒の本数。
        /// </summary>
        public int lizhibang;
    }

    /// <summary>
    /// 和了点計算に使用する場況情報
    /// </summary>
    [Serializable]
    public class HuleParam
    {
        /// <summary>
        /// 和了点計算時に使用する ルール。
        /// </summary>
        public Rule rule;
        /// <summary>
        /// 場風。(0: 東、1: 南、2: 西、3: 北)
        /// </summary>
        public int zhuangfeng;
        /// <summary>
        /// 自風。(0: 東、1: 南、2: 西、3: 北)
        /// </summary>
        public int menfeng;
        /// <summary>
        /// 状況役
        /// </summary>
        public Hupai hupai;
        /// <summary>
        /// ドラ表示牌の配列。
        /// </summary>
        public List<string> baopai;
        /// <summary>
        /// 裏ドラ表示牌の配列。リーチのない場合は null。
        /// </summary>
        public List<string> fubaopai;
        /// <summary>
        /// 供託
        /// </summary>
        public Jicun jicun;
    }

    /// <summary>
    /// 和了点計算に使用する場況情報の簡易版
    /// </summary>
    public class HuleInput
    {
        public Majiang.Rule rule { get; set; }
        public int? zhuangfeng { get; set; }
        public int? menfeng { get; set; }
        public int? lizhi { get; set; }
        public bool? yifa { get; set; }
        public bool? qianggang { get; set; }
        public bool? lingshang { get; set; }
        public int? haidi { get; set; }
        public int? tianhu { get; set; }
        public List<string> baopai { get; set; }
        public List<string> fubaopai { get; set; }
        public int? changbang { get; set; }
        public int? lizhibang { get; set; }
    }

    /// <summary>
    /// 和了結果
    /// </summary>
    [Serializable]
    public class HuleResult : EntityBase
    {
        /// <summary>
        /// 和了役の配列。
        /// </summary>
        public List<Yaku> hupai;
        /// <summary>
        /// 符。役満の場合は null。
        /// </summary>
        public int? fu;
        /// <summary>
        /// 翻数。役満の場合は null。
        /// </summary>
        public int? fanshu;
        /// <summary>
        /// 役満複合数。
        /// 複合には四暗刻をダブル役満にする類のものと、大三元と字一色の複合のような役の複合のケースがある。
        /// 役満でない場合は null。
        /// </summary>
        public int? damanguan;
        /// <summary>
        /// 和了打点。供託収入は含まない。
        /// </summary>
        public int defen;
        /// <summary>
        /// 供託を含めたその局の点数の収支。
        /// その局の東家から順に並べる。
        /// リーチ宣言による1000点減は収支に含めない。
        /// </summary>
        public List<int> fenpei;

        public override string ToString()
        {
            var parameters = new List<string>
            {
                $"hupai={hupai.JoinJS()}",
                fu != null ? $"fu={fu}" : null,
                fanshu != null ? $"fanshu={fanshu}" : null,
                damanguan != null ? $"damanguan={damanguan}" : null,
                $"defen={defen}",
                $"fenpei={fenpei.JoinJS()}",
            };

            return $"HuleResult({string.Join(", ", parameters.Where(p => p != null))})";
        }
    }

    /// <summary>
    /// 和了点計算
    /// </summary>
    public static partial class Util
    {
        /// <summary>
        /// 牌の面子（順子や刻子）を生成する
        /// </summary>
        /// <param name="s"></param>
        /// <param name="bingpai"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static List<List<string>> mianzi(string s, int[] bingpai, int n = 1)
        {
            if (n > 9) return new List<List<string>>{ new List<string>() };

            if (bingpai[n] == 0) return mianzi(s, bingpai, n + 1);

            List<List<string>> shunzi = new List<List<string>>();
            if (n <= 7 && bingpai[n] > 0 && bingpai[n + 1] > 0 && bingpai[n + 2] > 0)
            {
                bingpai[n]--; bingpai[n + 1]--; bingpai[n + 2]--;
                shunzi = mianzi(s, bingpai, n);
                bingpai[n]++; bingpai[n + 1]++; bingpai[n + 2]++;
                foreach (List<string> s_mianzi in shunzi)
                {
                    s_mianzi.Insert(0, s + (n) + (n + 1) + (n + 2));
                }
            }

            List<List<string>> kezi = new List<List<string>>();
            if (bingpai[n] == 3)
            {
                bingpai[n] -= 3;
                kezi = mianzi(s, bingpai, n + 1);
                bingpai[n] += 3;
                foreach (List<string> k_mianzi in kezi)
                {
                    k_mianzi.Insert(0, s + n + n + n);
                }
            }

            return shunzi.Concat(kezi).ToList();
        }

        /// <summary>
        /// 牌の面子（順子や刻子）を全て生成する
        /// </summary>
        /// <param name="shoupai"></param>
        /// <returns></returns>
        public static List<List<string>> mianzi_all(Shoupai shoupai)
        {
            var shupai_all = new List<List<string>>{ new List<string>() };
            foreach (char s in "mps")
            {
                var new_mianzi = new List<List<string>>();
                foreach (var mm in shupai_all)
                {
                    foreach (var nn in mianzi(s.ToString(), shoupai._bingpai[s]))
                    {
                        new_mianzi.Add(mm.Concat(nn).ToList());
                    }
                }
                shupai_all = new_mianzi;
            }

            List<string> zipai = new List<string>();
            for (int n = 1; n <= 7; n++)
            {
                if (shoupai._bingpai.z[n] == 0) continue;
                if (shoupai._bingpai.z[n] != 3) return new List<List<string>>();
                zipai.Add("z" + n + n + n);
            }

            List<string> fulou = shoupai._fulou.Select(m => m.Replace('0', '5')).ToList();

            return shupai_all.Select(shupai => shupai.Concat(zipai).Concat(fulou).ToList()).ToList();
        }

        /// <summary>
        /// 面子に和了牌を追加する
        /// </summary>
        /// <param name="mianzi"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static List<List<string>> add_hulepai(List<string> mianzi, string p)
        {
            string s = p.Length > 0 ? p[0].ToString() : "";
            string n = p.Length > 1 ? p[1].ToString() : "";
            string d = p.Length > 2 ? p[2].ToString() : "";
            Regex regexp = new Regex($"^({s}.*{n})");
            string replacer = $"$1{d}!";

            List<List<string>> new_mianzi = new List<List<string>>();

            for (int i = 0; i < mianzi.Count; i++)
            {
                if (Regex.IsMatch(mianzi[i], @"[\+\=\-]|\d{4}")) continue;
                if (i > 0 && mianzi[i] == mianzi[i - 1]) continue;
                string m = regexp.Replace(mianzi[i], replacer);
                if (m == mianzi[i]) continue;
                List<string> tmp_mianzi = new List<string>(mianzi);
                tmp_mianzi[i] = m;
                new_mianzi.Add(tmp_mianzi);
            }

            return new_mianzi;
        }

        /// <summary>
        /// 一般的なの和了形を返す
        /// </summary>
        /// <param name="shoupai"></param>
        /// <param name="hulepai"></param>
        /// <returns></returns>
        public static List<List<string>> hule_mianzi_yiban(Shoupai shoupai, string hulepai)
        {
            var mianzi = new List<List<string>>();

            foreach (var s in "mpsz")
            {
                var bingpai = shoupai._bingpai[s];
                for (int n = 1; n < bingpai.Length; n++)
                {
                    if (bingpai[n] < 2) continue;
                    bingpai[n] -= 2;
                    string jiangpai = $"{s}{n}{n}";
                    foreach (var mm in mianzi_all(shoupai))
                    {
                        mm.Insert(0, jiangpai);
                        if (mm.Count != 5) continue;
                        mianzi = mianzi.Concat(add_hulepai(mm, hulepai)).ToList();
                    }
                    bingpai[n] += 2;
                }
            }

            return mianzi;
        }

        /// <summary>
        /// 七対子形の和了形を返す
        /// </summary>
        /// <param name="shoupai"></param>
        /// <param name="hulepai"></param>
        /// <returns></returns>
        public static List<List<string>> hule_mianzi_qidui(Shoupai shoupai, string hulepai)
        {
            if (shoupai._fulou.Count > 0) return new List<List<string>>();

            var mianzi = new List<string>();

            foreach (var s in "mpsz")
            {
                var bingpai = shoupai._bingpai[s];
                for (int n = 1; n < bingpai.Length; n++)
                {
                    if (bingpai[n] == 0) continue;
                    if (bingpai[n] == 2)
                    {
                        string m = (s.ToString() + n == hulepai.Substring(0, 2))
                                    ? s.ToString() + n + n + hulepai[2] + '!'
                                    : s.ToString() + n + n;
                        mianzi.Add(m);
                    }
                    else return new List<List<string>>();
                }
            }

            return (mianzi.Count == 7) ? new List<List<string>> { mianzi } : new List<List<string>>();
        }

        /// <summary>
        /// 国士無双形の和了形を返す
        /// </summary>
        /// <param name="shoupai"></param>
        /// <param name="hulepai"></param>
        /// <returns></returns>
        public static List<List<string>> hule_mianzi_guoshi(Shoupai shoupai, string hulepai)
        {
            if (shoupai._fulou.Count > 0) return new List<List<string>>();

            var mianzi = new List<string>();
            int n_duizi = 0;

            foreach (var s in "mpsz")
            {
                var bingpai = shoupai._bingpai[s];
                var nn = (s == 'z') ? new List<int> { 1, 2, 3, 4, 5, 6, 7 } : new List<int> { 1, 9 };
                foreach (var n in nn)
                {
                    if (bingpai[n] == 2)
                    {
                        string m = (s.ToString() + n == hulepai.Substring(0, 2))
                                    ? s.ToString() + n + n + (hulepai.Length > 2 ? hulepai[2].ToString() : "") + '!'
                                    : s.ToString() + n + n;
                        mianzi.Insert(0, m);
                        n_duizi++;
                    }
                    else if (bingpai[n] == 1)
                    {
                        string m = (s.ToString() + n == hulepai.Substring(0, 2))
                                    ? s.ToString() + n + (hulepai.Length > 2 ? hulepai[2].ToString() : "") + '!'
                                    : s.ToString() + n;
                        mianzi.Add(m);
                    }
                    else return new List<List<string>>();
                }
            }

            return (n_duizi == 1) ? new List<List<string>> { mianzi } : new List<List<string>>();
        }

        /// <summary>
        /// 九蓮宝燈の和了形を返す
        /// </summary>
        /// <param name="shoupai"></param>
        /// <param name="hulepai"></param>
        /// <returns></returns>
        public static List<List<string>> hule_mianzi_jiulian(Shoupai shoupai, string hulepai)
        {
            if (shoupai._fulou.Count > 0) return new List<List<string>>();

            char s = hulepai[0];
            if (s == 'z') return new List<List<string>>();

            string mianzi = s.ToString();
            int[] bingpai = shoupai._bingpai[s];
            for (int n = 1; n <= 9; n++)
            {
                if (bingpai[n] == 0) return new List<List<string>>();
                if ((n == 1 || n == 9) && bingpai[n] < 3) return new List<List<string>>();
                int n_pai = (n == int.Parse(hulepai[1].ToString())) ? bingpai[n] - 1 : bingpai[n];
                for (int i = 0; i < n_pai; i++)
                {
                    mianzi += n.ToString();
                }
            }
            if (mianzi.Length != 14) return new List<List<string>>();
            mianzi += hulepai.Substring(1) + "!";

            return new List<List<string>> { new List<string> { mianzi } };
        }

        /// <summary>
        /// shoupai の手牌から rongpai で和了したときの和了形の一覧を返す。
        /// 和了形にならない場合は空配列を返す。
        /// </summary>
        /// <param name="shoupai"></param>
        /// <param name="rongpai"></param>
        /// <returns></returns>
        public static List<List<string>> hule_mianzi(Shoupai shoupai, string rongpai = null)
        {
            Shoupai new_shoupai = shoupai.clone();
            if (!rongpai.IsNullOrEmpty()) new_shoupai.zimo(rongpai);

            if (new_shoupai._zimo == null || new_shoupai._zimo.Length > 2) return new List<List<string>>();
            string hulepai = (!rongpai.IsNullOrEmpty() ? rongpai : new_shoupai._zimo + "_").Replace("0", "5");

            List<List<string>> result = new List<List<string>>();
            result.AddRange(hule_mianzi_yiban(new_shoupai, hulepai));
            result.AddRange(hule_mianzi_qidui(new_shoupai, hulepai));
            result.AddRange(hule_mianzi_guoshi(new_shoupai, hulepai));
            result.AddRange(hule_mianzi_jiulian(new_shoupai, hulepai));

            return result;
        }

        /// <summary>
        /// 順子情報
        /// </summary>
        public class Shunzi
        {
            public List<int> m { get; set; }
            public List<int> p { get; set; }
            public List<int> s { get; set; }

            public List<int> this[char key]
            {
                get
                {
                    switch (key)
                    {
                        case 'm': return m;
                        case 'p': return p;
                        case 's': return s;
                        case 'z': return null;
                        default: return null;
                    }
                }
            }
        }

        /// <summary>
        /// 刻子情報
        /// </summary>
        public class Kezi
        {
            public List<int> m { get; set; }
            public List<int> p { get; set; }
            public List<int> s { get; set; }
            public List<int> z { get; set; }

            public List<int> this[char key]
            {
                get
                {
                    switch (key)
                    {
                        case 'm': return m;
                        case 'p': return p;
                        case 's': return s;
                        case 'z': return z;
                        default: return null;
                    }
                }
            }
        }

        /// <summary>
        /// 麻雀の和了形の詳細情報
        /// </summary>
        public class Hudi
        {
            /// <summary>
            /// 符
            /// </summary>
            public int fu { get; set; }
            /// <summary>
            /// 門前かどうか
            /// </summary>
            public bool menqian { get; set; }
            /// <summary>
            /// 自摸かどうか
            /// </summary>
            public bool zimo { get; set; }
            /// <summary>
            /// 順子情報
            /// </summary>
            public Shunzi shunzi { get; set; }
            /// <summary>
            /// 刻子情報
            /// </summary>
            public Kezi kezi { get; set; }
            /// <summary>
            /// 順子の数
            /// </summary>
            public int n_shunzi { get; set; }
            /// <summary>
            /// 刻子の数
            /// </summary>
            public int n_kezi { get; set; }
            /// <summary>
            /// 暗刻の数
            /// </summary>
            public int n_ankezi { get; set; }
            /// <summary>
            /// 槓子の数
            /// </summary>
            public int n_gangzi { get; set; }
            /// <summary>
            /// 幺九牌の数
            /// </summary>
            public int n_yaojiu { get; set; }
            /// <summary>
            /// 字牌の数
            /// </summary>
            public int n_zipai { get; set; }
            /// <summary>
            /// 単騎待ちかどうか
            /// </summary>
            public bool danqi { get; set; }
            /// <summary>
            /// 平和かどうか
            /// </summary>
            public bool pinghu { get; set; }
            /// <summary>
            /// 場風
            /// </summary>
            public int zhuangfeng { get; set; }
            /// <summary>
            /// 自風
            /// </summary>
            public int menfeng { get; set; }
        }

        readonly static Regex _yaojiu = new Regex(@"^.*[z19].*$");
        readonly static Regex _zipai = new Regex(@"^z.*$");

        readonly static Regex _kezi = new Regex(@"^[mpsz](\d)\1\1.*$");
        readonly static Regex _ankezi = new Regex(@"^[mpsz](\d)\1\1(?:\1|_!)?$");
        readonly static Regex _gangzi = new Regex(@"^[mpsz](\d)\1\1.*\1.*$");

        readonly static Regex _danqi = new Regex(@"^[mpsz](\d)\1[\+\=\-_]\!$");
        readonly static Regex _kanzhang = new Regex(@"^[mps]\d\d[\+\=\-_]\!\d$");
        readonly static Regex _bianzhang = new Regex(@"^[mps](123[\+\=\-_]\!|7[\+\=\-_]\!89)$");

        /// <summary>
        /// 和了形から符（点数計算の基礎となる数値）を計算します。
        /// </summary>
        /// <param name="mianzi"></param>
        /// <param name="zhuangfeng"></param>
        /// <param name="menfeng"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        public static Hudi get_hudi(List<string> mianzi, int zhuangfeng, int menfeng, Rule rule)
        {
            var zhuangfengpai = new Regex($"^z{zhuangfeng + 1}.*$");
            var menfengpai = new Regex($"^z{menfeng + 1}.*$");
            var sanyuanpai = new Regex(@"^z[567].*$");

            var hudi = new Hudi
            {
                fu = 20,
                menqian = true,
                zimo = true,
                shunzi = new Shunzi
                {
                    m = new List<int>{0, 0, 0, 0, 0, 0, 0, 0},
                    p = new List<int>{0, 0, 0, 0, 0, 0, 0, 0},
                    s = new List<int>{0, 0, 0, 0, 0, 0, 0, 0},
                },
                kezi = new Kezi
                {
                    m = new List<int>{0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                    p = new List<int>{0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                    s = new List<int>{0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                    z = new List<int>{0, 0, 0, 0, 0, 0, 0, 0},
                },
                n_shunzi = 0,
                n_kezi = 0,
                n_ankezi = 0,
                n_gangzi = 0,
                n_yaojiu = 0,
                n_zipai = 0,
                danqi = false,
                pinghu = false,
                zhuangfeng = zhuangfeng,
                menfeng = menfeng,
            };

            foreach (var m in mianzi)
            {
                if (Regex.IsMatch(m, @"[\+\=\-](?!\!)")) hudi.menqian = false;
                if (Regex.IsMatch(m, @"[\+\=\-]\!")) hudi.zimo = false;

                if (mianzi.Count == 1) continue;

                if (_danqi.IsMatch(m)) hudi.danqi = true;

                if (mianzi.Count == 13) continue;

                if (_yaojiu.IsMatch(m)) hudi.n_yaojiu++;
                if (_zipai.IsMatch(m)) hudi.n_zipai++;

                if (mianzi.Count != 5) continue;

                if (m == mianzi[0])
                {
                    var fu = 0;
                    if (zhuangfengpai.IsMatch(m)) fu += 2;
                    if (menfengpai.IsMatch(m)) fu += 2;
                    if (sanyuanpai.IsMatch(m)) fu += 2;
                    fu = rule.連風牌は2符 && fu > 2 ? 2 : fu;
                    hudi.fu += fu;
                    if (hudi.danqi) hudi.fu += 2;
                }
                else if (_kezi.IsMatch(m))
                {
                    hudi.n_kezi++;
                    var fu = 2;
                    if (_yaojiu.IsMatch(m)) fu *= 2;
                    if (_ankezi.IsMatch(m)) { fu *= 2; hudi.n_ankezi++; }
                    if (_gangzi.IsMatch(m)) { fu *= 4; hudi.n_gangzi++; }
                    hudi.fu += fu;
                    hudi.kezi[m[0]][int.Parse(m[1].ToString())]++;
                }
                else
                {
                    hudi.n_shunzi++;
                    if (_kanzhang.IsMatch(m)) hudi.fu += 2;
                    if (_bianzhang.IsMatch(m)) hudi.fu += 2;
                    hudi.shunzi[m[0]][int.Parse(m[1].ToString())]++;
                }
            }

            if (mianzi.Count == 7)
            {
                hudi.fu = 25;
            }
            else if (mianzi.Count == 5)
            {
                hudi.pinghu = (hudi.menqian && hudi.fu == 20);
                if (hudi.zimo)
                {
                    if (!hudi.pinghu) hudi.fu += 2;
                }
                else
                {
                    if (hudi.menqian) hudi.fu += 10;
                    else if (hudi.fu == 20) hudi.fu = 30;
                }
                hudi.fu = (int)Math.Ceiling(hudi.fu / 10.0) * 10;
            }

            return hudi;
        }

        /// <summary>
        /// 前準備としての役を取得します。
        /// </summary>
        /// <param name="hupai"></param>
        /// <returns></returns>
        public static List<Yaku> get_pre_hupai(Hupai hupai)
        {
            List<Yaku> pre_hupai = new List<Yaku>();

            if (hupai.lizhi == 1) pre_hupai.Add(new Yaku { name = "立直", fanshu = "1" });
            if (hupai.lizhi == 2) pre_hupai.Add(new Yaku { name = "ダブル立直", fanshu = "2" });
            if (hupai.yifa) pre_hupai.Add(new Yaku { name = "一発", fanshu = "1" });
            if (hupai.haidi == 1) pre_hupai.Add(new Yaku { name = "海底摸月", fanshu = "1" });
            if (hupai.haidi == 2) pre_hupai.Add(new Yaku { name = "河底撈魚", fanshu = "1" });
            if (hupai.lingshang) pre_hupai.Add(new Yaku { name = "嶺上開花", fanshu = "1" });
            if (hupai.qianggang) pre_hupai.Add(new Yaku { name = "槍槓", fanshu = "1" });

            if (hupai.tianhu == 1) pre_hupai = new List<Yaku> { new Yaku { name = "天和", fanshu = "*" } };
            if (hupai.tianhu == 2) pre_hupai = new List<Yaku> { new Yaku { name = "地和", fanshu = "*" } };

            return pre_hupai;
        }

        /// <summary>
        /// 和了形から役を取得
        /// </summary>
        /// <param name="mianzi"></param>
        /// <param name="hudi"></param>
        /// <param name="pre_hupai"></param>
        /// <param name="post_hupai"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        public static List<Yaku> get_hupai(List<string> mianzi, Hudi hudi, List<Yaku> pre_hupai, List<Yaku> post_hupai, Rule rule)
        {
            // 門前清自摸和
            List<Yaku> menqianqing()
            {
                if (hudi.menqian && hudi.zimo)
                {
                    return new List<Yaku> { new Yaku { name = "門前清自摸和", fanshu = "1" } };
                }
                return new List<Yaku>();
            };

            // 翻牌
            List<Yaku> fanpai()
            {
                var feng_hanzi = new List<string> { "東", "南", "西", "北" };
                List<Yaku> fanpai_all = new List<Yaku>();
                if (hudi.kezi.z[hudi.zhuangfeng + 1] > 0)
                    fanpai_all.Add(new Yaku { name = "場風 " + feng_hanzi[hudi.zhuangfeng], fanshu = "1" });
                if (hudi.kezi.z[hudi.menfeng + 1] > 0)
                    fanpai_all.Add(new Yaku { name = "自風 " + feng_hanzi[hudi.menfeng], fanshu = "1" });
                if (hudi.kezi.z[5] > 0) fanpai_all.Add(new Yaku { name = "翻牌 白", fanshu = "1" });
                if (hudi.kezi.z[6] > 0) fanpai_all.Add(new Yaku { name = "翻牌 發", fanshu = "1" });
                if (hudi.kezi.z[7] > 0) fanpai_all.Add(new Yaku { name = "翻牌 中", fanshu = "1" });
                return fanpai_all;
            };

            // 平和
            List<Yaku> pinghu()
            {
                if (hudi.pinghu) return new List<Yaku> { new Yaku { name = "平和", fanshu = "1" } };
                return new List<Yaku>();
            };

            // 断幺九
            List<Yaku> duanyaojiu()
            {
                if (hudi.n_yaojiu > 0) return new List<Yaku>();
                if (rule.クイタンあり || hudi.menqian) return new List<Yaku> { new Yaku { name = "断幺九", fanshu = "1" } };
                return new List<Yaku>();
            };

            // 一盃口
            List<Yaku> yibeikou()
            {
                if (!hudi.menqian) return new List<Yaku>();
                var shunzi = hudi.shunzi;
                var beikou = shunzi.m.Concat(shunzi.p).Concat(shunzi.s).Select(x => x >> 1).Sum();
                if (beikou == 1) return new List<Yaku> { new Yaku { name = "一盃口", fanshu = "1" } };
                return new List<Yaku>();
            };

            // 三色同順
            List<Yaku> sansetongshun()
            {
                var shunzi = hudi.shunzi;
                for (int n = 1; n <= 7; n++)
                {
                    if (shunzi.m[n] > 0 && shunzi.p[n] > 0 && shunzi.s[n] > 0)
                        return new List<Yaku> { new Yaku { name = "三色同順", fanshu = hudi.menqian ? "2" : "1" } };
                }
                return new List<Yaku>();
            };

            // 一気通貫
            List<Yaku> yiqitongguan()
            {
                var shunzi = hudi.shunzi;
                foreach (var s in "mps")
                {
                    if (shunzi[s][1] > 0 && shunzi[s][4] > 0 && shunzi[s][7] > 0)
                        return new List<Yaku> { new Yaku { name = "一気通貫", fanshu = hudi.menqian ? "2" : "1" } };
                }
                return new List<Yaku>();
            };

            // 混全帯幺九
            List<Yaku> hunquandaiyaojiu()
            {
                if (hudi.n_yaojiu == 5 && hudi.n_shunzi > 0 && hudi.n_zipai > 0)
                    return new List<Yaku> { new Yaku { name = "混全帯幺九", fanshu = hudi.menqian ? "2" : "1" } };
                return new List<Yaku>();
            };

            // 七対子
            List<Yaku> qiduizi()
            {
                if (mianzi.Count == 7) return new List<Yaku> { new Yaku { name = "七対子", fanshu = "2" } };
                return new List<Yaku>();
            };

            // 対々和
            List<Yaku> duiduihu()
            {
                if (hudi.n_kezi == 4) return new List<Yaku> { new Yaku { name = "対々和", fanshu = "2" } };
                return new List<Yaku>();
            };

            // 三暗刻
            List<Yaku> sananke()
            {
                if (hudi.n_ankezi == 3) return new List<Yaku> { new Yaku { name = "三暗刻", fanshu = "2" } };
                return new List<Yaku>();
            };

            // 三槓子
            List<Yaku> sangangzi()
            {
                if (hudi.n_gangzi == 3) return new List<Yaku> { new Yaku { name = "三槓子", fanshu = "2" } };
                return new List<Yaku>();
            };

            // 三色同刻
            List<Yaku> sansetongke()
            {
                var kezi = hudi.kezi;
                for (int n = 1; n <= 9; n++)
                {
                    if (kezi.m[n] > 0 && kezi.p[n] > 0 && kezi.s[n] > 0)
                        return new List<Yaku> { new Yaku { name = "三色同刻", fanshu = "2" } };
                }
                return new List<Yaku>();
            };

            List<Yaku> hunlaotou()
            {
                if (hudi.n_yaojiu == mianzi.Count && hudi.n_shunzi == 0 && hudi.n_zipai > 0)
                    return new List<Yaku> { new Yaku { name = "混老頭", fanshu = "2" } };
                return new List<Yaku>();
            };

            List<Yaku> xiaosanyuan()
            {
                if (hudi.kezi.z[5] + hudi.kezi.z[6] + hudi.kezi.z[7] == 2 &&
                        Regex.IsMatch(mianzi[0], "^z[567]"))
                    return new List<Yaku> { new Yaku { name = "小三元", fanshu = "2" } };
                return new List<Yaku>();
            };

            List<Yaku> hunyise()
            {
                foreach (char s in "mps")
                {
                    var yise = new Regex($"^[z${s}]");
                    if (mianzi.Count(m => yise.IsMatch(m)) == mianzi.Count
                            && hudi.n_zipai > 0)
                        return new List<Yaku> { new Yaku { name = "混一色", fanshu = hudi.menqian ? "3" : "2" } };
                }
                return new List<Yaku>();
            };

            List<Yaku> chunquandaiyaojiu()
            {
                if (hudi.n_yaojiu == 5 && hudi.n_shunzi > 0 && hudi.n_zipai == 0)
                    return new List<Yaku> { new Yaku { name = "純全帯幺九", fanshu = hudi.menqian ? "3" : "2" } };
                return new List<Yaku>();
            };

            List<Yaku> erbeikou()
            {
                if (!hudi.menqian) return new List<Yaku>();
                var shunzi = hudi.shunzi;
                var beikou = shunzi.m.Concat(shunzi.p).Concat(shunzi.s)
                                .Select(x => x >> 1).Aggregate((a, b) => a + b);
                if (beikou == 2)
                    return new List<Yaku> { new Yaku { name = "二盃口", fanshu = "3" } };
                return new List<Yaku>();
            };

            List<Yaku> qingyise()
            {
                foreach (char s in "mps")
                {
                    var yise = new Regex($"^[${s}]");
                    if (mianzi.Count(m => yise.IsMatch(m)) == mianzi.Count)
                        return new List<Yaku> { new Yaku { name = "清一色", fanshu = hudi.menqian ? "6" : "5" } };
                }
                return new List<Yaku>();
            };

            List<Yaku> guoshiwushuang()
            {
                if (mianzi.Count != 13) return new List<Yaku>();
                if (hudi.danqi) return new List<Yaku> { new Yaku { name = "国士無双十三面", fanshu = "**" } };
                else return new List<Yaku> { new Yaku { name = "国士無双", fanshu = "*" } };
            };

            List<Yaku> sianke()
            {
                if (hudi.n_ankezi != 4) return new List<Yaku>();
                if (hudi.danqi) return new List<Yaku> { new Yaku { name = "四暗刻単騎", fanshu = "**" } };
                else return new List<Yaku> { new Yaku { name = "四暗刻", fanshu = "*" } };
            };

            List<Yaku> dasanyuan()
            {
                var result = new List<Yaku>();
                var kezi = hudi.kezi;

                if (kezi.z[5] + kezi.z[6] + kezi.z[7] == 3)
                {
                    var baoMianzi = mianzi.Where(m => Regex.IsMatch(m, @"^z([567])\1\1(?:[\+\=\-]|\1)(?!\!)")).ToList();
                    string baojia = null;

                    if (baoMianzi.Count > 2)
                    {
                        var match = Regex.Match(baoMianzi[2], @"[\+\=\-]");
                        if (match.Success)
                        {
                            baojia = match.Value;
                        }
                    }

                    var item = new Yaku
                    {
                        name = "大三元",
                        fanshu = "*",
                    };

                    if (baojia != null)
                    {
                        item.baojia = baojia;
                    }

                    result.Add(item);
                }

                return result;
            };

            List<Yaku> sixihu()
            {
                var result = new List<Yaku>();
                var kezi = hudi.kezi;

                if (kezi.z[1] + kezi.z[2] + kezi.z[3] + kezi.z[4] == 4)
                {
                    var baoMianzi = mianzi.Where(m => Regex.IsMatch(m, @"^z([1234])\1\1(?:[\+\=\-]|\1)(?!\!)")).ToList();
                    string baojia = null;

                    if (baoMianzi.Count > 3)
                    {
                        var match = Regex.Match(baoMianzi[3], @"[\+\=\-]");
                        if (match.Success)
                        {
                            baojia = match.Value;
                        }
                    }

                    var item = new Yaku
                    {
                        name = "大四喜",
                        fanshu = "**",
                    };

                    if (baojia != null)
                    {
                        item.baojia = baojia;
                    }

                    result.Add(item);
                }
                else if (kezi.z[1] + kezi.z[2] + kezi.z[3] + kezi.z[4] == 3 && Regex.IsMatch(mianzi[0], @"^z[1234]"))
                {
                    result.Add(new Yaku
                    {
                        name = "小四喜",
                        fanshu = "*",
                    });
                }

                return result;
            };

            List<Yaku> ziyise()
            {
                if (hudi.n_zipai == mianzi.Count)
                    return new List<Yaku> { new Yaku { name = "字一色", fanshu = "*" } };
                return new List<Yaku>();
            };

            List<Yaku> lvyise()
            {
                if (mianzi.Count(m => Regex.IsMatch(m, @"^[mp]")) > 0) return new List<Yaku>();
                if (mianzi.Count(m => Regex.IsMatch(m, @"^z[^6]")) > 0) return new List<Yaku>();
                if (mianzi.Count(m => Regex.IsMatch(m, @"^s.*[1579]")) > 0) return new List<Yaku>();
                return new List<Yaku> { new Yaku { name = "緑一色", fanshu = "*" } };
            };

            List<Yaku> qinglaotou()
            {
                if (hudi.n_yaojiu == 5 && hudi.n_kezi == 4 && hudi.n_zipai == 0)
                    return new List<Yaku> { new Yaku { name = "清老頭", fanshu = "*" } };
                return new List<Yaku>();
            };

            List<Yaku> sigangzi()
            {
                if (hudi.n_gangzi == 4) return new List<Yaku> { new Yaku { name = "四槓子", fanshu = "*" } };
                return new List<Yaku>();
            };

            List<Yaku> jiulianbaodeng()
            {
                if (mianzi.Count != 1) return new List<Yaku>();
                if (Regex.IsMatch(mianzi[0], @"^[mpsz]1112345678999"))
                    return new List<Yaku> { new Yaku { name = "純正九蓮宝燈", fanshu = "**" } };

                else return new List<Yaku> { new Yaku { name = "九蓮宝燈", fanshu = "*" } };
            };

            var damanguan = (pre_hupai.Count > 0 && pre_hupai[0].fanshu[0] == '*')
                            ? pre_hupai : new List<Yaku>();
            damanguan = damanguan
                .Concat(guoshiwushuang())
                .Concat(sianke())
                .Concat(dasanyuan())
                .Concat(sixihu())
                .Concat(ziyise())
                .Concat(lvyise())
                .Concat(qinglaotou())
                .Concat(sigangzi())
                .Concat(jiulianbaodeng());

            foreach (var h in damanguan)
            {
                if (!rule.ダブル役満あり) h.fanshu = "*";
                if (!rule.役満パオあり) h.baojia = null;
            }
            if (damanguan.Count > 0) return damanguan;

            var hupai = pre_hupai
                    .Concat(menqianqing())
                    .Concat(fanpai())
                    .Concat(pinghu())
                    .Concat(duanyaojiu())
                    .Concat(yibeikou())
                    .Concat(sansetongshun())
                    .Concat(yiqitongguan())
                    .Concat(hunquandaiyaojiu())
                    .Concat(qiduizi())
                    .Concat(duiduihu())
                    .Concat(sananke())
                    .Concat(sangangzi())
                    .Concat(sansetongke())
                    .Concat(hunlaotou())
                    .Concat(xiaosanyuan())
                    .Concat(hunyise())
                    .Concat(chunquandaiyaojiu())
                    .Concat(erbeikou())
                    .Concat(qingyise());

            if (hupai.Count > 0) hupai = hupai.Concat(post_hupai);

            return hupai;
        }

        public static List<Yaku> get_post_hupai(Shoupai shoupai, string rongpai, List<string> baopai, List<string> fubaopai)
        {
            var new_shoupai = shoupai.clone();
            if (rongpai != null)
            {
                new_shoupai.zimo(rongpai);
            }
            var paistr = new_shoupai.ToString();

            var post_hupai = new List<Yaku>();

            var suitstr = Regex.Matches(paistr, @"[mpsz][^mpsz,]*").Cast<Match>().Select(m => m.Value).ToList();

            var n_baopai = 0;
            MatchCollection nn;
            foreach (var _p in baopai)
            {
                var p = Majiang.Shan.zhenbaopai(_p);
                var regexp = new Regex(p[1].ToString());
                foreach (var _m in suitstr)
                {
                    if (_m[0] != p[0]) continue;
                    var m = _m.Replace('0', '5');
                    nn = regexp.Matches(m);
                    if (nn != null) n_baopai += nn.Count;
                }
            }
            if (n_baopai > 0) post_hupai.Add(new Yaku { name = "ドラ", fanshu = n_baopai.ToString() });

            var n_hongpai = 0;
            nn = Regex.Matches(paistr, "0");
            if (nn != null) n_hongpai = nn.Count;
            if (n_hongpai > 0) post_hupai.Add(new Yaku { name = "赤ドラ", fanshu = n_hongpai.ToString() });

            var n_fubaopai = 0;
            foreach (var _p in fubaopai ?? new List<string>())
            {
                var p = Majiang.Shan.zhenbaopai(_p);
                var regexp = new Regex(p[1].ToString());
                foreach (var _m in suitstr)
                {
                    if (_m[0] != p[0]) continue;
                    var m = _m.Replace('0', '5');
                    nn = regexp.Matches(m);
                    if (nn != null) n_fubaopai += nn.Count;
                }
            }
            if (n_fubaopai > 0) post_hupai.Add(new Yaku { name = "裏ドラ", fanshu = n_fubaopai.ToString() });

            return post_hupai;
        }

        public static HuleResult get_defen(int? fu, List<Yaku> hupai, string rongpai, HuleParam param)
        {
            if (hupai.Count == 0) return new HuleResult { defen = 0 };

            int menfeng = param.menfeng;
            int? fanshu = null, damanguan = null, defen = null, baseVal = null, baojia = null, defen2 = null, base2 = null, baojia2 = null;

            if (hupai[0].fanshu[0] == '*') 
            {
                fu = null;
                damanguan = !param.rule.役満の複合あり ? 1
                        : hupai.Select(h => h.fanshu.Length).Aggregate((x, y) => x + y);
                baseVal = 8000 * damanguan.Value;

                var h = hupai.Find(h => !h.baojia.IsNullOrEmpty());
                if (h != null) 
                {
                    baojia2 = (menfeng + new Dictionary<string, int> { { "+", 1 }, { "=", 2 }, { "-", 3 } }[h.baojia]) % 4;
                    base2 = 8000 * Math.Min(h.fanshu.Length, damanguan.Value);
                }
            }
            else 
            {
                fanshu = hupai.Select(h => int.Parse(h.fanshu)).Aggregate((x, y) => x + y);
                baseVal = (fanshu >= 13 && param.rule.数え役満あり)
                                        ? 8000
                    : (fanshu >= 11) ? 6000
                    : (fanshu >=  8) ? 4000
                    : (fanshu >=  6) ? 3000
                    : param.rule.切り上げ満貫あり && fu << (2 + fanshu.Value) == 1920
                            ? 2000
                            : Math.Min(fu.Value << (2 + fanshu.Value), 2000);
            }

            List<int> fenpei = new List<int> { 0, 0, 0, 0 };
            int chang = param.jicun.changbang;
            int lizhi = param.jicun.lizhibang;

            if (baojia2 != null) 
            {
                if (rongpai != null) base2 = base2 / 2;
                baseVal = baseVal - base2;
                defen2 = base2 * (menfeng == 0 ? 6 : 4);
                fenpei[menfeng] += defen2.Value;
                fenpei[baojia2.Value] -= defen2.Value;
            }
            else defen2 = 0;

            if (rongpai != null || baseVal == 0) 
            {
                baojia = (baseVal == 0)
                            ? baojia2
                            : (menfeng + new Dictionary<string, int> { { "+", 1 }, { "=", 2 }, { "-", 3 } }[rongpai[2].ToString()]) % 4;
                defen = (int)Math.Ceiling((double)baseVal * (menfeng == 0 ? 6 : 4) / 100) * 100;
                fenpei[menfeng] += defen.Value + chang * 300 + lizhi * 1000;
                fenpei[baojia.Value] -= defen.Value + chang * 300;
            }
            else 
            {
                int zhuangjia = (int)Math.Ceiling((double)baseVal * 2 / 100) * 100;
                int sanjia = (int)Math.Ceiling((double)baseVal / 100) * 100;
                if (menfeng == 0) 
                {
                    defen = zhuangjia * 3;
                    for (int l = 0; l < 4; l++) 
                    {
                        if (l == menfeng)
                            fenpei[l] += defen.Value + chang * 300 + lizhi * 1000;
                        else
                            fenpei[l] -= zhuangjia + chang * 100;
                    }
                }
                else 
                {
                    defen = zhuangjia + sanjia * 2;
                    for (int l = 0; l < 4; l++) 
                    {
                        if (l == menfeng)
                            fenpei[l] += defen.Value + chang * 300 + lizhi * 1000;
                        else if (l == 0)
                            fenpei[l] -= zhuangjia + chang * 100;
                        else
                            fenpei[l] -= sanjia + chang * 100;
                    }
                }
            }

            return new HuleResult
            {
                hupai = hupai,
                fu = fu,
                fanshu = fanshu,
                damanguan = damanguan,
                defen = defen.Value + defen2.Value,
                fenpei = fenpei
            };
        }

        /// <summary>
        /// shoupai の和了点を計算し、和了情報とともに返す。
        /// ツモ和了の場合は shoupai はツモ牌を加えた状態で rongpai は null とする。
        /// ロン和了の場合は shoupai はロン牌を加えない状態で rongpai はロンした 牌 とする。
        /// rongpai には誰がロンしたかを示す +(下家から和了)/=(対面から和了)/-(上家から和了) のフラグを付加する。
        /// param は以下の構造のオブジェクトであり、和了点計算に使用する場況情報を示す。
        /// </summary>
        /// <param name="shoupai"></param>
        /// <param name="rongpai"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static HuleResult hule(Shoupai shoupai, string rongpai, HuleParam param)
        {
            if (rongpai != null)
            {
                if (!Regex.IsMatch(rongpai, @"[\+\=\-]$")) throw new Exception(rongpai);
                rongpai = rongpai.Substring(0, 2) + rongpai.Substring(rongpai.Length - 1);
            }

            HuleResult max = null;

            var pre_hupai = get_pre_hupai(param.hupai);
            var post_hupai = get_post_hupai(shoupai, rongpai, param.baopai, param.fubaopai);

            foreach (var mianzi in hule_mianzi(shoupai, rongpai))
            {
                var hudi = get_hudi(mianzi, param.zhuangfeng, param.menfeng, param.rule);
                var hupai = get_hupai(mianzi, hudi, pre_hupai, post_hupai, param.rule);
                var rv = get_defen(hudi.fu, hupai, rongpai, param);

                if (max == null || rv.defen > max.defen
                    || rv.defen == max.defen
                        && ((rv.fanshu == null || rv.fanshu == 0) || rv.fanshu > max.fanshu
                            || rv.fanshu == max.fanshu && rv.fu > max.fu)) max = rv;
            }

            return max;
        }

        /// <summary>
        /// param で指定された値を元に Majiang.Util#hule の第3パラメータに使用する場況情報を返す。
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static HuleParam hule_param(HuleInput data = null)
        {
            if (data == null) data = new HuleInput();

            HuleParam rv = new HuleParam
            {
                rule = data.rule ?? new Majiang.Rule(),
                zhuangfeng = data.zhuangfeng ?? 0,
                menfeng = data.menfeng ?? 1,
                hupai = new Hupai
                {
                    lizhi = data.lizhi ?? 0,
                    yifa = data.yifa ?? false,
                    qianggang = data.qianggang ?? false,
                    lingshang = data.lingshang ?? false,
                    haidi = data.haidi ?? 0,
                    tianhu = data.tianhu ?? 0
                },
                baopai = data.baopai != null ? data.baopai.ToList() : new List<string>(),
                fubaopai = data.fubaopai != null ? data.fubaopai.ToList() : null,
                jicun = new Jicun
                {
                    changbang = data.changbang ?? 0,
                    lizhibang = data.lizhibang ?? 0
                }
            };

            return rv;
        }
    }
}
