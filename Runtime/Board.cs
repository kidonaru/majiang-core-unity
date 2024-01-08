using System.Collections.Generic;

namespace Majiang
{
    /// <summary>
    /// 卓情報インターフェース
    /// </summary>
    public interface IBoard
    {
        string title { get; }
        List<string> player { get; }
        int qijia { get; }
        int zhuangfeng { get; }
        int jushu { get; }
        int changbang { get; }
        int lizhibang { get; }
        List<int> defen { get; }
        IShan shan { get; }
        List<Shoupai> shoupai { get; }
        List<He> he { get; }
        List<int> player_id { get; }
        int lunban { get; }

        int menfeng(int id);
    }

    /// <summary>
    /// 卓情報
    /// </summary>
    public class Board : IBoard
    {
        /// <summary>
        /// 牌山の状態
        /// 卓情報用の簡易版
        /// </summary>
        public class Shan : IShan
        {
            /// <summary>
            /// ツモ可能な残り牌数を返す。
            /// </summary>
            public int paishu { get; set; }
            /// <summary>
            /// ドラ表示牌の配列を返す。
            /// </summary>
            public List<string> baopai { get; set; }
            /// <summary>
            /// 牌山固定前は null を返す。
            /// 牌山固定後は裏ドラ表示牌の配列を返す。
            /// </summary>
            public List<string> fubaopai { get; set; }

            public Shan(string baopai)
            {
                this.paishu = 136 - 13 * 4 - 14;
                this.baopai = new List<string> { baopai };
            }

            /// <summary>
            /// 次のツモ牌を返す。
            /// </summary>
            /// <param name="p"></param>
            /// <returns></returns>
            public string zimo(string p)
            {
                this.paishu--;
                return !p.IsNullOrEmpty() ? p : "_";
            }

            public string zimo()
            {
                return zimo(null);
            }

            /// <summary>
            /// カンドラを増やす。
            /// </summary>
            /// <param name="baopai"></param>
            public void kaigang(string baopai)
            {
                this.baopai.Add(baopai);
            }

            public Majiang.Shan kaigang()
            {
                throw new System.NotImplementedException();
            }

            public string gangzimo()
            {
                throw new System.NotImplementedException();
            }

            public Majiang.Shan close()
            {
                throw new System.NotImplementedException();
            }
        }

        /// <summary>
        /// 対局名を示す文字列。
        /// 牌譜#title と同じ。
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// 対局者情報。
        /// 仮東から順に並べる。 牌譜#player と同じ。
        /// </summary>
        public List<string> player { get; set; }
        /// <summary>
        /// 起家
        /// (0: 仮東、1: 仮南、2: 仮西、3: 仮北)。
        /// </summary>
        public int qijia { get; set; }
        /// <summary>
        /// 場風
        /// (0: 東、1: 南、2: 西、3: 北)。
        /// </summary>
        public int zhuangfeng { get; set; }
        /// <summary>
        /// 局数
        /// (0: 一局、1: 二局、2: 三局、3: 四局)。
        /// </summary>
        public int jushu { get; set; }
        /// <summary>
        /// 本場。
        /// </summary>
        public int changbang { get; set; }
        /// <summary>
        /// 現在の供託リーチ棒の数。
        /// </summary>
        public int lizhibang { get; set; }
        /// <summary>
        /// 現在の対局者の持ち点。
        /// 仮東から順に並べる。
        /// </summary>
        public List<int> defen { get; set; }
        /// <summary>
        /// その局の牌山を表す
        /// Majiang.Shan の簡易版。
        /// </summary>
        public IShan shan { get; set; }
        /// <summary>
        /// その局の対局者の手牌を表す
        /// Majiang.Shoupai のインスタンスの配列。その局の東家から順に並べる。
        /// </summary>
        public List<Shoupai> shoupai { get; set; }
        /// <summary>
        /// その局の対局者の捨て牌を表す
        /// Majiang.He のインスタンスの配列。その局の東家から順に並べる。
        /// </summary>
        public List<He> he { get; set; }
        /// <summary>
        /// 対局者の席順
        /// (0: 仮東、1: 仮南、2: 仮西、3: 仮北)の逆引き表。
        /// 起家が仮南で東一局なら、[ 1, 2, 3, 0 ] となる。
        /// </summary>
        public List<int> player_id { get; set; }
        /// <summary>
        /// 現在の手番
        /// (0: 東家、1: 南家、2: 西家、3: 北家)。
        /// </summary>
        public int lunban { get; set; }
        /// <summary>
        /// 成立待ちのリーチ宣言があるとき真。
        /// </summary>
        public bool _lizhi { get; set; }
        /// <summary>
        /// ダブロンの際に先の和了の 牌譜#fenpei を次の和了に引き継ぐ。
        /// </summary>
        public List<int> _fenpei { get; set; }

        public Board(Kaiju kaiju = null)
        {
            if (kaiju != null) this.kaiju(kaiju);
        }

        /// <summary>
        /// kaiju を卓情報に反映する。
        /// </summary>
        /// <param name="kaiju"></param>
        public void kaiju(Kaiju kaiju)
        {
            this.title = kaiju.title;
            this.player = kaiju.player;
            this.qijia = kaiju.qijia;

            this.zhuangfeng = 0;
            this.jushu = 0;
            this.changbang = 0;
            this.lizhibang = 0;
            this.defen = new List<int>{ 0, 0, 0, 0 };
            this.shan = null;
            this.shoupai = new List<Shoupai>{ null, null, null, null };
            this.he = new List<He>{ null, null, null, null };
            this.player_id = new List<int> { 0, 1, 2, 3 };
            this.lunban = -1;

            this._lizhi = false;
            this._fenpei = null;
        }

        /// <summary>
        /// 現在の自風を返す
        /// 席順 id (0: 仮東、1: 仮南、2: 仮西、3: 仮北)に対する現在の自風(0: 東、1: 南、2: 西、3: 北)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int menfeng(int id)
        {
            return (id + 4 - this.qijia + 4 - this.jushu) % 4;
        }

        /// <summary>
        /// qipai を卓情報に反映する。
        /// </summary>
        /// <param name="qipai"></param>
        public void qipai(Qipai qipai)
        {
            this.zhuangfeng = qipai.zhuangfeng;
            this.jushu = qipai.jushu;
            this.changbang = qipai.changbang;
            this.lizhibang = qipai.lizhibang;
            this.shan = new Shan(qipai.baopai);
            for (int l = 0; l < 4; l++)
            {
                string paistr = qipai.shoupai[l].Length > 0 ? qipai.shoupai[l] : new string('_', 13);
                this.shoupai[l] = Shoupai.fromString(paistr);
                this.he[l] = new He();
                this.player_id[l] = (this.qijia + this.jushu + l) % 4;
                this.defen[this.player_id[l]] = qipai.defen[l];
            }
            this.lunban = -1;

            this._lizhi = false;
            this._fenpei = null;
        }

        /// <summary>
        /// 成立待ちのリーチ宣言を成立させる。
        /// </summary>
        internal void lizhi()
        {
            if (this._lizhi)
            {
                this.defen[this.player_id[this.lunban]] -= 1000;
                this.lizhibang++;
                this._lizhi = false;
            }
        }

        /// <summary>
        /// zimo を卓情報に反映する。
        /// 牌譜#槓自摸 (メッセージ#槓自摸)の場合も本メソッドを使用する。
        /// </summary>
        /// <param name="zimo"></param>
        public void zimo(Zimo zimo)
        {
            this.lizhi();
            this.lunban = zimo.l;
            this.shoupai[zimo.l].zimo(((Shan) this.shan).zimo(zimo.p), false);
        }

        /// <summary>
        /// dapai を卓情報に反映する。
        /// </summary>
        /// <param name="dapai"></param>
        public void dapai(Dapai dapai)
        {
            this.lunban = dapai.l;
            this.shoupai[dapai.l].dapai(dapai.p, false);
            this.he[dapai.l].dapai(dapai.p);
            this._lizhi = dapai.p.EndsWith("*");
        }

        /// <summary>
        /// fulou を卓情報に反映する。
        /// </summary>
        /// <param name="fulou"></param>
        public void fulou(Fulou fulou)
        {
            this.lizhi();
            this.he[this.lunban].fulou(fulou.m);
            this.lunban = fulou.l;
            this.shoupai[fulou.l].fulou(fulou.m, false);
        }

        /// <summary>
        /// gang を卓情報に反映する。
        /// </summary>
        /// <param name="gang"></param>
        public void gang(Gang gang)
        {
            this.lunban = gang.l;
            this.shoupai[gang.l].gang(gang.m, false);
        }

        /// <summary>
        /// kaigang を卓情報に反映する。
        /// </summary>
        /// <param name="kaigang"></param>
        public void kaigang(Kaigang kaigang)
        {
            ((Shan) this.shan).kaigang(kaigang.baopai);
        }

        /// <summary>
        /// hule を卓情報に反映する。
        /// </summary>
        /// <param name="hule"></param>
        public void hule(Hule hule)
        {
            Shoupai shoupai = this.shoupai[hule.l];
            shoupai._fromString(hule.shoupai);
            if (hule.baojia != null) shoupai.dapai(shoupai.get_dapai().Pop());
            if (this._fenpei != null)
            {
                this.changbang = 0;
                this.lizhibang = 0;
                for (int l = 0; l < 4; l++)
                {
                    this.defen[this.player_id[l]] += this._fenpei[l];
                }
            }
            ((Shan) this.shan).fubaopai = hule.fubaopai;
            this._fenpei = hule.fenpei;
        }

        /// <summary>
        /// pingju を卓情報に反映する。
        /// </summary>
        /// <param name="pingju"></param>
        public void pingju(Pingju pingju)
        {
            if (!pingju.name.StartsWith("三家和")) this.lizhi();
            for (int l = 0; l < 4; l++)
            {
                if (pingju.shoupai.Count > l && pingju.shoupai[l] != null)
                    this.shoupai[l]._fromString(pingju.shoupai[l]);
            }
        }

        /// <summary>
        /// paipu を卓情報に反映する。
        /// </summary>
        /// <param name="paipu"></param>
        public void jieju(Paipu paipu)
        {
            for (int id = 0; id < 4; id++)
            {
                this.defen[id] = paipu.defen.Count > id ? paipu.defen[id] : 0;
            }
            this.lunban = -1;
        }
    }

}
