using System;
using System.Linq;
using NUnit.Framework;

namespace Majiang.Test
{
    public class ShanTest
    {
        private Majiang.Shan Shan(Rule rule = null)
        {
            rule = rule ?? new Rule { };
            return new Majiang.Shan(rule);
        }

        [Test, Description("クラスが存在すること")]
        public void ClassExists()
        {
            Assert.IsNotNull(typeof(Majiang.Shan));
        }

        [Test, Description("static zhenbaopai(p)")]
        public void TestZhenbaopai()
        {
            Assert.AreEqual("m2", Majiang.Shan.zhenbaopai("m1"), "一萬 → 二萬");
            Assert.AreEqual("m1", Majiang.Shan.zhenbaopai("m9"), "九萬 → 一萬");
            Assert.AreEqual("m6", Majiang.Shan.zhenbaopai("m0"), "赤五萬 → 六萬");
            Assert.AreEqual("p2", Majiang.Shan.zhenbaopai("p1"), "一筒 → 二筒");
            Assert.AreEqual("p1", Majiang.Shan.zhenbaopai("p9"), "九筒 → 一筒");
            Assert.AreEqual("p6", Majiang.Shan.zhenbaopai("p0"), "赤五筒 → 六筒");
            Assert.AreEqual("s2", Majiang.Shan.zhenbaopai("s1"), "一索 → 二索");
            Assert.AreEqual("s1", Majiang.Shan.zhenbaopai("s9"), "九索 → 一索");
            Assert.AreEqual("s6", Majiang.Shan.zhenbaopai("s0"), "赤五索 → 六索");
            Assert.AreEqual("z2", Majiang.Shan.zhenbaopai("z1"), "東 → 南");
            Assert.AreEqual("z1", Majiang.Shan.zhenbaopai("z4"), "北 → 東");
            Assert.AreEqual("z6", Majiang.Shan.zhenbaopai("z5"), "白 → 發");
            Assert.AreEqual("z5", Majiang.Shan.zhenbaopai("z7"), "中 → 白");
            Assert.Throws<Exception>(() => Majiang.Shan.zhenbaopai("z0"), "不正な牌 → エラー");
        }

        [Test, Description("constructor(rule)")]
        public void TestConstructor()
        {
            string pai = "m1,m1,m1,m1,m2,m2,m2,m2,m3,m3,m3,m3,m4,m4,m4,m4,m5,m5,m5,m5," +
                         "m6,m6,m6,m6,m7,m7,m7,m7,m8,m8,m8,m8,m9,m9,m9,m9," +
                         "p1,p1,p1,p1,p2,p2,p2,p2,p3,p3,p3,p3,p4,p4,p4,p4,p5,p5,p5,p5," +
                         "p6,p6,p6,p6,p7,p7,p7,p7,p8,p8,p8,p8,p9,p9,p9,p9," +
                         "s1,s1,s1,s1,s2,s2,s2,s2,s3,s3,s3,s3,s4,s4,s4,s4,s5,s5,s5,s5," +
                         "s6,s6,s6,s6,s7,s7,s7,s7,s8,s8,s8,s8,s9,s9,s9,s9," +
                         "z1,z1,z1,z1,z2,z2,z2,z2,z3,z3,z3,z3,z4,z4,z4,z4," +
                         "z5,z5,z5,z5,z6,z6,z6,z6,z7,z7,z7,z7";
            Assert.AreEqual(pai,
                new Majiang.Shan(new Rule { 赤牌 = new Hongpai { m = 0, p = 0, s = 0 } })._pai.Concat().OrderBy(x => x)
                    .JoinJS(), "赤牌なしでインスタンスが生成できること");

            pai = "m0,m1,m1,m1,m1,m2,m2,m2,m2,m3,m3,m3,m3,m4,m4,m4,m4,m5,m5,m5," +
                  "m6,m6,m6,m6,m7,m7,m7,m7,m8,m8,m8,m8,m9,m9,m9,m9," +
                  "p0,p0,p1,p1,p1,p1,p2,p2,p2,p2,p3,p3,p3,p3,p4,p4,p4,p4,p5,p5," +
                  "p6,p6,p6,p6,p7,p7,p7,p7,p8,p8,p8,p8,p9,p9,p9,p9," +
                  "s0,s0,s0,s1,s1,s1,s1,s2,s2,s2,s2,s3,s3,s3,s3,s4,s4,s4,s4,s5," +
                  "s6,s6,s6,s6,s7,s7,s7,s7,s8,s8,s8,s8,s9,s9,s9,s9," +
                  "z1,z1,z1,z1,z2,z2,z2,z2,z3,z3,z3,z3,z4,z4,z4,z4," +
                  "z5,z5,z5,z5,z6,z6,z6,z6,z7,z7,z7,z7";
            Assert.AreEqual(pai,
                new Majiang.Shan(new Rule { 赤牌 = new Hongpai { m = 1, p = 2, s = 3 } })._pai.Concat().OrderBy(x => x)
                    .JoinJS(), "赤牌ありでインスタンスが生成できること");
        }

        [Test, Description("get paishu()")]
        public void TestPaishu()
        {
            Assert.AreEqual(122, Shan().paishu, "牌山生成直後の残牌数は122");
        }

        [Test, Description("get baopai()")]
        public void TestBaopai()
        {
            Assert.AreEqual(1, Shan().baopai.Count, "牌山生成直後のドラは1枚");
        }

        [Test, Description("get fubaopai()")]
        public void TestFubaopai()
        {
            Assert.IsNull(Shan().fubaopai, "牌山生成直後は null を返す");
            Assert.AreEqual(1, Shan().close().fubaopai.Count, "牌山固定後は裏ドラを返す");
            Assert.IsNull(Shan(new Rule { 裏ドラあり = false }).close().fubaopai, "裏ドラなしの場合は牌山固定後も null を返す");
        }

        [Test, Description("zimo()")]
        public void TestZimo()
        {
            Assert.NotNull(Shan().zimo(), "牌山生成直後にツモれること");
            var shan = Shan();
            Assert.AreEqual(shan.paishu - 1, shan.zimo() != null ? shan.paishu : 0, "ツモ後に残牌数が減ること");
            shan = Shan();
            while (shan.paishu > 0)
            {
                shan.zimo();
            }

            Assert.Throws<Exception>(() => shan.zimo(), "王牌はツモれないこと");
            Assert.Throws<Exception>(() => Shan().close().zimo(), "牌山固定後はツモれないこと");
        }

        [Test, Description("gangzimo()")]
        public void TestGangzimo()
        {
            Assert.NotNull(Shan().gangzimo(), "牌山生成直後に槓ツモできること");
            var shan = Shan();
            Assert.AreEqual(shan.paishu - 1, shan.gangzimo() != null ? shan.paishu : 0, "槓ツモ後に残牌数が減ること");
            shan = Shan();
            Assert.Throws<Exception>(() =>
            {
                shan.gangzimo();
                shan.zimo();
            }, "槓ツモ直後はツモれないこと");
            shan = Shan();
            Assert.Throws<Exception>(() =>
            {
                shan.gangzimo();
                shan.gangzimo();
            }, "槓ツモ直後に続けて槓ツモできないこと");
            shan = Shan();
            while (shan.paishu > 0)
            {
                shan.zimo();
            }

            Assert.Throws<Exception>(() => shan.gangzimo(), "ハイテイで槓ツモできないこと");
            Assert.Throws<Exception>(() => Shan().close().gangzimo(), "牌山固定後は槓ツモできないこと");
            shan = Shan();
            for (int i = 0; i < 4; i++)
            {
                shan.gangzimo();
                shan.kaigang();
            }

            Assert.Throws<Exception>(() => shan.gangzimo(), "5つ目の槓ツモができないこと");
            shan = Shan(new Rule { カンドラあり = false });
            for (int i = 0; i < 4; i++)
            {
                shan.gangzimo();
            }

            Assert.AreEqual(1, shan.baopai.Count);
            Assert.Throws<Exception>(() => shan.gangzimo(), "カンドラなしでも5つ目の槓ツモができないこと");
        }

        [Test, Description("kaigang()")]
        public void TestKaigang()
        {
            Assert.Throws<Exception>(() => Shan().kaigang(), "牌山生成直後に開槓できないこと");
            var shan = Shan();
            Assert.NotNull(shan.gangzimo(), "槓ツモ後に開槓できること");
            Assert.NotNull(shan.kaigang(), "槓ツモ後に開槓できること");
            shan = Shan();
            shan.gangzimo();
            Assert.AreEqual(shan.baopai.Count + 1, shan.kaigang().baopai.Count, "開槓によりドラが増えること");
            shan = Shan();
            shan.gangzimo();
            Assert.AreEqual(2, shan.kaigang().close().fubaopai.Count, "開槓により裏ドラが増えること");
            shan = Shan();
            shan.gangzimo();
            Assert.NotNull(shan.kaigang().zimo(), "開槓後はツモできること");
            shan = Shan();
            shan.gangzimo();
            Assert.NotNull(shan.kaigang().gangzimo(), "開槓後は槓ツモできること");
            shan = Shan();
            shan.gangzimo();
            Assert.Throws<Exception>(() => shan.close().kaigang(), "牌山固定後は開槓できないこと");
            shan = Shan(new Rule { カンドラあり = false });
            shan.gangzimo();
            Assert.Throws<Exception>(() => shan.kaigang(), "カンドラなしの場合は開槓できないこと");
            shan = Shan(new Rule { カン裏あり = false });
            shan.gangzimo();
            Assert.AreEqual(1, shan.kaigang().close().fubaopai.Count, "カン裏なしの場合は開槓で裏ドラが増えないこと");
            shan = Shan(new Rule { 裏ドラあり = false });
            shan.gangzimo();
            Assert.IsNull(shan.kaigang().close().fubaopai, "裏ドラなしの場合は開槓で裏ドラ発生しないこと");
        }
    }
}