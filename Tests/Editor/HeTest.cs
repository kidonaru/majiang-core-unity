using System;
using System.Linq;
using NUnit.Framework;

namespace Majiang.Test
{
    public class HeTest
    {
        private Majiang.He He()
        {
            return new Majiang.He();
        }

        [Test, Description("クラスが存在すること")]
        public void ClassExists()
        {
            Assert.IsNotNull(typeof(Majiang.He));
        }

        [Test, Description("constructor()")]
        public void TestConstructor()
        {
            Assert.NotNull(He(), "インスタンスが生成できること");
            Assert.AreEqual(0, He()._pai.Count, "インスタンス生成時は捨て牌の長さが0であること");
        }

        [Test, Description("dapai(p)")]
        public void TestDapai()
        {
            Assert.Throws<Exception>(() => He().dapai("z8"), "不正な打牌ができないこと");
            var he = He();
            Assert.AreEqual(he._pai.Count + 1, he.dapai("m1")._pai.Count, "打牌後捨て牌の長さが1増えること");
            Assert.AreEqual("m1_", He().dapai("m1_")._pai.Last(), "ツモ切りを表現できること");
            Assert.AreEqual("m1*", He().dapai("m1*")._pai.Last(), "リーチを表現できること");
            Assert.AreEqual("m1_*", He().dapai("m1_*")._pai.Last(), "ツモ切りリーチを表現できること");
        }

        [Test, Description("fulou(m)")]
        public void TestFulou()
        {
            Assert.Throws<Exception>(() => He().dapai("m1").fulou("m1-"), "不正な面子で鳴けないこと");
            Assert.Throws<Exception>(() => He().dapai("m1").fulou("m1111"), "不正な面子で鳴けないこと");
            Assert.Throws<Exception>(() => He().dapai("m1").fulou("m12-3"), "不正な面子で鳴けないこと");
            var he = He().dapai("m1_");
            Assert.AreEqual(he._pai.Count, he.fulou("m111+")._pai.Count, "鳴かれても捨て牌の長さが変わらないこと");
            he = He().dapai("m2*");
            Assert.AreEqual("m2*-", he.fulou("m12-3")._pai.Last(), "誰から鳴かれたか表現できること");
        }

        [Test, Description("find(p)")]
        public void TestFind()
        {
            var he = He();
            Assert.NotNull(he.dapai("m1").find("m1"), "捨てられた牌を探せること");
            Assert.NotNull(he.dapai("m2_").find("m2"), "ツモ切りの牌を探せること");
            Assert.NotNull(he.dapai("m3*").find("m3"), "リーチ打牌を探せること");
            Assert.NotNull(he.dapai("m0").find("m5"), "赤牌を探せること");
            Assert.NotNull(he.dapai("m4_").fulou("m234-").find("m4"), "鳴かれた牌を探せること");
            Assert.NotNull(he.find("m0_*"), "入力が正規化されていない場合でも探せること");
        }
    }
}