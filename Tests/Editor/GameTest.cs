using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace Majiang.Test
{
    public class GameTest
    {
        public static ConcurrentDictionary<int, Message> MSG = new ConcurrentDictionary<int, Message>();

        public class Player : Majiang.Player
        {
            public int _delay;

            public Player(int id, List<Reply> reply = null, int delay = 0)
            {
                this._id = id;
                this._reply = reply ?? new List<Reply>();
                this._delay = delay;
            }

            public override void action(Message msg, Action<Reply> callback = null)
            {
                MSG[this._id] = msg;
                if (callback != null)
                {
                    if (this._delay > 0)
                    {
                        set_timeout(() => callback.Invoke(this._reply.Shift()), this._delay);
                    }
                    else
                    {
                        callback.Invoke(this._reply.Shift());
                    }
                }
            }

            public static void set_timeout(Action action, int delay)
            {
                delay = Mathf.Max(delay, 1);

                Task.Delay(delay).ContinueWith(t =>
                {
                    action();
                }, TaskScheduler.Default);
            }
        }

        public class ViewParam : EntityBase
        {
            public bool kaiju;
            public Message update;
            public bool redraw;
            public Paipu summary;

            public override string ToString()
            {
                return $"kaiju: {kaiju}, update: {update}, redraw: {redraw}, summary: {summary}";
            }
        }

        public class SayParam : EntityBase
        {
            public string type;
            public int lunban;

            public override string ToString()
            {
                return $"type: {type}, lunban: {lunban}";
            }
        }

        public class View : Majiang.View
        {
            public ViewParam _param = null;
            public SayParam _say = null;

            public void kaiju()
            {
                _param = new ViewParam { kaiju = true };
            }

            public void update(Message paipuLog = null)
            {
                _param = new ViewParam { update = paipuLog };
            }

            public void redraw()
            {
                _param = new ViewParam { redraw = true };
            }

            public void summary(Paipu paipu = null)
            {
                _param = new ViewParam { summary = paipu };
            }

            public void say(string type, int lunban)
            {
                _say = new SayParam { type = type, lunban = lunban };
            }
        }

        public class GameParam
        {
            public Action<Paipu> callback;
            public Rule rule;
            public int? qijia;
            public int? lizhibang;
            public int? changbang;
            public List<string> shoupai;
            public List<string> zimo;
            public List<string> gangzimo;
            public string baopai;
            public List<int> defen;
        }

        public Majiang.Game init_game(GameParam param = null)
        {
            if (param == null)
            {
                param = new GameParam();
            }

            var players = Enumerable.Range(0, 4).Select(id => new Player(id)).ToList<Majiang.Player>();
            var rule = param.rule ?? new Majiang.Rule();
            var game = new Majiang.Game(players, param.callback, rule);

            game.view = new View();
            game._sync = true;
            if (param.qijia != null) game.kaiju(param.qijia.Value);
            else game.kaiju();

            if (param.lizhibang != null)
            {
                game.model.lizhibang = param.lizhibang.Value;
            }
            if (param.changbang != null)
            {
                game.model.changbang = param.changbang.Value;
            }

            game.qipai();

            if (param.shoupai != null)
            {
                for (int l = 0; l < 4; l++)
                {
                    if (string.IsNullOrEmpty(param.shoupai[l])) continue;
                    var paistr = param.shoupai[l];
                    if (paistr == "_") paistr = new string('_', 13);
                    game.model.shoupai[l] = Majiang.Shoupai.fromString(paistr);
                }
            }
            if (param.zimo != null)
            {
                var pai = ((Shan) game.model.shan)._pai;
                for (int i = 0; i < param.zimo.Count; i++)
                {
                    pai[pai.Count - 1 - i] = param.zimo[i];
                }
            }
            if (param.gangzimo != null)
            {
                var pai = ((Shan) game.model.shan)._pai;
                for (int i = 0; i < param.gangzimo.Count; i++)
                {
                    pai[i] = param.gangzimo[i];
                }
            }
            if (!string.IsNullOrEmpty(param.baopai))
            {
                ((Shan)game.model.shan)._baopai = new List<string> { param.baopai };
            }
            if (param.defen != null)
            {
                for (int l = 0; l < 4; l++)
                {
                    var id = game.model.player_id[l];
                    game.model.defen[id] = param.defen[l];
                }
            }

            return game;
        }

        public void set_reply(Majiang.Game game, List<Reply> reply)
        {
            for (int l = 0; l < 4; l++)
            {
                var id = game.model.player_id[l];
                game._players[id]._reply = new List<Reply> { reply[l] };
            }
        }

        public Message last_paipu(Majiang.Game game, int i = 0)
        {
            var log = game._paipu.log[game._paipu.log.Count - 1];
            return log[log.Count - 1 + i];
        }

        [Test, Description("クラスが存在すること")]
        public void ClassExists()
        {
            Assert.NotNull(typeof(Majiang.Game));
        }

        [Test, Description("constructor()")]
        public void TestConstructor()
        {
            var game = new Majiang.Game();
            var rule = new Majiang.Rule();

            // インスタンスが生成できること
            Assert.IsNotNull(game, "インスタンスが生成できること");

            // タイトルが設定されていること
            Assert.IsNotNull(game._model.title, "タイトルが設定されていること");

            // タイトルが設定可能なこと
            game = new Majiang.Game(new List<Majiang.Player>(), null, rule, "タイトル");
            Assert.AreEqual(game._model.title, "タイトル", "タイトルが設定可能なこと");

            // 対局者名が設定されていること
            CollectionAssert.AreEqual(game._model.player, new List<string> { "私", "下家", "対面", "上家" }, "対局者名が設定されていること");

            // 局数が初期化されていること
            Assert.AreEqual(game._model.zhuangfeng, 0, "局数が初期化されていること - zhuangfeng");
            Assert.AreEqual(game._model.jushu, 0, "局数が初期化されていること - jushu");

            // 供託が初期化されていること
            Assert.AreEqual(game._model.changbang, 0, "供託が初期化されていること - changbang");
            Assert.AreEqual(game._model.lizhibang, 0, "供託が初期化されていること - lizhibang");

            // 持ち点が初期化されていること
            CollectionAssert.AreEqual(game._model.defen, new List<int> { 25000, 25000, 25000, 25000 }, "持ち点が初期化されていること");

            // 持ち点が変更可能なこと
            rule = new Majiang.Rule { 配給原点 = 30000 };
            game = new Majiang.Game(new List<Majiang.Player>(), null, rule);
            CollectionAssert.AreEqual(game._model.defen, new List<int> { 30000, 30000, 30000, 30000 }, "持ち点が変更可能なこと");
        }

        [Test, Description("speed()")]
        public void TestSpeed()
        {
            // speed が変更できること
            var game = new Majiang.Game();
            game.speed = 5;
            Assert.AreEqual(game.speed, 5, "speed が変更できること");
        }

        [Test, Description("delay()")]
        public void TestDelay()
        {
            var game = new Majiang.Game();

            // speed: 0 → 0ms
            bool called = false;
            game.speed = 0;
            game.delay(() => { called = true; });
            Assert.IsFalse(called, "speed: 0 → 0ms");
            Thread.Sleep(10);
            Assert.IsTrue(called);

            // speed: 1 → 500ms
            called = false;
            game.speed = 1;
            game.delay(() => { called = true; });
            Thread.Sleep(200);
            Assert.IsFalse(called);
            Thread.Sleep(300 + 10);
            Assert.IsTrue(called);

            // speed: 3 → 600ms
            called = false;
            game.speed = 3;
            game.delay(() => { called = true; });
            Thread.Sleep(500);
            Assert.IsFalse(called);
            Thread.Sleep(100 + 10);
            Assert.IsTrue(called);

            // speed: 0, timeout: 100 → 0ms
            called = false;
            game.speed = 0;
            game.delay(() => { called = true; }, 100);
            Assert.IsFalse(called, "speed: 0, timeout: 100 → 0ms");
            Thread.Sleep(10);
            Assert.IsTrue(called);

            // speed: 5, timeout: 100 → 100ms
            called = false;
            game.speed = 5;
            game.delay(() => { called = true; }, 100);
            Thread.Sleep(10);
            Assert.IsFalse(called);
            Thread.Sleep(100);
            Assert.IsTrue(called);
        }

        [Test, Description("stop()")]
        public void TestStop()
        {
            var game = new Majiang.Game();

            // 停止すること
            game.stop();
            Assert.IsTrue(game._stop != null, "停止すること");
            game._reply = new ConcurrentDictionary<int, Reply>(new Dictionary<int, Reply>
            {
                {0, new Reply()},
                {1, new Reply()},
                {2, new Reply()},
                {3, new Reply()},
            });
            game.next();

            // 停止時に指定したコールバックが呼ばれること
            bool callbackCalled = false;
            game.stop(() => { callbackCalled = true; });
            game._reply = new ConcurrentDictionary<int, Reply>(new Dictionary<int, Reply>
            {
                {0, new Reply()},
                {1, new Reply()},
                {2, new Reply()},
                {3, new Reply()},
            });
            game.next();
            Assert.IsTrue(callbackCalled, "停止時に指定したコールバックが呼ばれること");
        }

        [Test, Description("start()")]
        public void TestStart()
        {
            var game = new Majiang.Game();

            // 再開すること
            game.stop();
            game.start();
            Assert.IsFalse(game._stop != null, "再開すること");
            Assert.IsNotNull(game._timeout_id, "再開すること");
            Thread.Sleep(10);
            Assert.IsNull(game._timeout_id);

            // 二重起動しないこと
            game._timeout_id = new CancellationTokenSource();
            game.start();
            Assert.IsNotNull(game._timeout_id, "二重起動しないこと");
        }

        [Test, Description("notify_players(type, msg)")]
        public void TestNotifyPlayers()
        {
            var players = Enumerable.Range(0, 4).Select(id => new Player(id)).ToList<Majiang.Player>();
            var game = new Majiang.Game(players);
            var type = "test";
            var msg = new List<Message> {
                new Message { kaiju = new Kaiju{ id = 1 } },
                new Message { kaiju = new Kaiju{ id = 2 } },
                new Message { kaiju = new Kaiju{ id = 3 } },
                new Message { kaiju = new Kaiju{ id = 4 } },
            };

            // 通知が伝わること
            MSG.Clear();
            game.notify_players(type, msg);
            Assert.AreEqual(MSG.Count, 0, "通知が伝わること");

            Thread.Sleep(10);

            CollectionAssert.AreEqual(MSG.Values.ToList(), msg, "通知が伝わること");
        }

        [Test, Description("call_players(type, msg, timeout)")]
        public void TestCallPlayers()
        {
            var players = Enumerable.Range(0, 4).Select(id => new Player(id)).ToList<Majiang.Player>();
            var game = new Majiang.Game(players);
            game.speed = 1;
            var type = "test";
            var msg = new List<Message> {
                new Message { },
                new Message { },
                new Message { },
                new Message { },
            };

            int stopCount = 0;
            Action stop = () => { stopCount++; };

            // 通知が伝わること
            game.stop(stop);
            MSG.Clear();
            game.call_players(type, msg);
            Assert.AreEqual(MSG.Count, 0, "通知が伝わること");

            Thread.Sleep(300);

            CollectionAssert.AreEqual(MSG.Values.ToList(), msg, "通知が伝わること");
            Assert.IsTrue(stopCount > 0, "通知が伝わること");

            // 応答が返ること
            stopCount = 0;
            game.stop(stop);
            game.call_players(type, msg);

            Thread.Sleep(300);

            Assert.IsTrue(stopCount > 0, "応答が返ること");

            // 遅い player がいても応答を取得できること
            stopCount = 0;
            game.stop(stop);
            foreach (var player in players) { ((Player)player)._delay = 100; }
            game.call_players(type, msg, 0);

            Thread.Sleep(300 + 100);

            Assert.IsTrue(stopCount > 0, "遅い player がいても応答を取得できること");
        }

        [Test, Description("kaiju(qijia)")]
        public void TestKaiju()
        {
            var players = Enumerable.Range(0, 4).Select(id => new Player(id)).ToList<Majiang.Player>();
            var rule = new Majiang.Rule();
            var game = new Majiang.Game(players, null, rule);
            game.view = new View();
            game.speed = 0;
            game._sync = true;
            game.stop();

            // 起家が設定されること
            MSG.Clear();
            game.kaiju(0);
            Assert.AreEqual(game._model.qijia, 0, "起家が設定されること");

            // 牌譜が初期化されること
            Assert.AreEqual(game._paipu.title, game._model.title, "牌譜が初期化されること");
            CollectionAssert.AreEqual(game._paipu.player, game._model.player, "牌譜が初期化されること");
            Assert.AreEqual(game._paipu.qijia, game._model.qijia, "牌譜が初期化されること");
            Assert.AreEqual(game._paipu.log.Count, 0, "牌譜が初期化されること");

            // 表示処理が呼び出されること
            Assert.AreEqual(((View)game._view)._param, new ViewParam { kaiju = true }, "表示処理が呼び出されること");

            Thread.Sleep(10);

            // 通知が伝わること
            for (int id = 0; id < 4; id++)
            {
                var msg = new Message
                {
                    kaiju = new Kaiju
                    {
                        id = id,
                        rule = game._rule,
                        title = game._model.title,
                        player = game._model.player,
                        qijia = 0
                    }
                };
                Assert.AreEqual(MSG[id], msg, "通知が伝わること");
            }

            // 起家を乱数で設定できること
            game.stop();
            game.kaiju();
            Assert.IsTrue(game._model.qijia == 0 ||
                        game._model.qijia == 1 ||
                        game._model.qijia == 2 ||
                        game._model.qijia == 3, "起家を乱数で設定できること");
        }

        [Test, Description("qipai(shan)")]
        public void TestQipai()
        {
            var players = Enumerable.Range(0, 4).Select(id => new Player(id)).ToList<Majiang.Player>();
            var rule = new Majiang.Rule();
            var game = new Majiang.Game(players, null, rule);
            game.view = new View();
            game._sync = true;
            game.kaiju();

            // 牌山が生成されること
            game.qipai();
            var shan = game.model.shan;
            Assert.AreEqual(shan.paishu, 70, "牌山が生成されること");
            Assert.AreEqual(shan.baopai.Count, 1, "牌山が生成されること");
            Assert.IsNull(shan.fubaopai, "牌山が生成されること");

            // 配牌されること
            for (int l = 0; l < 4; l++)
            {
                var shoupai_str = Regex.Replace(game.model.shoupai[l].ToString(), @"[mpsz]", "");
                Assert.AreEqual(shoupai_str.Length, 13, "配牌されること");
            }

            // 河が初期化されること
            for (int l = 0; l < 4; l++)
            {
                Assert.AreEqual(game.model.he[l]._pai.Count, 0, "河が初期化されること");
            }

            // 第一ツモ巡であること
            Assert.IsTrue(game._diyizimo, "第一ツモ巡であること");

            // 四風連打中であること
            Assert.IsTrue(game._fengpai, "四風連打中であること");

            // 牌譜が記録されること
            Assert.IsTrue(last_paipu(game).qipai != null, "牌譜が記録されること");

            // 表示処理が呼び出されること
            Assert.AreEqual(((View)game._view)._param, new ViewParam { redraw = true }, "表示処理が呼び出されること");

            // 通知が伝わること
            for (int l = 0; l < 4; l++)
            {
                var id = game.model.player_id[l];
                Assert.AreEqual(MSG[id].qipai.defen[l], 25000, "通知が伝わること");
                Assert.IsNotNull(MSG[id].qipai.shoupai[l], "通知が伝わること");
            }

            // 使用する牌山を指定できること
            game = init_game();
            var shan2 = new Majiang.Shan(game._rule);
            var shoupai = new Majiang.Shoupai(shan2._pai.GetRange(shan2._pai.Count - 13, 13));
            game.qipai(shan2);
            Assert.AreEqual(game.model.shoupai[0].ToString(), shoupai.ToString(), "使用する牌山を指定できること");

            // 途中流局なしの場合、最初から四風連打中でないこと
            game = init_game(new GameParam { rule = new Majiang.Rule{ 途中流局あり = false } });
            game.qipai();
            Assert.IsFalse(game._fengpai, "途中流局なしの場合、最初から四風連打中でないこと");
        }

        [Test, Description("zimo()")]
        public void TestZimo()
        {
            var game = init_game();

            // 手番が更新されること
            game.zimo();
            Assert.AreEqual(game.model.lunban, 0, "手番が更新されること");

            // 牌山からツモられること
            Assert.AreEqual(game.model.shan.paishu, 69, "牌山からツモられること");

            // 手牌にツモ牌が加えられること
            Assert.IsNotNull(game.model.shoupai[0].get_dapai(), "手牌にツモ牌が加えられること");

            // 牌譜が記録されること
            Assert.IsTrue(last_paipu(game).zimo != null, "牌譜が記録されること");

            // 表示処理が呼び出されること
            Assert.AreEqual(((View)game._view)._param, new ViewParam { update = last_paipu(game) }, "表示処理が呼び出されること");

            // 通知が伝わること
            for (int l = 0; l < 4; l++)
            {
                var id = game.model.player_id[l];
                Assert.AreEqual(MSG[id].zimo.l, game.model.lunban, "通知が伝わること");
                if (l == game.model.lunban)
                    Assert.IsNotNull(MSG[id].zimo.p, "通知が伝わること");
                else
                    Assert.IsNull(MSG[id].zimo.p, "通知が伝わること");
            }
        }

        [Test, Description("dapai(dapai)")]
        public void TestDapai()
        {
            var game = init_game();
            string dapai;

            // 手牌から打牌が切り出されること
            game.zimo();
            dapai = game.model.shoupai[0].get_dapai()[0];
            game.dapai(dapai);
            Assert.IsNull(game.model.shoupai[0].get_dapai(), "手牌から打牌が切り出されること");

            // 河に打牌されること
            Assert.AreEqual(game.model.he[0]._pai[0], dapai, "河に打牌されること");

            // 牌譜が記録されること
            Assert.IsTrue(last_paipu(game).dapai != null, "牌譜が記録されること");

            // 表示処理が呼び出されること
            Assert.AreEqual(((View)game._view)._param, new ViewParam { update = last_paipu(game) }, "表示処理が呼び出されること");

            // 通知が伝わること
            for (int l = 0; l < 4; l++)
            {
                var id = game.model.player_id[l];
                Assert.AreEqual(MSG[id].dapai.l, game.model.lunban, "通知が伝わること");
                Assert.AreEqual(MSG[id].dapai.p, dapai, "通知が伝わること");
            }

            // 風牌以外の打牌で四風連打中でなくなること
            game = init_game(new GameParam { shoupai = new List<string> { "_", "", "", "" } });
            game.zimo();
            game.dapai("m1");
            Assert.IsFalse(game._fengpai, "風牌以外の打牌で四風連打中でなくなること");

            // 異なる風牌の打牌で四風連打中でなくなること
            game = init_game(new GameParam { shoupai = new List<string> { "_", "_", "", "" } });
            game.zimo();
            game.dapai("z1");
            game.zimo();
            game.dapai("z2");
            Assert.IsFalse(game._fengpai, "異なる風牌の打牌で四風連打中でなくなること");

            // 第一ツモ巡終了で四風連打中でなくなること
            game = init_game(new GameParam { shoupai = new List<string> { "_", "_", "", "" } });
            game.zimo();
            game.dapai("z1");
            game.zimo();
            game._diyizimo = false;
            game.dapai("z1");
            Assert.IsFalse(game._fengpai, "第一ツモ巡終了で四風連打中でなくなること");

            // ダブルリーチ
            game = init_game(new GameParam { shoupai = new List<string> { "_", "", "", "" } });
            game.zimo();
            game.dapai("m1*");
            Assert.AreEqual(game._lizhi[game.model.lunban], 2, "ダブルリーチ");
            Assert.IsTrue(game._yifa[game.model.lunban], "ダブルリーチ");

            // リーチ
            game = init_game(new GameParam { shoupai = new List<string> { "_", "", "", "" } });
            game._diyizimo = false;
            game.zimo();
            game.dapai("m1_*");
            Assert.AreEqual(game._lizhi[game.model.lunban], 1, "リーチ");
            Assert.IsTrue(game._yifa[game.model.lunban], "リーチ");

            // 一発なし
            game = init_game(new GameParam { rule = new Majiang.Rule{ 一発あり = false }, shoupai = new List<string> { "_", "", "", "" } });
            game._diyizimo = false;
            game.zimo();
            game.dapai("m1*");
            Assert.AreEqual(game._lizhi[game.model.lunban], 1, "一発なし");
            Assert.IsFalse(game._yifa[game.model.lunban], "一発なし");

            // リーチ後の打牌で一発の権利を失うこと
            game = init_game(new GameParam { shoupai = new List<string> { "_", "", "", "" } });
            game._yifa[0] = true;
            game.zimo();
            game.dapai("m1");
            Assert.IsFalse(game._yifa[game.model.lunban], "リーチ後の打牌で一発の権利を失うこと");

            // テンパイ時に和了牌が河にある場合、フリテンとなること
            game = init_game(new GameParam { shoupai = new List<string> { "m123p456s789z11122", "", "", "" } });
            game.model.lunban = 0;
            game.dapai("m1");
            Assert.IsFalse(game._neng_rong[game.model.lunban], "テンパイ時に和了牌が河にある場合、フリテンとなること");

            // 打牌によりフリテンが解除されること
            game = init_game(new GameParam { shoupai = new List<string> { "_", "", "", "" } });
            game._neng_rong[0] = false;
            game.zimo();
            game.dapai("m1");
            Assert.IsTrue(game._neng_rong[game.model.lunban], "打牌によりフリテンが解除されること");

            // リーチ後はフリテンが解除されないこと
            game = init_game(new GameParam { shoupai = new List<string> { "_____________*", "", "", "" } });
            game._neng_rong[0] = false;
            game.zimo();
            var dapai7 = game.model.shoupai[0]._zimo;
            game.dapai(dapai7);
            Assert.IsFalse(game._neng_rong[game.model.lunban], "リーチ後はフリテンが解除されないこと");

            // 加槓後の打牌で開槓されること
            game = init_game(new GameParam { shoupai = new List<string> { "__________,s333=", "", "", "" } });
            game.zimo();
            game.gang("s333=3");
            game.gangzimo();
            game.dapai("p1");
            Assert.AreEqual(game.model.shan.baopai.Count, 2, "加槓後の打牌で開槓されること");
        }

        [Test, Description("fulou(fulou)")]
        public void TestFulou()
        {
            var game = init_game(new GameParam { shoupai = new List<string> { "_", "_", "", "" } });

            // 河から副露牌が拾われること
            game.zimo();
            game.dapai("m2_");
            game.fulou("m12-3");
            Assert.AreEqual(game.model.he[0]._pai[0], "m2_-", "河から副露牌が拾われること");

            // 手番が更新されること(上家からのチー)
            Assert.AreEqual(game.model.lunban, 1, "手番が更新されること(上家からのチー)");

            // 手牌が副露されること
            Assert.AreEqual(game.model.shoupai[1]._fulou[0], "m12-3", "手牌が副露されること");

            // 牌譜が記録されること
            Assert.IsTrue(last_paipu(game).fulou != null, "牌譜が記録されること");

            // 表示処理が呼び出されること
            Assert.AreEqual(((View)game._view)._param, new ViewParam { update = last_paipu(game) }, "表示処理が呼び出されること");

            // 通知が伝わること
            for (int l = 0; l < 4; l++)
            {
                var id = game.model.player_id[l];
                Assert.AreEqual(MSG[id].fulou.l, game.model.lunban, "通知が伝わること");
                Assert.AreEqual(MSG[id].fulou.m, "m12-3", "通知が伝わること");
            }

            // 大明槓が副露されること
            var game2 = init_game(new GameParam { shoupai = new List<string> { "_", "", "", "_" } });
            game2.zimo();
            game2.dapai("m2");
            game2.fulou("m2222+");
            Assert.AreEqual(game2.model.shoupai[3]._fulou[0], "m2222+", "大明槓が副露されること");

            // 第一ツモ巡でなくなること
            var game3 = init_game(new GameParam { shoupai = new List<string> { "_", "_", "", "_" } });
            game3.zimo();
            game3.dapai("m3");
            game3.fulou("m123-");
            Assert.IsFalse(game3._diyizimo, "第一ツモ巡でなくなること");

            // 一発がなくなること
            var game4 = init_game(new GameParam { shoupai = new List<string> { "_", "_", "", "_" } });
            game4.zimo();
            game4.dapai("m3*");
            game4.fulou("m123-");
            Assert.IsFalse(game4._yifa.Any(x => x), "一発がなくなること");
        }

        [Test, Description("gang(gang)")]
        public void TestGang()
        {
            var game = init_game(new GameParam { shoupai = new List<string> { "__________,s555+", "", "", "" } });

            // 加槓が副露されること
            game.zimo();
            game.gang("s555+0");
            Assert.AreEqual(game.model.shoupai[0]._fulou[0], "s555+0", "加槓が副露されること");

            // 牌譜が記録されること
            Assert.IsTrue(last_paipu(game).gang != null, "牌譜が記録されること");

            // 表示処理が呼び出されること
            Assert.AreEqual(((View)game._view)._param, new ViewParam { update = last_paipu(game) }, "表示処理が呼び出されること");

            // 通知が伝わること
            for (int l = 0; l < 4; l++)
            {
                var id = game.model.player_id[l];
                Assert.AreEqual(MSG[id].gang.l, game.model.lunban, "通知が伝わること");
                Assert.AreEqual(MSG[id].gang.m, "s555+0", "通知が伝わること");
            }

            // 暗槓が副露されること
            var game2 = init_game(new GameParam { shoupai = new List<string> { "_", "", "", "" } });
            game2.zimo();
            game2.gang("s5550");
            Assert.AreEqual(game2.model.shoupai[0]._fulou[0], "s5550", "暗槓が副露されること");

            // 後乗せの槓が開槓されること
            var game3 = init_game(new GameParam { shoupai = new List<string> { "_______,s222+,z111=", "", "", "" } });
            game3.zimo();
            game3.gang("z111=1");
            game3.gangzimo();
            game3.gang("s222+2");
            Assert.AreEqual(game3.model.shan.baopai.Count, 2, "後乗せの槓が開槓されること");
        }

        [Test, Description("gangzimo()")]
        public void TestGangzimo()
        {
            var game = init_game(new GameParam { shoupai = new List<string> { "_", "", "", "" } });

            // 牌山からツモられること
            game.zimo();
            game.gang("m5550");
            game.gangzimo();
            Assert.AreEqual(game.model.shan.paishu, 68, "牌山からツモられること");

            // 手牌にツモ牌が加えられること
            Assert.IsTrue(game.model.shoupai[0].get_dapai().Count > 0, "手牌にツモ牌が加えられること");

            // 牌譜が記録されること
            Assert.IsNotNull(last_paipu(game, -1).gangzimo, "牌譜が記録されること");

            // 表示処理が呼び出されること
            Assert.AreEqual(((View)game._view)._param, new ViewParam { update = last_paipu(game, -1) }, "表示処理が呼び出されること");

            // 通知が伝わること
            for (int l = 0; l < 4; l++)
            {
                var id = game.model.player_id[l];
                Assert.AreEqual(MSG[id].gangzimo.l, game.model.lunban, "通知が伝わること");
                if (l == game.model.lunban)
                    Assert.IsNotNull(MSG[id].gangzimo.p, "通知が伝わること");
                else
                    Assert.IsNull(MSG[id].gangzimo.p, "通知が伝わること");
            }

            // 第一ツモ巡でなくなること
            game = init_game(new GameParam { shoupai = new List<string> { "_", "", "", "_" } });
            game.zimo();
            game.gang("m3333");
            game.gangzimo();
            Assert.IsFalse(game._diyizimo, "第一ツモ巡でなくなること");

            // 一発がなくなること
            game = init_game(new GameParam { shoupai = new List<string> { "_", "_", "", "_" } });
            game.zimo();
            game.dapai("m3*");
            game.zimo();
            game.gang("m4444");
            game.gangzimo();
            Assert.IsFalse(game._yifa.Any(x => x), "一発がなくなること");

            // 加槓の場合、即座には開槓されないこと
            game = init_game(new GameParam { shoupai = new List<string> { "__________,s333=", "", "", "" } });
            game.zimo();
            game.gang("s333=3");
            game.gangzimo();
            Assert.AreEqual(game.model.shan.baopai.Count, 1, "加槓の場合、即座には開槓されないこと");

            // カンドラ後乗せではない場合、加槓も即座に開槓されること
            game = init_game(new GameParam { rule = new Majiang.Rule{ カンドラ後乗せ = false }, shoupai = new List<string> { "__________,s333=", "", "", "" } });
            game.zimo();
            game.gang("s333=3");
            game.gangzimo();
            Assert.AreEqual(game.model.shan.baopai.Count, 2, "カンドラ後乗せではない場合、加槓も即座に開槓されること");

            // 大明槓の場合、即座には開槓されないこと
            game = init_game(new GameParam { shoupai = new List<string> { "_", "", "_", "" } });
            game.zimo();
            game.dapai("s3");
            game.fulou("s3333=");
            game.gangzimo();
            Assert.AreEqual(game.model.shan.baopai.Count, 1, "大明槓の場合、即座には開槓されないこと");

            // カンドラ後乗せではない場合、大明槓も即座に開槓されること
            game = init_game(new GameParam { rule = new Majiang.Rule{ カンドラ後乗せ = false }, shoupai = new List<string> { "_", "", "_", "" } });
            game.zimo();
            game.dapai("s3");
            game.fulou("s3333=");
            game.gangzimo();
            Assert.AreEqual(game.model.shan.baopai.Count, 2, "カンドラ後乗せではない場合、大明槓も即座に開槓されること");
        }

        [Test, Description("kakigang()")]
        public void TestKakigang()
        {
            var game = init_game(new GameParam { shoupai = new List<string> { "__________,s555+", "", "", "" } });

            // 槓ドラが増えること
            game.zimo();
            game.gang("s555+0");
            game.gangzimo();
            game.kaigang();
            Assert.AreEqual(game.model.shan.baopai.Count, 2, "槓ドラが増えること");
            Assert.IsNull(game._gang, "槓ドラが増えること");

            // 牌譜が記録されること
            Assert.IsNotNull(last_paipu(game).kaigang, "牌譜が記録されること");

            // 表示処理が呼び出されること
            Assert.AreEqual(((View)game._view)._param, new ViewParam { update = last_paipu(game) }, "表示処理が呼び出されること");

            // 通知が伝わること
            for (int l = 0; l < 4; l++)
            {
                var id = game.model.player_id[l];
                Assert.AreEqual(MSG[id].kaigang.baopai, game.model.shan.baopai.Last(), "通知が伝わること");
            }

            // カンドラなしの場合、開槓しないこと
            var rule = new Majiang.Rule{ カンドラあり = false };
            game = init_game(new GameParam { rule = rule, shoupai = new List<string> { "_", "", "", "" } });
            game.zimo();
            game.gang("m1111");
            game.gangzimo();
            Assert.AreEqual(game.model.shan.baopai.Count, 1, "カンドラなしの場合、開槓しないこと");
            Assert.IsNull(game._gang, "カンドラなしの場合、開槓しないこと");

        }

        [Test, Description("hule()")]
        public void TestHule()
        {
            var game = init_game(new GameParam { shoupai = new List<string> { "_", "", "m123p456s789z1122", "" } });

            // 牌譜が記録されること
            game.zimo();
            game.dapai("z1");
            game._hule = new List<int> { 2 };
            game.hule();
            Assert.IsNotNull(last_paipu(game).hule, "牌譜が記録されること");

            // 表示処理が呼び出されること
            Assert.AreEqual(((View)game._view)._param, new ViewParam { update = last_paipu(game) }, "表示処理が呼び出されること");

            // 通知が伝わること
            for (int l = 0; l < 4; l++)
            {
                var id = game.model.player_id[l];
                Assert.AreEqual(MSG[id].hule.l, 2, "通知が伝わること");
            }

            // 通知のタイミングを変更できること
            game = init_game(new GameParam { shoupai = new List<string> { "m123p456s789z1122", "", "", "" }, zimo = new List<string> { "z2" } });
            game.wait = 20;
            game.zimo();
            MSG.Clear();
            game._sync = false;
            game.stop();
            game.hule();
            Assert.AreEqual(MSG.Count, 0, "通知のタイミングを変更できること");
            Thread.Sleep(20 + 10);
            Assert.AreEqual(MSG.Count, 4, "通知のタイミングを変更できること");

            // 立直・一発
            game = init_game(new GameParam { shoupai = new List<string> { "m123p456s789z1122", "_", "", "" } });
            game._diyizimo = false;
            game.zimo();
            game.dapai(game.model.shoupai[0]._zimo + "_*");
            game.zimo();
            game.dapai("z1");
            game._hule = new List<int> { 0 };
            game.hule();
            Assert.IsTrue(last_paipu(game).hule.hupai.Any(h => h.name == "立直"), "立直・一発");
            Assert.IsTrue(last_paipu(game).hule.hupai.Any(h => h.name == "一発"), "立直・一発");

            // ダブル立直
            game = init_game(new GameParam { shoupai = new List<string> { "m123p456s789z1122", "_", "", "" } });
            game.zimo();
            game.dapai(game.model.shoupai[0]._zimo + "_*");
            game.zimo();
            game.dapai("z1");
            game._hule = new List<int> { 0 };
            game.hule();
            Assert.IsTrue(last_paipu(game).hule.hupai.Any(h => h.name == "ダブル立直"), "ダブル立直");

            // 槍槓
            game = init_game(new GameParam { shoupai = new List<string> { "_________m1,m111=", "_", "m23p456s789z11222", "" } });
            game.zimo();
            game.gang("m111=1");
            game._hule = new List<int> { 2 };
            game.hule();
            Assert.IsTrue(last_paipu(game).hule.hupai.Any(h => h.name == "槍槓"), "槍槓");

            // 嶺上開花
            game = init_game(new GameParam { shoupai = new List<string> { "m123p456s78z11,m111=", "", "", "" }, zimo = new List<string> { "m4" }, gangzimo = new List<string> { "s9" } });
            game.zimo();
            game.gang("m111=1");
            game.gangzimo();
            game.hule();
            Assert.IsTrue(last_paipu(game).hule.hupai.Any(h => h.name == "嶺上開花"), "嶺上開花");

            // 最終牌で嶺上開花
            game = init_game(new GameParam { shoupai = new List<string> { "m123p456s78z11,m111=", "", "", "" }, zimo = new List<string> { "m4" }, gangzimo = new List<string> { "s9" } });
            game._diyizimo = false;
            game.zimo();
            game.gang("m111=1");
            while (game.model.shan.paishu > 1) game.model.shan.zimo();
            game.gangzimo();
            game.hule();
            Assert.IsFalse(last_paipu(game).hule.hupai.Any(h => h.name == "海底摸月"), "最終牌で嶺上開花");

            // 海底摸月
            game = init_game(new GameParam { shoupai = new List<string> { "m123p456s789z1122", "", "", "" }, zimo = new List<string> { "z2" } });
            game._diyizimo = false;
            game.zimo();
            while (game.model.shan.paishu > 0) game.model.shan.zimo();
            game.hule();
            Assert.IsTrue(last_paipu(game).hule.hupai.Any(h => h.name == "海底摸月"), "海底摸月");

            // 河底撈魚
            game = init_game(new GameParam { shoupai = new List<string> { "_", "", "m123p456s789z1122", "" } });
            game._diyizimo = false;
            game.zimo();
            while (game.model.shan.paishu > 0) game.model.shan.zimo();
            game.dapai("z2");
            game._hule = new List<int> { 2 };
            game.hule();
            Assert.IsTrue(last_paipu(game).hule.hupai.Any(h => h.name == "河底撈魚"), "河底撈魚");

            // 天和
            game = init_game(new GameParam { shoupai = new List<string> { "m123p456s789z1122", "", "", "" }, zimo = new List<string> { "z2" } });
            game.zimo();
            game.hule();
            Assert.IsTrue(last_paipu(game).hule.hupai.Any(h => h.name == "天和"), "天和");

            // 地和
            game = init_game(new GameParam { shoupai = new List<string> { "_", "m123p456s789z1122", "", "" }, zimo = new List<string> { "m1", "z2" } });
            game.zimo();
            game.dapai("m1_");
            game.zimo();
            game.hule();
            Assert.IsTrue(last_paipu(game).hule.hupai.Any(h => h.name == "地和"), "地和");

            // 槍槓でダブロン
            game = init_game(new GameParam { shoupai = new List<string> { "__________,m111=", "m23p456s789z11122", "m23p789s456z33344", "" } });
            game.zimo();
            game.gang("m111=1");
            game._hule = new List<int> { 1, 2 };
            game.hule();
            game.hule();
            Assert.IsTrue(last_paipu(game).hule.hupai.Any(h => h.name == "槍槓"), "槍槓でダブロン");

            // 子の和了は輪荘
            game = init_game(new GameParam { shoupai = new List<string> { "_", "m123p456s789z1122", "", "" } });
            game.zimo();
            game.dapai("z1");
            game._hule = new List<int> { 1 };
            game.hule();
            Assert.IsFalse(game._lianzhuang, "子の和了は輪荘");

            // 親の和了は連荘
            game = init_game(new GameParam { shoupai = new List<string> { "m123p456s789z1122", "", "", "" }, zimo = new List<string> { "z1" } });
            game.zimo();
            game.hule();
            Assert.IsTrue(game._lianzhuang, "親の和了は連荘");

            // ダブロンは親の和了があれば連荘
            game = init_game(new GameParam { shoupai = new List<string> { "m23p456s789z11122", "_", "m23p789s546z33344", "" }, zimo = new List<string> { "m2", "m1" } });
            game.zimo();
            game.dapai("m2");
            game.zimo();
            game.dapai("m1");
            game._hule = new List<int> { 2, 0 };
            game.hule();
            game.hule();
            Assert.IsTrue(game._lianzhuang, "ダブロンは親の和了があれば連荘");

            // 連荘なしの場合は親の和了があっても輪荘
            game = init_game(new GameParam { rule = new Majiang.Rule{ 連荘方式 = 0 }, shoupai = new List<string> { "m123p456s789z1122", "", "", "" }, zimo = new List<string> { "z1" } });
            game.zimo();
            game.hule();
            Assert.IsFalse(game._lianzhuang, "連荘なしの場合は親の和了があっても輪荘");

            // 一局戦の場合は親の和了でも輪荘
            game = init_game(new GameParam { rule = new Majiang.Rule{ 場数 = 0 }, shoupai = new List<string> { "m123p456s789z1122", "", "", "" }, zimo = new List<string> { "z1" } });
            game.zimo();
            game.hule();
            Assert.IsFalse(game._lianzhuang, "一局戦の場合は親の和了でも輪荘");
        }

        [Test, Description("pingju()")]
        public void TestPingju()
        {
            var game = init_game();

            // 途中流局
            game.pingju("九種九牌");
            Assert.IsTrue(game._no_game, "途中流局");
            Assert.IsTrue(game._lianzhuang, "途中流局");

            // 牌譜が記録されること
            Assert.IsNotNull(last_paipu(game).pingju, "牌譜が記録されること");

            // 表示処理が呼び出されること
            Assert.AreEqual(((View)game._view)._param, new ViewParam { update = last_paipu(game) }, "表示処理が呼び出されること");

            // 通知が伝わること
            for (int l = 0; l < 4; l++)
            {
                var id = game.model.player_id[l];
                Assert.AreEqual(MSG[id].pingju.name, "九種九牌", "通知が伝わること");
            }

            // 通知のタイミングを変更できること
            game = init_game();
            game.wait = 20;
            game.zimo();
            MSG.Clear();
            game._sync = false;
            game.stop();
            game.pingju("九種九牌");
            Assert.AreEqual(MSG.Count, 0, "通知のタイミングを変更できること");
            Thread.Sleep(20 + 10);
            Assert.AreEqual(MSG.Count, 4, "通知のタイミングを変更できること");

            // 全員テンパイ
            game = init_game(new GameParam { rule = new Majiang.Rule{ 流し満貫あり = false }, shoupai = new List<string> { "m22p12366s406789", "m55p40s123,z111-,p678-", "m67p678s22,s56-7,p444-", "m12345p33s333,m406-" } });
            game.pingju();
            Assert.AreEqual(last_paipu(game).pingju.name, "荒牌平局", "全員テンパイ");
            Assert.AreEqual(last_paipu(game).pingju.shoupai.Count(s => !s.IsNullOrEmpty()), 4, "全員テンパイ");
            Assert.IsTrue(game._fenpei.SequenceEqual(new int[] { 0, 0, 0, 0 }), "全員テンパイ");

            // 全員ノーテン
            game = init_game(new GameParam { rule = new Majiang.Rule{ 流し満貫あり = false }, shoupai = new List<string> { "m40789p4667s8z577", "m99p12306z277,m345-", "m3p1234689z55,s7-89", "m2233467p234555" } });
            game.pingju();
            Assert.AreEqual(last_paipu(game).pingju.name, "荒牌平局", "全員ノーテン");
            Assert.AreEqual(last_paipu(game).pingju.shoupai.Count(s => !s.IsNullOrEmpty()), 0, "全員ノーテン");
            Assert.IsTrue(game._fenpei.SequenceEqual(new int[] { 0, 0, 0, 0 }), "全員ノーテン");

            // 2人テンパイ
            game = init_game(new GameParam { rule = new Majiang.Rule{ 流し満貫あり = false }, shoupai = new List<string> { "m22p12366s406789", "m99p12306z277,m345-", "m3p1234689z55,s7-89", "m12345p33s333,m406-" } });
            game.pingju();
            Assert.AreEqual(last_paipu(game).pingju.name, "荒牌平局", "2人テンパイ");
            Assert.AreEqual(last_paipu(game).pingju.shoupai.Count(s => !s.IsNullOrEmpty()), 2, "2人テンパイ");
            Assert.IsTrue(game._fenpei.SequenceEqual(new int[] { 1500, -1500, -1500, 1500 }), "2人テンパイ");


            // 形式テンパイとならない牌姿
            game = init_game(new GameParam { rule = new Majiang.Rule{ 流し満貫あり = false }, shoupai = new List<string> { "m123p456s789z1111", "m99p12306z277,m345-", "m3p1234689z55,s7-89", "m12345p33s333,m406-" } });
            game.pingju();
            Assert.AreEqual(last_paipu(game).pingju.name, "荒牌平局", "形式テンパイとならない牌姿");
            Assert.AreEqual(last_paipu(game).pingju.shoupai.Count(s => !s.IsNullOrEmpty()), 1, "形式テンパイとならない牌姿");
            Assert.IsTrue(game._fenpei.SequenceEqual(new int[] { -1000, -1000, -1000, 3000 }), "形式テンパイとならない牌姿");

            // ノーテン宣言ありの場合、宣言なしをノーテンとみなすこと
            game = init_game(new GameParam { rule = new Majiang.Rule{ 流し満貫あり = false, ノーテン宣言あり = true }, shoupai = new List<string> { "m22p12366s406789", "m55p40s123,z111-,p678-", "m67p678s22,s56-7,p444-", "m12345p33s333,m406-" } });
            game.pingju("", new List<string> { "", "_", "_", "_" });
            Assert.AreEqual(last_paipu(game).pingju.name, "荒牌平局", "ノーテン宣言ありの場合、宣言なしをノーテンとみなすこと");
            Assert.AreEqual(last_paipu(game).pingju.shoupai.Count(s => !s.IsNullOrEmpty()), 3, "ノーテン宣言ありの場合、宣言なしをノーテンとみなすこと");
            Assert.IsTrue(game._fenpei.SequenceEqual(new int[] { -3000, 1000, 1000, 1000 }), "ノーテン宣言ありの場合、宣言なしをノーテンとみなすこと");

            // ノーテン宣言であってもリーチ者の手牌は公開すること
            game = init_game(new GameParam { rule = new Majiang.Rule{ 流し満貫あり = false, ノーテン宣言あり = true }, shoupai = new List<string> { "m22p12366s406789*", "", "", "" } });
            game.pingju();
            Assert.AreEqual(last_paipu(game).pingju.name, "荒牌平局", "ノーテン宣言であってもリーチ者の手牌は公開すること");
            Assert.IsNotNull(last_paipu(game).pingju.shoupai[0], "ノーテン宣言であってもリーチ者の手牌は公開すること");

            // ノーテン罰なし
            game = init_game(new GameParam { rule = new Majiang.Rule{ 流し満貫あり = false, ノーテン罰あり = false }, shoupai = new List<string> { "m22p12366s406789", "m99p12306z277,m345-", "m3p1234689z55,s7-89", "m12345p33s333,m406-" } });
            game.pingju();
            Assert.AreEqual(last_paipu(game).pingju.name, "荒牌平局", "ノーテン罰なし");
            Assert.AreEqual(last_paipu(game).pingju.shoupai.Count(s => !s.IsNullOrEmpty()), 1, "ノーテン罰なし");
            Assert.IsTrue(game._fenpei.SequenceEqual(new int[] { 0, 0, 0, 0 }), "ノーテン罰なし");

            // テンパイ連荘
            game = init_game(new GameParam { rule = new Majiang.Rule{ 流し満貫あり = false }, shoupai = new List<string> { "m22p12366s406789", "m99p12306z277,m345-", "m3p1234689z55,s7-89", "m2233467p234555" } });
            game.pingju();
            Assert.IsTrue(game._lianzhuang, "テンパイ連荘");

            // ノーテン親流れ
            game = init_game(new GameParam { rule = new Majiang.Rule{ 流し満貫あり = false }, shoupai = new List<string> { "m40789p4667s8z577", "m99p12306z277,m345-", "m3p1234689z55,s7-89", "m2233467p234555" } });
            game.pingju();
            Assert.IsFalse(game._lianzhuang, "ノーテン親流れ");

            // 和了連荘の場合、親のテンパイでも輪荘すること
            game = init_game(new GameParam { rule = new Majiang.Rule{ 流し満貫あり = false, 連荘方式 = 1 }, shoupai = new List<string> { "m22p12366s406789", "m99p12306z277,m345-", "m3p1234689z55,s7-89", "m2233467p234555" } });
            game.pingju();
            Assert.IsFalse(game._lianzhuang, "和了連荘の場合、親のテンパイでも輪荘すること");

            // ノーテン連荘の場合、親がノーテンでも連荘すること
            game = init_game(new GameParam { rule = new Majiang.Rule{ 流し満貫あり = false, 連荘方式 = 3 }, shoupai = new List<string> { "m40789p4667s8z577", "m99p12306z277,m345-", "m3p1234689z55,s7-89", "m2233467p234555" } });
            game.pingju();
            Assert.IsTrue(game._lianzhuang, "ノーテン連荘の場合、親がノーテンでも連荘すること");

            // 一局戦の場合、親がノーテンでも連荘すること
            game = init_game(new GameParam { rule = new Majiang.Rule{ 流し満貫あり = false, 場数 = 0 }, shoupai = new List<string> { "m40789p4667s8z577", "m99p12306z277,m345-", "m3p1234689z55,s7-89", "m2233467p234555" } });
            game.pingju();
            Assert.IsTrue(game._lianzhuang, "一局戦の場合、親がノーテンでも連荘すること");

            // 流し満貫
            game = init_game(new GameParam { shoupai = new List<string> { "_", "_", "_", "_" } });
            game.zimo(); game.dapai("z1");
            game.zimo(); game.dapai("m2");
            game.zimo(); game.dapai("p2");
            game.zimo(); game.dapai("s2");
            game.pingju();
            Assert.AreEqual(last_paipu(game).pingju.name, "流し満貫");
            Assert.IsTrue(game._fenpei.SequenceEqual(new int[] { 12000, -4000, -4000, -4000 }));

            // 鳴かれた場合、流し満貫は成立しない
            game = init_game(new GameParam { shoupai = new List<string> { "_", "_", "_", "_" } });
            game.zimo(); game.dapai("z1");
            game.fulou("z111-"); game.dapai("m2");
            game.zimo(); game.dapai("p2");
            game.zimo(); game.dapai("s2");
            game.pingju();
            Assert.AreEqual(last_paipu(game).pingju.name, "荒牌平局");

            // 2人が流し満貫
            game = init_game(new GameParam { shoupai = new List<string> { "_", "_", "_", "_" } });
            game.zimo(); game.dapai("z1");
            game.zimo(); game.dapai("m1");
            game.zimo(); game.dapai("p2");
            game.zimo(); game.dapai("s2");
            game.pingju();
            Assert.AreEqual(last_paipu(game).pingju.name, "流し満貫");
            Assert.IsTrue(game._fenpei.SequenceEqual(new int[] { 8000, 4000, -6000, -6000 }));

            // ノーテン罰なしのルールの場合、リーチ者と親以外は手牌を開かないこと
            game = init_game(new GameParam { rule = new Majiang.Rule { 流し満貫あり = false, ノーテン罰あり = false }, shoupai = new List<string> { "m567999s4466777", "m05p123s56z333*,s8888", "m11p789s06,z555-,p406-", "" } });
            game.pingju();
            Assert.IsTrue(last_paipu(game).pingju.shoupai.SequenceEqual(new List<string> { "m567999s4466777", "m05p123s56z333*,s8888", "", "" }));

            // ノーテン罰なしで和了連荘のルールの場合、リーチ者以外は手牌を開かないこと
            game = init_game(new GameParam { rule = new Majiang.Rule { 流し満貫あり = false, ノーテン罰あり = false, 連荘方式 = 1 }, shoupai = new List<string> { "m567999s4466777", "m05p123s56z333*,s8888", "m11p789s06,z555-,p406-", "" } });
            game.pingju();
            Assert.IsTrue(last_paipu(game).pingju.shoupai.SequenceEqual(new List<string> { "", "m05p123s56z333*,s8888", "", "" }));
        }

        [Test, Description("last()")]
        public void TestLast()
        {
            // 連荘時に局が進まないこと
            var game = init_game();
            game._lianzhuang = true;
            game.last();
            Assert.AreEqual(game.model.zhuangfeng, 0);
            Assert.AreEqual(game.model.jushu, 0);

            // 輪荘時に局が進むこと
            game = init_game();
            game.model.zhuangfeng = 0;
            game.model.jushu = 3;
            game.last();
            Assert.AreEqual(game.model.zhuangfeng, 1);
            Assert.AreEqual(game.model.jushu, 0);

            // 東風戦は東四局で終局すること
            game = init_game(new GameParam { rule = new Majiang.Rule{ 場数 = 1 }, defen = new List<int> { 10000, 20000, 30000, 40000 } });
            game.model.zhuangfeng = 0;
            game.model.jushu = 3;
            game.last();
            Assert.AreEqual(game._status, "jieju");

            // 東南戦は南四局で終局すること
            game = init_game(new GameParam { rule = new Majiang.Rule{ 場数 = 2 }, defen = new List<int> { 10000, 20000, 30000, 40000 } });
            game.model.zhuangfeng = 1;
            game.model.jushu = 3;
            game.last();
            Assert.AreEqual(game._status, "jieju");

            // 一荘戦は北四局で終局すること
            game = init_game(new GameParam { rule = new Majiang.Rule{ 場数 = 4 }, defen = new List<int> { 10000, 20000, 30000, 40000 } });
            game.model.zhuangfeng = 3;
            game.model.jushu = 3;
            game.last();
            Assert.AreEqual(game._status, "jieju");

            // 連荘中でもトビ終了すること
            game = init_game(new GameParam { defen = new List<int> { 50100, 30000, 20000, -100 } });
            game._lianzhuang = true;
            game.last();
            Assert.AreEqual(game._status, "jieju");

            // トビ終了なし
            game = init_game(new GameParam { rule = new Majiang.Rule{ トビ終了あり = false }, defen = new List<int> { 50100, 30000, 20000, -100 } });
            game.last();
            Assert.AreEqual(game._status, "qipai");

            // オーラス止め(東風戦)
            game = init_game(new GameParam { rule = new Majiang.Rule{ 場数 = 1 }, defen = new List<int> { 40000, 30000, 20000, 10000 } });
            game.model.zhuangfeng = 0;
            game.model.jushu = 3;
            game._lianzhuang = true;
            game.last();
            Assert.AreEqual(game._status, "jieju");

            // オーラス止め(東南戦)
            game = init_game(new GameParam { defen = new List<int> { 40000, 30000, 20000, 10000 } });
            game.model.zhuangfeng = 1;
            game.model.jushu = 3;
            game._lianzhuang = true;
            game.last();
            Assert.AreEqual(game._status, "jieju");

            // 途中流局ではオーラス止めしないこと
            game = init_game(new GameParam { defen = new List<int> { 40000, 30000, 20000, 10000 } });
            game.model.zhuangfeng = 1;
            game.model.jushu = 3;
            game._lianzhuang = true;
            game._no_game = true;
            game.last();
            Assert.AreEqual(game._status, "qipai");

            // オーラス止めなし
            game = init_game(new GameParam { rule = new Majiang.Rule{ オーラス止めあり = false }, defen = new List<int> { 40000, 30000, 20000, 10000 } });
            game.model.zhuangfeng = 1;
            game.model.jushu = 3;
            game._lianzhuang = true;
            game.last();
            Assert.AreEqual(game._status, "qipai");

            // 一荘戦では延長戦がないこと
            game = init_game(new GameParam { rule = new Majiang.Rule{ 場数 = 4 } });
            game.model.zhuangfeng = 3;
            game.model.jushu = 3;
            game.last();
            Assert.AreEqual(game._status, "jieju");

            // 延長戦なし
            game = init_game(new GameParam { rule = new Majiang.Rule{ 延長戦方式 = 0 } });
            game.model.zhuangfeng = 1;
            game.model.jushu = 3;
            game.last();
            Assert.AreEqual(game._status, "jieju");

            // 延長戦突入
            game = init_game();
            game.model.zhuangfeng = 1;
            game.model.jushu = 3;
            game.last();
            Assert.AreEqual(game._status, "qipai");

            // 延長戦サドンデス
            game = init_game(new GameParam { defen = new List<int> { 10000, 20000, 30000, 40000 } });
            game.model.zhuangfeng = 2;
            game.model.jushu = 0;
            game.last();
            Assert.AreEqual(game._max_jushu, 7);
            Assert.AreEqual(game._status, "jieju");

            // 連荘優先サドンデス
            game = init_game(new GameParam { rule = new Majiang.Rule{ 延長戦方式 = 2 } });
            game.model.zhuangfeng = 1;
            game.model.jushu = 3;
            game.last();
            Assert.AreEqual(game._max_jushu, 8);
            Assert.AreEqual(game._status, "qipai");

            // 4局固定延長戦オーラス止め
            game = init_game(new GameParam { rule = new Majiang.Rule{ 延長戦方式 = 3 } });
            game.model.zhuangfeng = 1;
            game.model.jushu = 3;
            game.last();
            Assert.AreEqual(game._max_jushu, 11);
            Assert.AreEqual(game._status, "qipai");

            // 延長戦は最大四局で終了すること
            game = init_game();
            game.model.zhuangfeng = 2;
            game.model.jushu = 3;
            game.last();
            Assert.AreEqual(game._status, "jieju");

            // 一局戦には延長戦はない
            game = init_game(new GameParam { rule = new Majiang.Rule{ 場数 = 0 } });
            game.model.zhuangfeng = 0;
            game.model.jushu = 0;
            game.last();
            Assert.AreEqual(game._status, "jieju");
        }

        [Test, Description("jieju()")]
        public void TestJieju()
        {
            var game = init_game(new GameParam { qijia = 1, defen = new List<int> { 20400, 28500, 20500, 30600 } });

            // 牌譜が記録されること
            game.jieju();
            CollectionAssert.AreEqual(game._paipu.defen, new List<int> { 30600, 20400, 28500, 20500 });
            CollectionAssert.AreEqual(game._paipu.rank, new List<int> { 1, 4, 2, 3 });
            CollectionAssert.AreEqual(game._paipu.point, new List<string> { "40.6", "-29.6", "8.5", "-19.5" });

            // 表示処理が呼び出されること
            Assert.IsNotNull(((View)game._view)._param.summary);

            // 通知が伝わること
            for (int l = 0; l < 4; l++) {
                int id = game.model.player_id[l];
                Assert.IsNotNull(MSG[id].jieju);
            }

            int doneCount = 0;
            Action done = () => { doneCount++; };

            // 通知のタイミングを変更できること
            game = init_game();
            game.wait = 20;
            MSG.Clear();
            game._sync = false;
            game.stop(done);
            game.jieju();
            Assert.AreEqual(MSG.Count, 0);

            Thread.Sleep(20 + 10);
            Assert.AreEqual(MSG.Count, 4);
            Assert.IsTrue(doneCount > 0);

            // 同点の場合は起家に近い方を上位とする
            game = init_game(new GameParam { qijia = 2 });
            game.jieju();
            CollectionAssert.AreEqual(game._paipu.rank, new List<int> { 3, 4, 1, 2 });
            CollectionAssert.AreEqual(game._paipu.point, new List<string> { "-15.0", "-25.0", "35.0", "5.0" });

            // 終局時に残った供託リーチ棒はトップの得点に加算
            game = init_game(new GameParam { qijia = 3, defen = new List<int> { 9000, 19000, 29000, 40000 }, lizhibang = 3 });
            game.jieju();
            CollectionAssert.AreEqual(game._paipu.defen, new List<int> { 19000, 29000, 43000, 9000 });

            // 1000点未満のポイントは四捨五入する
            game = init_game(new GameParam {
                qijia = 0,
                defen = new List<int> { 20400, 28500, 20500, 30600 },
                rule = new Majiang.Rule{ 順位点 = new List<string> { "20", "10", "-10", "-20" } }
            });
            game.jieju();
            CollectionAssert.AreEqual(game._paipu.point, new List<string> { "-30", "9", "-19", "40" });

            // 1000点未満のポイントは四捨五入する(負のケース)
            game = init_game(new GameParam { qijia = 0, defen = new List<int> { -1500, 83800, 6000, 11700 }, rule = new Majiang.Rule{ 順位点 = new List<string> { "20", "10", "-10", "-20" } } });
            game.jieju();
            CollectionAssert.AreEqual(game._paipu.point, new List<string> { "-51", "93", "-34", "-8" });

            // 順位点を変更できること
            game = init_game(new GameParam { rule = new Majiang.Rule{ 順位点 = new List<string> { "30", "10", "-10", "-30" } }, qijia = 2 });
            game.jieju();
            CollectionAssert.AreEqual(game._paipu.rank, new List<int> { 3, 4, 1, 2 });
            CollectionAssert.AreEqual(game._paipu.point, new List<string> { "-15", "-35", "45", "5" });

            // [追加] 負の点数ケース追加
            game = init_game(new GameParam { qijia = 0, defen = new List<int> { 8900, 8900, -3100, 85300 }, rule = new Majiang.Rule{ 順位点 = new List<string> { "20", "10", "-10", "-20" } } });
            game.jieju();
            CollectionAssert.AreEqual(game._paipu.point, new List<string> { "-11", "-31", "-53", "95" });

            // [追加] 負の点数ケース追加
            game = init_game(new GameParam { qijia = 0, defen = new List<int> { 35800, -1900, 27100, 39000 }, rule = new Majiang.Rule{ 順位点 = new List<string> { "20", "10", "-10", "-20" } } });
            game.jieju();
            CollectionAssert.AreEqual(game._paipu.point, new List<string> { "16", "-52", "-13", "49" });

            // ハンドラがある場合、それを呼び出すこと
            game = init_game();
            doneCount = 0;
            game.handler = done;
            game.jieju();

            Thread.Sleep(10);
            Assert.AreEqual(doneCount, 1);
        }

        [Test, Description("reply_kaiju()")]
        public void TestReplyKaiju()
        {
            // 配牌に遷移すること
            var players = Enumerable.Range(0, 4).Select(id => new Player(id)).ToList<Majiang.Player>();
            Majiang.Rule rule = new Majiang.Rule();
            Majiang.Game game = new Majiang.Game(players, null, rule);
            game.view = new View();
            game._sync = true;
            game.kaiju();
            game.next();
            Assert.IsNotNull(last_paipu(game).qipai);
        }

        [Test, Description("reply_qipai()")]
        public void TestReplyQipai()
        {
            // ツモに遷移すること
            var game = init_game();
            game.qipai();
            game.next();
            Assert.IsNotNull(last_paipu(game).zimo);
        }

        [Test, Description("reply_zimo()")]
        public void TestReplyZimo()
        {
            // 打牌
            var game = init_game(new GameParam { zimo = new List<string> { "m1" } });
            set_reply(game, new List<Reply> { new Reply { dapai = "m1_" }, new Reply(), new Reply(), new Reply() });
            game.zimo();
            game.next();
            Assert.AreEqual(last_paipu(game).dapai.p, "m1_");

            // リーチ
            game = init_game(new GameParam { shoupai = new List<string> { "m123p456s789z1122", "", "", "" }, zimo = new List<string> { "m1" } });
            set_reply(game, new List<Reply> { new Reply { dapai = "m1_*" }, new Reply(), new Reply(), new Reply() });
            game.zimo();
            game.next();
            Assert.AreEqual(last_paipu(game).dapai.p, "m1_*");

            // 打牌(不正応答)
            game = init_game(new GameParam { zimo = new List<string> { "m1" } });
            set_reply(game, new List<Reply> { new Reply { dapai = "m2_" }, new Reply(), new Reply(), new Reply() });
            game.zimo();
            game.next();
            Assert.IsTrue(last_paipu(game).dapai != null);

            // 九種九牌
            game = init_game(new GameParam { shoupai = new List<string> { "m123459z1234567", "", "", "" } });
            set_reply(game, new List<Reply> { new Reply { daopai = "-" }, new Reply(), new Reply(), new Reply() });
            game.zimo();
            game.next();
            Assert.AreEqual(last_paipu(game).pingju.name, "九種九牌");

            // 九種九牌(不正応答)
            game = init_game(new GameParam { shoupai = new List<string> { "m234567z1234567", "", "", "" } });
            set_reply(game, new List<Reply> { new Reply { daopai = "-" }, new Reply(), new Reply(), new Reply() });
            game.zimo();
            game.next();
            Assert.IsTrue(last_paipu(game).dapai != null);

            // 途中流局なしの場合は九種九牌にできないこと
            game = init_game(new GameParam { rule = new Majiang.Rule { 途中流局あり = false }, shoupai = new List<string> { "m123459z1234567", "", "", "" } });
            set_reply(game, new List<Reply> { new Reply { daopai = "-" }, new Reply(), new Reply(), new Reply() });
            game.zimo();
            game.next();
            Assert.IsTrue(last_paipu(game).dapai != null);

            // ツモ和了
            game = init_game(new GameParam { shoupai = new List<string> { "m123p456s789z1122", "", "", "" }, zimo = new List<string> { "z1" } });
            set_reply(game, new List<Reply> { new Reply { hule = "-" }, new Reply(), new Reply(), new Reply() });
            game.zimo();
            game.next();
            Assert.AreEqual(((View)game._view)._say, new SayParam { type = "zimo", lunban = 0 });
            Assert.IsTrue(last_paipu(game).hule != null);

            // ツモ和了(不正応答)
            game = init_game(new GameParam { shoupai = new List<string> { "m123p456s789z1122", "", "", "" }, zimo = new List<string> { "z3" } });
            set_reply(game, new List<Reply> { new Reply { hule = "-" }, new Reply(), new Reply(), new Reply() });
            game.zimo();
            game.next();
            Assert.IsTrue(last_paipu(game).dapai != null);

            // カン
            game = init_game(new GameParam { shoupai = new List<string> { "m123p456z1122,s888+", "", "", "" }, zimo = new List<string> { "s8" } });
            set_reply(game, new List<Reply> { new Reply { gang = "s888+8" }, new Reply(), new Reply(), new Reply() });
            game.zimo();
            game.next();
            Assert.AreEqual(last_paipu(game).gang.m, "s888+8");

            // カン(不正応答)
            game = init_game(new GameParam { shoupai = new List<string> { "m123p456z1122,s888+", "", "", "" }, zimo = new List<string> { "s7" } });
            set_reply(game, new List<Reply> { new Reply { gang = "s888+8" }, new Reply(), new Reply(), new Reply() });
            game.zimo();
            game.next();
            Assert.IsTrue(last_paipu(game).dapai != null);

            // 5つめのカンができないこと
            game = init_game(new GameParam { shoupai = new List<string> { "m123p456z1122,s888+", "", "", "" }, zimo = new List<string> { "s8" } });
            game._n_gang = new List<int> { 0, 0, 0, 4 };
            set_reply(game, new List<Reply> { new Reply { gang = "s888+8" }, new Reply(), new Reply(), new Reply() });
            game.zimo();
            game.next();
            Assert.IsTrue(last_paipu(game).dapai != null);

            // 無応答のときにツモ切りすること
            game = init_game(new GameParam { zimo = new List<string> { "m1" } });
            game.zimo();
            game.next();
            Assert.AreEqual(last_paipu(game).dapai.p, "m1_");

            // 槓ツモ
            game = init_game(new GameParam { shoupai = new List<string> { "_", "", "", "" } });
            game.zimo();
            game.gang("m1111");
            game.gangzimo();
            game.next();
            Assert.IsTrue(last_paipu(game).dapai != null);
        }

        [Test, Description("reply_dapai()")]
        public void TestReplyDapai()
        {
            // 応答なし
            var game = init_game(new GameParam { shoupai = new List<string> { "_", "", "", "" } });
            game.zimo();
            game.dapai("m1");
            game.next();
            Assert.IsTrue(last_paipu(game).zimo != null);

            // ロン和了
            game = init_game(new GameParam { shoupai = new List<string> { "_", "m123p456s789z1122", "", "" } });
            game.zimo();
            set_reply(game, new List<Reply> { new Reply(), new Reply { hule = "-" }, new Reply(), new Reply() });
            game.dapai("z1");
            game.next();
            Assert.AreEqual(((View)game._view)._say, new SayParam { type = "rong", lunban = 1 });
            Assert.AreEqual(last_paipu(game).hule.l, 1);

            // 和了見逃しでフリテンになること
            game = init_game(new GameParam { shoupai = new List<string> { "_", "m123p456s789z1122", "", "" } });
            game.zimo();
            game.dapai("z1");
            game.next();
            Assert.IsFalse(game._neng_rong[1]);

            // ダブロン
            game = init_game(new GameParam { shoupai = new List<string> { "_", "m23446p45688s345", "m34s33,s444-,s666+,p406-", "" } });
            game.zimo();
            set_reply(game, new List<Reply> { new Reply(), new Reply { hule = "-" }, new Reply { hule = "-" }, new Reply() });
            game.dapai("m5*");
            game.next();
            Assert.AreEqual(((View)game._view)._say, new SayParam { type = "rong", lunban = 2 });
            Assert.AreEqual(last_paipu(game).hule.l, 1);
            CollectionAssert.AreEqual(game._hule, new List<int> { 2 });

            // ダブロンを頭ハネに変更できること
            game = init_game(new GameParam { rule = new Majiang.Rule { 最大同時和了数 = 1 }, shoupai = new List<string> { "_", "m23446p45688s345", "m34s33,s444-,s666+,p406-", "" } });
            game.zimo();
            set_reply(game, new List<Reply> { new Reply(), new Reply { hule = "-" }, new Reply { hule = "-" }, new Reply() });
            game.dapai("m5*");
            game.next();
            Assert.AreEqual(((View)game._view)._say, new SayParam { type = "rong", lunban = 1 });
            Assert.AreEqual(last_paipu(game).hule.l, 1);
            Assert.IsEmpty(game._hule);

            // 三家和
            game = init_game(new GameParam { shoupai = new List<string> { "_", "m23446p45688s345", "m34s33,s444-,s666+,p406-", "m23467s88,s222+,z666=" } });
            game.zimo();
            set_reply(game, new List<Reply> { new Reply(), new Reply { hule = "-" }, new Reply { hule = "-" }, new Reply { hule = "-" } });
            game.dapai("m5*");
            game.next();
            Assert.AreEqual(((View)game._view)._say, new SayParam { type = "rong", lunban = 3 });
            Assert.AreEqual(last_paipu(game).pingju.name, "三家和");
            CollectionAssert.AreEqual(last_paipu(game).pingju.shoupai, new List<string> { "", "m23446p45688s345", "m34s33,s444-,s666+,p406-", "m23467s88,s222+,z666=" });

            // トリロン可に変更できること
            game = init_game(new GameParam { rule = new Majiang.Rule { 最大同時和了数 = 3 }, shoupai = new List<string> { "_", "m23446p45688s345", "m34s33,s444-,s666+,p406-", "m23467s88,s222+,z666=" } });
            game.zimo();
            set_reply(game, new List<Reply> { new Reply(), new Reply { hule = "-" }, new Reply { hule = "-" }, new Reply { hule = "-" } });
            game.dapai("m5*");
            game.next();
            Assert.AreEqual(((View)game._view)._say, new SayParam { type = "rong", lunban = 3 });
            Assert.AreEqual(last_paipu(game).hule.l, 1);
            CollectionAssert.AreEqual(game._hule, new List<int> { 2, 3 });

            // リーチ成立
            game = init_game(new GameParam { shoupai = new List<string> { "m55688p234567s06", "", "", "" }, qijia = 0, zimo = new List<string> { "s7" } });
            game.zimo();
            game.dapai("m5*");
            game.next();
            Assert.AreEqual(game.model.defen[0], 24000);
            Assert.AreEqual(game.model.lizhibang, 1);
            Assert.IsTrue(last_paipu(game).zimo != null);

            // リーチ不成立
            game = init_game(new GameParam { shoupai = new List<string> { "m55688p234567s06", "m23446p45688s345", "", "" }, qijia = 0, zimo = new List<string> { "s7" } });
            game.zimo();
            set_reply(game, new List<Reply> { new Reply(), new Reply { hule = "-" }, new Reply(), new Reply() });
            game.dapai("m5*");
            game.next();
            Assert.AreEqual(game.model.defen[0], 25000);
            Assert.AreEqual(game.model.lizhibang, 0);
            Assert.IsTrue(last_paipu(game).hule != null);

            // 四家立直
            game = init_game(new GameParam { shoupai = new List<string> { "m11156p5688s2346", "m2227p11340s2356", "m2346789p345699", "m34056p4678s3456" }, qijia = 0, zimo = new List<string> { "p4", "s1", "m7", "s6" } });
            foreach (var p in new List<string> { "s6*", "m7*", "p6*", "p4*" })
            {
                game.zimo();
                game.dapai(p);
            }
            game.next();
            Assert.AreEqual(last_paipu(game).pingju.name, "四家立直");
            CollectionAssert.AreEqual(last_paipu(game).pingju.shoupai, new List<string> { "m11156p45688s234*", "m222p11340s12356*", "m23467789p34599*", "m34056p678s34566*" });

            // 途中流局なしの場合は四家立直でも続行すること
            game = init_game(new GameParam { rule = new Majiang.Rule { 途中流局あり = false }, shoupai = new List<string> { "m11156p5688s2346", "m2227p11340s2356", "m2346789p345699", "m34056p4678s3456" }, qijia = 0, zimo = new List<string> { "p4", "s1", "m7", "s6" } });
            foreach (var p in new List<string> { "s6*", "m7*", "p6*", "p4*" })
            {
                game.zimo();
                game.dapai(p);
            }
            game.next();
            Assert.IsTrue(last_paipu(game).zimo != null);

            // 四風連打
            game = init_game(new GameParam { shoupai = new List<string> { "_", "_", "_", "_" } });
            for (int l = 0; l < 4; l++)
            {
                game.zimo();
                game.dapai("z1");
            }
            game.next();
            Assert.AreEqual(last_paipu(game).pingju.name, "四風連打");
            CollectionAssert.AreEqual(last_paipu(game).pingju.shoupai, new List<string> { "", "", "", "" });

            // 途中流局なしの場合は四風連打とならず、第一ツモ巡が終了すること
            game = init_game(new GameParam { rule = new Majiang.Rule { 途中流局あり = false }, shoupai = new List<string> { "_", "_", "_", "_" } });
            for (int l = 0; l < 4; l++)
            {
                game.zimo();
                game.dapai("z1");
            }
            game.next();
            Assert.IsFalse(game._diyizimo);
            Assert.IsTrue(last_paipu(game).zimo != null);

            // 四開槓
            game = init_game(new GameParam { shoupai = new List<string> { "_", "m111p22279s57,s333=", "m123p456s222789z2", "" }, zimo = new List<string> { "m1" }, gangzimo = new List<string> { "p2", "s3", "s2", "z7" } });
            game.zimo();
            game.dapai("m1_");
            game.fulou("m1111-");
            game.gangzimo();
            game.gang("p2222");
            game.gangzimo();
            game.gang("s333=3");
            game.gangzimo();
            game.dapai("s2");
            game.fulou("s2222-");
            game.gangzimo();
            game.dapai("z7_");
            game.next();
            Assert.AreEqual(last_paipu(game).pingju.name, "四開槓");
            CollectionAssert.AreEqual(last_paipu(game).pingju.shoupai, new List<string> { "", "", "", "" });

            // 1人で四開槓
            game = init_game(new GameParam { shoupai = new List<string> { "m1112,p111+,s111=,z111-", "", "", "" }, zimo = new List<string> { "m1" }, gangzimo = new List<string> { "p1", "s1", "z1", "z7" } });
            game.zimo();
            game.gang("m1111");
            game.gangzimo();
            game.gang("p111+1");
            game.gangzimo();
            game.gang("s111=1");
            game.gangzimo();
            game.gang("z111-1");
            game.gangzimo();
            game.dapai("z7");
            game.next();
            Assert.IsTrue(last_paipu(game).zimo != null);

            // 途中流局なしでは四開槓とならない
            game = init_game(new GameParam { rule = new Majiang.Rule { 途中流局あり = false }, shoupai = new List<string> { "_", "m111p22279s57,s333=", "m123p456s222789z2", "" }, zimo = new List<string> { "m1" }, gangzimo = new List<string> { "p2", "s3", "s2", "z7" } });
            game.zimo();
            game.dapai("m1_");
            game.fulou("m1111-");
            game.gangzimo();
            game.gang("p2222");
            game.gangzimo();
            game.gang("s333=3");
            game.gangzimo();
            game.dapai("s2");
            game.fulou("s2222-");
            game.gangzimo();
            game.dapai("z7_");
            game.next();
            Assert.IsTrue(last_paipu(game).zimo != null);

            // 流局
            game = init_game(new GameParam { rule = new Majiang.Rule { 流し満貫あり = false, ノーテン宣言あり = true }, shoupai = new List<string> { "_", "m222p11340s12356", "m23467789p34599", "_" } });
            game.zimo();
            while (game.model.shan.paishu > 0) game.model.shan.zimo();
            set_reply(game, new List<Reply> { new Reply(), new Reply { daopai = "-" }, new Reply { daopai = "-" }, new Reply() });
            game.dapai(game.model.shoupai[0].get_dapai()[0]);
            game.next();
            Assert.AreEqual(last_paipu(game).pingju.name, "荒牌平局");
            CollectionAssert.AreEqual(last_paipu(game).pingju.shoupai, new List<string> { "", "m222p11340s12356", "m23467789p34599", "" });

            // カン
            game = init_game(new GameParam { shoupai = new List<string> { "_", "", "", "m111234p567s3378" } });
            game.zimo();
            set_reply(game, new List<Reply> { new Reply(), new Reply(), new Reply(), new Reply { fulou = "m1111+" } });
            game.dapai("m1");
            game.next();
            Assert.AreEqual(((View)game._view)._say, new SayParam { type = "gang", lunban = 3 });
            Assert.AreEqual(last_paipu(game).fulou.m, "m1111+");

            // カン(不正応答)
            game = init_game(new GameParam { shoupai = new List<string> { "_", "", "", "m111234p567s3378" } });
            game.zimo();
            set_reply(game, new List<Reply> { new Reply(), new Reply(), new Reply(), new Reply { fulou = "m1111+" } });
            game.dapai("m2");
            game.next();
            Assert.IsTrue(last_paipu(game).zimo != null);

            // 5つめのカンができないこと
            game = init_game(new GameParam { shoupai = new List<string> { "_", "", "", "m111234p567s3378" } });
            game._n_gang = new List<int> { 4, 0, 0, 0 };
            game.zimo();
            set_reply(game, new List<Reply> { new Reply(), new Reply(), new Reply(), new Reply { fulou = "m1111+" } });
            game.dapai("m1");
            game.next();
            Assert.IsTrue(last_paipu(game).zimo != null);

            // ポン
            game = init_game(new GameParam { shoupai = new List<string> { "_", "", "m112345p567s3378", "" } });
            game.zimo();
            set_reply(game, new List<Reply> { new Reply(), new Reply(), new Reply { fulou = "m111=" }, new Reply() });
            game.dapai("m1");
            game.next();
            Assert.AreEqual(((View)game._view)._say, new SayParam { type = "peng", lunban = 2 });
            Assert.AreEqual(last_paipu(game).fulou.m, "m111=");

            // ポン(不正応答)
            game = init_game(new GameParam { shoupai = new List<string> { "_", "", "m112345p567s3378", "" } });
            game.zimo();
            set_reply(game, new List<Reply> { new Reply(), new Reply(), new Reply { fulou = "m111=" }, new Reply() });
            game.dapai("m2");
            game.next();
            Assert.IsTrue(last_paipu(game).zimo != null);

            // チー
            game = init_game(new GameParam { shoupai = new List<string> { "_", "m112345p567s3378", "", "" } });
            game.zimo();
            set_reply(game, new List<Reply> { new Reply(), new Reply { fulou = "m456-" }, new Reply(), new Reply() });
            game.dapai("m6");
            game.next();
            Assert.AreEqual(((View)game._view)._say, new SayParam { type = "chi", lunban = 1 });
            Assert.AreEqual(last_paipu(game).fulou.m, "m456-");

            // チー(不正応答)
            game = init_game(new GameParam { shoupai = new List<string> { "_", "m112345p567s3378", "", "" } });
            game.zimo();
            set_reply(game, new List<Reply> { new Reply(), new Reply { fulou = "m456-" }, new Reply(), new Reply() });
            game.dapai("m5");
            game.next();
            Assert.IsTrue(last_paipu(game).zimo != null);

            // ポンとチーの競合はポンを優先
            game = init_game(new GameParam { shoupai = new List<string> { "_", "m23567p456s889z11", "m11789p123s11289", "" } });
            game.zimo();
            set_reply(game, new List<Reply> { new Reply(), new Reply { fulou = "m1-23" }, new Reply { fulou = "m111=" }, new Reply() });
            game.dapai("m1");
            game.next();
            Assert.AreEqual(((View)game._view)._say, new SayParam { type = "peng", lunban = 2 });
            Assert.AreEqual(last_paipu(game).fulou.m, "m111=");
        }

        [Test, Description("reply_fulou()")]
        public void TestReplyFulou()
        {
            // 大明槓
            var game = init_game(new GameParam { shoupai = new List<string> { "_", "m1112356p456s889", "", "" } });
            game.zimo();
            game.dapai("m1");
            game.fulou("m1111-");
            game.next();
            Assert.IsTrue(last_paipu(game).gangzimo != null);

            // チー・ポン → 打牌
            game = init_game(new GameParam { shoupai = new List<string> { "_", "m23567p456s889z11", "", "" } });
            game.zimo();
            game.dapai("m1");
            set_reply(game, new List<Reply> { new Reply(), new Reply { dapai = "s9" }, new Reply(), new Reply() });
            game.fulou("m1-23");
            game.next();
            Assert.AreEqual(last_paipu(game).dapai.p, "s9");

            // チー・ポン → 打牌(不正応答)
            game = init_game(new GameParam { shoupai = new List<string> { "_", "m23456p456s889z11", "", "" } });
            game.zimo();
            game.dapai("m1");
            set_reply(game, new List<Reply> { new Reply(), new Reply { dapai = "m4" }, new Reply(), new Reply() });
            game.fulou("m1-23");
            game.next();
            Assert.AreEqual(last_paipu(game).dapai.p, "z1");

            // 無応答のときに右端の牌を切ること
            game = init_game(new GameParam { shoupai = new List<string> { "_", "m23567p456s889z11", "", "" } });
            game.zimo();
            game.dapai("m1");
            game.fulou("m1-23");
            game.next();
            Assert.AreEqual(last_paipu(game).dapai.p, "z1");
        }

        [Test, Description("reply_gang()")]
        public void TestReplyGang()
        {
            // 応答なし
            var game = init_game(new GameParam { shoupai = new List<string> { "m45p456s11789,m111+", "", "", "" }, zimo = new List<string> { "m1" } });
            game.zimo();
            game.gang("m111+1");
            game.next();
            Assert.IsTrue(last_paipu(game).gangzimo != null);

            // ロン和了(槍槓)
            game = init_game(new GameParam { shoupai = new List<string> { "m45p456s11789,m111+", "", "", "m23456p123s67899" }, zimo = new List<string> { "m1" } });
            game.zimo();
            set_reply(game, new List<Reply> { new Reply(), new Reply(), new Reply(), new Reply { hule = "-" } });
            game.gang("m111+1");
            game.next();
            Assert.AreEqual(((View)game._view)._say, new SayParam { type = "rong", lunban = 3 });
            Assert.AreEqual(last_paipu(game).hule.l, 3);

            // ロン和了(不正応答)
            game = init_game(new GameParam { shoupai = new List<string> { "m45p456s11789,m111+", "", "", "m33456p123s67899" }, zimo = new List<string> { "m1" } });
            game.zimo();
            set_reply(game, new List<Reply> { new Reply(), new Reply(), new Reply(), new Reply { hule = "-" } });
            game.gang("m111+1");
            game.next();
            Assert.IsTrue(last_paipu(game).gangzimo != null);

            // 暗槓は槍槓できない
            game = init_game(new GameParam { shoupai = new List<string> { "m11145p456s11789", "", "", "m23456p123s67899" }, zimo = new List<string> { "m1" } });
            game.zimo();
            set_reply(game, new List<Reply> { new Reply(), new Reply(), new Reply(), new Reply { hule = "-" } });
            game.gang("m1111");
            game.next();
            Assert.IsTrue(last_paipu(game, -1).gangzimo != null);
            Assert.IsTrue(last_paipu(game).kaigang != null);

            // 和了見逃しでフリテンになること
            game = init_game(new GameParam { shoupai = new List<string> { "m45p456s11789,m111+", "", "", "m23456p123s67899" }, zimo = new List<string> { "m1" } });
            game.zimo();
            game.gang("m111+1");
            game.next();
            Assert.IsFalse(game._neng_rong[3]);

            // ダブロン
            game = init_game(new GameParam { shoupai = new List<string> { "m11p222s88,z666=,m505-", "m23446p45688s345", "m34s33,s444-,s666+,p406-", "" }, zimo = new List<string> { "m5" } });
            game.zimo();
            set_reply(game, new List<Reply> { new Reply(), new Reply { hule = "-" }, new Reply { hule = "-" }, new Reply() });
            game.gang("m505-5");
            game.next();
            Assert.AreEqual(((View)game._view)._say, new SayParam { type = "rong", lunban = 2 });
            Assert.AreEqual(last_paipu(game).hule.l, 1);
            CollectionAssert.AreEqual(game._hule, new List<int> { 2 });

            // ダブロンを頭ハネに変更できること
            game = init_game(new GameParam { rule = new Majiang.Rule { 最大同時和了数 = 1 }, shoupai = new List<string> { "m11p222s88,z666=,m505-", "m23446p45688s345", "m34s33,s444-,s666+,p406-", "" }, zimo = new List<string> { "m5" } });
            game.zimo();
            set_reply(game, new List<Reply> { new Reply(), new Reply { hule = "-" }, new Reply { hule = "-" }, new Reply() });
            game.gang("m505-5");
            game.next();
            Assert.AreEqual(((View)game._view)._say, new SayParam { type = "rong", lunban = 1 });
            Assert.AreEqual(last_paipu(game).hule.l, 1);
            CollectionAssert.AreEqual(game._hule, new List<int>());
        }

        [Test, Description("reply_hule()")]
        public void TestReplyHule()
        {
            // 親のツモ和了
            var game = init_game(new GameParam { shoupai = new List<string> { "m345567p111s3368", "", "", "" }, qijia = 0, changbang = 1, lizhibang = 1, defen = new List<int> { 25000, 25000, 25000, 24000 }, baopai = "p2", zimo = new List<string> { "s7" } });
            game._diyizimo = false;
            game.zimo();
            game._hule = new List<int> { 0 };
            game.hule();
            game.next();
            CollectionAssert.AreEqual(game.model.defen, new List<int> { 28400, 24200, 24200, 23200 });
            Assert.AreEqual(game.model.changbang, 2);
            Assert.AreEqual(game.model.lizhibang, 0);
            Assert.IsTrue(last_paipu(game).qipai != null);

            // 子のロン和了
            game = init_game(new GameParam { shoupai = new List<string> { "_", "m345567p111s66z11", "", "" }, qijia = 0, changbang = 1, lizhibang = 1, defen = new List<int> { 25000, 25000, 25000, 24000 }, baopai = "p2", zimo = new List<string> { "s7" } });
            game.zimo();
            game.dapai("z1");
            game._hule = new List<int> { 1 };
            game.hule();
            game.next();
            CollectionAssert.AreEqual(game.model.defen, new List<int> { 23100, 27900, 25000, 24000 });
            Assert.AreEqual(game.model.changbang, 0);
            Assert.AreEqual(game.model.lizhibang, 0);
            Assert.IsTrue(last_paipu(game).qipai != null);

            // ダブロンで連荘
            game = init_game(new GameParam { shoupai = new List<string> { "m23p456s789z11122", "_", "m23p789s546z33344", "" }, qijia = 0, changbang = 1, lizhibang = 1, defen = new List<int> { 25000, 25000, 25000, 24000 }, baopai = "s9", zimo = new List<string> { "m2", "m1" } });
            game.zimo();
            game.dapai("m2");
            game.zimo();
            game.dapai("m1");
            game._hule = new List<int> { 2, 0 };
            game.hule();
            game.next();
            CollectionAssert.AreEqual(game.model.defen, new List<int> { 25000, 23400, 27600, 24000 });
            Assert.AreEqual(game.model.changbang, 0);
            Assert.AreEqual(game.model.lizhibang, 0);
            Assert.IsTrue(last_paipu(game).hule != null);
            game.next();
            CollectionAssert.AreEqual(game.model.defen, new List<int> { 28900, 19500, 27600, 24000 });
            Assert.AreEqual(game.model.changbang, 2);
            Assert.AreEqual(game.model.lizhibang, 0);
            Assert.IsTrue(last_paipu(game).qipai != null);
        }

        [Test, Description("reply_pingju()")]
        public void TestReplyPingju()
        {
            // 流局
            var game = init_game(new GameParam { shoupai = new List<string> { "m123p456s789z1122", "_", "_", "_" }, qijia = 0, changbang = 1, lizhibang = 1, defen = new List<int> { 25000, 25000, 25000, 24000 }, zimo = new List<string> { "m2", "m3", "m4", "m5" } });
            foreach (var p in new List<string> { "m2", "m3", "m4", "m5" })
            {
                game.zimo();
                game.dapai(p);
            }
            game.pingju();
            game.next();
            CollectionAssert.AreEqual(game.model.defen, new List<int> { 28000, 24000, 24000, 23000 });
            Assert.AreEqual(game.model.changbang, 2);
            Assert.AreEqual(game.model.lizhibang, 1);
            Assert.IsTrue(last_paipu(game).qipai != null);
        }

        [Test, Description("_callback()")]
        public void TestCallback()
        {
            Paipu result = null;

            // 終局時にコンストラクタで指定したコールバックが呼ばれること
            Action<Paipu> callback = (paipu) =>
            {
                result = paipu;
            };
            var game = init_game(new GameParam { rule = new Majiang.Rule { 場数 = 0 }, shoupai = new List<string> { "m123p456s789z1122", "", "", "" }, zimo = new List<string> { "z2" }, callback = callback });
            game.zimo();
            game.hule();
            game.next();
            game.next();

            Thread.Sleep(100);

            Assert.AreEqual(result, game._paipu);
        }

        [Test, Description("get_dapai()")]
        public void TestGetDapai()
        {
            // 現在の手番の可能な打牌を返すこと
            var game = init_game(new GameParam { shoupai = new List<string> { "m123,z111+,z222=,z333-", "", "", "" }, zimo = new List<string> { "m1" } });
            game.zimo();
            CollectionAssert.AreEqual(game.get_dapai(), new List<string> { "m1", "m2", "m3", "m1_" });

            // 現物喰い替えありに変更できること
            game = init_game(new GameParam { rule = new Majiang.Rule { 喰い替え許可レベル = 2 }, shoupai = new List<string> { "_", "m1234p567,z111=,s789-", "", "" } });
            game.zimo();
            game.dapai("m1");
            game.fulou("m1-23");
            CollectionAssert.AreEqual(game.get_dapai(), new List<string> { "m1", "m4", "p5", "p6", "p7" });
        }

        [Test, Description("get_chi_mianzi()")]
        public void TestGetChiMianzi()
        {
            // 現在打たれた牌でチーできる面子を返すこと
            var game = init_game(new GameParam { shoupai = new List<string> { "", "_", "m1234p456s789z111", "" } });
            game.zimo();
            game.dapai(game.get_dapai()[0]);
            game.zimo();
            game.dapai("m2");
            CollectionAssert.AreEqual(game.get_chi_mianzi(2), new List<string> { "m12-3", "m2-34" });

            // 自身はチーできないこと
            game = init_game(new GameParam { shoupai = new List<string> { "m1234p456s789z111", "", "", "" } });
            game.zimo();
            game.dapai(game.get_dapai().Last());
            Assert.Throws<Exception>(() => game.get_chi_mianzi(0));

            // 対面はチーできないこと
            game = init_game(new GameParam { shoupai = new List<string> { "_", "", "m1234p456s789z111", "" } });
            game.zimo();
            game.dapai("m2");
            CollectionAssert.AreEqual(game.get_chi_mianzi(2), new List<string>());

            // ハイテイ牌はチーできないこと
            game = init_game(new GameParam { shoupai = new List<string> { "_", "m1234p456s789z111", "", "" } });
            game.zimo();
            while (game.model.shan.paishu > 0) game.model.shan.zimo();
            game.dapai("m2");
            CollectionAssert.AreEqual(game.get_chi_mianzi(1), new List<string>());

            // 現物喰い替えありに変更できること
            game = init_game(new GameParam { rule = new Majiang.Rule { 喰い替え許可レベル = 2 }, shoupai = new List<string> { "_", "m1123,p456-,z111=,s789-", "", "" } });
            game.zimo();
            game.dapai("m1");
            CollectionAssert.AreEqual(game.get_chi_mianzi(1), new List<string> { "m1-23" });
        }

        [Test, Description("get_peng_mianzi(l)")]
        public void TestGetPengMianzi()
        {
            // 現在打たれた牌でポンできる面子を返すこと
            var game = init_game(new GameParam { shoupai = new List<string> { "", "_", "", "m1123p456s789z111" } });
            game.zimo();
            game.dapai(game.get_dapai()[0]);
            game.zimo();
            game.dapai("m1");
            CollectionAssert.AreEqual(game.get_peng_mianzi(3), new List<string> { "m111=" });

            // 自身はポンできないこと
            game = init_game(new GameParam { shoupai = new List<string> { "m1123p456s789z111", "", "", "" } });
            game.zimo();
            game.dapai(game.get_dapai().Last());
            Assert.Throws<Exception>(() => game.get_peng_mianzi(0));

            // ハイテイ牌はポンできないこと
            game = init_game(new GameParam { shoupai = new List<string> { "_", "", "m1123p456s789z111", "" } });
            game.zimo();
            while (game.model.shan.paishu > 0) game.model.shan.zimo();
            game.dapai("m1");
            CollectionAssert.AreEqual(game.get_peng_mianzi(2), new List<string>());
        }

        [Test, Description("get_gang_mianzi(l)")]
        public void TestGetGangMianzi()
        {
            // 現在打たれた牌で大明槓できる面子を返すこと
            var game = init_game(new GameParam { shoupai = new List<string> { "", "_", "", "m1112p456s789z111" } });
            game.zimo();
            game.dapai(game.get_dapai()[0]);
            game.zimo();
            game.dapai("m1");
            CollectionAssert.AreEqual(game.get_gang_mianzi(3), new List<string> { "m1111=" });

            // 自身は大明槓できないこと
            game = init_game(new GameParam { shoupai = new List<string> { "m1112p456s789z111", "", "", "" } });
            game.zimo();
            game.dapai(game.get_dapai().Last());
            Assert.Throws<Exception>(() => game.get_gang_mianzi(0));

            // ハイテイ牌は大明槓できないこと
            game = init_game(new GameParam { shoupai = new List<string> { "_", "", "m1112p456s789z111", "" } });
            game.zimo();
            while (game.model.shan.paishu > 0) game.model.shan.zimo();
            game.dapai("m1");
            CollectionAssert.AreEqual(game.get_gang_mianzi(2), new List<string>());

            // 現在の手番が暗槓もしくは加槓できる面子を返すこと
            game = init_game(new GameParam { shoupai = new List<string> { "m1111p4569s78,z111=", "", "", "" }, zimo = new List<string> { "z1" } });
            game.zimo();
            CollectionAssert.AreEqual(game.get_gang_mianzi(), new List<string> { "m1111", "z111=1" });

            // ハイテイ牌は暗槓もしくは加槓できないこと
            game = init_game(new GameParam { shoupai = new List<string> { "m1111p4567s78,z111=", "", "", "" }, zimo = new List<string> { "z1" } });
            game.zimo();
            while (game.model.shan.paishu > 0) game.model.shan.zimo();
            CollectionAssert.AreEqual(game.get_gang_mianzi(), new List<string>());

            // リーチ後の暗槓なしに変更できること
            game = init_game(new GameParam { rule = new Majiang.Rule { リーチ後暗槓許可レベル = 0 }, shoupai = new List<string> { "m111p456s789z1122*", "", "", "" }, zimo = new List<string> { "m1" } });
            game.zimo();
            CollectionAssert.AreEqual(game.get_gang_mianzi(), new List<string>());
        }

        [Test, Description("allow_lizhi(p)")]
        public void TestAllowLizhi()
        {
            // 指定された打牌でリーチ可能な場合、真を返す
            var game = init_game(new GameParam { shoupai = new List<string> { "m123p456s789z1123", "", "", "" }, zimo = new List<string> { "z2" } });
            game.zimo();
            Assert.IsNotNull(game.allow_lizhi("z3*"));

            // ツモ番がない場合、リーチできない
            game = init_game(new GameParam { shoupai = new List<string> { "m123p456s789z1123", "", "", "" }, zimo = new List<string> { "z2" } });
            game.zimo();
            while (game.model.shan.paishu >= 4) game.model.shan.zimo();
            Assert.IsNull(game.allow_lizhi("z3*"));

            // 持ち点が1000点に満たない場合、リーチできない
            game = init_game(new GameParam { shoupai = new List<string> { "m123p456s789z1123", "", "", "" }, zimo = new List<string> { "z2" }, defen = new List<int> { 900, 19100, 45000, 35000 } });
            game.zimo();
            Assert.IsNull(game.allow_lizhi("z3*"));

            // ツモ番なしでもリーチできるように変更できること
            game = init_game(new GameParam { rule = new Majiang.Rule { ツモ番なしリーチあり = true }, shoupai = new List<string> { "m123p456s789z1123", "", "", "" }, zimo = new List<string> { "z2" } });
            game.zimo();
            while (game.model.shan.paishu >= 4) game.model.shan.zimo();
            Assert.IsNotNull(game.allow_lizhi("z3*"));

            // 持ち点が1000点に満たなくてもリーチできるように変更できること
            game = init_game(new GameParam { rule = new Majiang.Rule { トビ終了あり = false }, shoupai = new List<string> { "m123p456s789z1123", "", "", "" }, zimo = new List<string> { "z2" }, defen = new List<int> { 900, 19100, 45000, 35000 } });
            game.zimo();
            Assert.IsNotNull(game.allow_lizhi("z3*"));
        }

        [Test, Description("allow_hule(p)")]
        public void TestAllowHule()
        {
            // ツモ和了
            var game = init_game(new GameParam { shoupai = new List<string> { "m123p456s789z3344", "", "", "" }, zimo = new List<string> { "z4" } });
            game.zimo();
            Assert.IsTrue(game.allow_hule());

            // リーチ・ツモ
            game = init_game(new GameParam { shoupai = new List<string> { "m123p456s789z4*,z333=", "", "", "" }, zimo = new List<string> { "z4" } });
            game.zimo();
            Assert.IsTrue(game.allow_hule());

            // 嶺上開花
            game = init_game(new GameParam { shoupai = new List<string> { "_", "", "m123p456s789z3334", "" }, gangzimo = new List<string> { "z4" } });
            game.zimo();
            game.dapai("z3");
            game.fulou("z3333=");
            game.gangzimo();
            Assert.IsTrue(game.allow_hule());

            // ハイテイ・ツモ
            game = init_game(new GameParam { shoupai = new List<string> { "m123p456s789z4,z333=", "", "", "" }, zimo = new List<string> { "z4" } });
            game.zimo();
            while (game.model.shan.paishu > 0) game.model.shan.zimo();
            Assert.IsTrue(game.allow_hule());

            // 場風のみ・ツモ
            game = init_game(new GameParam { shoupai = new List<string> { "_", "m123p456s789z4,z111=", "", "" }, zimo = new List<string> { "m1", "z4" } });
            game.zimo();
            game.dapai("m1");
            game.zimo();
            Assert.IsTrue(game.allow_hule());

            // 自風のみ・ツモ
            game = init_game(new GameParam { shoupai = new List<string> { "_", "m123p456s789z4,z222=", "", "" }, zimo = new List<string> { "m1", "z4" } });
            game.zimo();
            game.dapai("m1");
            game.zimo();
            Assert.IsTrue(game.allow_hule());

            // リーチ・ロン
            game = init_game(new GameParam { shoupai = new List<string> { "_", "m123p456s789z3334*", "", "" } });
            game.zimo();
            game.dapai("z4");
            Assert.IsTrue(game.allow_hule(1));

            // 槍槓
            game = init_game(new GameParam { shoupai = new List<string> { "m1234p567s789,m111=", "", "m23p123567s12377", "" } });
            game.zimo();
            game.gang("m111=1");
            Assert.IsTrue(game.allow_hule(2));

            // ハイテイ・ロン
            game = init_game(new GameParam { shoupai = new List<string> { "_", "", "", "m123p456s789z4,z333=" } });
            game.zimo();
            while (game.model.shan.paishu > 0) game.model.shan.zimo();
            game.dapai("z4");
            Assert.IsTrue(game.allow_hule(3));

            // 場風のみ・ロン
            game = init_game(new GameParam { shoupai = new List<string> { "_", "m123p456s789z4,z111=", "", "" } });
            game.zimo();
            game.dapai("z4");
            Assert.IsTrue(game.allow_hule(1));

            // 自風のみ・ロン
            game = init_game(new GameParam { shoupai = new List<string> { "_", "m123p456s789z4,z222=", "", "" } });
            game.zimo();
            game.dapai("z4");
            Assert.IsTrue(game.allow_hule(1));

            // フリテンはロン和了できないこと
            game = init_game(new GameParam { shoupai = new List<string> { "m123p456s789z3344", "", "", "" }, zimo = new List<string> { "z4", "z3" } });
            game.zimo();
            game.dapai("z4");
            game.zimo();
            game.dapai("z3");
            Assert.IsFalse(game.allow_hule(0));

            // クイタンなしにできること
            game = init_game(new GameParam { rule = new Majiang.Rule { クイタンあり = false }, shoupai = new List<string> { "_", "m234p567s2244,m888-", "", "" } });
            game.zimo();
            game.dapai("s4");
            Assert.IsFalse(game.allow_hule(1));
        }

        [Test, Description("allow_pingju()")]
        public void TestAllowPingju()
        {
            // 九種九牌
            var game = init_game(new GameParam { shoupai = new List<string> { "m123459z1234567", "", "", "" } });
            game.zimo();
            Assert.IsTrue(game.allow_pingju());

            // 第一ツモ以降は九種九牌にならない
            game = init_game(new GameParam { shoupai = new List<string> { "_", "_", "m123459z1234567", "" } });
            game.zimo();
            game.dapai("s2");
            game.fulou("s2-34");
            game.dapai("z3");
            game.zimo();
            Assert.IsFalse(game.allow_pingju());

            // 途中流局なしの場合は九種九牌にできない
            game = init_game(new GameParam { rule = new Majiang.Rule { 途中流局あり = false }, shoupai = new List<string> { "m123459z1234567", "", "", "" } });
            game.zimo();
            Assert.IsFalse(game.allow_pingju());
        }

        [Test, Description("static get_dapai(rule, shoupai)")]
        public void TestStaticGetDapai()
        {
            // 喰い替えなし
            var shoupai = Majiang.Shoupai.fromString("m5678p567,z111=,s789-").fulou("m0-67");
            Assert.AreEqual(new List<string> { "p5", "p6", "p7" }, Majiang.Game.get_dapai(new Majiang.Rule { 喰い替え許可レベル = 0 }, shoupai));

            // 現物以外の喰い替えあり
            Assert.AreEqual(new List<string> { "m8", "p5", "p6", "p7" }, Majiang.Game.get_dapai(new Majiang.Rule { 喰い替え許可レベル = 1 }, shoupai));

            // 現物喰い替えもあり
            Assert.AreEqual(new List<string> { "m5", "m8", "p5", "p6", "p7" }, Majiang.Game.get_dapai(new Majiang.Rule { 喰い替え許可レベル = 2 }, shoupai));
        }

        [Test, Description("static get_chi_mianzi(rule, shoupai, p, paishu)")]
        public void TestStaticGetChiMianzi()
        {
            // 喰い替えなし
            var shoupai1 = Majiang.Shoupai.fromString("m1234,p456-,z111=,s789-");
            var shoupai2 = Majiang.Shoupai.fromString("m1123,p456-,z111=,s789-");
            var rule = new Majiang.Rule { 喰い替え許可レベル = 0 };
            Assert.AreEqual(new List<string>(), Majiang.Game.get_chi_mianzi(rule, shoupai1, "m1-", 1));
            Assert.AreEqual(new List<string>(), Majiang.Game.get_chi_mianzi(rule, shoupai2, "m1-", 1));

            // 現物以外の喰い替えあり
            rule = new Majiang.Rule { 喰い替え許可レベル = 1 };
            Assert.AreEqual(new List<string> { "m1-23" }, Majiang.Game.get_chi_mianzi(rule, shoupai1, "m1-", 1));
            Assert.AreEqual(new List<string>(), Majiang.Game.get_chi_mianzi(rule, shoupai2, "m1-", 1));

            // 現物喰い替えもあり
            rule = new Majiang.Rule { 喰い替え許可レベル = 2 };
            Assert.AreEqual(new List<string> { "m1-23" }, Majiang.Game.get_chi_mianzi(rule, shoupai1, "m1-", 1));
            Assert.AreEqual(new List<string> { "m1-23" }, Majiang.Game.get_chi_mianzi(rule, shoupai2, "m1-", 1));

            // ハイテイは鳴けない
            Assert.AreEqual(new List<string>(), Majiang.Game.get_chi_mianzi(new Majiang.Rule { 喰い替え許可レベル = 2 }, shoupai1, "m1-", 0));

            // ツモした状態でチーできない
            Assert.IsNull(Majiang.Game.get_chi_mianzi(new Majiang.Rule(), Majiang.Shoupai.fromString("m123p456s12789z123"), "s3-", 1));
        }

        [Test, Description("static get_peng_mianzi(rule, shoupai, p, paishu)")]
        public void TestStaticGetPengMianzi()
        {
            // 喰い替えのためにポンできないケースはない
            var shoupai = Majiang.Shoupai.fromString("m1112,p456-,z111=,s789-");
            Assert.AreEqual(new List<string> { "m111+" }, Majiang.Game.get_peng_mianzi(new Majiang.Rule { 喰い替え許可レベル = 0 }, shoupai, "m1+", 1));

            // ハイテイは鳴けない
            Assert.AreEqual(new List<string>(), Majiang.Game.get_peng_mianzi(new Majiang.Rule { 喰い替え許可レベル = 0 }, shoupai, "m1+", 0));

            // ツモした状態でポンできない
            Assert.IsNull(Majiang.Game.get_peng_mianzi(new Majiang.Rule(), Majiang.Shoupai.fromString("m123p456s11789z123"), "s1-", 1));
        }

        [Test, Description("static get_gang_mianzi(rule, shoupai, p, paishu)")]
        public void TestStaticGetGangMianzi()
        {
            // リーチ後の暗槓なし
            var shoupai1 = Majiang.Shoupai.fromString("m1112p456s789z111z1*");
            var shoupai2 = Majiang.Shoupai.fromString("m1112p456s789z111m1*");
            var shoupai3 = Majiang.Shoupai.fromString("m23p567s33345666s3*");
            var shoupai4 = Majiang.Shoupai.fromString("s1113445678999s1*");
            var shoupai5 = Majiang.Shoupai.fromString("m23s77789s7*,s5550,z6666");
            var rule = new Majiang.Rule { リーチ後暗槓許可レベル = 0 };
            Assert.AreEqual(new List<string>(), Majiang.Game.get_gang_mianzi(rule, shoupai1, null, 1));
            Assert.AreEqual(new List<string>(), Majiang.Game.get_gang_mianzi(rule, shoupai2, null, 1));
            Assert.AreEqual(new List<string>(), Majiang.Game.get_gang_mianzi(rule, shoupai3, null, 1));
            Assert.AreEqual(new List<string>(), Majiang.Game.get_gang_mianzi(rule, shoupai4, null, 1));
            Assert.AreEqual(new List<string>(), Majiang.Game.get_gang_mianzi(rule, shoupai5, null, 1));

            // リーチ後の牌姿の変わる暗槓なし
            rule = new Majiang.Rule { リーチ後暗槓許可レベル = 1 };
            Assert.AreEqual(new List<string> { "z1111" }, Majiang.Game.get_gang_mianzi(rule, shoupai1, null, 1));
            Assert.AreEqual(new List<string>(), Majiang.Game.get_gang_mianzi(rule, shoupai2, null, 1));
            Assert.AreEqual(new List<string>(), Majiang.Game.get_gang_mianzi(rule, shoupai3, null, 1));
            Assert.AreEqual(new List<string>(), Majiang.Game.get_gang_mianzi(rule, shoupai4, null, 1));
            Assert.AreEqual(new List<string>(), Majiang.Game.get_gang_mianzi(rule, shoupai5, null, 1));

            // リーチ後の待ちの変わる暗槓なし
            rule = new Majiang.Rule { リーチ後暗槓許可レベル = 2 };
            Assert.AreEqual(new List<string> { "z1111" }, Majiang.Game.get_gang_mianzi(rule, shoupai1, null, 1));
            Assert.AreEqual(new List<string>(), Majiang.Game.get_gang_mianzi(rule, shoupai2, null, 1));
            Assert.AreEqual(new List<string> { "s3333" }, Majiang.Game.get_gang_mianzi(rule, shoupai3, null, 1));
            Assert.AreEqual(new List<string> { "s1111" }, Majiang.Game.get_gang_mianzi(rule, shoupai4, null, 1));
            Assert.AreEqual(new List<string>(), Majiang.Game.get_gang_mianzi(rule, shoupai5, null, 1));

            // ハイテイはカンできない
            Assert.AreEqual(new List<string>(), Majiang.Game.get_gang_mianzi(new Majiang.Rule(), Majiang.Shoupai.fromString("m1112p456s789z111z1"), null, 0));
            Assert.AreEqual(new List<string>(), Majiang.Game.get_gang_mianzi(new Majiang.Rule(), Majiang.Shoupai.fromString("m1112p456s789z111"), "z1=", 0));
        }

        [Test, Description("static allow_lizhi(rule, shoupai, p, paishu, defen)")]
        public void TestStaticAllowLizhi()
        {
            // 打牌できない場合、リーチはできない
            var rule = new Majiang.Rule();
            var shoupai = Majiang.Shoupai.fromString("m123p456s789z1122");
            Assert.IsNull(Majiang.Game.allow_lizhi(rule, shoupai));

            // すでにリーチしている場合、リーチはできない
            shoupai = Majiang.Shoupai.fromString("m123p456s789z11223*");
            Assert.IsNull(Majiang.Game.allow_lizhi(rule, shoupai));

            // メンゼンでない場合、リーチはできない
            shoupai = Majiang.Shoupai.fromString("m123p456s789z23,z111=");
            Assert.IsNull(Majiang.Game.allow_lizhi(rule, shoupai));

            // ツモ番がない場合、リーチはできない
            shoupai = Majiang.Shoupai.fromString("m123p456s789z11223");
            Assert.IsNull(Majiang.Game.allow_lizhi(rule, shoupai, "z3", 3));

            // ルールが許せばツモ番がなくてもリーチは可能
            shoupai = Majiang.Shoupai.fromString("m123p456s789z11223");
            Assert.IsNotNull(Majiang.Game.allow_lizhi(new Majiang.Rule { ツモ番なしリーチあり = true }, shoupai, "z3", 3));

            // 持ち点が1000点に満たない場合、リーチはできない
            shoupai = Majiang.Shoupai.fromString("m123p456s789z11223");
            Assert.IsNull(Majiang.Game.allow_lizhi(rule, shoupai, "z3", 4, 900));

            // トビなしなら持ち点が1000点に満たなくてもリーチは可能
            shoupai = Majiang.Shoupai.fromString("m123p456s789z11223");
            Assert.IsNotNull(Majiang.Game.allow_lizhi(new Majiang.Rule { トビ終了あり = false }, shoupai, "z3", 4, 900));

            // テンパイしていない場合、リーチはできない
            shoupai = Majiang.Shoupai.fromString("m123p456s789z11234");
            Assert.IsNull(Majiang.Game.allow_lizhi(rule, shoupai));

            // 形式テンパイと認められない牌姿でリーチはできない
            shoupai = Majiang.Shoupai.fromString("m123p456s789z11112");
            Assert.IsNull(Majiang.Game.allow_lizhi(rule, shoupai, "z2"));

            // 指定された打牌でリーチ可能な場合、真を返すこと
            shoupai = Majiang.Shoupai.fromString("m123p456s789z11112");
            Assert.IsNotNull(Majiang.Game.allow_lizhi(rule, shoupai, "z1"));

            // 指定された打牌でリーチできない場合、偽を返すこと
            shoupai = Majiang.Shoupai.fromString("m123p456s789z11112");
            Assert.IsNull(Majiang.Game.allow_lizhi(rule, shoupai, "z2"));

            // 打牌が指定されていない場合、リーチ可能な打牌一覧を返す
            shoupai = Majiang.Shoupai.fromString("m123p456s788z11122");
            Assert.AreEqual(new List<string> { "s7", "s8" }, Majiang.Game.allow_lizhi(rule, shoupai));
            shoupai = Majiang.Shoupai.fromString("m123p456s789z11223");
            Assert.AreEqual(new List<string> { "z3_" }, Majiang.Game.allow_lizhi(rule, shoupai));

            // リーチ可能な打牌がない場合、false を返す
            shoupai = Majiang.Shoupai.fromString("m11112344449999");
            Assert.IsNull(Majiang.Game.allow_lizhi(rule, shoupai));
        }

        [Test, Description("static allow_hule(shoupai, p, zhuangfeng, menfeng, hupai, neng_rong)")]
        public void TestStaticAllowHule()
        {
            var rule = new Majiang.Rule();

            // フリテンの場合、ロン和了できない
            var shoupai = Majiang.Shoupai.fromString("m123p456z1122,s789-");
            Assert.IsFalse(Majiang.Game.allow_hule(rule, shoupai, "z1=", 0, 1, false, false));

            // 和了形になっていない場合、和了できない
            shoupai = Majiang.Shoupai.fromString("m123p456z11223,s789-");
            Assert.IsFalse(Majiang.Game.allow_hule(rule, shoupai, null, 0, 1, false, true));

            // 役あり和了形の場合、和了できる
            shoupai = Majiang.Shoupai.fromString("m123p456s789z3377");
            Assert.IsTrue(Majiang.Game.allow_hule(rule, shoupai, "z3+", 0, 1, true, true));

            // 役なし和了形の場合、和了できない
            shoupai = Majiang.Shoupai.fromString("m123p456s789z3377");
            Assert.IsFalse(Majiang.Game.allow_hule(rule, shoupai, "z3+", 0, 1, false, true));

            // クイタンなしの場合、クイタンでは和了できない
            shoupai = Majiang.Shoupai.fromString("m22555p234s78,p777-");
            Assert.IsFalse(Majiang.Game.allow_hule(new Majiang.Rule { クイタンあり = false }, shoupai, "s6=", 0, 1, false, true));

            // ツモ和了
            shoupai = Majiang.Shoupai.fromString("m123p456s789z33377");
            Assert.IsTrue(Majiang.Game.allow_hule(rule, shoupai, null, 0, 1, false, false));

            // ロン和了
            shoupai = Majiang.Shoupai.fromString("m123p456z1122,s789-");
            Assert.IsTrue(Majiang.Game.allow_hule(rule, shoupai, "z1=", 0, 1, false, true));
        }

        [Test, Description("static allow_pingju(rule, shoupai, diyizimo)")]
        public void TestStaticAllowPingju()
        {
            var rule = new Majiang.Rule();

            // 第一巡でない場合、九種九牌とならない
            var shoupai = Majiang.Shoupai.fromString("m1234569z1234567");
            Assert.IsFalse(Majiang.Game.allow_pingju(rule, shoupai, false));

            // ツモ後でない場合、九種九牌とならない
            shoupai = Majiang.Shoupai.fromString("m123459z1234567");
            Assert.IsFalse(Majiang.Game.allow_pingju(rule, shoupai, true));

            // 途中流局なし場合、九種九牌とならない
            shoupai = Majiang.Shoupai.fromString("m1234569z1234567");
            Assert.IsFalse(Majiang.Game.allow_pingju(new Majiang.Rule { 途中流局あり = false }, shoupai, true));

            // 八種九牌は流局にできない
            shoupai = Majiang.Shoupai.fromString("m1234567z1234567");
            Assert.IsFalse(Majiang.Game.allow_pingju(rule, shoupai, true));

            // 九種九牌
            shoupai = Majiang.Shoupai.fromString("m1234569z1234567");
            Assert.IsTrue(Majiang.Game.allow_pingju(rule, shoupai, true));
        }

        [Test, Description("static allow_no_daopai(rule, shoupai, paishu)")]
        public void TestStaticAllowNoDaopai()
        {
            var rule = new Majiang.Rule { ノーテン宣言あり = true };

            // 最終打牌以外はノーテン宣言できない
            var shoupai = Majiang.Shoupai.fromString("m123p456z1122,s789-");
            Assert.IsFalse(Majiang.Game.allow_no_daopai(rule, shoupai, 1));
            Assert.IsFalse(Majiang.Game.allow_no_daopai(rule, shoupai.zimo("z3"), 0));

            // ノーテン宣言ありのルールでない場合、ノーテン宣言できない
            shoupai = Majiang.Shoupai.fromString("m123p456z1122,s789-");
            Assert.IsFalse(Majiang.Game.allow_no_daopai(new Majiang.Rule(), shoupai, 0));

            // リーチしている場合、ノーテン宣言できない
            shoupai = Majiang.Shoupai.fromString("m123p456p789z1122*");
            Assert.IsFalse(Majiang.Game.allow_no_daopai(rule, shoupai, 0));

            // テンパイしていない場合、ノーテン宣言できない
            shoupai = Majiang.Shoupai.fromString("m123p456p789z1123");
            Assert.IsFalse(Majiang.Game.allow_no_daopai(rule, shoupai, 0));

            // 形式テンパイと認められない牌姿の場合、ノーテン宣言できない
            shoupai = Majiang.Shoupai.fromString("m123p456p789z1111");
            Assert.IsFalse(Majiang.Game.allow_no_daopai(rule, shoupai, 0));

            // ノーテン宣言
            shoupai = Majiang.Shoupai.fromString("m123p456z1122,s789-");
            Assert.IsTrue(Majiang.Game.allow_no_daopai(rule, shoupai, 0));
        }

        private List<Paipu> data;

        [SetUp]
        public void SetUp()
        {
            data = TestUtil.LoadDataFromJsonl<Paipu>("Packages/com.kidonaru.majiang-core-unity/Tests/Editor/data/script.jsonl");
        }

        [Test, Description("シナリオ通りに局が進むこと")]
        public void TestScript()
        {
            foreach (var script in data) {
                var game = new Majiang.Editor.Game(
                    (Paipu) script.Clone(),
                    new Majiang.Rule { 順位点 = new List<string> { "20", "10", "-10", "-20" } }
                ).do_sync();

                CollectionAssert.AreEqual(script.log, game._paipu.log, script.title);
                script.log.Clear();
                game._paipu.log.Clear();

                Assert.AreEqual(script, game._paipu, script.title);
            }
        }
    }
}