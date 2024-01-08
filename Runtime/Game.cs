using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Random = System.Random;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("majiang-core.test")]
//[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("majiang-core.editor")]
namespace Majiang
{
    public class Reply : EntityBase
    {
        /// <summary>
        /// 倒牌
        /// </summary>
        public string daopai;
        /// <summary>
        /// 打牌
        /// </summary>
        public string dapai;
        /// <summary>
        /// 副露
        /// </summary>
        public string fulou;
        /// <summary>
        /// 槓
        /// </summary>
        public string gang;
        /// <summary>
        /// 和了
        /// </summary>
        public string hule;
        /// <summary>
        /// 流局
        /// </summary>
        public string pingju;
        /// <summary>
        /// 終局
        /// </summary>
        public string jieju;

        public override string ToString()
        {
            var parameters = new List<string>
            {
                daopai != null ? $"daopai={daopai}" : null,
                dapai != null ? $"dapai={dapai}" : null,
                fulou != null ? $"fulou={fulou}" : null,
                gang != null ? $"gang={gang}" : null,
                hule != null ? $"hule={hule}" : null,
                pingju != null ? $"pingju={pingju}" : null,
                jieju != null ? $"jieju={jieju}" : null
            };

            return $"Reply({string.Join(", ", parameters.Where(p => p != null))})";
        }
    }

    /// <summary>
    /// 卓情報
    /// 描画の際に使用する卓に関する情報を表現するオブジェクト
    /// </summary>
    public class BoardModel : EntityBase, IBoard
    {
        /// <summary>
        /// 対局名を示す文字列。 牌譜#title と同じ。
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// 対局者情報。
        /// 仮東から順に並べる。 牌譜#player と同じ。
        /// </summary>
        public List<string> player { get; set; }
        /// <summary>
        /// 起家
        /// (0: 仮東、1: 仮南、2: 仮西、3: 仮北)
        /// </summary>
        public int qijia { get; set; }
        /// <summary>
        /// 場風
        /// (0: 東、1: 南、2: 西、3: 北)
        /// </summary>
        public int zhuangfeng { get; set; }
        /// <summary>
        /// 局数
        /// (0: 一局、1: 二局、2: 三局、3: 四局)
        /// </summary>
        public int jushu { get; set; }
        /// <summary>
        /// 本場
        /// </summary>
        public int changbang { get; set; }
        /// <summary>
        /// 現在の供託リーチ棒の数
        /// </summary>
        public int lizhibang { get; set; }
        /// <summary>
        /// 現在の対局者の持ち点
        /// 仮東から順に並べる。
        /// </summary>
        public List<int> defen { get; set; }
        /// <summary>
        /// 牌山情報
        /// </summary>
        public IShan shan { get; set; }
        /// <summary>
        /// 対局者の手牌
        /// その局の東家から順に並べる。
        /// </summary>
        public List<Shoupai> shoupai { get; set; }
        /// <summary>
        /// 対局者の捨て牌
        /// その局の東家から順に並べる。
        /// </summary>
        public List<He> he { get; set; }
        /// <summary>
        /// 対局者の席順(0: 仮東、1: 仮南、2: 仮西、3: 仮北)の逆引き表。
        /// 起家が仮南で東一局なら、[ 1, 2, 3, 0 ] となる。
        /// </summary>
        public List<int> player_id { get; set; }
        /// <summary>
        /// 現在の手番
        /// (0: 東家、1: 南家、2: 西家、3: 北家)
        /// </summary>
        public int lunban { get; set; }

        /// <summary>
        /// 自風を取得
        /// 席順 id (0: 仮東、1: 仮南、2: 仮西、3: 仮北)に対する現在の自風(0: 東、1: 南、2: 西、3: 北)を返す。
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int menfeng(int id)
        {
            return (id + 4 - this.qijia + 4 - this.jushu) % 4;
        }

        public override string ToString()
        {
            var parameters = new List<string>
            {
                title != null ? $"title={title}" : null,
                player != null ? $"player={player.JoinJS()}" : null,
                $"qijia={qijia}",
                $"zhuangfeng={zhuangfeng}",
                $"jushu={jushu}",
                $"changbang={changbang}",
                $"lizhibang={lizhibang}",
                defen != null ? $"defen={defen.JoinJS()}" : null,
                shan != null ? $"shan={shan}" : null,
                shoupai != null ? $"shoupai={shoupai.JoinJS()}" : null,
                he != null ? $"he={he.JoinJS()}" : null,
                player_id != null ? $"player_id={player_id.JoinJS()}" : null,
                $"lunban={lunban}",
            };

            return $"BoardModel({string.Join(", ", parameters.Where(p => p != null))})";
        }

        public override object Clone()
        {
            return new BoardModel
            {
                title = title,
                player = player.Concat(),
                qijia = qijia,
                zhuangfeng = zhuangfeng,
                jushu = jushu,
                changbang = changbang,
                lizhibang = lizhibang,
                defen = defen.Concat(),
                shan = (Shan) ((Shan) shan)?.Clone(),
                shoupai = shoupai?.Clone(),
                he = he?.Clone(),
                player_id = player_id.Concat(),
                lunban = lunban
            };
        }
    }

    /// <summary>
    /// 局進行を実現するクラス
    /// </summary>
    public class Game
    {
        /// <summary>
        /// プレイヤーの配列
        /// </summary>
        public List<Player> _players;
        /// <summary>
        /// 対局終了時に呼び出す関数。
        /// </summary>
        public Action<Paipu> _callback;
        /// <summary>
        /// ルール
        /// </summary>
        public Rule _rule;
        /// <summary>
        /// 卓情報
        /// </summary>
        public BoardModel _model;
        /// <summary>
        /// 卓情報 を描画するクラス。
        /// Majiang.Game からは適切なタイミングでメソッドを呼び出して描画のきっかけを与える。
        /// </summary>
        public View _view;
        /// <summary>
        /// 牌譜
        /// </summary>
        public Paipu _paipu;
        /// <summary>
        /// 現在のステータス
        /// Majiang.Game#call_players を呼び出した際の type を保存する。
        /// </summary>
        public string _status;
        /// <summary>
        /// 対局者からの応答を格納する配列。
        /// Majiang.Game#call_players 呼び出し時に配列を生成する。
        /// </summary>
        public ConcurrentDictionary<int, Reply> _reply;
        /// <summary>
        /// 最終局(オーラス)の局数。
        /// 東風戦の場合、初期値は 3。東南戦なら 7。
        /// 延長戦により最終局が移動する場合はこの値を変更する。
        /// </summary>
        public int _max_jushu;
        /// <summary>
        /// 第一ツモ巡の間は true。
        /// </summary>
        public bool _diyizimo;
        /// <summary>
        /// 四風連打の可能性がある間は true。
        /// </summary>
        public bool _fengpai;
        /// <summary>
        /// 最後に打牌した 牌。
        /// 次の打牌で上書きする。
        /// </summary>
        public string _dapai;
        /// <summary>
        /// 現在処理中のカンの 面子。
        /// 開槓すると null に戻す。
        /// </summary>
        public string _gang;
        /// <summary>
        /// 各対局者(その局の東家からの順)のリーチ状態を示す配列。
        /// 0: リーチなし、1: 通常のリーチ、2: ダブルリーチ。
        /// </summary>
        public List<int> _lizhi;
        /// <summary>
        /// 各対局者が一発可能かを示す配列。
        /// 添え字は手番(0: 東、1: 南、2: 西、3: 北)。
        /// </summary>
        public List<bool> _yifa;
        /// <summary>
        /// 各対局者が行ったカンの数。
        /// 添え字は手番(0: 東、1: 南、2: 西、3: 北)。
        /// </summary>
        public List<int> _n_gang;
        /// <summary>
        /// 各対局者のフリテン状態。
        /// 添え字は手番(0: 東、1: 南、2: 西、3: 北)。
        /// ロン和了可能なら true。
        /// </summary>
        public List<bool> _neng_rong;
        /// <summary>
        /// 和了応答した対局者の手番(0: 東、1: 南、2: 西、3: 北)の配列。
        /// 南家、西家のダブロンの時は [ 1, 2 ] となる。
        /// </summary>
        public List<int> _hule;
        /// <summary>
        /// 処理中の和了が槍槓のとき qiangang、嶺上開花のとき lingshang、それ以外なら null。
        /// </summary>
        public string _hule_option;
        /// <summary>
        /// 途中流局の処理中のとき true。
        /// </summary>
        public bool _no_game;
        /// <summary>
        /// 連荘の処理中のとき true。
        /// </summary>
        public bool _lianzhuang;
        /// <summary>
        /// 現在処理中の局開始時の積み棒の数。
        /// </summary>
        public int _changbang;
        /// <summary>
        /// 現在処理中の和了、あるいは流局で移動する点数の配列。
        /// 添え字は手番(0: 東、1: 南、2: 西、3: 北)。
        /// </summary>
        public List<int> _fenpei;
        /// <summary>
        /// true の場合、同期モードとなり、setTimeout() による非同期呼び出しは行わない。
        /// </summary>
        public bool _sync;
        /// <summary>
        /// 関数が設定されている場合、Majiang.Game#next 呼び出しの際にその関数を呼び出して処理を停止する。
        /// </summary>
        public Action _stop;
        /// <summary>
        /// 局の進行速度。0～5 で指定する。
        /// 初期値は 3。 指定された速度 × 200(ms) で Majiang.Game#next を呼び出すことで局の進行速度を調整する。
        /// </summary>
        public int _speed;
        /// <summary>
        /// ダイアログへの応答速度(ms)。
        /// 初期値は 0。 指定された時間後に Majiang.Game#next を呼び出す。
        /// </summary>
        public int _wait;
        /// <summary>
        /// 非同期で Majiang.Game#next を呼び出すタイマーのID。
        /// 値が設定されていれば非同期呼出し待ちであり、clearTimeout() を呼び出せば非同期呼出しをキャンセルできる。
        /// </summary>
        public object _timeout_id;
        /// <summary>
        /// Majiang.Game#jieju から呼ぶ出される関数。
        /// Majiang.Game#set-handler で設定する。
        /// </summary>
        public Action _handler;

        /// <summary>
        /// 同期用ロック
        /// </summary>
        private readonly object _lock = new object();
        /// <summary>
        /// コルーチン用MonoBehaviour
        /// 設定されているときは、setTimeoutでコルーチンを実行する
        /// </summary>
        public MonoBehaviour _mono_behaviour;

        /// <summary>
        /// コンストラクタ
        /// players で指定された4名を対局者とし、rule で指定されたルールにしたがい対局を行う。
        /// 対局終了時に callback で指定した関数が呼ばれる(対局の牌譜が引数で渡される)。
        /// title で牌譜に残すタイトルを指定できる。
        /// rule を省略した場合は、Majiang.rule() の呼び出しで得られるルールの初期値が採用される。
        /// </summary>
        /// <param name="players">プレイヤー</param>
        /// <param name="callback">コールバック</param>
        /// <param name="rule">ルール</param>
        /// <param name="title">タイトル</param>
        public Game(
            List<Player> players = null,
            Action<Paipu> callback = null,
            Rule rule = null,
            string title = "")
        {
            this._players = players;
            this._callback = callback;
            this._rule = rule ?? new Rule();

            this._model = new BoardModel
            {
                title = title ?? "電脳麻将\n" + DateTime.Now,
                player = new List<string> { "私", "下家", "対面", "上家" },
                qijia = 0,
                zhuangfeng = 0,
                jushu = 0,
                changbang = 0,
                lizhibang = 0,
                defen = Enumerable.Repeat(0, 4).Select(x => this._rule.配給原点).ToList(),
                shan = null,
                shoupai = new List<Shoupai> { null, null, null, null },
                he = new List<He> { null, null, null, null },
                player_id = new List<int> { 0, 1, 2, 3 },
                lunban = 0
            };

            this._view = null;
            this._status = "";
            this._reply = new ConcurrentDictionary<int, Reply>();
            this._sync = false;
            this._stop = null;
            this._speed = 3;
            this._wait = 0;
            this._timeout_id = null;
            this._handler = null;
        }

        /// <summary>
        /// 卓情報を返す。
        /// </summary>
        public BoardModel model => this._model;

        /// <summary>
        /// ビューを設定
        /// </summary>
        public View view { set => this._view = value; }

        /// <summary>
        /// ゲームの速度の取得、または設定
        /// </summary>
        public int speed
        {
            get => this._speed;
            set => this._speed = value;
        }

        /// <summary>
        /// ゲームの待ち時間を設定
        /// </summary>
        public int wait { set => this._wait = value; }

        /// <summary>
        /// callback を Majiang.Game#jieju から呼び出すように設定する。
        /// </summary>
        public Action handler { set => this._handler = value; }

        /// <summary>
        /// (Unity) MonoBehaviourを設定
        /// 設定されているときは、コルーチンを実行する
        /// </summary>
        public MonoBehaviour mono_behaviour { set => this._mono_behaviour = value; }

        /// <summary>
        /// プレイヤー名を設定
        /// </summary>
        public List<string> player { set => this._model.player = value; }

        /// <summary>
        /// インスタンス変数 _paipu の適切な位置に摸打情報を追加する。
        /// </summary>
        /// <param name="paipu"></param>
        internal void add_paipu(Message paipu)
        {
            this._paipu.log[this._paipu.log.Count - 1].Add(paipu);
        }

        /// <summary>
        /// timeout で指定した時間(ms)休止した後に callback を呼び出す。
        /// ゲームに「タメ」を作るためにチー/ポンなどの発声のタイミングで呼び出される。
        /// timeout の指定がない場合は、インスタンス変数 _speed に応じて待ち時間を決定するが、最低でも 500ms は待ち合わせる。
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="timeout"></param>
        internal void delay(Action callback, int? timeout = null)
        {
            if (this._sync)
            {
                callback?.Invoke();
                return;
            }

            timeout = this._speed == 0 ? 0
                    : timeout == null ? Math.Max(500, this._speed * 200)
                    : timeout;
            set_timeout(callback, timeout.Value);
        }

        /// <summary>
        /// 非同期モードの対局を停止する。
        /// 停止の際に callback を呼び出す。
        /// </summary>
        /// <param name="callback"></param>
        public void stop(Action callback = null)
        {
            this._stop = callback ?? (() => { });
        }

        /// <summary>
        /// 非同期モードの対局を再開する。
        /// </summary>
        public void start()
        {
            if (this._timeout_id != null) return;
            this._stop = null;
            this._timeout_id = set_timeout(this.next, 0);
        }

        /// <summary>
        /// 対局者に msg を通知する。
        /// 対局者からの応答はない。 type は メッセージ の種別を示す。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="msg"></param>
        internal void notify_players(string type, List<Message> msg)
        {
            for (var l = 0; l < 4; l++)
            {
                var id = this._model.player_id[l];
                var m = msg[l];

                if (this._sync)
                    this._players[id].action(m);
                else
                    set_timeout(() => this._players[id].action(m), 0);
            }
        }

        /// <summary>
        /// 対局者に msg を通知する。
        /// 対局者からの応答を待って、Majiang.Game#next が非同期に呼び出される。
        /// type は メッセージ の種別を示す。
        /// timeout で Majiang.Game#next 呼び出しまでの待ち時間(ms)を指定し、局の進行速度を調整することもできる。
        /// timeout の指定がない場合は、インスタンス変数 _speed に応じて待ち時間を決定する。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="msg"></param>
        /// <param name="timeout"></param>
        internal void call_players(string type, List<Message> msg, int? timeout = null)
        {
            timeout = this._speed == 0 ? 0
                    : timeout == null ? this._speed * 200
                    : timeout;
            this._status = type;
            this._reply.Clear();
            for (int l = 0; l < 4; l++)
            {
                int id = this._model.player_id[l];
                var m = msg[l];
                if (this._sync)
                    this._players[id].action(
                            m, reply => this.reply(id, reply));
                else set_timeout(() =>
                        {
                            this._players[id].action(
                                m, reply => this.reply(id, reply));
                        }, 0);
            }
            if (!this._sync)
                this._timeout_id = set_timeout(this.next, timeout.Value);
        }

        /// <summary>
        /// 対局者が応答の際に呼び出す。
        /// id は対局者の席順(0〜3)、reply は応答内容。
        /// </summary>
        /// <param name="id"></param>
        /// <param name="reply"></param>
        internal void reply(int id, Reply reply)
        {
            lock (_lock)
            {
                this._reply[id] = reply ?? new Reply();
                if (this._sync) return;
                if (this._reply.Count < 4) return;
                if (this._timeout_id == null)
                    this._timeout_id = set_timeout(this.next, 0);
            }
        }

        /// <summary>
        /// 対局者からの応答を読み出し、対局の次のステップに進む。
        /// </summary>
        internal void next()
        {
            lock (_lock)
            {
                this.cancel_timeout();
                this._timeout_id = null;
                if (this._reply.Count < 4) return;
                if (this._stop != null)
                {
                    this._stop();
                    return;
                }

                if (this._status == "kaiju") this.reply_kaiju();
                else if (this._status == "qipai") this.reply_qipai();
                else if (this._status == "zimo") this.reply_zimo();
                else if (this._status == "dapai") this.reply_dapai();
                else if (this._status == "fulou") this.reply_fulou();
                else if (this._status == "gang") this.reply_gang();
                else if (this._status == "gangzimo") this.reply_zimo();
                else if (this._status == "hule") this.reply_hule();
                else if (this._status == "pingju") this.reply_pingju();
                else this._callback?.Invoke(this._paipu);
            }
        }

        /// <summary>
        /// デバッグ用。同期モードで対局を開始する。
        /// 対局終了まで一切の非同期呼び出しは行わず、無停止で対局を完了する。
        /// </summary>
        /// <returns></returns>
        public Game do_sync()
        {
            this._sync = true;

            this.kaiju();

            for (; ; )
            {
                if (this._status == "kaiju") this.reply_kaiju();
                else if (this._status == "qipai") this.reply_qipai();
                else if (this._status == "zimo") this.reply_zimo();
                else if (this._status == "dapai") this.reply_dapai();
                else if (this._status == "fulou") this.reply_fulou();
                else if (this._status == "gang") this.reply_gang();
                else if (this._status == "gangzimo") this.reply_zimo();
                else if (this._status == "hule") this.reply_hule();
                else if (this._status == "pingju") this.reply_pingju();
                else break;
            }

            this._callback?.Invoke(this._paipu);

            return this;
        }

        /// <summary>
        /// 非同期モードで対局を開始する。
        /// qijia で起家を指定することもできる(0〜3)。
        /// qijia を指定しない場合はランダムに起家を決定する。
        /// </summary>
        /// <param name="qijia"></param>
        public virtual void kaiju(int? qijia = null)
        {
            this._model.qijia = qijia ?? (int)Math.Floor(new Random().NextDouble() * 4);
            this._max_jushu = this._rule.場数 == 0 ? 0 : this._rule.場数 * 4 - 1;
            this._paipu = new Paipu
            {
                title = this._model.title,
                player = this._model.player,
                qijia = this._model.qijia,
                log = new List<List<Message>>(),
                defen = new List<int>(this._model.defen),
                point = new List<string>(),
                rank = new List<int>()
            };
            var msg = new List<Message>{null, null, null, null};
            for (int id = 0; id < 4; id++)
            {
                msg[id] = new Message
                {
                    kaiju = new Kaiju
                    {
                        id = id,
                        rule = this._rule,
                        title = this._paipu.title,
                        player = this._paipu.player,
                        qijia = this._paipu.qijia,
                    }
                };
            }
            this.call_players("kaiju", msg, 0);
            if (this._view != null) this._view.kaiju();
        }

        /// <summary>
        /// 配牌の局進行を行う。
        /// </summary>
        /// <param name="shan"></param>
        public virtual void qipai(Shan shan = null)
        {
            var model = this._model;

            model.shan = shan ?? new Shan(this._rule);
            for (int l = 0; l < 4; l++)
            {
                var qipai = new List<string>();
                for (int i = 0; i < 13; i++)
                {
                    qipai.Add(model.shan.zimo());
                }
                model.shoupai[l] = new Shoupai(qipai);
                model.he[l] = new He();
                model.player_id[l] = (model.qijia + model.jushu + l) % 4;
            }
            model.lunban = -1;

            this._diyizimo = true;
            this._fengpai = this._rule.途中流局あり;

            this._dapai = null;
            this._gang = null;

            this._lizhi = new List<int> { 0, 0, 0, 0 };
            this._yifa = new List<bool> { false, false, false, false };
            this._n_gang = new List<int> { 0, 0, 0, 0 };
            this._neng_rong = new List<bool> { true, true, true, true };

            this._hule = new List<int>();
            this._hule_option = null;
            this._no_game = false;
            this._lianzhuang = false;
            this._changbang = model.changbang;
            this._fenpei = null;

            this._paipu.defen = model.defen.ToList();
            this._paipu.log.Add(new List<Message>());
            var paipu = new Message
            {
                qipai = new Qipai
                {
                    zhuangfeng = model.zhuangfeng,
                    jushu = model.jushu,
                    changbang = model.changbang,
                    lizhibang = model.lizhibang,
                    defen = model.player_id.Select(id => model.defen[id]).ToList(),
                    baopai = model.shan.baopai[0],
                    shoupai = model.shoupai.Select(shoupai => shoupai.ToString()).ToList()
                }
            };
            this.add_paipu(paipu);

            var msg = new List<Message>{null, null, null, null};
            for (int l = 0; l < 4; l++)
            {
                msg[l] = (Message) paipu.Clone();
                for (int i = 0; i < 4; i++)
                {
                    if (i != l) msg[l].qipai.shoupai[i] = "";
                }
            }
            this.call_players("qipai", msg, 0);

            if (this._view != null) this._view.redraw();
        }

        /// <summary>
        /// ツモの局進行を行う。
        /// </summary>
        internal void zimo()
        {
            this._model.lunban = (this._model.lunban + 1) % 4;

            string zimo = this._model.shan.zimo();
            this._model.shoupai[this._model.lunban].zimo(zimo);

            var paipu = new Message
            {
                zimo = new Zimo { l = this._model.lunban, p = zimo }
            };
            this.add_paipu(paipu);

            var msg = new List<Message>{null, null, null, null};
            for (int l = 0; l < 4; l++)
            {
                msg[l] = (Message) paipu.Clone();
                if (l != this._model.lunban) msg[l].zimo.p = null;
            }
            this.call_players("zimo", msg);

            if (this._view != null) this._view.update(paipu);
        }

        /// <summary>
        /// dapai で指定された牌を打牌する局進行を行う。
        /// </summary>
        /// <param name="dapai"></param>
        internal void dapai(string dapai)
        {
            var model = this._model;

            this._yifa[model.lunban] = false;

            if (!model.shoupai[model.lunban].lizhi)
                this._neng_rong[model.lunban] = true;

            model.shoupai[model.lunban].dapai(dapai);
            model.he[model.lunban].dapai(dapai);

            if (this._diyizimo)
            {
                if (!Regex.IsMatch(dapai, @"^z[1234]")) this._fengpai = false;
                if (this._dapai != null && this._dapai.Substring(0, 2) != dapai.Substring(0, 2))
                    this._fengpai = false;
            }
            else
                this._fengpai = false;

            if (dapai.EndsWith("*"))
            {
                this._lizhi[model.lunban] = this._diyizimo ? 2 : 1;
                this._yifa[model.lunban] = this._rule.一発あり;
            }

            if (Majiang.Util.xiangting(model.shoupai[model.lunban]) == 0
                && Majiang.Util.tingpai(model.shoupai[model.lunban])
                                .Any(p => model.he[model.lunban].find(p)))
            {
                this._neng_rong[model.lunban] = false;
            }

            this._dapai = dapai;

            var paipu = new Message { dapai = new Dapai { l = model.lunban, p = dapai } };
            this.add_paipu(paipu);

            if (!this._gang.IsNullOrEmpty()) this.kaigang();

            var msg = new List<Message>{null, null, null, null};
            for (int l = 0; l < 4; l++)
            {
                msg[l] = new Message { dapai = new Dapai { l = model.lunban, p = dapai } };
            }
            this.call_players("dapai", msg);

            if (this._view != null) this._view.update(paipu);
        }

        /// <summary>
        /// fulou で指定された面子を副露する局進行を行う。
        /// 大明槓は副露に含める。
        /// </summary>
        /// <param name="fulou"></param>
        internal void fulou(string fulou)
        {
            var model = this._model;

            this._diyizimo = false;
            this._yifa = new List<bool> { false, false, false, false };

            model.he[model.lunban].fulou(fulou);

            var d = Regex.Match(fulou, @"[\+\=\-]").Value;
            model.lunban = (model.lunban + "_-=+".IndexOf(d)) % 4;

            model.shoupai[model.lunban].fulou(fulou);

            if (Regex.IsMatch(fulou, @"^[mpsz]\d{4}"))
            {
                this._gang = fulou;
                this._n_gang[model.lunban]++;
            }

            var paipu = new Message { fulou = new Fulou { l = model.lunban, m = fulou } };
            this.add_paipu(paipu);

            var msg = new List<Message>{null, null, null, null};
            for (int l = 0; l < 4; l++)
            {
                msg[l] = new Message { fulou = new Fulou { l = model.lunban, m = fulou } };
            }
            this.call_players("fulou", msg);

            if (this._view != null) this._view.update(paipu);
        }

        /// <summary>
        /// gang で指定された面子で加槓あるいは暗槓する局進行を行う。
        /// </summary>
        /// <param name="gang"></param>
        internal void gang(string gang)
        {
            var model = this._model;

            model.shoupai[model.lunban].gang(gang);

            var paipu = new Message { gang = new Gang { l = model.lunban, m = gang } };
            this.add_paipu(paipu);

            if (!string.IsNullOrEmpty(this._gang)) this.kaigang();

            this._gang = gang;
            this._n_gang[model.lunban]++;

            var msg = new List<Message>{null, null, null, null};
            for (int l = 0; l < 4; l++)
            {
                msg[l] = new Message { gang = new Gang { l = model.lunban, m = gang } };
            }
            this.call_players("gang", msg);

            if (this._view != null) this._view.update(paipu);
        }

        /// <summary>
        /// リンシャン牌ツモの局進行を行う。
        /// </summary>
        internal void gangzimo()
        {
            var model = this._model;

            this._diyizimo = false;
            this._yifa = new List<bool> { false, false, false, false };

            var zimo = model.shan.gangzimo();
            model.shoupai[model.lunban].zimo(zimo);

            var paipu = new Message { gangzimo = new Zimo { l = model.lunban, p = zimo } };
            this.add_paipu(paipu);

            if (!this._rule.カンドラ後乗せ || Regex.IsMatch(this._gang, @"^[mpsz]\d{4}$"))
                this.kaigang();

            var msg = new List<Message>{null, null, null, null};
            for (int l = 0; l < 4; l++)
            {
                msg[l] = new Message { gangzimo = new Zimo { l = model.lunban, p = zimo } };
                if (l != model.lunban) msg[l].gangzimo.p = null;
            }
            this.call_players("gangzimo", msg);

            if (this._view != null) this._view.update(paipu);
        }

        /// <summary>
        /// 開槓の局進行を行う。
        /// </summary>
        internal void kaigang()
        {
            this._gang = null;

            if (!this._rule.カンドラあり) return;

            var model = this._model;

            model.shan.kaigang();
            var baopai = model.shan.baopai.Pop();

            var paipu = new Message { kaigang = new Kaigang { baopai = baopai } };
            this.add_paipu(paipu);

            var msg = new List<Message>{null, null, null, null};
            for (int l = 0; l < 4; l++)
            {
                msg[l] = new Message { kaigang = new Kaigang { baopai = baopai } };
            }
            this.notify_players("kaigang", msg);

            if (this._view != null) this._view.update(paipu);
        }

        /// <summary>
        /// 和了の局進行を行う。
        /// </summary>
        internal void hule()
        {
            var model = this._model;

            if (this._status != "hule")
            {
                model.shan.close();
                this._hule_option = this._status == "gang" ? "qianggang"
                                    : this._status == "gangzimo" ? "lingshang"
                                    : null;
            }

            var menfeng = this._hule.Count > 0 ? this._hule.Shift() : model.lunban;
            var rongpai = menfeng == model.lunban ? null
                            : (this._hule_option == "qianggang"
                                ? this._gang[0] + this._gang.Substring(this._gang.Length - 1)
                                : this._dapai.Substring(0, 2)
                            ) + "_+=-"[(4 + model.lunban - menfeng) % 4];
            var shoupai = model.shoupai[menfeng].clone();
            var fubaopai = shoupai.lizhi ? model.shan.fubaopai : null;

            var param = new HuleParam
            {
                rule = this._rule,
                zhuangfeng = model.zhuangfeng,
                menfeng = menfeng,
                hupai = new Hupai
                {
                    lizhi = this._lizhi[menfeng],
                    yifa = this._yifa[menfeng],
                    qianggang = this._hule_option == "qianggang",
                    lingshang = this._hule_option == "lingshang",
                    haidi = model.shan.paishu > 0
                            || this._hule_option == "lingshang" ? 0
                            : rongpai.IsNullOrEmpty() ? 1
                            : 2,
                    tianhu = !(this._diyizimo && rongpai.IsNullOrEmpty()) ? 0
                            : menfeng == 0 ? 1
                            : 2
                },
                baopai = model.shan.baopai,
                fubaopai = fubaopai,
                jicun = new Jicun { changbang = model.changbang, lizhibang = model.lizhibang }
            };
            var hule = Majiang.Util.hule(shoupai, rongpai, param);

            if (this._rule.連荘方式 > 0 && menfeng == 0) this._lianzhuang = true;
            if (this._rule.場数 == 0) this._lianzhuang = false;
            this._fenpei = hule.fenpei;

            var paipu = new Message
            {
                hule = new Hule
                {
                    l = menfeng,
                    shoupai = !rongpai.IsNullOrEmpty() ? shoupai.zimo(rongpai).ToString()
                                    : shoupai.ToString(),
                    baojia = !rongpai.IsNullOrEmpty() ? model.lunban : (int?)null,
                    fubaopai = fubaopai,
                    fu = hule.fu,
                    fanshu = hule.fanshu,
                    damanguan = hule.damanguan,
                    defen = hule.defen,
                    hupai = hule.hupai,
                    fenpei = hule.fenpei
                }
            };
            this.add_paipu(paipu);

            var msg = new List<Message>{null, null, null, null};
            for (int l = 0; l < 4; l++)
            {
                msg[l] = (Message) paipu.Clone();
            }
            this.call_players("hule", msg, this._wait);

            if (this._view != null) this._view.update(paipu);
        }

        /// <summary>
        /// 流局の局進行を行う。
        /// name が指定された場合は途中流局とする。
        /// shoupai には流局時に公開した 牌姿 を指定する。
        /// </summary>
        /// <param name="name"></param>
        /// <param name="shoupai"></param>
        internal void pingju(string name = null, List<string> shoupai = null)
        {
            if (shoupai == null)
            {
                shoupai = new List<string> { "", "", "", "" };
            }

            BoardModel model = this._model;

            List<int> fenpei = new List<int> { 0, 0, 0, 0 };

            if (name.IsNullOrEmpty())
            {
                int n_tingpai = 0;
                for (int l = 0; l < 4; l++)
                {
                    if (this._rule.ノーテン宣言あり && string.IsNullOrEmpty(shoupai[l])
                        && !model.shoupai[l].lizhi) continue;
                    if (!this._rule.ノーテン罰あり
                        && (this._rule.連荘方式 != 2 || l != 0)
                        && !model.shoupai[l].lizhi)
                    {
                        shoupai[l] = "";
                    }
                    else if (Majiang.Util.xiangting(model.shoupai[l]) == 0
                            && Majiang.Util.tingpai(model.shoupai[l]).Count > 0)
                    {
                        n_tingpai++;
                        shoupai[l] = model.shoupai[l].ToString();
                        if (this._rule.連荘方式 == 2 && l == 0)
                            this._lianzhuang = true;
                    }
                    else
                    {
                        shoupai[l] = "";
                    }
                }
                if (this._rule.流し満貫あり)
                {
                    for (int l = 0; l < 4; l++)
                    {
                        bool all_yaojiu = true;
                        foreach (string p in model.he[l]._pai)
                        {
                            if (Regex.IsMatch(p, @"[\+\=\-]$")) { all_yaojiu = false; break; }
                            if (Regex.IsMatch(p, @"^z")) continue;
                            if (Regex.IsMatch(p, @"^[mps][19]")) continue;
                            all_yaojiu = false; break;
                        }
                        if (all_yaojiu)
                        {
                            name = "流し満貫";
                            for (int i = 0; i < 4; i++)
                            {
                                fenpei[i] += l == 0 && i == l ? 12000
                                        : l == 0 ? -4000
                                        : l != 0 && i == l ? 8000
                                        : l != 0 && i == 0 ? -4000
                                        : -2000;
                            }
                        }
                    }
                }
                if (name.IsNullOrEmpty())
                {
                    name = "荒牌平局";
                    if (this._rule.ノーテン罰あり
                        && 0 < n_tingpai && n_tingpai < 4)
                    {
                        for (int l = 0; l < 4; l++)
                        {
                            fenpei[l] = !string.IsNullOrEmpty(shoupai[l]) ? (int)(3000 / n_tingpai)
                                                                        : (int)(-3000 / (4 - n_tingpai));
                        }
                    }
                }
                if (this._rule.連荘方式 == 3) this._lianzhuang = true;
            }
            else
            {
                this._no_game = true;
                this._lianzhuang = true;
            }

            if (this._rule.場数 == 0) this._lianzhuang = true;

            this._fenpei = fenpei;

            var paipu = new Message
            {
                pingju = new Pingju { name = name, shoupai = shoupai, fenpei = fenpei }
            };
            this.add_paipu(paipu);

            var msg = new List<Message>{ null, null, null, null };
            for (int l = 0; l < 4; l++)
            {
                msg[l] = (Message) paipu.Clone();
            }
            this.call_players("pingju", msg, this._wait);

            if (this._view != null) this._view.update(paipu);
        }

        /// <summary>
        /// 対局終了の判断を行う。
        /// </summary>
        internal void last()
        {
            var model = this._model;

            model.lunban = -1;
            if (this._view != null) this._view.update();

            if (!this._lianzhuang)
            {
                model.jushu++;
                model.zhuangfeng += (model.jushu / 4);
                model.jushu = model.jushu % 4;
            }

            bool jieju = false;
            int guanjun = -1;
            var defen = model.defen;
            for (int i = 0; i < 4; i++)
            {
                int id = (model.qijia + i) % 4;
                if (defen[id] < 0 && this._rule.トビ終了あり) jieju = true;
                if (defen[id] >= 30000 && (guanjun < 0 || defen[id] > defen[guanjun])) guanjun = id;
            }

            int sum_jushu = model.zhuangfeng * 4 + model.jushu;

            if (15 < sum_jushu) jieju = true;
            else if ((this._rule.場数 + 1) * 4 - 1 < sum_jushu) jieju = true;
            else if (this._max_jushu < sum_jushu)
            {
                if (this._rule.延長戦方式 == 0) jieju = true;
                else if (this._rule.場数 == 0) jieju = true;
                else if (guanjun >= 0) jieju = true;
                else
                {
                    this._max_jushu += this._rule.延長戦方式 == 3 ? 4
                                    : this._rule.延長戦方式 == 2 ? 1
                                    : 0;
                }
            }
            else if (this._max_jushu == sum_jushu)
            {
                if (this._rule.オーラス止めあり && guanjun == model.player_id[0]
                    && this._lianzhuang && !this._no_game) jieju = true;
            }

            if (jieju) this.delay(() => this.jieju(), 0);
            else this.delay(() => this.qipai(), 0);
        }

        /// <summary>
        /// 対局終了の処理を行う。
        /// </summary>
        internal void jieju()
        {
            var model = this._model;

            List<int> paiming = new List<int>();
            var defen = model.defen;
            for (int i = 0; i < 4; i++)
            {
                int id = (model.qijia + i) % 4;
                for (int j = 0; j < 4; j++)
                {
                    if (j == paiming.Count || defen[id] > defen[paiming[j]])
                    {
                        paiming.Insert(j, id);
                        break;
                    }
                }
            }
            defen[paiming[0]] += model.lizhibang * 1000;
            this._paipu.defen = defen;

            List<int> rank = new List<int> { 0, 0, 0, 0 };
            for (int i = 0; i < 4; i++)
            {
                rank[paiming[i]] = i + 1;
            }
            this._paipu.rank = rank;

            // 点数を丸める
            double round_point(double point)
            {
                int s = Math.Sign(point);
                int a = Math.Abs((int) Math.Round(point * 10));
                int d = a % 10;

                // プラスは四捨五入、マイナスは五捨六入
                if (d >= (s == +1 ? 5 : 6))
                    return s * (a / 10 + 1);
                else
                    return s * (a / 10);
            }

            bool round = this._rule.順位点.Find(p => Regex.IsMatch(p, @"\.\d")).IsNullOrEmpty();
            double[] point = { 0, 0, 0, 0 };
            for (int i = 1; i < 4; i++)
            {
                int id = paiming[i];
                point[id] = (defen[id] - 30000) / 1000.0 + double.Parse(this._rule.順位点[i]);
                if (round) point[id] = round_point(point[id]);
                point[paiming[0]] -= point[id];
            }
            this._paipu.point = point.Select(p => p.ToString(round ? "F0" : "F1")).ToList();

            var paipu = new Message { jieju = this._paipu };

            var msg = new List<Message>{ null, null, null, null };
            for (int l = 0; l < 4; l++)
            {
                msg[l] = (Message) paipu.Clone();
            }
            this.call_players("jieju", msg, this._wait);

            if (this._view != null) this._view.summary(this._paipu);

            if (this._handler != null) this._handler();
        }

        /// <summary>
        /// 手番 l からの応答を取得する。
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
        internal Reply get_reply(int l)
        {
            var model = this._model;
            if (this._reply.TryGetValue(model.player_id[l], out Reply reply))
            {
                return reply;
            }
            return new Reply();
        }

        /// <summary>
        /// 配牌の局進行メソッドを呼び出す。
        /// </summary>
        internal void reply_kaiju()
        {
            this.delay(() => this.qipai(), 0);
        }

        /// <summary>
        /// ツモの局進行メソッドを呼び出す。
        /// </summary>
        internal void reply_qipai()
        {
            this.delay(this.zimo, 0);
        }

        /// <summary>
        /// ツモ応答の妥当性を確認し、次の局進行メソッドを呼び出す。
        /// </summary>
        internal void reply_zimo()
        {
            var model = this._model;

            var reply = this.get_reply(model.lunban);
            if (!string.IsNullOrEmpty(reply.daopai))
            {
                if (this.allow_pingju())
                {
                    var shoupai = new List<string> { "", "", "", "" };
                    shoupai[model.lunban] = model.shoupai[model.lunban].ToString();
                    this.delay(() => this.pingju("九種九牌", shoupai), 0);
                    return;
                }
            }
            else if (!string.IsNullOrEmpty(reply.hule))
            {
                if (this.allow_hule())
                {
                    if (this._view != null) this._view.say("zimo", model.lunban);
                    this.delay(() => this.hule());
                    return;
                }
            }
            else if (!string.IsNullOrEmpty(reply.gang))
            {
                if (this.get_gang_mianzi().Find(m => m == reply.gang) != null)
                {
                    if (this._view != null) this._view.say("gang", model.lunban);
                    this.delay(() => this.gang(reply.gang));
                    return;
                }
            }
            else if (!string.IsNullOrEmpty(reply.dapai))
            {
                var dapai = Regex.Replace(reply.dapai, @"\*$", "");
                if (this.get_dapai().Find(p => p == dapai) != null)
                {
                    if (reply.dapai.Substring(reply.dapai.Length - 1) == "*" && this.allow_lizhi(dapai) != null)
                    {
                        if (this._view != null) this._view.say("lizhi", model.lunban);
                        this.delay(() => this.dapai(reply.dapai));
                        return;
                    }
                    this.delay(() => this.dapai(dapai), 0);
                    return;
                }
            }

            var p = this.get_dapai().Last();
            this.get_dapai().RemoveAt(this.get_dapai().Count - 1);
            this.delay(() => this.dapai(p), 0);
        }

        /// <summary>
        /// 打牌応答の妥当性を確認し、次の局進行メソッドを呼び出す。
        /// </summary>
        internal void reply_dapai()
        {
            var model = this._model;
            int l;
            Reply reply;

            for (int i = 1; i < 4; i++)
            {
                l = (model.lunban + i) % 4;
                reply = this.get_reply(l);
                if (!reply.hule.IsNullOrEmpty() && this.allow_hule(l))
                {
                    if (this._rule.最大同時和了数 == 1 && this._hule.Count > 0)
                        continue;
                    if (this._view != null) this._view.say("rong", l);
                    this._hule.Add(l);
                }
                else
                {
                    var shoupai = model.shoupai[l].clone().zimo(this._dapai);
                    if (Majiang.Util.xiangting(shoupai) == -1)
                        this._neng_rong[l] = false;
                }
            }
            if (this._hule.Count == 3 && this._rule.最大同時和了数 == 2)
            {
                var shoupai = new List<string> { "", "", "", "" };
                foreach (var _l in this._hule)
                {
                    shoupai[_l] = model.shoupai[_l].ToString();
                }
                this.delay(() => this.pingju("三家和", shoupai));
                return;
            }
            else if (this._hule.Count > 0)
            {
                this.delay(() => this.hule());
                return;
            }

            if (this._dapai.Substring(this._dapai.Length - 1) == "*")
            {
                model.defen[model.player_id[model.lunban]] -= 1000;
                model.lizhibang++;

                if (this._lizhi.Count(x => x > 0) == 4
                    && this._rule.途中流局あり)
                {
                    var shoupai = model.shoupai.Select(s => s.ToString()).ToList();
                    this.delay(() => this.pingju("四家立直", shoupai));
                    return;
                }
            }

            if (this._diyizimo && model.lunban == 3)
            {
                this._diyizimo = false;
                if (this._fengpai)
                {
                    this.delay(() => this.pingju("四風連打"), 0);
                    return;
                }
            }

            if (this._n_gang.Sum() == 4)
            {
                if (this._n_gang.Max() < 4 && this._rule.途中流局あり)
                {
                    this.delay(() => this.pingju("四開槓"), 0);
                    return;
                }
            }

            if (model.shan.paishu == 0)
            {
                var shoupai = new List<string> { "", "", "", "" };
                for (l = 0; l < 4; l++)
                {
                    reply = this.get_reply(l);
                    if (!string.IsNullOrEmpty(reply.daopai)) shoupai[l] = reply.daopai;
                }
                this.delay(() => this.pingju("", shoupai), 0);
                return;
            }

            for (int i = 1; i < 4; i++)
            {
                l = (model.lunban + i) % 4;
                reply = this.get_reply(l);
                if (!string.IsNullOrEmpty(reply.fulou))
                {
                    var m = reply.fulou.Replace("0", "5");
                    if (Regex.IsMatch(m, @"^[mpsz](\d)\1\1\1"))
                    {
                        if (this.get_gang_mianzi(l).Find(m => m == reply.fulou) != null)
                        {
                            if (this._view != null) this._view.say("gang", l);
                            this.delay(() => this.fulou(reply.fulou));
                            return;
                        }
                    }
                    else if (Regex.IsMatch(m, @"^[mpsz](\d)\1\1"))
                    {
                        if (this.get_peng_mianzi(l).Find(m => m == reply.fulou) != null)
                        {
                            if (this._view != null) this._view.say("peng", l);
                            this.delay(() => this.fulou(reply.fulou));
                            return;
                        }
                    }
                }
            }
            l = (model.lunban + 1) % 4;
            reply = this.get_reply(l);
            if (!string.IsNullOrEmpty(reply.fulou))
            {
                if (this.get_chi_mianzi(l).Find(m => m == reply.fulou) != null)
                {
                    if (this._view != null) this._view.say("chi", l);
                    this.delay(() => this.fulou(reply.fulou));
                    return;
                }
            }

            this.delay(() => this.zimo(), 0);
        }

        /// <summary>
        /// 副露応答の妥当性を確認し、次の局進行メソッドを呼び出す。
        /// </summary>
        internal void reply_fulou()
        {
            var model = this._model;

            if (this._gang != null)
            {
                this.delay(() => this.gangzimo(), 0);
                return;
            }

            var reply = this.get_reply(model.lunban);
            if (!string.IsNullOrEmpty(reply.dapai))
            {
                if (this.get_dapai().Find(p => p == reply.dapai) != null)
                {
                    this.delay(() => this.dapai(reply.dapai), 0);
                    return;
                }
            }

            var p = this.get_dapai().Last();
            this.get_dapai().RemoveAt(this.get_dapai().Count - 1);
            this.delay(() => this.dapai(p), 0);
        }

        /// <summary>
        /// 槓応答の妥当性を確認し、次の局進行メソッドを呼び出す。
        /// </summary>
        internal void reply_gang()
        {
            var model = this._model;

            if (Regex.IsMatch(this._gang, @"^[mpsz]\d{4}$"))
            {
                this.delay(() => this.gangzimo(), 0);
                return;
            }

            for (int i = 1; i < 4; i++)
            {
                int l = (model.lunban + i) % 4;
                var reply = this.get_reply(l);
                if (!reply.hule.IsNullOrEmpty() && this.allow_hule(l))
                {
                    if (this._rule.最大同時和了数 == 1 && this._hule.Count > 0)
                        continue;
                    if (this._view != null) this._view.say("rong", l);
                    this._hule.Add(l);
                }
                else
                {
                    var p = this._gang[0] + this._gang.Substring(this._gang.Length - 1);
                    var shoupai = model.shoupai[l].clone().zimo(p);
                    if (Majiang.Util.xiangting(shoupai) == -1)
                        this._neng_rong[l] = false;
                }
            }
            if (this._hule.Count > 0)
            {
                this.delay(() => this.hule());
                return;
            }

            this.delay(() => this.gangzimo(), 0);
        }

        /// <summary>
        /// 和了応答の妥当性を確認し、次の局進行メソッドを呼び出す。
        /// </summary>
        internal void reply_hule()
        {
            var model = this._model;

            for (int l = 0; l < 4; l++)
            {
                model.defen[model.player_id[l]] += this._fenpei[l];
            }
            model.changbang = 0;
            model.lizhibang = 0;

            if (this._hule.Count > 0)
            {
                this.delay(() => this.hule());
                return;
            }
            else
            {
                if (this._lianzhuang) model.changbang = this._changbang + 1;
                this.delay(() => this.last(), 0);
                return;
            }
        }

        /// <summary>
        /// 流局応答の妥当性を確認し、次の局進行メソッドを呼び出す。
        /// </summary>
        internal void reply_pingju()
        {
            var model = this._model;

            for (int l = 0; l < 4; l++)
            {
                model.defen[model.player_id[l]] += this._fenpei[l];
            }
            model.changbang++;

            this.delay(() => this.last(), 0);
        }

        /// <summary>
        /// 打牌可能な牌の一覧を取得
        /// Majiang.Game#static-get_dapai を呼び出し、インスタンス変数 _rule にしたがって現在の手番の手牌から打牌可能な牌の一覧を返す。
        /// </summary>
        /// <returns></returns>
        internal List<string> get_dapai()
        {
            var model = this._model;
            return Game.get_dapai(this._rule, model.shoupai[model.lunban]);
        }

        /// <summary>
        /// チー可能な面子の一覧を取得
        /// Majiang.Game#static-get_chi_mianzi を呼び出し、インスタンス変数 _rule にしたがって手番 l が現在の打牌でチー可能な面子の一覧を返す。
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
        internal List<string> get_chi_mianzi(int l)
        {
            var model = this._model;
            var d = "_+=-"[(4 + model.lunban - l) % 4];
            return Game.get_chi_mianzi(this._rule, model.shoupai[l], this._dapai + d, model.shan.paishu);
        }

        /// <summary>
        /// ポン可能な面子の一覧を取得
        /// Majiang.Game#static-get_peng_mianzi を呼び出し、インスタンス変数 _rule にしたがって手番 l が現在の打牌でポン可能な面子の一覧を返す。
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
        internal List<string> get_peng_mianzi(int l)
        {
            var model = this._model;
            var d = "_+=-"[(4 + model.lunban - l) % 4];
            return Game.get_peng_mianzi(this._rule, model.shoupai[l], this._dapai + d, model.shan.paishu);
        }

        /// <summary>
        /// カン可能な面子の一覧を取得
        /// Majiang.Game#static-get_gang_mianzi を呼び出し、インスタンス変数 _rule にしたがってカン可能な面子の一覧を返す。
        /// l が指定された場合は大明槓、null の場合は暗槓と加槓が対象になる。
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
        internal List<string> get_gang_mianzi(int? l = null)
        {
            var model = this._model;
            if (l == null)
            {
                return Game.get_gang_mianzi(this._rule, model.shoupai[model.lunban], null, model.shan.paishu, this._n_gang.Sum());
            }
            else
            {
                var d = "_+=-"[(4 + model.lunban - l.Value) % 4];
                return Game.get_gang_mianzi(this._rule, model.shoupai[l.Value], this._dapai + d, model.shan.paishu, this._n_gang.Sum());
            }
        }

        /// <summary>
        /// リーチ可能か判定
        /// Majiang.Game#static-allow_lizhi を呼び出し、インスタンス変数 _rule にしたがってリーチ可能か判定する。
        /// p が null のときはリーチ可能な打牌一覧を返す。
        /// p が 牌 のときは p を打牌してリーチ可能なら null以外 を返す。
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        internal List<string> allow_lizhi(string p)
        {
            var model = this._model;
            return Game.allow_lizhi(this._rule, model.shoupai[model.lunban], p, model.shan.paishu, model.defen[model.player_id[model.lunban]]);
        }

        /// <summary>
        /// 和了可能か判定
        /// Majiang.Game#static-allow_hule を呼び出し、インスタンス変数 _rule にしたがって和了可能か判定する。
        /// l が null のときは現在の手番がツモ和了可能なら true を返す。
        /// l が指定された場合は手番 l がロン和了可能なら true を返す。
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
        internal bool allow_hule(int? l = null)
        {
            var model = this._model;
            if (l == null)
            {
                var hupai = model.shoupai[model.lunban].lizhi || this._status == "gangzimo" || model.shan.paishu == 0;
                return Game.allow_hule(this._rule, model.shoupai[model.lunban], null, model.zhuangfeng, model.lunban, hupai);
            }
            else
            {
                var p = (this._status == "gang" ? this._gang[0] + this._gang.Substring(this._gang.Length - 1) : this._dapai) + "_+=-"[(4 + model.lunban - l.Value) % 4];
                var hupai = model.shoupai[l.Value].lizhi || this._status == "gang" || model.shan.paishu == 0;
                return Game.allow_hule(this._rule, model.shoupai[l.Value], p, model.zhuangfeng, l.Value, hupai, this._neng_rong[l.Value]);
            }
        }

        /// <summary>
        /// 九種九牌流局可能か判定
        /// Majiang.Game#static-allow_pingju を呼び出し、インスタンス変数 _rule にしたがって現在の手番が九種九牌流局可能か判定する。
        /// </summary>
        /// <returns></returns>
        internal bool allow_pingju()
        {
            var model = this._model;
            return Game.allow_pingju(this._rule, model.shoupai[model.lunban], this._diyizimo);
        }

        /// <summary>
        /// 打牌の取得
        /// Majiang.Shoupai#get_dapai を呼び出し、rule にしたがって shoupai から打牌可能な牌の一覧を返す。
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="shoupai"></param>
        /// <returns></returns>
        public static List<string> get_dapai(Rule rule, Shoupai shoupai)
        {
            if (rule.喰い替え許可レベル == 0) return shoupai.get_dapai(true);
            if (rule.喰い替え許可レベル == 1 && !string.IsNullOrEmpty(shoupai._zimo) && shoupai._zimo.Length > 2)
            {
                var match = Regex.Match(shoupai._zimo, @"\d(?=[\+\=\-])");
                var n = match != null ? int.Parse(match.Value) : 0;
                if (n == 0) n = 5;
                var deny = shoupai._zimo[0].ToString() + n;
                return shoupai.get_dapai(false).Where(p => p.Replace("0", "5") != deny).ToList();
            }
            return shoupai.get_dapai(false);
        }

        /// <summary>
        ///　チー可能な面子の一覧を取得
        /// Majiang.Shoupai#get_chi_mianzi を呼び出し、rule にしたがって shoupai から p でチー可能な面子の一覧を返す。 paishu には現在の残り牌数を指定すること。
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="shoupai"></param>
        /// <param name="p"></param>
        /// <param name="paishu"></param>
        /// <returns></returns>
        public static List<string> get_chi_mianzi(Rule rule, Shoupai shoupai, string p, int paishu)
        {
            var mianzi = shoupai.get_chi_mianzi(p, rule.喰い替え許可レベル == 0);
            if (mianzi == null) return mianzi;
            if (rule.喰い替え許可レベル == 1 && shoupai._fulou.Count == 3 && shoupai._bingpai[p[0]][int.Parse(p[1].ToString())] == 2) mianzi = new List<string>();
            return paishu == 0 ? new List<string>() : mianzi;
        }

        /// <summary>
        /// ポン可能な面子の一覧を取得
        /// Majiang.Shoupai#get_peng_mianzi を呼び出し、rule にしたがって shoupai から p でポン可能な面子の一覧を返す。
        /// paishu には現在の残り牌数を指定すること。
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="shoupai"></param>
        /// <param name="p"></param>
        /// <param name="paishu"></param>
        /// <returns></returns>
        public static List<string> get_peng_mianzi(Rule rule, Shoupai shoupai, string p, int paishu)
        {
            var mianzi = shoupai.get_peng_mianzi(p);
            if (mianzi == null) return mianzi;
            return paishu == 0 ? new List<string>() : mianzi;
        }

        /// <summary>
        /// カン可能な面子の一覧を取得
        /// Majiang.Shoupai#get_gang_mianzi を呼び出し、rule にしたがって shoupai からカン可能な面子の一覧を返す。
        /// p が指定された場合は大明槓、null の場合は暗槓と加槓が対象になる。
        /// paishu には現在の残り牌数、n_gang にはその局に行われた槓の数を指定すること。
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="shoupai"></param>
        /// <param name="p"></param>
        /// <param name="paishu"></param>
        /// <param name="n_gang"></param>
        /// <returns></returns>
        public static List<string> get_gang_mianzi(Rule rule, Shoupai shoupai, string p, int paishu, int n_gang = 0)
        {
            var mianzi = shoupai.get_gang_mianzi(p);
            if (mianzi == null || mianzi.Count == 0) return mianzi;

            if (shoupai.lizhi)
            {
                if (rule.リーチ後暗槓許可レベル == 0) return new List<string>();
                else if (rule.リーチ後暗槓許可レベル == 1)
                {
                    var new_shoupai = shoupai.clone().dapai(shoupai._zimo);
                    int n_hule1 = 0, n_hule2 = 0;
                    foreach (var _p in Majiang.Util.tingpai(new_shoupai))
                    {
                        n_hule1 += Majiang.Util.hule_mianzi(new_shoupai, _p).Count;
                    }
                    new_shoupai = shoupai.clone().gang(mianzi[0]);
                    foreach (var _p in Majiang.Util.tingpai(new_shoupai))
                    {
                        n_hule2 += Majiang.Util.hule_mianzi(new_shoupai, _p).Count;
                    }
                    if (n_hule1 > n_hule2) return new List<string>();
                }
                else
                {
                    var new_shoupai = shoupai.clone().dapai(shoupai._zimo);
                    int n_tingpai1 = Majiang.Util.tingpai(new_shoupai).Count;
                    new_shoupai = shoupai.clone().gang(mianzi[0]);
                    if (Majiang.Util.xiangting(new_shoupai) > 0) return new List<string>();
                    int n_tingpai2 = Majiang.Util.tingpai(new_shoupai).Count;
                    if (n_tingpai1 > n_tingpai2) return new List<string>();
                }
            }
            return paishu == 0 || n_gang == 4 ? new List<string>() : mianzi;
        }

        /// <summary>
        /// リーチ可能か判定
        /// rule にしたがって shoupai からリーチ可能か判定する。
        /// p が null のときはリーチ可能な打牌一覧を返す。
        /// p が 牌 のときは p を打牌してリーチ可能なら true を返す。
        /// paishu には現在の残り牌数、defen には現在の持ち点を指定すること。
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="shoupai"></param>
        /// <param name="p"></param>
        /// <param name="paishu"></param>
        /// <param name="defen"></param>
        /// <returns></returns>
        public static List<string> allow_lizhi(Rule rule, Shoupai shoupai, string p = null, int? paishu = null, int? defen = null)
        {
            if (string.IsNullOrEmpty(shoupai._zimo)) return null;
            if (shoupai.lizhi) return null;
            if (!shoupai.menqian) return null;

            if (!rule.ツモ番なしリーチあり && paishu != null && paishu < 4) return null;
            if (rule.トビ終了あり && defen != null && defen < 1000) return null;

            if (Majiang.Util.xiangting(shoupai) > 0) return null;

            if (!string.IsNullOrEmpty(p))
            {
                var new_shoupai = shoupai.clone().dapai(p);
                return Majiang.Util.xiangting(new_shoupai) == 0 && Majiang.Util.tingpai(new_shoupai).Count > 0
                        ? new List<string> { p } : null;
            }
            else
            {
                var dapai = new List<string>();
                foreach (var p_item in Game.get_dapai(rule, shoupai))
                {
                    var new_shoupai = shoupai.clone().dapai(p_item);
                    if (Majiang.Util.xiangting(new_shoupai) == 0 && Majiang.Util.tingpai(new_shoupai).Count > 0)
                    {
                        dapai.Add(p_item);
                    }
                }
                return dapai.Count > 0 ? dapai : null;
            }
        }

        /// <summary>
        /// 和了可能か判定
        /// rule にしたがって shoupai で和了可能か判定する。
        /// p が null のときはツモ和了可能なら true を返す。
        /// p が 牌 のときは p でロン和了可能なら true を返す。
        /// zhuangfeng には場風(0: 東、1: 南、2: 西、3: 北)、menfeng には自風、状況役があるときは hupai に true、フリテンのときは neng_rong に false を指定すること。
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="shoupai"></param>
        /// <param name="p"></param>
        /// <param name="zhuangfeng"></param>
        /// <param name="menfeng"></param>
        /// <param name="hupai"></param>
        /// <param name="neng_rong"></param>
        /// <returns></returns>
        public static bool allow_hule(Rule rule, Shoupai shoupai, string p, int zhuangfeng, int menfeng, bool hupai, bool neng_rong = false)
        {
            if (!string.IsNullOrEmpty(p) && !neng_rong) return false;

            var new_shoupai = shoupai.clone();
            if (!string.IsNullOrEmpty(p)) new_shoupai.zimo(p);
            if (Majiang.Util.xiangting(new_shoupai) != -1) return false;

            if (hupai) return true;

            var param = new HuleParam
            {
                rule = rule,
                zhuangfeng = zhuangfeng,
                menfeng = menfeng,
                hupai = new Hupai(),
                baopai = new List<string>(),
                jicun = new Jicun { changbang = 0, lizhibang = 0 }
            };
            var hule = Majiang.Util.hule(shoupai, p, param);

            return hule.hupai != null;
        }

        /// <summary>
        /// 九種九牌流局可能か判定
        /// rule にしたがって shoupai で九種九牌流局可能か判定する。
        /// 第一ツモ順の場合は diyizimo に true を指定すること。
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="shoupai"></param>
        /// <param name="diyizimo"></param>
        /// <returns></returns>
        public static bool allow_pingju(Rule rule, Shoupai shoupai, bool diyizimo)
        {
            if (!(diyizimo && !string.IsNullOrEmpty(shoupai._zimo))) return false;
            if (!rule.途中流局あり) return false;

            int n_yaojiu = 0;
            foreach (var s in "mpsz")
            {
                var bingpai = shoupai._bingpai[s];
                var nn = (s == 'z') ? new List<int> { 1, 2, 3, 4, 5, 6, 7 } : new List<int> { 1, 9 };
                foreach (var n in nn)
                {
                    if (bingpai[n] > 0) n_yaojiu++;
                }
            }
            return n_yaojiu >= 9;
        }

        /// <summary>
        /// ノーテン宣言可能か判定
        /// rule にしたがって shoupai が「ノーテン宣言」可能か判定する。
        /// paishu には現在の残り牌数を指定すること。
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="shoupai"></param>
        /// <param name="paishu"></param>
        /// <returns></returns>
        public static bool allow_no_daopai(Rule rule, Shoupai shoupai, int paishu)
        {
            if (paishu > 0 || !string.IsNullOrEmpty(shoupai._zimo)) return false;
            if (!rule.ノーテン宣言あり) return false;
            if (shoupai.lizhi) return false;

            return Majiang.Util.xiangting(shoupai) == 0 && Majiang.Util.tingpai(shoupai).Count > 0;
        }

        internal object set_timeout(Action action, int delay)
        {
            if (_mono_behaviour != null)
            {
                return _mono_behaviour.StartCoroutine(delay_coroutine(action, delay));
            }
            else
            {
                return delay_task(action, delay);
            }
        }

        internal static IEnumerator delay_coroutine(Action action, int delay)
        {
            delay = Mathf.Max(delay, 1);

            yield return new WaitForSeconds(delay / 1000f);
            action?.Invoke();
        }

        internal static object delay_task(Action action, int delay)
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            delay = Mathf.Max(delay, 1);

            Task.Delay(delay, cts.Token).ContinueWith(t =>
            {
                if (!cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        action?.Invoke();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                }
            }, TaskScheduler.Default);

            return cts;
        }

        internal void cancel_timeout()
        {
            if (this._timeout_id != null)
            {
                if (_mono_behaviour != null)
                {
                    _mono_behaviour.StopCoroutine((Coroutine) this._timeout_id);
                }
                else
                {
                    ((CancellationTokenSource) this._timeout_id)?.Cancel();
                }
                this._timeout_id = null;
            }
        }
    }
}