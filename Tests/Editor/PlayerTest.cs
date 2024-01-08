using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Majiang.Test
{
    public class PlayerTest
    {
        public class InitPlayerParams
        {
            public Rule rule = null;
            public int? jushu = null;
            public string shoupai = null;
        }

        public Majiang.Player init_player(InitPlayerParams param = null)
        {
            if (param == null)
            {
                param = new InitPlayerParams();
            }

            var player = new Majiang.Player();

            var kaiju = new Kaiju
            {
                id = 1,
                rule = new Majiang.Rule(),
                title = "タイトル",
                player = new List<string> { "私", "下家", "対面", "上家" },
                qijia = 1
            };

            var qipai = new Qipai
            {
                zhuangfeng = 0,
                jushu = 0,
                changbang = 0,
                lizhibang = 0,
                defen = new List<int> { 25000, 25000, 25000, 25000 },
                baopai = "m1",
                shoupai = new List<string> { "", "", "", "" }
            };

            if (param.rule != null) kaiju.rule = param.rule;
            if (param.jushu != null) qipai.jushu = param.jushu.Value;

            int menfeng = (kaiju.id + 4 - kaiju.qijia + 4 - qipai.jushu) % 4;
            qipai.shoupai[menfeng] = string.IsNullOrEmpty(param.shoupai) ? "m123p456s789z1234" : param.shoupai;

            player.kaiju(kaiju);
            player.qipai(qipai);

            return player;
        }

        [Test, Description("クラスが存在すること")]
        public void ClassExists()
        {
            Assert.IsNotNull(typeof(Majiang.Player));
        }

        [Test, Description("constructor()")]
        public void TestConstructor()
        {
            var player = new Majiang.Player();
            Assert.IsNotNull(player, "インスタンスが生成できること");
            Assert.IsNotNull(player._model, "初期値が設定されること");
        }

        [Test, Description("kaiju(kaiju)")]
        public void TestKaiju()
        {
            var player = new Majiang.Player();
            var kaiju = new Kaiju { id = 1, rule = new Majiang.Rule(), title = "タイトル", player = new List<string> { "私", "下家", "対面", "上家" }, qijia = 2 };
            player.kaiju(kaiju);

            Assert.AreEqual(player._id, 1, "初期値が設定されること");
            Assert.AreEqual(player._rule, new Majiang.Rule(), "初期値が設定されること");
            Assert.AreEqual(player._model.title, "タイトル", "初期値が設定されること");
        }

        [Test, Description("qipai(qipai)")]
        public void TestQipai()
        {
            var player = new Majiang.Player();
            var kaiju = new Kaiju { id = 1, rule = new Majiang.Rule(), title = "タイトル", player = new List<string> { "私", "下家", "対面", "上家" }, qijia = 2 };
            player.kaiju(kaiju);

            var qipai = new Qipai { zhuangfeng = 1, jushu = 2, changbang = 3, lizhibang = 4, defen = new List<int> { 25000, 25000, 25000, 25000 }, baopai = "s5", shoupai = new List<string> { "", "m123p456s789z1234", "", "" } };
            player.qipai(qipai);

            Assert.AreEqual(player._menfeng, 1, "初期値が設定されること");
            Assert.IsTrue(player._diyizimo, "初期値が設定されること");
            Assert.AreEqual(player._n_gang, 0, "初期値が設定されること");
            Assert.IsTrue(player._neng_rong, "初期値が設定されること");
            Assert.AreEqual(player.shoupai.ToString(), "m123p456s789z1234", "初期値が設定されること");
            Assert.AreEqual(player.he._pai.Count, 0, "初期値が設定されること");
        }

        [Test, Description("zimo(zimo)")]
        public void TestZimo()
        {
            // 卓情報が更新されること
            var player = init_player();
            player.zimo(new Zimo { l = 0, p = "z5" });
            Assert.AreEqual(player.shoupai.ToString(), "m123p456s789z1234z5", "卓情報が更新されること");

            // 槓ヅモで槓数が増えること
            player = init_player();
            player.zimo(new Zimo { l = 0, p = "z5" }, true);
            Assert.AreEqual(player._n_gang, 1, "槓ヅモで槓数が増えること");
        }

        [Test, Description("dapai(dapai)")]
        public void TestDapai()
        {
            // 卓情報が更新されること
            var player = init_player(new InitPlayerParams { shoupai = "m123p456s789z1234z5" });
            player.dapai(new Dapai { l = 0, p = "z5_" });
            Assert.AreEqual(player.shoupai.ToString(), "m123p456s789z1234", "卓情報が更新されること");

            // 自身の打牌の後、第一ツモ巡でなくなること
            player = init_player(new InitPlayerParams { jushu = 3 });
            player.dapai(new Dapai { l = 0, p = "z5" });
            Assert.IsTrue(player._diyizimo, "自身の打牌の後、第一ツモ巡でなくなること");
            player.dapai(new Dapai { l = 1, p = "z1" });
            Assert.IsFalse(player._diyizimo, "自身の打牌の後、第一ツモ巡でなくなること");

            // 自身の打牌に和了牌がある場合、フリテンとなること
            player = init_player(new InitPlayerParams { shoupai = "m123p406s789z11222" });
            player.dapai(new Dapai { l = 0, p = "p0" });
            Assert.IsFalse(player._neng_rong, "自身の打牌に和了牌がある場合、フリテンとなること");

            // 自身の打牌でフリテンが解除されること
            player = init_player(new InitPlayerParams { shoupai = "m123p456s789z11223" });
            player._neng_rong = false;
            player.dapai(new Dapai { l = 0, p = "z3_" });
            Assert.IsTrue(player._neng_rong, "自身の打牌でフリテンが解除されること");

            // リーチ宣言まではフリテンが解除されること
            player = init_player(new InitPlayerParams { shoupai = "m123p456s789z11232" });
            player._neng_rong = false;
            player.dapai(new Dapai { l = 0, p = "z3*" });
            Assert.IsTrue(player._neng_rong, "リーチ宣言まではフリテンが解除されること");

            // リーチ後はフリテンが解除されないこと
            player = init_player(new InitPlayerParams { shoupai = "m123p456s789z11223*" });
            player._neng_rong = false;
            player.dapai(new Dapai { l = 0, p = "z3_" });
            Assert.IsFalse(player._neng_rong, "リーチ後はフリテンが解除されないこと");

            // 和了牌を見逃した場合、フリテンとなること
            player = init_player(new InitPlayerParams { shoupai = "m123p46s789z11122" });
            player.dapai(new Dapai { l = 1, p = "p0" });
            Assert.IsFalse(player._neng_rong, "和了牌を見逃した場合、フリテンとなること");
        }

        [Test, Description("fulou(fulou)")]
        public void TestFulou()
        {
            // 卓情報が更新されること
            var player = init_player(new InitPlayerParams { shoupai = "m123p456s789z1134" });
            player.dapai(new Dapai { l = 2, p = "z1" });
            player.fulou(new Fulou { l = 0, m = "z111=" });
            Assert.AreEqual(player.shoupai.ToString(), "m123p456s789z34,z111=,", "卓情報が更新されること");

            // 第一ツモ巡でなくなること
            player = init_player(new InitPlayerParams { jushu = 1 });
            player.dapai(new Dapai { l = 0, p = "z3" });
            Assert.IsTrue(player._diyizimo, "第一ツモ巡でなくなること");
            player.fulou(new Fulou { l = 1, m = "z333=" });
            Assert.IsFalse(player._diyizimo, "第一ツモ巡でなくなること");
        }

        [Test, Description("gang(gang)")]
        public void TestGang()
        {
            // 卓情報が更新されること
            var player = init_player(new InitPlayerParams { shoupai = "m123p456s788z12,z111=" });
            player.gang(new Gang { l = 0, m = "z111=1" });
            Assert.AreEqual(player.shoupai.ToString(), "m123p456s788z2,z111=1", "卓情報が更新されること");

            // 第一ツモ巡でなくなること
            player = init_player(new InitPlayerParams { jushu = 1 });
            player.gang(new Gang { l = 0, m = "m9999" });
            Assert.IsFalse(player._diyizimo, "第一ツモ巡でなくなること");

            // 和了牌を見逃した場合、フリテンとなること
            player = init_player(new InitPlayerParams { shoupai = "m34p456s788z11222" });
            player.dapai(new Dapai { l = 2, p = "m5" });
            player.fulou(new Fulou { l = 3, m = "m555-" });
            player.dapai(new Dapai { l = 2, p = "s4" });
            player.fulou(new Fulou { l = 3, m = "s444-" });
            player.zimo(new Zimo { l = 0, p = "s9" });
            player.dapai(new Dapai { l = 0, p = "s8" });
            player.gang(new Gang { l = 3, m = "s444-4" });
            Assert.IsTrue(player._neng_rong, "和了牌を見逃した場合、フリテンとなること");
            player.gang(new Gang { l = 3, m = "m555-0" });
            Assert.IsFalse(player._neng_rong, "和了牌を見逃した場合、フリテンとなること");
        }

        [Test, Description("kaigang(kaigang)")]
        public void TestKaigang()
        {
            // 卓情報が更新されること
            var player = init_player();
            player.kaigang(new Kaigang { baopai = "p1" });
            Assert.AreEqual(player.shan.baopai.Pop(), "p1", "卓情報が更新されること");
        }

        [Test, Description("hule(hule)")]
        public void TestHule()
        {
            // 卓情報が更新されること
            var player = init_player();
            player.hule(new Hule { l = 1, shoupai = "m123p456s789z1122z1*", fubaopai = new List<string> { "s1" } });
            Assert.AreEqual(player._model.shoupai[1].ToString(), "m123p456s789z1122z1*", "卓情報が更新されること");
            Assert.AreEqual(player.shan.fubaopai[0], "s1", "卓情報が更新されること");
        }

        [Test, Description("pingju(pingju)")]
        public void TestPingju()
        {
            // 卓情報が更新されること
            var player = init_player();
            player.dapai(new Dapai { l = 1, p = "m3*" });
            player.pingju(new Pingju { name = "", shoupai = new List<string> { "", "", "", "m123p456s789z1122*" } });
            Assert.AreEqual(player._model.shoupai[3].ToString(), "m123p456s789z1122*", "卓情報が更新されること");
            Assert.AreEqual(player._model.lizhibang, 1, "卓情報が更新されること");
        }

        [Test, Description("jieju(paipu)")]
        public void TestJieju()
        {
            // 卓情報が更新されること
            var player = init_player();
            var paipu = new Paipu { defen = new List<int> { 10000, 20000, 30000, 40000 } };
            player.jieju(paipu);
            CollectionAssert.AreEqual(player._model.defen, paipu.defen, "卓情報が更新されること");

            // 牌譜を取得していること
            player = init_player();
            player.jieju(new Paipu { defen = new List<int>() });
            Assert.IsNotNull(player._paipu, "牌譜を取得していること");
        }

        [Test, Description("get_dapai(shoupai)")]
        public void TestGetDapai()
        {
            // 喰い替えなし
            var player = init_player();
            var shoupai = Majiang.Shoupai.fromString("m14p45677s6788,m234-,");
            CollectionAssert.AreEqual(player.get_dapai(shoupai), new List<string> { "p4", "p5", "p6", "p7", "s6", "s7", "s8" }, "喰い替えなし");

            // 喰い替えあり
            player = init_player(new InitPlayerParams { rule = new Majiang.Rule { 喰い替え許可レベル = 1 } });
            shoupai = Majiang.Shoupai.fromString("m14p45677s6788,m234-,");
            CollectionAssert.AreEqual(player.get_dapai(shoupai), new List<string> { "m1", "p4", "p5", "p6", "p7", "s6", "s7", "s8" }, "喰い替えあり");
        }

        [Test, Description("get_chi_mianzi(shoupai, p)")]
        public void TestGetChiMianzi()
        {
            // 喰い替えなし
            var player = init_player();
            var shoupai = Majiang.Shoupai.fromString("p1112344,z111=,z222+");
            CollectionAssert.AreEqual(player.get_chi_mianzi(shoupai, "p4-"), new List<string>(), "喰い替えなし");

            // 喰い替えあり
            player = init_player(new InitPlayerParams { rule = new Majiang.Rule { 喰い替え許可レベル = 1 } });
            shoupai = Majiang.Shoupai.fromString("p1112344,z111=,z222+");
            CollectionAssert.AreEqual(player.get_chi_mianzi(shoupai, "p4-"), new List<string> { "p234-" }, "喰い替えあり");

            // ハイテイ牌でチーできないこと
            player = init_player();
            while (player.shan.paishu > 0) player.shan.zimo();
            shoupai = Majiang.Shoupai.fromString("m23p456s789z11123");
            CollectionAssert.AreEqual(player.get_chi_mianzi(shoupai, "m1-"), new List<string>(), "ハイテイ牌でチーできないこと");
        }

        [Test, Description("get_peng_mianzi(shoupai, p)")]
        public void TestGetPengMianzi()
        {
            // ポンできるメンツを返すこと
            var player = init_player();
            var shoupai = Majiang.Shoupai.fromString("m123p456s789z1123");
            CollectionAssert.AreEqual(player.get_peng_mianzi(shoupai, "z1+"), new List<string> { "z111+" }, "ポンできるメンツを返すこと");

            // ハイテイ牌でポンできないこと
            player = init_player();
            while (player.shan.paishu > 0) player.shan.zimo();
            shoupai = Majiang.Shoupai.fromString("m123p456s789z1123");
            CollectionAssert.AreEqual(player.get_peng_mianzi(shoupai, "z1+"), new List<string>(), "ハイテイ牌でポンできないこと");
        }

        [Test, Description("get_gang_mianzi(shoupai, p)")]
        public void TestGetGangMianzi()
        {
            // 暗槓できるメンツを返すこと
            var player = init_player();
            var shoupai = Majiang.Shoupai.fromString("m123p456s789z11112");
            CollectionAssert.AreEqual(player.get_gang_mianzi(shoupai), new List<string> { "z1111" }, "暗槓できるメンツを返すこと");

            // 大明槓できるメンツを返すこと
            player = init_player();
            shoupai = Majiang.Shoupai.fromString("m123p456s789z1112");
            CollectionAssert.AreEqual(player.get_gang_mianzi(shoupai, "z1="), new List<string> { "z1111=" }, "大明槓できるメンツを返すこと");

            // ハイテイ牌でカンできないこと
            player = init_player();
            while (player.shan.paishu > 0) player.shan.zimo();
            shoupai = Majiang.Shoupai.fromString("m123p456s789z11112");
            CollectionAssert.AreEqual(player.get_gang_mianzi(shoupai), new List<string>(), "ハイテイ牌でカンできないこと");

            // 5つ目のカンはできないこと
            player = init_player();
            player._n_gang = 4;
            shoupai = Majiang.Shoupai.fromString("m123p456s789z11112");
            CollectionAssert.AreEqual(player.get_gang_mianzi(shoupai), new List<string>(), "5つ目のカンはできないこと");

            // リーチ後の暗槓あり
            player = init_player();
            shoupai = Majiang.Shoupai.fromString("m123p456s789z1112z1*");
            CollectionAssert.AreEqual(player.get_gang_mianzi(shoupai), new List<string> { "z1111" }, "リーチ後の暗槓あり");

            // リーチ後の暗槓なし
            player = init_player(new InitPlayerParams { rule = new Majiang.Rule { リーチ後暗槓許可レベル = 0 } });
            shoupai = Majiang.Shoupai.fromString("m123p456s789z1112z1*");
            CollectionAssert.AreEqual(player.get_gang_mianzi(shoupai), new List<string>(), "リーチ後の暗槓なし");
        }

        [Test, Description("allow_lizhi(shoupai, p)")]
        public void TestAllowLizhi()
        {
            // リーチ可能な牌の一覧を返すこと
            var player = init_player();
            var shoupai = Majiang.Shoupai.fromString("m223p456s789z11122");
            CollectionAssert.AreEqual(player.allow_lizhi(shoupai), new List<string> { "m2", "m3" }, "リーチ可能な牌の一覧を返すこと");

            // リーチ可能か判定すること
            player = init_player();
            shoupai = Majiang.Shoupai.fromString("m223p456s789z11122");
            Assert.IsTrue(player.allow_lizhi(shoupai, "m2") != null, "リーチ可能か判定すること");

            // ツモ番なしリーチなし
            player = init_player();
            while (player.shan.paishu >= 4) player.shan.zimo();
            shoupai = Majiang.Shoupai.fromString("m223p456s789z11122");
            Assert.IsFalse(player.allow_lizhi(shoupai) != null, "ツモ番なしリーチなし");

            // ツモ番なしリーチあり
            player = init_player(new InitPlayerParams { rule = new Majiang.Rule { ツモ番なしリーチあり = true } });
            while (player.shan.paishu >= 4) player.shan.zimo();
            shoupai = Majiang.Shoupai.fromString("m223p456s789z11122");
            Assert.IsTrue(player.allow_lizhi(shoupai) != null, "ツモ番なしリーチあり");

            // トビ終了あり
            player = init_player();
            player._model.defen[player._id] = 900;
            shoupai = Majiang.Shoupai.fromString("m223p456s789z11122");
            Assert.IsFalse(player.allow_lizhi(shoupai) != null, "トビ終了あり");

            // トビ終了なし
            player = init_player(new InitPlayerParams { rule = new Majiang.Rule { トビ終了あり = false } });
            player._model.defen[player._id] = 900;
            shoupai = Majiang.Shoupai.fromString("m223p456s789z11122");
            Assert.IsTrue(player.allow_lizhi(shoupai) != null, "トビ終了なし");
        }

        [Test, Description("allow_hule(shoupai, p, hupai)")]
        public void TestAllowHule()
        {
            // 役なしの場合、偽を返すこと
            var player = init_player();
            var shoupai = Majiang.Shoupai.fromString("m123p456s789z1122");
            Assert.IsFalse(player.allow_hule(shoupai, "z2="), "役なしの場合、偽を返すこと");

            // 状況役ありの場合(リーチ)、真を返すこと
            player = init_player();
            shoupai = Majiang.Shoupai.fromString("m123p456s789z1122*");
            Assert.IsTrue(player.allow_hule(shoupai, "z2="), "状況役ありの場合(リーチ)、真を返すこと");

            // 状況役ありの場合(ハイテイ)、真を返すこと
            player = init_player();
            while (player.shan.paishu > 0) player.shan.zimo();
            shoupai = Majiang.Shoupai.fromString("m123p456s789z1122");
            Assert.IsTrue(player.allow_hule(shoupai, "z2="), "状況役ありの場合(ハイテイ)、真を返すこと");

            // 状況役ありの場合(槍槓)、真を返すこと
            player = init_player();
            shoupai = Majiang.Shoupai.fromString("m123p456s789z1122");
            Assert.IsTrue(player.allow_hule(shoupai, "z2=", true), "状況役ありの場合(槍槓)、真を返すこと");

            // フリテンの場合、偽を返すこと
            player = init_player();
            player._neng_rong = false;
            shoupai = Majiang.Shoupai.fromString("m123p456s789z1122");
            Assert.IsFalse(player.allow_hule(shoupai, "z1="), "フリテンの場合、偽を返すこと");
        }

        [Test, Description("allow_pingju(shoupai)")]
        public void TestAllowPingju()
        {
            // 九種九牌で流せること
            var player = init_player();
            var shoupai = Majiang.Shoupai.fromString("m1234569z1234567");
            Assert.IsTrue(player.allow_pingju(shoupai), "九種九牌で流せること");

            // 最初のツモ巡をすぎた場合、流せないこと
            player = init_player();
            player._diyizimo = false;
            shoupai = Majiang.Shoupai.fromString("m123459z1234567");
            Assert.IsFalse(player.allow_pingju(shoupai), "最初のツモ巡をすぎた場合、流せないこと");

            // 途中流局なしの場合、流せないこと
            player = init_player(new InitPlayerParams { rule = new Majiang.Rule { 途中流局あり = false } });
            shoupai = Majiang.Shoupai.fromString("m123459z1234567");
            Assert.IsFalse(player.allow_pingju(shoupai), "途中流局なしの場合、流せないこと");
        }

        [Test, Description("allow_no_daopai(shoupai)")]
        public void TestAllowNoDaopai()
        {
            // ノーテン宣言できること
            var player = init_player(new InitPlayerParams { rule = new Majiang.Rule { ノーテン宣言あり = true } });
            var shoupai = Majiang.Shoupai.fromString("m123p456s789z1122");
            while (player.shan.paishu > 0) player.shan.zimo();
            Assert.IsTrue(player.allow_no_daopai(shoupai), "ノーテン宣言できること");

            // ノーテン宣言なしの場合、ノーテン宣言できないこと
            player = init_player();
            shoupai = Majiang.Shoupai.fromString("m123p456s789z1122");
            while (player.shan.paishu > 0) player.shan.zimo();
            Assert.IsFalse(player.allow_no_daopai(shoupai), "ノーテン宣言なしの場合、ノーテン宣言できないこと");

            // テンパイしていない場合、ノーテン宣言できないこと
            player = init_player(new InitPlayerParams { rule = new Majiang.Rule { ノーテン宣言あり = true } });
            shoupai = Majiang.Shoupai.fromString("m123p456s789z1123");
            while (player.shan.paishu > 0) player.shan.zimo();
            Assert.IsFalse(player.allow_no_daopai(shoupai), "テンパイしていない場合、ノーテン宣言できないこと");

            // 流局していない場合、ノーテン宣言できないこと
            player = init_player(new InitPlayerParams { rule = new Majiang.Rule { ノーテン宣言あり = true } });
            shoupai = Majiang.Shoupai.fromString("m123p456s789z1122");
            Assert.IsFalse(player.allow_no_daopai(shoupai), "流局していない場合、ノーテン宣言できないこと");
        }

        private class TestPlayer : Majiang.Player {
            public override void action_kaiju(Kaiju kaiju)
            {
                this._callback(null);
            }
            public override void action_qipai(Qipai qipai)
            {
                this._callback(null);
            }
            public override void action_zimo(Zimo zimo, bool gang)
            {
                this._callback(null);
            }
            public override void action_dapai(Dapai dapai)
            {
                this._callback(null);
            }
            public override void action_fulou(Fulou fulou)
            {
                this._callback(null);
            }
            public override void action_gang(Gang gang)
            {
                this._callback(null);
            }
            public override void action_hule(Hule hule)
            {
                this._callback(null);
            }
            public override void action_pingju(Pingju pingju)
            {
                this._callback(null);
            }
            public override void action_jieju(Paipu paipu)
            {
                this._callback(null);
            }
        }

        [Test, Description("action(msg, callback)")]
        public void TestAction()
        {
            var player = new TestPlayer();

            Action<Reply> error = reply => { throw new Exception(); };

            int doneCount = 0;
            Action<Reply> done = reply => { doneCount++; };

            // 開局 (kaiju)
            var kaiju = new Message { kaiju = new Kaiju { id = 1, rule = new Majiang.Rule(), title = "タイトル", player = new List<string> { "私", "下家", "対面", "上家" }, qijia = 2 } };
            player.action(kaiju, done);

            // 配牌 (qipai)
            var qipai = new Message { qipai = new Qipai { zhuangfeng = 1, jushu = 2, changbang = 3, lizhibang = 4, defen = new List<int> { 25000, 25000, 25000, 25000 }, baopai = "s5", shoupai = new List<string> { "", "m123p456s789z1234", "", "" } } };
            player.action(qipai, done);

            // 自摸 (zimo)
            player.action(new Message { zimo = new Zimo { l = 0, p = "m1" } }, done);

            // 打牌 (dapai)
            player.action(new Message { dapai = new Dapai { l = 0, p = "m1_" } }, done);

            // 副露 (fulou)
            player.action(new Message { fulou = new Fulou { l = 1, m = "m1-23" } }, done);

            // 槓 (gang)
            player.action(new Message { gang = new Gang { l = 2, m = "s1111" } }, done);

            // 槓自摸 (gangzimo)
            player.action(new Message { gangzimo = new Zimo { l = 2, p = "s2" } }, done);

            // 開槓 (kaigang)
            player.action(new Message { kaigang = new Kaigang { baopai = "m1" } }, error);

            // 和了 (hule)
            var hule = new Message { hule = new Hule {
                l = 2,
                shoupai = "p7p7,z111-,z222=,z333+,z444-",
                baojia = 3,
                fubaopai = null,
                damanguan = 2,
                defen = 64000,
                hupai = new List<Yaku> {
                    new Yaku { name = "大四喜", fanshu = "**", baojia = "0" }
                },
                fenpei = new List<int> { -32000, 0, 64000, -32000 }
            }};
            player.action(hule, done);

            // 流局 (pingju)
            var pingju = new Message { pingju = new Pingju {
                name = "荒牌平局",
                shoupai = new List<string> { "", "p2234406z333555", "", "p11223346777z77" },
                fenpei = new List<int> { -1500, 1500, -1500, 1500 }
            }};
            player.action(pingju, done);

            // 終局 (jieju)
            player.action(new Message { jieju = new Paipu{ defen = new List<int>() } }, done);

            // その他
            player.action(new Message { }, error);

            Assert.AreEqual(doneCount, 10, "すべてのコールバックが呼ばれること");
        }

    }
}