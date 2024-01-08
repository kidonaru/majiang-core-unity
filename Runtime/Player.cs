using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Majiang
{
    /// <summary>
    /// プレイヤー
    /// </summary>
    public class Player
    {
        /// <summary>
        /// メッセージ#開局 で通知された自身の席順
        /// (0: 仮東、1: 仮南、2: 仮西、3: 仮北)。
        /// </summary>
        public int _id;
        /// <summary>
        /// Majiang.Player#action 呼び出し時に指定された応答送信用関数。
        /// </summary>
        public Action<Reply> _callback;
        /// <summary>
        /// メッセージ#開局 で通知された対局の ルール。
        /// </summary>
        public Rule _rule;
        /// <summary>
        /// Majiang.Board で設定する 卓情報。
        /// </summary>
        public Board _model;
        /// <summary>
        /// 現在の局の自風。
        /// (0: 東、1: 南、2: 西、3: 北)
        /// </summary>
        public int _menfeng;
        /// <summary>
        /// 第一ツモ巡の間は true。
        /// </summary>
        public bool _diyizimo;
        /// <summary>
        /// 現在の局で全ての対局者が行ったカンの総数。
        /// </summary>
        public int _n_gang;
        /// <summary>
        /// 自身のフリテン状態。ロン和了可能なら true。
        /// </summary>
        public bool _neng_rong;
        /// <summary>
        /// メッセージ#終局 で伝えられた対戦結果の 牌譜。
        /// </summary>
        public Paipu _paipu;

        /// <summary>
        /// プレイヤーのアクションに対する返答
        /// </summary>
        public List<Reply> _reply;

        public Player()
        {
            this._model = new Board();
        }

        /// <summary>
        /// msg に対応するメソッドを呼び出す。
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="callback"></param>
        public virtual void action(Message msg, Action<Reply> callback = null)
        {
            this._callback = callback;

            if (msg.kaiju != null) this.kaiju(msg.kaiju);
            else if (msg.qipai != null) this.qipai(msg.qipai);
            else if (msg.zimo != null) this.zimo(msg.zimo);
            else if (msg.dapai != null) this.dapai(msg.dapai);
            else if (msg.fulou != null) this.fulou(msg.fulou);
            else if (msg.gang != null) this.gang(msg.gang);
            else if (msg.gangzimo != null) this.zimo(msg.gangzimo, true);
            else if (msg.kaigang != null) this.kaigang(msg.kaigang);
            else if (msg.hule != null) this.hule(msg.hule);
            else if (msg.pingju != null) this.pingju(msg.pingju);
            else if (msg.jieju != null) this.jieju(msg.jieju);
        }

        /// <summary>
        /// 自身の手牌を返す。
        /// </summary>
        public Shoupai shoupai => this._model.shoupai[this._menfeng];

        /// <summary>
        /// 自身の捨て牌を返す。
        /// </summary>
        public He he => this._model.he[this._menfeng];

        /// <summary>
        /// 牌山を返す。
        /// </summary>
        public IShan shan => this._model.shan;

        /// <summary>
        /// 自身の手牌がテンパイしている場合、和了牌の一覧を返す。
        /// テンパイしていない場合は空の配列を返す。
        /// </summary>
        public List<string> hulepai
        {
            get
            {
                List<string> ret = null;
                if (Majiang.Util.xiangting(this.shoupai) == 0)
                {
                    ret = Majiang.Util.tingpai(this.shoupai);
                }
                return ret ?? new List<string>();
            }
        }

        /// <summary>
        /// kaiju から 卓情報 を初期化し、Majiang.Player#action_kaiju を呼び出し応答を返す。
        /// </summary>
        /// <param name="kaiju"></param>
        public void kaiju(Kaiju kaiju)
        {
            this._id = kaiju.id;
            this._rule = kaiju.rule;
            this._model.kaiju(kaiju);

            if (this._callback != null) this.action_kaiju(kaiju);
        }

        /// <summary>
        /// qipai から 卓情報 を設定し、Majiang.Player#action_qipai を呼び出し応答を返す。
        /// </summary>
        /// <param name="qipai"></param>
        public virtual void qipai(Qipai qipai)
        {
            this._model.qipai(qipai);
            this._menfeng = this._model.menfeng(this._id);
            this._diyizimo = true;
            this._n_gang = 0;
            this._neng_rong = true;

            if (this._callback != null) this.action_qipai(qipai);
        }

        /// <summary>
        /// zimo から 卓情報 を設定し、Majiang.Player#action_zimo を呼び出し応答を返す。
        /// gangzimo が真の場合は槓自摸を表す。
        /// </summary>
        /// <param name="zimo"></param>
        /// <param name="gangzimo"></param>
        public virtual void zimo(Zimo zimo, bool gangzimo = false)
        {
            this._model.zimo(zimo);
            if (gangzimo) this._n_gang++;

            if (this._callback != null) this.action_zimo(zimo, gangzimo);
        }

        /// <summary>
        /// dapai から 卓情報 を設定し、Majiang.Player#action_dapai を呼び出し応答を返す。
        /// </summary>
        /// <param name="dapai"></param>
        public virtual void dapai(Dapai dapai)
        {
            if (dapai.l == this._menfeng)
            {
                if (!this.shoupai.lizhi) this._neng_rong = true;
            }

            this._model.dapai(dapai);

            if (this._callback != null) this.action_dapai(dapai);

            if (dapai.l == this._menfeng)
            {
                this._diyizimo = false;
                if (this.hulepai.Any(p => this.he.find(p))) this._neng_rong = false;
            }
            else
            {
                string s = dapai.p[0].ToString();
                int n = Util.get_pai_rank(dapai.p);
                if (this.hulepai.Any(p => p == s + n)) this._neng_rong = false;
            }
        }

        /// <summary>
        /// fulou から 卓情報 を設定し、Majiang.Player#action_fulou を呼び出し応答を返す。
        /// </summary>
        /// <param name="fulou"></param>
        public virtual void fulou(Fulou fulou)
        {
            this._model.fulou(fulou);

            if (this._callback != null) this.action_fulou(fulou);

            this._diyizimo = false;
        }

        /// <summary>
        /// gang から 卓情報 を設定し、Majiang.Player#action_gang を呼び出し応答を返す。
        /// </summary>
        /// <param name="gang"></param>
        public virtual void gang(Gang gang)
        {
            this._model.gang(gang);

            if (this._callback != null) this.action_gang(gang);

            this._diyizimo = false;
            if (gang.l != this._menfeng && !Regex.IsMatch(gang.m, @"^[mpsz]\d{4}$"))
            {
                string s = gang.m[0].ToString();
                int n = Util.get_pai_rank(gang.m);
                if (this.hulepai.Any(p => p == s + n)) this._neng_rong = false;
            }
        }

        /// <summary>
        /// kaigang から 卓情報 を設定する。
        /// </summary>
        /// <param name="kaigang"></param>
        public virtual void kaigang(Kaigang kaigang)
        {
            this._model.kaigang(kaigang);
        }

        /// <summary>
        /// hule から 卓情報 を設定し、Majiang.Player#action_hule を呼び出し応答を返す。
        /// </summary>
        /// <param name="hule"></param>
        public void hule(Hule hule)
        {
            this._model.hule(hule);
            if (this._callback != null) this.action_hule(hule);
        }

        /// <summary>
        /// pingju から 卓情報 を設定し、Majiang.Player#action_pingju を呼び出し応答を返す。
        /// </summary>
        /// <param name="pingju"></param>
        public void pingju(Pingju pingju)
        {
            this._model.pingju(pingju);
            if (this._callback != null) this.action_pingju(pingju);
        }

        /// <summary>
        /// Majiang.Player#action_jieju を呼び出し応答を返す。
        /// </summary>
        /// <param name="paipu"></param>
        public void jieju(Paipu paipu)
        {
            this._model.jieju(paipu);
            this._paipu = paipu;
            if (this._callback != null) this.action_jieju(paipu);
        }

        /// <summary>
        /// ルール と 卓情報 を使用して Majiang.Game#static-get_dapai を呼び出し、shoupai が打牌可能な牌の一覧を返す。
        /// </summary>
        /// <param name="shoupai"></param>
        /// <returns></returns>
        public List<string> get_dapai(Shoupai shoupai) {
            return Majiang.Game.get_dapai(this._rule, shoupai);
        }

        /// <summary>
        /// ルール と 卓情報 を使用して Majiang.Game#static-get_chi_mianzi を呼び出し、shoupai が p でチー可能な面子の一覧を返す。
        /// </summary>
        /// <param name="shoupai"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public List<string> get_chi_mianzi(Shoupai shoupai, string p) {
            return Majiang.Game.get_chi_mianzi(this._rule, shoupai, p, this.shan.paishu);
        }

        /// <summary>
        /// ルール と 卓情報 を使用して Majiang.Game#static-get_peng_mianzi を呼び出し、shoupai が p でポン可能な面子の一覧を返す。
        /// </summary>
        /// <param name="shoupai"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public List<string> get_peng_mianzi(Shoupai shoupai, string p) {
            return Majiang.Game.get_peng_mianzi(this._rule, shoupai, p, this.shan.paishu);
        }

        /// <summary>
        /// ルール と 卓情報 を使用して Majiang.Game#static-get_gang_mianzi を呼び出し、shoupai がカン可能な面子の一覧を返す。 p が指定された場合は大明槓、null の場合は暗槓と加槓が対象になる。
        /// </summary>
        /// <param name="shoupai"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public List<string> get_gang_mianzi(Shoupai shoupai, string p = null) {
            return Majiang.Game.get_gang_mianzi(this._rule, shoupai, p, this.shan.paishu, this._n_gang);
        }

        /// <summary>
        /// ルール と 卓情報 を使用して Majiang.Game#static-allow_lizhi を呼び出し、shoupai からリーチ可能か判定する。 p が null のときはリーチ可能な打牌一覧を返す。
        /// p が牌のときは p を打牌してリーチ可能なら true を返す。
        /// </summary>
        /// <param name="shoupai"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public List<string> allow_lizhi(Shoupai shoupai, string p = null) {
            return Majiang.Game.allow_lizhi(this._rule, shoupai, p, this.shan.paishu, this._model.defen[this._id]);
        }

        /// <summary>
        /// ルール と 卓情報 を使用して Majiang.Game#static-allow_hule を呼び出し、shoupai で和了可能か判定する。 p が null のときはツモ和了可能なら true を返す。
        /// p が牌のときは p でロン和了可能なら true を返す。
        /// </summary>
        /// <param name="shoupai"></param>
        /// <param name="p"></param>
        /// <param name="hupai"></param>
        /// <returns></returns>
        public bool allow_hule(Shoupai shoupai, string p, bool hupai = false) {
            hupai = hupai || shoupai.lizhi || (this.shan.paishu == 0);
            return Majiang.Game.allow_hule(this._rule, shoupai, p, this._model.zhuangfeng, this._menfeng, hupai, this._neng_rong);
        }

        /// <summary>
        /// ルール と 卓情報 を使用して Majiang.Game#static-allow_pingju を呼び出し、shoupai で九種九牌流局可能か判定する。
        /// </summary>
        /// <param name="shoupai"></param>
        /// <returns></returns>
        public bool allow_pingju(Shoupai shoupai) {
            return Majiang.Game.allow_pingju(this._rule, shoupai, this._diyizimo);
        }

        /// <summary>
        /// ルール と 卓情報 を使用して Majiang.Game#static-allow_no_daopai を呼び出し、shoupai で「テンパイ宣言」可能か判定する。
        /// </summary>
        /// <param name="shoupai"></param>
        /// <returns></returns>
        public bool allow_no_daopai(Shoupai shoupai) {
            return Majiang.Game.allow_no_daopai(this._rule, shoupai, this.shan.paishu);
        }

        ////// virtual methods

        /// <summary>
        /// kaiju を確認し空応答する処理を実装する。
        /// </summary>
        /// <param name="kaiju"></param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void action_kaiju(Kaiju kaiju)
        {
             throw new NotImplementedException();
        }

        /// <summary>
        /// qipai を確認し空応答する処理を実装する。
        /// </summary>
        /// <param name="qipai"></param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void action_qipai(Qipai qipai)
        {
             throw new NotImplementedException();
        }

        /// <summary>
        /// zimo から適切な応答(打牌・槓・和了・倒牌)を選択し返す処理を実装する。
        /// gangzimo が真の場合は槓自摸を表す。
        /// </summary>
        /// <param name="zimo"></param>
        /// <param name="gangzimo"></param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void action_zimo(Zimo zimo, bool gangzimo)
        {
             throw new NotImplementedException();
        }

        /// <summary>
        /// dapai から適切な応答(副露・和了・倒牌)を選択し返す処理を実装する。
        /// </summary>
        /// <param name="dapai"></param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void action_dapai(Dapai dapai)
        {
             throw new NotImplementedException();
        }

        /// <summary>
        /// fulou から適切な応答(打牌)を選択し返す処理を実装する。
        /// </summary>
        /// <param name="fulou"></param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void action_fulou(Fulou fulou)
        {
             throw new NotImplementedException();
        }

        /// <summary>
        /// gang から適切な応答(打牌・槓・和了)を選択し返す処理を実装する。
        /// </summary>
        /// <param name="gang"></param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void action_gang(Gang gang)
        {
             throw new NotImplementedException();
        }

        /// <summary>
        /// hule を確認し空応答する処理を実装する。
        /// </summary>
        /// <param name="hule"></param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void action_hule(Hule hule)
        {
             throw new NotImplementedException();
        }

        /// <summary>
        /// pingju を確認し空応答する処理を実装する。
        /// </summary>
        /// <param name="pingju"></param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void action_pingju(Pingju pingju)
        {
             throw new NotImplementedException();
        }

        /// <summary>
        /// paipu を処理し空応答する処理を実装する。
        /// </summary>
        /// <param name="paipu"></param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void action_jieju(Paipu paipu)
        {
             throw new NotImplementedException();
        }
    }
}
