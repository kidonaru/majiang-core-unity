using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Majiang.Test
{
    public class BoardTest
    {
        public Majiang.Board init_board()
        {
            var board = new Majiang.Board(new Kaiju
            {
                title = "タイトル",
                player = new List<string> { "私", "下家", "対面", "上家" },
                qijia = 1
            });

            var qipai = new Qipai
            {
                zhuangfeng = 1,
                jushu = 2,
                changbang = 3,
                lizhibang = 4,
                defen = new List<int> { 10000, 20000, 30000, 36000 },
                baopai = "m1",
                shoupai = new List<string> { "", "", "", "" }
            };
            board.qipai(qipai);

            return board;
        }

        [Test, Description("クラスが存在すること")]
        public void ClassExists()
        {
            Assert.IsNotNull(typeof(Majiang.Board));
        }

        [Test, Description("constructor(kaiju)")]
        public void TestConstructor()
        {
            var board = new Majiang.Board(new Kaiju
            {
                title = "タイトル",
                player = new List<string> { "私", "下家", "対面", "上家" },
                qijia = 1
            });
            Assert.IsNotNull(board, "インスタンスが生成できること");
            Assert.AreEqual(board.title, "タイトル", "タイトルが設定されること");
            CollectionAssert.AreEqual(board.player, new List<string> { "私", "下家", "対面", "上家" }, "対局者情報が設定されること");
            Assert.AreEqual(board.qijia, 1, "起家が設定されること");

            Assert.IsNotNull(new Majiang.Board(), "パラメータなしでもインスタンスが生成できること");
        }

        [Test, Description("manfeng(id)")]
        public void TestManfeng()
        {
            var board = new Majiang.Board(new Kaiju { });

            board.qijia = 0; board.jushu = 0;
            Assert.AreEqual(board.menfeng(0), 0, "起家: 仮東、東一局");
            Assert.AreEqual(board.menfeng(1), 1, "起家: 仮東、東一局");
            Assert.AreEqual(board.menfeng(2), 2, "起家: 仮東、東一局");
            Assert.AreEqual(board.menfeng(3), 3, "起家: 仮東、東一局");

            board.qijia = 0; board.jushu = 1;
            Assert.AreEqual(board.menfeng(0), 3, "起家: 仮東、東二局");
            Assert.AreEqual(board.menfeng(1), 0, "起家: 仮東、東二局");
            Assert.AreEqual(board.menfeng(2), 1, "起家: 仮東、東二局");
            Assert.AreEqual(board.menfeng(3), 2, "起家: 仮東、東二局");

            board.qijia = 1; board.jushu = 0;
            Assert.AreEqual(board.menfeng(0), 3, "起家: 仮南、東一局");
            Assert.AreEqual(board.menfeng(1), 0, "起家: 仮南、東一局");
            Assert.AreEqual(board.menfeng(2), 1, "起家: 仮南、東一局");
            Assert.AreEqual(board.menfeng(3), 2, "起家: 仮南、東一局");
        }

        [Test, Description("zimo(zimo)")]
        public void TestZimo()
        {
            var board = new Majiang.Board(new Kaiju
            {
                title = "タイトル",
                player = new List<string> { "私", "下家", "対面", "上家" },
                qijia = 1
            });
            var qipai = new Qipai
            {
                zhuangfeng = 1,
                jushu = 2,
                changbang = 3,
                lizhibang = 4,
                defen = new List<int> { 10000, 20000, 30000, 36000 },
                baopai = "m1",
                shoupai = new List<string> { "", "m123p456s789z1234", "", "" }
            };
            board.qipai(qipai);
            Assert.AreEqual(board.zhuangfeng, 1, "場風が設定されること");
            Assert.AreEqual(board.jushu, 2, "局数が設定されること");
            Assert.AreEqual(board.changbang, 3, "本場が設定されること");
            Assert.AreEqual(board.lizhibang, 4, "供託が設定されること");
            Assert.AreEqual(board.shan.baopai[0], "m1", "ドラが設定されること");
            Assert.AreEqual(board.defen[0], 20000, "持ち点が設定されること");
            Assert.AreEqual(board.shoupai[1].ToString(), "m123p456s789z1234", "手牌が設定されること");
            Assert.AreEqual(board.he.Select(he => he._pai.Count).Sum(), 0, "捨て牌が初期化されること");
            Assert.AreEqual(board.lunban, -1, "手番が初期化されること");
        }

        [Test, Description("dapai(dapai)")]
        public void TestDapai()
        {
            var board = init_board();
            board.zimo(new Zimo { l = 0, p = "m1" });
            board.dapai(new Dapai { l = 0, p = "m1_" });
            Assert.IsNull(board.shoupai[0].get_dapai(), "手牌から打牌が切り出されること");
            Assert.AreEqual(board.he[0]._pai[0], "m1_", "捨て牌に加えられること");

            // 少牌となる打牌ができること
            board = init_board();
            board.dapai(new Dapai { l = 0, p = "m1" });
        }

        [Test, Description("fulou(fulou)")]
        public void TestFulou()
        {
            var board = init_board();
            board.zimo(new Zimo { l = 0, p = "m1" });
            board.dapai(new Dapai { l = 0, p = "m1_" });
            board.fulou(new Fulou { l = 2, m = "m111=" });
            Assert.AreEqual(board.he[0]._pai[0], "m1_=", "河から副露牌が拾われること");
            Assert.AreEqual(board.lunban, 2, "手番が移動すること");
            Assert.AreEqual(board.shoupai[2]._fulou[0], "m111=", "手牌が副露されること");

            // リーチ宣言後の副露でリーチが成立すること
            board = init_board();
            board.zimo(new Zimo { l = 0, p = "m1" });
            board.dapai(new Dapai { l = 0, p = "m1_*" });
            board.fulou(new Fulou { l = 2, m = "m111=" });
            Assert.AreEqual(board.defen[board.player_id[0]], 9000, "リーチ宣言後の副露でリーチが成立すること");
            Assert.AreEqual(board.lizhibang, 5, "リーチ宣言後の副露でリーチが成立すること");

            // 多牌となる副露ができること
            board = init_board();
            board.zimo(new Zimo { l = 0, p = "m1" });
            board.dapai(new Dapai { l = 2, p = "m1" });
            board.fulou(new Fulou { l = 0, m = "m111=" });
        }

        [Test, Description("gang(gang)")]
        public void TestGang()
        {
            var board = init_board();
            board.zimo(new Zimo { l = 0, p = "m1" });
            board.gang(new Gang { l = 0, m = "m1111" });
            Assert.AreEqual(board.shoupai[0]._fulou[0], "m1111", "手牌が副露されること");

            // 少牌となるカンができること
            board = init_board();
            board.gang(new Gang { l = 0, m = "m1111" });
        }

        [Test, Description("kaigang(kaigang)")]
        public void TestKaigang()
        {
            var board = init_board();
            board.zimo(new Zimo { l = 0, p = "m1" });
            board.gang(new Gang { l = 0, m = "m1111" });
            board.kaigang(new Kaigang { baopai = "s9" });
            Assert.AreEqual(board.shan.baopai[1], "s9", "ドラが増えること");
        }

        [Test, Description("hule(hule)")]
        public void TestHule()
        {
            var board = init_board();
            board.zimo(new Zimo { l = 0, p = "m1" });
            board.hule(new Hule { l = 0, shoupai = "m123p456s789z1122z2*", fubaopai = new List<string> { "s9" } });
            Assert.AreEqual(board.shoupai[0].ToString(), "m123p456s789z1122z2*", "和了者の手牌が設定されること");
            Assert.AreEqual(board.shan.fubaopai[0], "s9", "裏ドラを参照できること");

            // ダブロンの際に持ち点の移動が反映されていること
            board = init_board();
            board.zimo(new Zimo { l = 1, p = "" });
            board.dapai(new Dapai { l = 1, p = "p4_" });
            board.hule(new Hule
            {
                l = 2,
                shoupai = "m444678p44s33p4,s505=",
                baojia = 1,
                fubaopai = null,
                fu = 30,
                fanshu = 2,
                defen = 2000,
                hupai = new List<Yaku> {
                    new Yaku { name = "断幺九", fanshu = "1" },
                    new Yaku { name = "赤ドラ", fanshu = "1" }
                },
                fenpei = new List<int> { 0, -2900, 6900, 0 }
            });
            board.hule(new Hule
            {
                l = 0,
                shoupai = "p06s12344p4,z777-,p333+",
                baojia = 1,
                fubaopai = null,
                fu = 30,
                fanshu = 2,
                defen = 2900,
                hupai = new List<Yaku> {
                    new Yaku { name = "役牌 中", fanshu = "1" },
                    new Yaku { name = "赤ドラ", fanshu = "1" }
                },
                fenpei = new List<int> { 0, -2900, 2900, 0 }
            });
            Assert.AreEqual(board.changbang, 0, "ダブロンの際に持ち点の移動が反映されていること");
            Assert.AreEqual(board.lizhibang, 0, "ダブロンの際に持ち点の移動が反映されていること");
            CollectionAssert.AreEqual(board.defen, new List<int> { 17100, 36900, 36000, 10000 }, "ダブロンの際に持ち点の移動が反映されていること");
        }

        [Test, Description("pingju(pingju)")]
        public void TestPingju()
        {
            // 倒牌した手牌が設定されること
            var board = init_board();
            board.pingju(new Pingju { name = "", shoupai = new List<string> { "", "m123p456s789z1122", "", "" } });
            Assert.AreEqual(board.shoupai[1].ToString(), "m123p456s789z1122", "倒牌した手牌が設定されること");

            // リーチ宣言後の流局でリーチが成立すること
            board = init_board();
            board.zimo(new Zimo { l = 0, p = "m1" });
            board.dapai(new Dapai { l = 0, p = "m1_*" });
            board.pingju(new Pingju { name = "荒牌平局", shoupai = new List<string>() });
            Assert.AreEqual(board.defen[board.player_id[0]], 9000, "リーチ宣言後の流局でリーチが成立すること");
            Assert.AreEqual(board.lizhibang, 5, "リーチ宣言後の流局でリーチが成立すること");

            // 三家和の場合はリーチが成立しないこと
            board = init_board();
            board.zimo(new Zimo { l = 0, p = "m1" });
            board.dapai(new Dapai { l = 0, p = "m1_*" });
            board.pingju(new Pingju { name = "三家和", shoupai = new List<string>() });
            Assert.AreEqual(board.defen[board.player_id[0]], 10000, "三家和の場合はリーチが成立しないこと");
            Assert.AreEqual(board.lizhibang, 4, "三家和の場合はリーチが成立しないこと");
        }

        [Test, Description("jieju(paipu)")]
        public void TestJieju()
        {
            var board = init_board();
            board.lunban = 0;
            var paipu = new Paipu {
                defen = new List<int> { 17100, 36900, 36000, 10000 }
            };
            board.jieju(paipu);
            CollectionAssert.AreEqual(board.defen, new List<int> { 17100, 36900, 36000, 10000 }, "終局時の持ち点が反映されること");
            Assert.AreEqual(board.lunban, -1, "手番が初期化されること");
        }
    }
}