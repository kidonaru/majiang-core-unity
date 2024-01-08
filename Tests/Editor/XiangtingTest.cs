using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Majiang.Test
{
    public class XiangtingTest
    {
        [Serializable]
        public class XiangtingTestEntity
        {
            public List<string> q;
            public List<int> x;
        }

        private List<XiangtingTestEntity> data1;
        private List<XiangtingTestEntity> data2;
        private List<XiangtingTestEntity> data3;
        private List<XiangtingTestEntity> data4;

        [SetUp]
        public void SetUp()
        {
            data1 = TestUtil.LoadDataFromJsonl<XiangtingTestEntity>("Packages/com.kidonaru.majiang-core-unity/Tests/Editor/data/xiangting_1.jsonl");
            data2 = TestUtil.LoadDataFromJsonl<XiangtingTestEntity>("Packages/com.kidonaru.majiang-core-unity/Tests/Editor/data/xiangting_2.jsonl");
            data3 = TestUtil.LoadDataFromJsonl<XiangtingTestEntity>("Packages/com.kidonaru.majiang-core-unity/Tests/Editor/data/xiangting_3.jsonl");
            data4 = TestUtil.LoadDataFromJsonl<XiangtingTestEntity>("Packages/com.kidonaru.majiang-core-unity/Tests/Editor/data/xiangting_4.jsonl");
        }

        [Test, Description("xiangting_yiban(shoupai)")]
        public void TestXiangtingYiban()
        {
            Assert.AreEqual(13, Majiang.Util.xiangting_yiban(Majiang.Shoupai.fromString()), "空の手牌は13向聴");
            Assert.AreEqual(0, Majiang.Util.xiangting_yiban(Majiang.Shoupai.fromString("m123p406s789z1122")), "聴牌形");
            Assert.AreEqual(-1, Majiang.Util.xiangting_yiban(Majiang.Shoupai.fromString("m123p456s789z11222")), "和了形");
            Assert.AreEqual(0, Majiang.Util.xiangting_yiban(Majiang.Shoupai.fromString("m123p456s789z2,z111=")),
                "副露あり");
            Assert.AreEqual(1, Majiang.Util.xiangting_yiban(Majiang.Shoupai.fromString("m12389p456s12789z1")), "雀頭なし");
            Assert.AreEqual(1, Majiang.Util.xiangting_yiban(Majiang.Shoupai.fromString("m12389p456s1289z11")), "搭子過多");
            Assert.AreEqual(2, Majiang.Util.xiangting_yiban(Majiang.Shoupai.fromString("m133345568z23677")), "搭子不足");
            var shoupai = Majiang.Shoupai.fromString("m123,p123-,s456-,m789-");
            shoupai._fulou.Add("z555=");
            Assert.AreEqual(0, Majiang.Util.xiangting_yiban(shoupai), "多牌: 5面子");
            Assert.AreEqual(1, Majiang.Util.xiangting_yiban(Majiang.Shoupai.fromString("p234s567,m222=,p0-67")),
                "少牌: 雀頭なし4面子");
            Assert.AreEqual(4, Majiang.Util.xiangting_yiban(Majiang.Shoupai.fromString("p222345z1234567")), "刻子＋順子");
            Assert.AreEqual(4, Majiang.Util.xiangting_yiban(Majiang.Shoupai.fromString("p2344456z123456")),
                "順子＋孤立牌＋順子");
            Assert.AreEqual(3, Majiang.Util.xiangting_yiban(Majiang.Shoupai.fromString("p11222345z12345")), "対子＋刻子＋順子");
            Assert.AreEqual(2, Majiang.Util.xiangting_yiban(Majiang.Shoupai.fromString("p2234556788z123")),
                "対子＋順子＋順子＋対子");
            Assert.AreEqual(0, Majiang.Util.xiangting_yiban(Majiang.Shoupai.fromString("m11122,p123-,s12-3,z111=,")),
                "副露直後の牌姿が和了形");

            TestUtil.ProcessInParallel(data1, data =>
            {
                    var shoupai = new Majiang.Shoupai(data.q);
                    Assert.AreEqual(data.x[0], Majiang.Util.xiangting_yiban(shoupai), "一般手: 10000パターン");
            });

            TestUtil.ProcessInParallel(data2, data =>
            {
                    var shoupai = new Majiang.Shoupai(data.q);
                    Assert.AreEqual(data.x[0], Majiang.Util.xiangting_yiban(shoupai), "混一手: 10000パターン");
            });

            TestUtil.ProcessInParallel(data3, data =>
            {
                    var shoupai = new Majiang.Shoupai(data.q);
                    Assert.AreEqual(data.x[0], Majiang.Util.xiangting_yiban(shoupai), "清一手: 10000パターン");
            });

            TestUtil.ProcessInParallel(data4, data =>
            {
                    var shoupai = new Majiang.Shoupai(data.q);
                    Assert.AreEqual(data.x[0], Majiang.Util.xiangting_yiban(shoupai), "国士手: 10000パターン");
            });
        }

        [Test, Description("xiangting_guoshi(shoupai)")]
        public void TestXiangtingGuoshi()
        {
            Assert.AreEqual(13, Majiang.Util.xiangting_guoshi(Majiang.Shoupai.fromString()), "空の手牌は13向聴");
            Assert.AreEqual(13, Majiang.Util.xiangting_guoshi(Majiang.Shoupai.fromString("m23455p345s45678")), "幺九牌なし");
            Assert.AreEqual(4, Majiang.Util.xiangting_guoshi(Majiang.Shoupai.fromString("m189p12s249z12345")), "雀頭なし");
            Assert.AreEqual(3, Majiang.Util.xiangting_guoshi(Majiang.Shoupai.fromString("m119p12s299z12345")), "雀頭あり");
            Assert.AreEqual(0, Majiang.Util.xiangting_guoshi(Majiang.Shoupai.fromString("m11p19s19z1234567")), "聴牌形");
            Assert.AreEqual(0, Majiang.Util.xiangting_guoshi(Majiang.Shoupai.fromString("m19p19s19z1234567")),
                "聴牌形(13面張)");
            Assert.AreEqual(-1, Majiang.Util.xiangting_guoshi(Majiang.Shoupai.fromString("m119p19s19z1234567")), "和了形");
            Assert.AreEqual(double.PositiveInfinity,
                Majiang.Util.xiangting_guoshi(Majiang.Shoupai.fromString("m19p19s19z1234,z777=")), "副露あり");
            Assert.AreEqual(-1,
                Majiang.Util.xiangting_guoshi(Majiang.Shoupai.fromString("m19p19s19z12345677").zimo("m1", false)),
                "多牌");
            Assert.AreEqual(1, Majiang.Util.xiangting_guoshi(Majiang.Shoupai.fromString("m119p19s19z12345")), "少牌");

            TestUtil.ProcessInParallel(data1, data =>
            {
                    var shoupai = new Majiang.Shoupai(data.q);
                    Assert.AreEqual(data.x[1], Majiang.Util.xiangting_guoshi(shoupai), "一般手: 10000パターン");
            });

            TestUtil.ProcessInParallel(data2, data =>
            {
                    var shoupai = new Majiang.Shoupai(data.q);
                    Assert.AreEqual(data.x[1], Majiang.Util.xiangting_guoshi(shoupai), "混一手: 10000パターン");
            });

            TestUtil.ProcessInParallel(data3, data =>
            {
                    var shoupai = new Majiang.Shoupai(data.q);
                    Assert.AreEqual(data.x[1], Majiang.Util.xiangting_guoshi(shoupai), "清一手: 10000パターン");
            });

            TestUtil.ProcessInParallel(data4, data =>
            {
                    var shoupai = new Majiang.Shoupai(data.q);
                    Assert.AreEqual(data.x[1], Majiang.Util.xiangting_guoshi(shoupai), "国士手: 10000パターン");
            });
        }

        [Test, Description("xiangting_qidui(shoupai)")]
        public void TestXiangtingQidui()
        {
            Assert.AreEqual(13, Majiang.Util.xiangting_qidui(Majiang.Shoupai.fromString()), "空の手牌は13向聴");
            Assert.AreEqual(6, Majiang.Util.xiangting_qidui(Majiang.Shoupai.fromString("m19p19s19z1234567")), "対子なし");
            Assert.AreEqual(2, Majiang.Util.xiangting_qidui(Majiang.Shoupai.fromString("m1188p288s05z1111")), "槓子あり");
            Assert.AreEqual(1, Majiang.Util.xiangting_qidui(Majiang.Shoupai.fromString("m1188p2388s05z111")), "暗刻あり");
            Assert.AreEqual(2, Majiang.Util.xiangting_qidui(Majiang.Shoupai.fromString("m1188p288s055z111")), "暗刻2つ");
            Assert.AreEqual(0, Majiang.Util.xiangting_qidui(Majiang.Shoupai.fromString("m1188p288s05z1177")), "聴牌形");
            Assert.AreEqual(-1, Majiang.Util.xiangting_qidui(Majiang.Shoupai.fromString("m1188p288s05z1177p2")), "和了形");
            Assert.AreEqual(double.PositiveInfinity,
                Majiang.Util.xiangting_qidui(Majiang.Shoupai.fromString("m1188p288s05z2,z111=")), "副露あり");
            Assert.AreEqual(-1,
                Majiang.Util.xiangting_qidui(Majiang.Shoupai.fromString("m1188p2288s05z1122").zimo("z7", false)
                    .zimo("z7", false)), "多牌: 8対子");
            Assert.AreEqual(3, Majiang.Util.xiangting_qidui(Majiang.Shoupai.fromString("m1188s05z1122")), "少牌");

            TestUtil.ProcessInParallel(data1, data =>
            {
                    var shoupai = new Majiang.Shoupai(data.q);
                    Assert.AreEqual(data.x[2], Majiang.Util.xiangting_qidui(shoupai), "一般手: 10000パターン");
            });

            TestUtil.ProcessInParallel(data2, data =>
            {
                    var shoupai = new Majiang.Shoupai(data.q);
                    Assert.AreEqual(data.x[2], Majiang.Util.xiangting_qidui(shoupai), "混一手: 10000パターン");
            });

            TestUtil.ProcessInParallel(data3, data =>
            {
                    var shoupai = new Majiang.Shoupai(data.q);
                    Assert.AreEqual(data.x[2], Majiang.Util.xiangting_qidui(shoupai), "清一手: 10000パターン");
            });

            TestUtil.ProcessInParallel(data4, data =>
            {
                    var shoupai = new Majiang.Shoupai(data.q);
                    Assert.AreEqual(data.x[2], Majiang.Util.xiangting_qidui(shoupai), "国士手: 10000パターン");
            });
        }

        [Test, Description("xiangting(shoupai)")]
        public void TestXiangting()
        {
            Assert.AreEqual(0, Majiang.Util.xiangting(Majiang.Shoupai.fromString("m123p406s789z1122")), "一般形聴牌");
            Assert.AreEqual(0, Majiang.Util.xiangting(Majiang.Shoupai.fromString("m19p19s19z1234567")), "国士無双形聴牌");
            Assert.AreEqual(0, Majiang.Util.xiangting(Majiang.Shoupai.fromString("m1188p288s05z1177")), "七対子形聴牌");

            TestUtil.ProcessInParallel(data1, data =>
            {
                var shoupai = new Majiang.Shoupai(data.q);
                Assert.AreEqual(data.x.Min(), Majiang.Util.xiangting(shoupai), "一般手: 10000パターン");
            });

            TestUtil.ProcessInParallel(data2, data =>
            {
                var shoupai = new Majiang.Shoupai(data.q);
                Assert.AreEqual(data.x.Min(), Majiang.Util.xiangting(shoupai), "混一手: 10000パターン");
            });

            TestUtil.ProcessInParallel(data3, data =>
            {
                var shoupai = new Majiang.Shoupai(data.q);
                Assert.AreEqual(data.x.Min(), Majiang.Util.xiangting(shoupai), "清一手: 10000パターン");
            });

            TestUtil.ProcessInParallel(data4, data =>
            {
                var shoupai = new Majiang.Shoupai(data.q);
                Assert.AreEqual(data.x.Min(), Majiang.Util.xiangting(shoupai), "国士手: 10000パターン");
            });
        }

        [Test, Description("tingpai(shoupai, f_xiangting)")]
        public void TestTingpai()
        {
            Assert.DoesNotThrow(() => Majiang.Util.tingpai(Majiang.Shoupai.fromString("m123p456s789z12345")),
                "打牌可能な状態のとき、エラーとなること");
            Assert.DoesNotThrow(() => Majiang.Util.tingpai(Majiang.Shoupai.fromString("m123p456z12345,s789-,")),
                "打牌可能な状態のとき、エラーとなること");
            CollectionAssert.AreEquivalent(new string[] { "z1", "z2", "z3", "z4" },
                Majiang.Util.tingpai(Majiang.Shoupai.fromString("m123p456s789z1234")), "副露なし");
            CollectionAssert.AreEquivalent(new string[] { "z1", "z2", "z3", "z4" },
                Majiang.Util.tingpai(Majiang.Shoupai.fromString("m123p456z1234,s789-")), "副露あり");
            CollectionAssert.AreEquivalent(
                new string[] { "m1", "m9", "p1", "p9", "s1", "s9", "z1", "z2", "z3", "z4", "z5", "z6", "z7" },
                Majiang.Util.tingpai(Majiang.Shoupai.fromString("m19p19s19z1234567")), "国士無双13面待ち");
            CollectionAssert.AreEquivalent(new string[] { "m1" },
                Majiang.Util.tingpai(Majiang.Shoupai.fromString("m1234444p456s789")), "打牌可能な手牌に4枚ある牌は待ち牌としないこと");
            CollectionAssert.AreEquivalent(new string[] { "m2" },
                Majiang.Util.tingpai(Majiang.Shoupai.fromString("m13p456s789z11,m2222")), "暗刻の牌は待ち牌とできること");
            CollectionAssert.AreEquivalent(new string[] { "m5", "p2", "p6", "p7", "p8", "p9", "s6", "z1", "z7" },
                Majiang.Util.tingpai(Majiang.Shoupai.fromString("m11155p2278s66z17")), "七対子と面子手で同じ向聴数");
            CollectionAssert.AreEquivalent(new string[] { "p7", "p8", "z1", "z7" },
                Majiang.Util.tingpai(Majiang.Shoupai.fromString("m11155p2278s66z17"), Majiang.Util.xiangting_qidui),
                "向聴数算出ルーチンを指定できること");
        }
    }
}