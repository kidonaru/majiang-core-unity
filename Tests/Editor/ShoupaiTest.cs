using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Majiang.Test
{
    public class ShoupaiTest
    {
        [Test, Description("クラスが存在すること")]
        public void ClassExists()
        {
            Assert.IsNotNull(typeof(Majiang.Shoupai));
        }

        [Test, Description("static valid_pai(p)")]
        public void TestValidPai()
        {
            Assert.AreEqual("m1", Majiang.Shoupai.valid_pai("m1"), "m1    : 正常");
            Assert.AreEqual("p2_", Majiang.Shoupai.valid_pai("p2_"), "p2_   : 正常(ツモ切り)");
            Assert.AreEqual("s3*", Majiang.Shoupai.valid_pai("s3*"), "s3*   : 正常(リーチ)");
            Assert.AreEqual("z4_*", Majiang.Shoupai.valid_pai("z4_*"), "z4_*  : 正常(ツモ切り・リーチ)");
            Assert.AreEqual("m0-", Majiang.Shoupai.valid_pai("m0-"), "m0-   : 正常(被副露)");
            Assert.AreEqual("p5_+", Majiang.Shoupai.valid_pai("p5_+"), "p5_+  : 正常(ツモ切り・被副露)");
            Assert.AreEqual("s6*=", Majiang.Shoupai.valid_pai("s6*="), "s6*=  : 正常(リーチ・被副露)");
            Assert.AreEqual("z7_*-", Majiang.Shoupai.valid_pai("z7_*-"), "z7_*- : 正常(ツモ切り・リーチ・被副露)");
            Assert.IsNull(Majiang.Shoupai.valid_pai("_"), "_     : 不正(裏向き牌)");
            Assert.IsNull(Majiang.Shoupai.valid_pai("x"), "x     : 不正");
            Assert.IsNull(Majiang.Shoupai.valid_pai("mm"), "mm    : 不正");
            Assert.IsNull(Majiang.Shoupai.valid_pai("z0"), "z0    : 不正");
            Assert.IsNull(Majiang.Shoupai.valid_pai("z8"), "z8    : 不正");
            Assert.IsNull(Majiang.Shoupai.valid_pai("m9x"), "m9x   : 不正");
            Assert.IsNull(Majiang.Shoupai.valid_pai("m9=*"), "m9=*  : 不正");
            Assert.IsNull(Majiang.Shoupai.valid_pai("m9*_"), "m9*_  : 不正");
            Assert.IsNull(Majiang.Shoupai.valid_pai("m9=_"), "m9=_  : 不正");
        }

        [Test, Description("static valid_mianzi(m)")]
        public void TestValidMianzi()
        {
            Assert.AreEqual("m111+", Majiang.Shoupai.valid_mianzi("m111+"), "m111+  : 正常");
            Assert.AreEqual("p555=", Majiang.Shoupai.valid_mianzi("p555="), "p555=  : 正常");
            Assert.AreEqual("s999-", Majiang.Shoupai.valid_mianzi("s999-"), "s999-  : 正常");
            Assert.AreEqual("z777+7", Majiang.Shoupai.valid_mianzi("z777+7"), "z777+7 : 正常");
            Assert.AreEqual("m2222", Majiang.Shoupai.valid_mianzi("m2222"), "m2222  : 正常");
            Assert.AreEqual("p550=", Majiang.Shoupai.valid_mianzi("p550="), "p550=  : 正常");
            Assert.AreEqual("p5550=", Majiang.Shoupai.valid_mianzi("p5550="), "p5550= : 正常");
            Assert.AreEqual("p505=", Majiang.Shoupai.valid_mianzi("p055="), "p055=  : 正常 => p505=");
            Assert.AreEqual("p505=0", Majiang.Shoupai.valid_mianzi("p055=0"), "p055=0 : 正常 => p505=0");
            Assert.AreEqual("p000=0", Majiang.Shoupai.valid_mianzi("p000=0"), "p000=0 : 正常");
            Assert.AreEqual("s5505-", Majiang.Shoupai.valid_mianzi("s0555-"), "s0555- : 正常 => s5505-");
            Assert.AreEqual("s5005-", Majiang.Shoupai.valid_mianzi("s0055-"), "s0055- : 正常 => s5005-");
            Assert.AreEqual("s5000", Majiang.Shoupai.valid_mianzi("s0005"), "s0005  : 正常 => s5000");
            Assert.AreEqual("s0000", Majiang.Shoupai.valid_mianzi("s0000"), "s0000  : 正常 => s0000");
            Assert.AreEqual("m1-23", Majiang.Shoupai.valid_mianzi("m1-23"), "m1-23  : 正常");
            Assert.AreEqual("m12-3", Majiang.Shoupai.valid_mianzi("m12-3"), "m12-3  : 正常");
            Assert.AreEqual("m123-", Majiang.Shoupai.valid_mianzi("m123-"), "m123-  : 正常");
            Assert.AreEqual("m1-23", Majiang.Shoupai.valid_mianzi("m231-"), "m231-  : 正常 => m1-23");
            Assert.AreEqual("m12-3", Majiang.Shoupai.valid_mianzi("m312-"), "m312-  : 正常 => m12-3");
            Assert.AreEqual("m123-", Majiang.Shoupai.valid_mianzi("m3-12"), "m3-12  : 正常 => m123-");
            Assert.AreEqual("m40-6", Majiang.Shoupai.valid_mianzi("m460-"), "m460-  : 正常 => m40-6");
            Assert.IsNull(Majiang.Shoupai.valid_mianzi("m1234-"), "m1234− : 不正");
            Assert.IsNull(Majiang.Shoupai.valid_mianzi("m135-"), "m135−  : 不正");
            Assert.IsNull(Majiang.Shoupai.valid_mianzi("m1234"), "m1234  : 不正");
            Assert.IsNull(Majiang.Shoupai.valid_mianzi("m123"), "m123   : 不正");
            Assert.IsNull(Majiang.Shoupai.valid_mianzi("m111"), "m111   : 不正");
            Assert.IsNull(Majiang.Shoupai.valid_mianzi("z111=0"), "z111=0 : 不正");
        }

        [Test, Description("constructor(qipai)")]
        public void TestConstructor()
        {
            Assert.IsNotNull(new Majiang.Shoupai(), "インスタンスが生成できること");

            var qipai = new List<string>
                { "m0", "m1", "m9", "p0", "p1", "p9", "s0", "s1", "s9", "z1", "z2", "z6", "z7" };
            Assert.IsNotNull(new Majiang.Shoupai(qipai), "配牌を指定してインスタンスが生成できること");

            Assert.IsNotNull(new Majiang.Shoupai(new List<string> { "_" }), "裏向きの牌を指定してインスタンスが生成できること");

            Assert.Throws<Exception>(() => new Majiang.Shoupai(new List<string> { "z0" }), "不正な牌を含む配牌で例外が発生すること");

            Assert.Throws<Exception>(() => new Majiang.Shoupai(new List<string> { "m1", "m1", "m1", "m1", "m1" }),
                "5枚目の牌を含む配牌で例外が発生すること");
        }

        [Test, Description("static fromString(paistr)")]
        public void TestFromStringStatic()
        {
            Assert.AreEqual("", Majiang.Shoupai.fromString().ToString(), "パラメータなしでもインスタンスが生成できること");
            Assert.AreEqual("", Majiang.Shoupai.fromString("").ToString(), "空文字列からでもインスタンスが生成できること");
            Assert.AreEqual("m123p456s789z4567", Majiang.Shoupai.fromString("z7654s987p654m321").ToString(),
                "副露なしの場合にインスタンスが生成できること");
            Assert.AreEqual("m1,p123-,s555=,z777+7,m9999",
                Majiang.Shoupai.fromString("m1,p123-,s555=,z777+7,m9999").ToString(), "副露ありの場合でもインスタンスが生成できること");
            Assert.AreEqual("____m123p456s789", Majiang.Shoupai.fromString("m123p456s789____").ToString(),
                "伏せ牌がある場合でもインスタンスが生成できること");
            Assert.AreEqual("____m123p456,s789-", Majiang.Shoupai.fromString("m123p456____,s789-").ToString(),
                "伏せ牌がある場合でもインスタンスが生成できること(副露あり)");
            Assert.AreEqual("m111p222s333", Majiang.Shoupai.fromString("m111p222s333").ToString(),
                "少牌の場合でもインスタンスが生成できること");
            Assert.AreEqual("m123456789p1234p5", Majiang.Shoupai.fromString("m123456789p123456").ToString(),
                "多牌の場合、14枚としてからインスタンスを生成すること");
            Assert.AreEqual("m123456789p1p2,z111=", Majiang.Shoupai.fromString("m123456789p123,z111=").ToString(),
                "多牌の場合、14枚としてからインスタンスを生成すること(副露あり)");
            Assert.AreEqual("m1m2,z111=,p123-,s555=,z777+",
                Majiang.Shoupai.fromString("m123,z111=,p123-,s555=,z777+").ToString(),
                "多牌の場合、14枚としてからインスタンスを生成すること(副露あり)");
            Assert.AreEqual("m1112345678999m1", Majiang.Shoupai.fromString("m11123456789991").ToString(),
                "ツモ牌を再現してインスタンスを生成すること");
            Assert.AreEqual("m1112345678999m0", Majiang.Shoupai.fromString("m11123456789990").ToString(),
                "ツモ牌を再現してインスタンスを生成すること(赤牌をツモ)");
            Assert.AreEqual("m12p345s678z23m3,z111=", Majiang.Shoupai.fromString("m12p345s678z23m3,z111=").ToString(),
                "ツモ牌を再現してインスタンスを生成すること(副露あり)");
            Assert.AreEqual("m0555p0055s0000", Majiang.Shoupai.fromString("m5550p5500s0000z00").ToString(),
                "手牌中の赤牌を再現してインスタンスを生成すること");
            Assert.AreEqual("m123p456s789z1112*", Majiang.Shoupai.fromString("m123p456s789z1112*").ToString(),
                "リーチを再現してインスタンスを生成すること");
            Assert.AreEqual("m123p456s789z2*,z1111", Majiang.Shoupai.fromString("m123p456s789z2*,z1111").ToString(),
                "リーチを再現してインスタンスを生成すること(暗槓あり)");
            Assert.AreEqual("m123p456s789z2*,z111=", Majiang.Shoupai.fromString("m123p456s789z2*,z111=").ToString(),
                "リーチを再現してインスタンスを生成すること(明槓あり)");
            Assert.AreEqual("m123p456s789z2*,z111-", Majiang.Shoupai.fromString("m123p456s789z2*,z111-").ToString(),
                "リーチを再現してインスタンスを生成すること(加槓あり)");
            Assert.AreEqual("m123p456s789z2*,z111+", Majiang.Shoupai.fromString("m123p456s789z2*,z111+").ToString(),
                "リーチを再現してインスタンスを生成すること(大明槓あり)");
            Assert.AreEqual("m123p456s789z2,m3-40", Majiang.Shoupai.fromString("m123p456s789z2,m403-").ToString(),
                "副露面子を正規化してインスタンスを生成すること(チー)");
            Assert.AreEqual("m123p456s789z2,m34-0", Majiang.Shoupai.fromString("m123p456s789z2,m304-").ToString(),
                "副露面子を正規化してインスタンスを生成すること(チー)");
            Assert.AreEqual("m123p456s789z2,m345-", Majiang.Shoupai.fromString("m123p456s789z2,m345-").ToString(),
                "副露面子を正規化してインスタンスを生成すること(チー)");
            Assert.AreEqual("m123p456s789z2,p500+", Majiang.Shoupai.fromString("m123p456s789z2,p050+").ToString(),
                "副露面子を正規化してインスタンスを生成すること(ポン)");
            Assert.AreEqual("m123p456s789z2,p505+", Majiang.Shoupai.fromString("m123p456s789z2,p055+").ToString(),
                "副露面子を正規化してインスタンスを生成すること(ポン)");
            Assert.AreEqual("m123p456s789z2,p550+", Majiang.Shoupai.fromString("m123p456s789z2,p550+").ToString(),
                "副露面子を正規化してインスタンスを生成すること(ポン)");
            Assert.AreEqual("m123p456s789z2,s5505=", Majiang.Shoupai.fromString("m123p456s789z2,s0555=").ToString(),
                "副露面子を正規化してインスタンスを生成すること(カン)");
            Assert.AreEqual("m123p456s789z2,s5000=", Majiang.Shoupai.fromString("m123p456s789z2,s0050=").ToString(),
                "副露面子を正規化してインスタンスを生成すること(カン)");
            Assert.AreEqual("m123p456s789z2,s5500", Majiang.Shoupai.fromString("m123p456s789z2,s0505").ToString(),
                "副露面子を正規化してインスタンスを生成すること(カン)");
            Assert.AreEqual("m123p456s789z2", Majiang.Shoupai.fromString("m123p456s789z2,z000+").ToString(),
                "不正な副露面子を無視してインスタンスを生成すること(不正な牌)");
            Assert.AreEqual("m123p456s789z2", Majiang.Shoupai.fromString("m123p456s789z2,z888+").ToString(),
                "不正な副露面子を無視してインスタンスを生成すること(不正な牌)");
            Assert.AreEqual("m123p456s789z2", Majiang.Shoupai.fromString("m123p456s789z2,z1-23").ToString(),
                "不正な副露面子を無視してインスタンスを生成すること(字牌でのチー)");
            Assert.AreEqual("m123p456s789z2", Majiang.Shoupai.fromString("m123p456s789z2,s1+23").ToString(),
                "不正な副露面子を無視してインスタンスを生成すること(下家からのチー)");
            Assert.AreEqual("m123p456s789z2", Majiang.Shoupai.fromString("m123p456s789z2,z11-").ToString(),
                "不正な副露面子を無視してインスタンスを生成すること(対子)");
            Assert.AreEqual("m123p456s789z2", Majiang.Shoupai.fromString("m123p456s789z2,s13-5").ToString(),
                "不正な副露面子を無視してインスタンスを生成すること(両嵌)");
            Assert.AreEqual("m123p456s789z2", Majiang.Shoupai.fromString("m123p456s789z2,m1p2s3-").ToString(),
                "不正な副露面子を無視してインスタンスを生成すること(連子)");
            Assert.AreEqual("p456s789z1,m12-3,p999=,", Majiang.Shoupai.fromString("p456s789z1,m12-3,p999=,").ToString(),
                "副露直後の手牌を再現してインスタンスを生成すること");
        }

        [Test, Description("clone()")]
        public void TestClone()
        {
            var shoupai = new Majiang.Shoupai();
            Assert.IsTrue(shoupai != shoupai.clone(), "インスタンスを複製すること");

            shoupai = Majiang.Shoupai.fromString("m123p456s789z4567");
            Assert.AreEqual(shoupai.ToString(), shoupai.clone().ToString(), "手牌を再現してインスタンスを複製すること");

            shoupai = Majiang.Shoupai.fromString("m1,p123-,s555=,z777+7,m9999");
            Assert.AreEqual(shoupai.ToString(), shoupai.clone().ToString(), "副露を再現してインスタンスを複製すること");

            shoupai = Majiang.Shoupai.fromString("m11123456789991");
            Assert.AreEqual(shoupai.ToString(), shoupai.clone().ToString(), "ツモ牌を再現してインスタンスを複製すること");

            shoupai = Majiang.Shoupai.fromString("m123p456s789z1112*");
            Assert.AreEqual(shoupai.ToString(), shoupai.clone().ToString(), "リーチを再現してインスタンスを複製すること");

            shoupai = Majiang.Shoupai.fromString("___________,m123-");
            Assert.AreEqual(shoupai.ToString(), shoupai.clone().ToString(), "伏せ牌を再現してインスタンスを複製すること");

            shoupai = Majiang.Shoupai.fromString("m123p456s789z4567");
            Assert.AreNotEqual(shoupai.ToString(), shoupai.clone().zimo("m1").ToString(), "複製後のツモが複製前の手牌に影響しないこと");

            shoupai = Majiang.Shoupai.fromString("m123p456s789z34567");
            Assert.AreNotEqual(shoupai.ToString(), shoupai.clone().dapai("m1").ToString(), "複製後の打牌が複製前の手牌に影響しないこと");

            shoupai = Majiang.Shoupai.fromString("m123p456s789z1167");
            Assert.AreNotEqual(shoupai.ToString(), shoupai.clone().fulou("z111=").ToString(), "複製後の副露が複製前の手牌に影響しないこと");

            shoupai = Majiang.Shoupai.fromString("m123p456s789z11112");
            Assert.AreNotEqual(shoupai.ToString(), shoupai.clone().gang("z1111").ToString(), "複製後のカンが複製前の手牌に影響しないこと");

            shoupai = Majiang.Shoupai.fromString("m123p456s789z11223");
            Assert.AreNotEqual(shoupai.ToString(), shoupai.clone().dapai("z3*").ToString(), "複製後のリーチが複製前の手牌に影響しないこと");
        }

        [Test, Description("fromString(paistr)")]
        public void TestFromString()
        {
            Assert.AreEqual("m123p456s789z1122z2", new Shoupai()._fromString("m123p456s789z1122z2").ToString(),
                "牌姿から手牌を更新できること");
            Assert.AreEqual("m123p456s789z2,z111=", new Shoupai()._fromString("m123p456s789z2,z111=").ToString(),
                "副露あり");
            Assert.AreEqual("m123p456s789z1122*", new Shoupai()._fromString("m123p456s789z1122*").ToString(), "リーチ後");
            Assert.AreEqual("__________,z111=", new Shoupai()._fromString("__________,z111=").ToString(), "伏せ牌あり");
        }

        [Test, Description("zumo(p)")]
        public void TestZumo()
        {
            Assert.AreEqual("m123p456s789z4567m1", Shoupai.fromString("m123p456s789z4567").zimo("m1").ToString(),
                "萬子をツモれること");
            Assert.AreEqual("m123p456s789z4567p1", Shoupai.fromString("m123p456s789z4567").zimo("p1").ToString(),
                "筒子をツモれること");
            Assert.AreEqual("m123p456s789z4567s1", Shoupai.fromString("m123p456s789z4567").zimo("s1").ToString(),
                "索子をツモれること");
            Assert.AreEqual("m123p456s789z4567z1", Shoupai.fromString("m123p456s789z4567").zimo("z1").ToString(),
                "字牌をツモれること");
            Assert.AreEqual("m123p456s789z4567m0", Shoupai.fromString("m123p456s789z4567").zimo("m0").ToString(),
                "赤牌をツモれること");
            Assert.AreEqual("m123p456s789z4567_", Shoupai.fromString("m123p456s789z4567").zimo("_").ToString(),
                "伏せ牌をツモれること");

            Assert.Throws<Exception>(() => Shoupai.fromString("m123p456s789z4567").zimo(), "不正な牌はツモれないこと");
            Assert.Throws<Exception>(() => Shoupai.fromString("m123p456s789z4567").zimo("z0"), "不正な牌はツモれないこと");
            Assert.Throws<Exception>(() => Shoupai.fromString("m123p456s789z4567").zimo("z8"), "不正な牌はツモれないこと");
            Assert.Throws<Exception>(() => Shoupai.fromString("m123p456s789z4567").zimo("mm"), "不正な牌はツモれないこと");
            Assert.Throws<Exception>(() => Shoupai.fromString("m123p456s789z4567").zimo("xx"), "不正な牌はツモれないこと");

            Assert.Throws<Exception>(() => Shoupai.fromString("m123p456s789z34567").zimo("m1"), "ツモの直後にはツモれないこと");
            Assert.Throws<Exception>(() => Shoupai.fromString("m123p456z34567,s789-,").zimo("m1"), "副露の直後にはツモれないこと");

            Assert.AreEqual("m123p456s789z34567m1",
                Shoupai.fromString("m123p456s789z34567").zimo("m1", false).ToString(), "多牌となる牌をツモることもできること");

            Assert.Throws<Exception>(() => Shoupai.fromString("m123p456s789z1111").zimo("z1"), "5枚目の牌はツモれないこと");
            Assert.Throws<Exception>(() => Shoupai.fromString("m455556s789z1111").zimo("m0"), "5枚目の牌はツモれないこと");
        }

        [Test, Description("dapai(p, check = true)")]
        public void TestDapai()
        {
            Assert.AreEqual("m23p456s789z34567", Shoupai.fromString("m123p456s789z34567").dapai("m1").ToString(),
                "萬子を打牌できること");
            Assert.AreEqual("m123p56s789z34567", Shoupai.fromString("m123p456s789z34567").dapai("p4").ToString(),
                "筒子を打牌できること");
            Assert.AreEqual("m123p456s89z34567", Shoupai.fromString("m123p456s789z34567").dapai("s7").ToString(),
                "索子を打牌できること");
            Assert.AreEqual("m123p456s789z4567", Shoupai.fromString("m123p456s789z34567").dapai("z3").ToString(),
                "字牌を打牌できること");
            Assert.AreEqual("m123p46s789z34567", Shoupai.fromString("m123p406s789z34567").dapai("p0").ToString(),
                "赤牌を打牌できること");
            Assert.AreEqual("m123p456s789z3456*", Shoupai.fromString("m123p456s789z34567").dapai("z7*").ToString(),
                "リーチできること");
            Assert.AreEqual("m123p456s789z1223*", Shoupai.fromString("m123p456s789z11223*").dapai("z1").ToString(),
                "リーチ後にもツモ切り以外の打牌ができること(チェックしない)");
            Assert.AreEqual("_____________", Shoupai.fromString("______________").dapai("m1").ToString(),
                "伏せ牌がある場合、任意の牌が打牌できること");

            Assert.Throws<Exception>(() => Shoupai.fromString("m123p456s789z34567").dapai(), "不正な牌は打牌できないこと");
            Assert.Throws<Exception>(() => Shoupai.fromString("m123p456s789z34567").dapai("z0"), "不正な牌は打牌できないこと");
            Assert.Throws<Exception>(() => Shoupai.fromString("m123p456s789z34567").dapai("z8"), "不正な牌は打牌できないこと");
            Assert.Throws<Exception>(() => Shoupai.fromString("m123p456s789z34567").dapai("mm"), "不正な牌は打牌できないこと");
            Assert.Throws<Exception>(() => Shoupai.fromString("m123p456s789z34567").dapai("xx"), "不正な牌は打牌できないこと");

            Assert.Throws<Exception>(() => Shoupai.fromString("m123p456s789z4567").dapai("_"), "伏せた牌は打牌できないこと");
            Assert.Throws<Exception>(() => Shoupai.fromString("m123p456s789z4567").dapai("m1"), "打牌の直後には打牌できないこと");

            Assert.AreEqual("m23p456s789z4567", Shoupai.fromString("m123p456s789z4567").dapai("m1", false).ToString(),
                "少牌となる牌を打牌することもできること");

            Assert.Throws<Exception>(() => Shoupai.fromString("m123p456s789z34567").dapai("z1"), "手牌にない牌は打牌できないこと");
            Assert.Throws<Exception>(() => Shoupai.fromString("m123p456s789z34567").dapai("p0"), "手牌にない牌は打牌できないこと");
            Assert.Throws<Exception>(() => Shoupai.fromString("m123p406s789z34567").dapai("p5"), "手牌にない牌は打牌できないこと");
        }

        [Test, Description("fulou(m, check = true)")]
        public void TestFulou()
        {
            Assert.AreEqual("p456s789z34567,m1-23,", Shoupai.fromString("m23p456s789z34567").fulou("m1-23").ToString(),
                "萬子を副露できること");
            Assert.AreEqual("m123s789z34567,p45-6,", Shoupai.fromString("m123p46s789z34567").fulou("p45-6").ToString(),
                "筒子を副露できること");
            Assert.AreEqual("m123p456z34567,s999+,", Shoupai.fromString("m123p456s99z34567").fulou("s999+").ToString(),
                "索子を副露できること");
            Assert.AreEqual("m123p456s789z67,z111=,", Shoupai.fromString("m123p456s789z1167").fulou("z111=").ToString(),
                "字牌を副露できること");
            Assert.AreEqual("m123s789z4567,p5005-", Shoupai.fromString("m123p500s789z4567").fulou("p5005-").ToString(),
                "赤牌を副露できること");
            Assert.AreEqual("m1p456s789z4567*,m1-23,",
                Shoupai.fromString("m123p456s789z4567*").fulou("m1-23").ToString(), "リーチ後にも副露できること(チェックしない)");
            Assert.AreEqual("___________,m1-23,", Shoupai.fromString("_____________").fulou("m1-23").ToString(),
                "伏せ牌がある場合、それを使って副露できること");

            Assert.Throws<Exception>(() => Shoupai.fromString("m123p456s789z4567").fulou("z3-45"), "不正な面子で副露できないこと");
            Assert.Throws<Exception>(() => Shoupai.fromString("m123p456s789z4567").fulou("m231-"), "不正な面子で副露できないこと");
            Assert.Throws<Exception>(() => Shoupai.fromString("_____________").fulou("m1111"), "暗槓は副露ではない");
            Assert.Throws<Exception>(() => Shoupai.fromString("_____________").fulou("m111+1"), "加槓は副露ではない");
            Assert.Throws<Exception>(() => Shoupai.fromString("m123p456s789z11567").fulou("z111="), "ツモの直後に副露できないこと");
            Assert.Throws<Exception>(() => Shoupai.fromString("m123p456s789z22,z111=,").fulou("z222="),
                "副露の直後に副露できないこと");

            Assert.AreEqual("m123p456s789z567,z111=,",
                Shoupai.fromString("m123p456s789z11567").fulou("z111=", false).ToString(), "多牌となる副露もできること");
            Assert.AreEqual("m123p456s789,z111=,z222=,",
                Shoupai.fromString("m123p456s789z22,z111=,").fulou("z222=", false).ToString(), "多牌となる副露もできること");

            Assert.Throws<Exception>(() => Shoupai.fromString("m123p456s789z2,z111=").fulou("z333="),
                "手牌にない牌を使って副露できないこと");
            Assert.Throws<Exception>(() => Shoupai.fromString("m123p40s789z22,z111=").fulou("p456-"),
                "手牌にない牌を使って副露できないこと");
            Assert.Throws<Exception>(() => Shoupai.fromString("m123p45s789z22,z111=").fulou("p406-"),
                "手牌にない牌を使って副露できないこと");
        }

        [Test, Description("gang(p, check = true)")]
        public void TestGang()
        {
            Assert.AreEqual("p456s789z4567,m1111", Shoupai.fromString("m1111p456s789z4567").gang("m1111").ToString(),
                "萬子で暗槓できること");
            Assert.AreEqual("p456s789z4567,m111+1",
                Shoupai.fromString("m1p456s789z4567,m111+").gang("m111+1").ToString(), "萬子で加槓できること");
            Assert.AreEqual("m123s789z4567,p5555", Shoupai.fromString("m123p5555s789z4567").gang("p5555").ToString(),
                "筒子で暗槓できること");
            Assert.AreEqual("m123s789z4567,p555=5",
                Shoupai.fromString("m123p5s789z4567,p555=").gang("p555=5").ToString(), "筒子で加槓できること");
            Assert.AreEqual("m123p456z4567,s9999", Shoupai.fromString("m123p456s9999z4567").gang("s9999").ToString(),
                "索子で暗槓できること");
            Assert.AreEqual("m123p456z4567,s999-9",
                Shoupai.fromString("m123p456s9z4567,s999-").gang("s999-9").ToString(), "索子で加槓できること");
            Assert.AreEqual("m123p456s789z6,z7777", Shoupai.fromString("m123p456s789z67777").gang("z7777").ToString(),
                "字牌で暗槓できること");
            Assert.AreEqual("m123p456s789z6,z777+7",
                Shoupai.fromString("m123p456s789z67,z777+").gang("z777+7").ToString(), "字牌で加槓できること");
            Assert.AreEqual("p456s789z4567,m5500", Shoupai.fromString("m0055p456s789z4567").gang("m5500").ToString(),
                "赤牌を含む暗槓ができること");
            Assert.AreEqual("m123s789z4567,p505=5",
                Shoupai.fromString("m123p5s789z4567,p505=").gang("p505=5").ToString(), "赤牌を含む加槓ができること");
            Assert.AreEqual("m123s789z4567,p555=0",
                Shoupai.fromString("m123p0s789z4567,p555=").gang("p555=0").ToString(), "赤牌で加槓できること");
            Assert.AreEqual("p456s789z4567*,m1111", Shoupai.fromString("m1111p456s789z4567*").gang("m1111").ToString(),
                "リーチ後にも暗槓できること(チェックしない)");
            Assert.AreEqual("p456s789z4567*,m111+1",
                Shoupai.fromString("m1p456s789z4567*,m111+").gang("m111+1").ToString(), "リーチ後にも加槓できること(チェックしない)");
            Assert.AreEqual("__________,m5550", Shoupai.fromString("______________").gang("m5550").ToString(),
                "伏せ牌がある場合、それを使って暗槓できること");
            Assert.AreEqual("__________,m555=0", Shoupai.fromString("___________,m555=").gang("m555=0").ToString(),
                "伏せ牌がある場合、それを使って加槓できること");

            Assert.Throws<Exception>(() => Shoupai.fromString("m1112456s789z4567").gang("m456-"), "順子で槓できないこと");
            Assert.Throws<Exception>(() => Shoupai.fromString("m1112456s789z4567").gang("m111+"), "刻子で槓できないこと");
            Assert.Throws<Exception>(() => Shoupai.fromString("m1112456s789z4567").gang("m1112"), "不正な面子で槓できないこと");
            Assert.Throws<Exception>(() => Shoupai.fromString("m2456s789z4567,m111+").gang("m111+2"), "不正な面子で槓できないこと");
            Assert.Throws<Exception>(() => Shoupai.fromString("m1111p456s789z456").gang("m1111"), "打牌の直後に槓できないこと");
            Assert.Throws<Exception>(() => Shoupai.fromString("m1111s789z4567,p456-,").gang("m1111"), "副露の直後に槓できないこと");
            Assert.Throws<Exception>(() => Shoupai.fromString("m1111p4444s789z567").gang("m1111").gang("p4444"),
                "槓の直後に槓できないこと");

            Assert.AreEqual("p456s789z456,m1111",
                Shoupai.fromString("m1111p456s789z456").gang("m1111", false).ToString(), "少牌となる槓もできること");
            Assert.AreEqual("s789z4567,p456-,m1111",
                Shoupai.fromString("m1111s789z4567,p456-,").gang("m1111", false).ToString(), "少牌となる槓もできること");
            Assert.AreEqual("s789z567,m1111,p4444",
                Shoupai.fromString("m1111p4444s789z567").gang("m1111", false).gang("p4444", false).ToString(),
                "少牌となる槓もできること");

            Assert.Throws<Exception>(() => Shoupai.fromString("m1112p456s789z4567").gang("m1111"), "手牌に4枚ない牌で暗槓できないこと");
            Assert.Throws<Exception>(() => Shoupai.fromString("m13p456s789z567,m222=").gang("m2222"),
                "手牌にない牌で加槓できないこと");
            Assert.Throws<Exception>(() => Shoupai.fromString("m10p456s789z567,m555=").gang("m5555"),
                "手牌にない牌で加槓できないこと");
            Assert.Throws<Exception>(() => Shoupai.fromString("m15p456s789z567,m555=").gang("m5550"),
                "手牌にない牌で加槓できないこと");
            Assert.Throws<Exception>(() => Shoupai.fromString("m12p456s789z567,m222=").gang("m111=1"),
                "明刻がない牌で加槓できないこと");
        }

        [Test, Description("get menqian()")]
        public void TestMenqian()
        {
            Assert.IsTrue(Shoupai.fromString("m123p0s789z4567").menqian, "副露がない場合、メンゼンと判定すること");
            Assert.IsFalse(Shoupai.fromString("p0s789z4567,m123-").menqian, "副露がある場合、メンゼンと判定しないこと");
            Assert.IsTrue(Shoupai.fromString("m123p0s789,z1111").menqian, "暗カンを副露と判定しないこと");
        }

        [Test, Description("get lizhi()")]
        public void TestLizhi()
        {
            Assert.IsFalse(Shoupai.fromString("_____________").lizhi, "配牌時はリーチ状態でないこと");
            Assert.IsTrue(Shoupai.fromString("_____________").zimo("z7").dapai("z7_*").lizhi, "リーチ宣言によりリーチ状態になること");
        }


        [Test, Description("get_dapai(check = true)")]
        public void TestGetDapai()
        {
            Assert.IsNull(Shoupai.fromString("m123p406s789z4567").get_dapai(), "ツモ直後または副露直後以外は打牌できないこと");
            Assert.IsNull(Shoupai.fromString("m123p406s789z2,z111+").get_dapai(), "ツモ直後または副露直後以外は打牌できないこと");
            Assert.IsNull(Shoupai.fromString("_____________").get_dapai(), "ツモ直後または副露直後以外は打牌できないこと");
            Assert.IsNull(Shoupai.fromString("__________,z111+").get_dapai(), "ツモ直後または副露直後以外は打牌できないこと");

            CollectionAssert.AreEqual(
                new string[] { "m1", "m2", "m3", "p4", "p0", "p6", "s7", "s8", "s9", "z1", "z2", "z3_" },
                Shoupai.fromString("m123p406s789z11123").get_dapai(), "ツモ直後は任意の手牌を打牌できること(メンゼン手)");

            CollectionAssert.AreEqual(
                new string[] { "m1", "m2", "m3", "p4", "p0", "p6", "s7", "s8", "s9", "z1", "z2_" },
                Shoupai.fromString("m123p406s789z12,z111+").get_dapai(), "ツモ直後は任意の手牌を打牌できること(副露手)");

            CollectionAssert.AreEqual(new string[] { "m1_" },
                Shoupai.fromString("m123p456s789z1234m1*").get_dapai(), "リーチ後はツモ切りしかできないこと");

            CollectionAssert.AreEqual(
                new string[] { "m1", "m2", "m3", "p4", "p0", "p5", "s7", "s8", "s9", "z1", "z2", "z3_" },
                Shoupai.fromString("m123p405s789z11123").get_dapai(), "赤牌を単独の牌として区別すること");

            CollectionAssert.AreEqual(
                new string[] { "m1", "m2", "m3", "p4", "p5", "s7", "s8", "s9", "z1", "z2", "z3", "p0_" },
                Shoupai.fromString("m123p45s789z11123p0").get_dapai(), "赤牌を単独の牌として区別すること");

            CollectionAssert.AreEqual(
                new string[] { "m1", "m2", "m3", "p4", "p5", "s7", "s8", "s9", "z1", "z2", "z3", "p5_" },
                Shoupai.fromString("m123p45s789z11123p5").get_dapai(), "手出しとツモ切りを区別すること");

            CollectionAssert.AreEqual(
                new string[] { "m1", "m2", "m3", "p4", "p0", "p5", "s7", "s8", "s9", "z1", "z2", "p0_" },
                Shoupai.fromString("m123p405s789z1112p0").get_dapai(), "手出しとツモ切りを区別すること");

            CollectionAssert.AreEqual(new string[] { },
                Shoupai.fromString("______________").get_dapai(), "伏せ牌のみの手牌では空配列を返すこと");

            CollectionAssert.AreEqual(new string[] { },
                Shoupai.fromString("___________,m123-,").get_dapai(), "伏せ牌のみの手牌では空配列を返すこと");

            CollectionAssert.AreEqual(new string[] { "m5", "p4", "p0", "p6", "s7", "s8", "s9", "z2", "z3" },
                Shoupai.fromString("m145p406s789z23,m1-23,").get_dapai(), "喰替えとなる打牌が許されないこと(両面鳴き)");

            CollectionAssert.AreEqual(new string[] { "m5", "p4", "p0", "p6", "s7", "s8", "s9", "z2", "z3" },
                Shoupai.fromString("m145p406s789z23,m234-,").get_dapai(), "喰替えとなる打牌が許されないこと(両面鳴き)");

            CollectionAssert.AreEqual(new string[] { "m1", "m2", "m3", "p2", "p8", "s7", "s8", "s9", "z2", "z3" },
                Shoupai.fromString("m123p258s789z23,p45-6,").get_dapai(), "喰替えとなる打牌が許されないこと(嵌張鳴き)");

            CollectionAssert.AreEqual(new string[] { "m1", "m2", "m3", "p4", "p5", "p6", "s4", "s6", "z2", "z3" },
                Shoupai.fromString("m123p456s467z23,s7-89,").get_dapai(), "喰替えとなる打牌が許されないこと(辺張鳴き)");

            CollectionAssert.AreEqual(new string[] { "m1", "m2", "m3", "p4", "p5", "p6", "s7", "s8", "s9", "z2" },
                Shoupai.fromString("m123p456s789z12,z111+,").get_dapai(), "喰替えとなる打牌が許されないこと(ポン)");

            CollectionAssert.AreEqual(new string[] { "m6", "p4", "p5", "p6", "s7", "s8", "s9", "z1", "z2" },
                Shoupai.fromString("m256p456s789z12,m340-,").get_dapai(), "喰替えとなる打牌が許されないこと(赤牌入りの鳴き)");

            CollectionAssert.AreEqual(new string[] { "m6", "p4", "p5", "p6", "s7", "s8", "s9", "z1", "z2" },
                Shoupai.fromString("m206p456s789z12,m345-,").get_dapai(), "喰替えとなる打牌が許されないこと(赤牌打牌)");

            CollectionAssert.AreEqual(new string[] { "m2", "p1", "s1", "s2", "s6", "s7", "s8" },
                Shoupai.fromString("m25p1s12678,z666+,m550-,").get_dapai(), "赤牌によるポンの際に正しく喰い替え判定すること");

            CollectionAssert.AreEqual(new string[] { },
                Shoupai.fromString("m14,p456-,s789-,z111+,m234-,").get_dapai(), "喰替えのため打牌できない場合があること");
            CollectionAssert.AreEqual(new string[] { },
                Shoupai.fromString("m14,p456-,s789-,z111+,m1-23,").get_dapai(), "喰替えのため打牌できない場合があること");
            CollectionAssert.AreEqual(new string[] { },
                Shoupai.fromString("m22,p456-,s789-,z111+,m12-3,").get_dapai(), "喰替えのため打牌できない場合があること");

            CollectionAssert.AreEqual(new string[] { "m1", "m4", "m5", "p4", "p0", "p6", "s7", "s8", "s9", "z2", "z3" },
                Shoupai.fromString("m145p406s789z23,m1-23,").get_dapai(false), "喰い替えを許すこともできること");
        }

        [Test, Description("get_chi_mianzi(p, check = true)")]
        public void TestGetChiMianzi()
        {
            Assert.IsNull(Shoupai.fromString("m123p456s789z12345").get_chi_mianzi("m1-"), "ツモ直後と副露の直後にチーできないこと");
            Assert.IsNull(Shoupai.fromString("m123p456s789z12,z333=,").get_chi_mianzi("m1-"), "ツモ直後と副露の直後にチーできないこと");
            Assert.IsNull(Shoupai.fromString("______________").get_chi_mianzi("m1-"), "ツモ直後と副露の直後にチーできないこと");

            CollectionAssert.AreEqual(new string[] { },
                Shoupai.fromString("m123p456s789z1234").get_chi_mianzi("m5-"), "チーできるメンツがない場合");
            CollectionAssert.AreEqual(new string[] { },
                Shoupai.fromString("_____________").get_chi_mianzi("m5-"), "チーできるメンツがない場合");

            CollectionAssert.AreEqual(new string[] { "m123-" },
                Shoupai.fromString("m123p456s789z1234").get_chi_mianzi("m3-"), "チーできるメンツが1つの場合");

            CollectionAssert.AreEqual(new string[] { "m123-", "m23-4" },
                Shoupai.fromString("m1234p456s789z123").get_chi_mianzi("m3-"), "チーできるメンツが2つの場合");

            CollectionAssert.AreEqual(new string[] { "m123-", "m23-4", "m3-45" },
                Shoupai.fromString("m12345p456s789z12").get_chi_mianzi("m3-"), "チーできるメンツが3つの場合");

            CollectionAssert.AreEqual(new string[] { "p40-6" },
                Shoupai.fromString("m123p456s789z1234").get_chi_mianzi("p0-"), "赤牌でチーできること");

            CollectionAssert.AreEqual(new string[] { "p3-40" },
                Shoupai.fromString("m123p34067s789z12").get_chi_mianzi("p3-"), "赤牌含みでチーできること");
            CollectionAssert.AreEqual(new string[] { "p34-0", "p4-06" },
                Shoupai.fromString("m123p34067s789z12").get_chi_mianzi("p4-"), "赤牌含みでチーできること");
            CollectionAssert.AreEqual(new string[] { "p406-", "p06-7" },
                Shoupai.fromString("m123p34067s789z12").get_chi_mianzi("p6-"), "赤牌含みでチーできること");
            CollectionAssert.AreEqual(new string[] { "p067-" },
                Shoupai.fromString("m123p34067s789z12").get_chi_mianzi("p7-"), "赤牌含みでチーできること");

            CollectionAssert.AreEqual(new string[] { "p3-40", "p3-45" },
                Shoupai.fromString("m123p340567s789z1").get_chi_mianzi("p3-"), "赤牌なしのメンツも選択すること");
            CollectionAssert.AreEqual(new string[] { "p34-0", "p34-5", "p4-06", "p4-56" },
                Shoupai.fromString("m123p340567s789z1").get_chi_mianzi("p4-"), "赤牌なしのメンツも選択すること");
            CollectionAssert.AreEqual(new string[] { "p406-", "p456-", "p06-7", "p56-7" },
                Shoupai.fromString("m123p340567s789z1").get_chi_mianzi("p6-"), "赤牌なしのメンツも選択すること");
            CollectionAssert.AreEqual(new string[] { "p067-", "p567-" },
                Shoupai.fromString("m123p340567s789z1").get_chi_mianzi("p7-"), "赤牌なしのメンツも選択すること");

            CollectionAssert.AreEqual(new string[] { "m123-" },
                Shoupai.fromString("m123p456s789z1234").get_chi_mianzi("m3_-"), "ツモ切りの牌をチーできること");

            CollectionAssert.AreEqual(new string[] { "m123-" },
                Shoupai.fromString("m123p456s789z1234").get_chi_mianzi("m3*-"), "リーチ宣言牌をチーできること");

            CollectionAssert.AreEqual(new string[] { },
                Shoupai.fromString("m123p456s789z1234*").get_chi_mianzi("m3-"), "リーチ後はチーできないこと");

            Assert.Throws<Exception>(() => Shoupai.fromString("m123p456s789z1234").get_chi_mianzi("mm-"),
                "不正な牌でチーできないこと");
            Assert.Throws<Exception>(() => Shoupai.fromString("m123p456s789z1234").get_chi_mianzi("m1"),
                "不正な牌でチーできないこと");

            CollectionAssert.AreEqual(new string[] { },
                Shoupai.fromString("m123p456s789z1234").get_chi_mianzi("z1-"), "字牌でチーできないこと");

            CollectionAssert.AreEqual(new string[] { },
                Shoupai.fromString("m123p456s789z1234").get_chi_mianzi("m1+"), "上家以外からチーできないこと");
            CollectionAssert.AreEqual(new string[] { },
                Shoupai.fromString("m123p456s789z1234").get_chi_mianzi("m1="), "上家以外からチーできないこと");
        }

        [Test, Description("get_peng_mianzi(p)")]
        public void TestGetPengMianzi()
        {
            Assert.IsNull(Shoupai.fromString("m112p456s789z12345").get_peng_mianzi("m1+"), "ツモ直後と副露の直後にポンできないこと");
            Assert.IsNull(Shoupai.fromString("m112p456s789z12,z333=,").get_peng_mianzi("m1="), "ツモ直後と副露の直後にポンできないこと");
            Assert.IsNull(Shoupai.fromString("______________").get_peng_mianzi("m1-"), "ツモ直後と副露の直後にポンできないこと");

            CollectionAssert.AreEqual(new string[] { },
                Shoupai.fromString("m123p456s789z1234").get_peng_mianzi("m1+"), "ポンできるメンツがない場合");
            CollectionAssert.AreEqual(new string[] { },
                Shoupai.fromString("_____________").get_peng_mianzi("m1="), "ポンできるメンツがない場合");

            CollectionAssert.AreEqual(new string[] { "m111+" },
                Shoupai.fromString("m112p456s789z1234").get_peng_mianzi("m1+"), "下家からポンできること");

            CollectionAssert.AreEqual(new string[] { "p444=" },
                Shoupai.fromString("m123p445s789z1234").get_peng_mianzi("p4="), "対面からポンできること");

            CollectionAssert.AreEqual(new string[] { "s777-" },
                Shoupai.fromString("m123p345s778z1234").get_peng_mianzi("s7-"), "上家からポンできること");

            CollectionAssert.AreEqual(new string[] { "p550+" },
                Shoupai.fromString("m123p455s789z1234").get_peng_mianzi("p0+"), "赤牌でポンできること");
            CollectionAssert.AreEqual(new string[] { "p500+" },
                Shoupai.fromString("m123p405s789z1234").get_peng_mianzi("p0+"), "赤牌でポンできること");
            CollectionAssert.AreEqual(new string[] { "p000+" },
                Shoupai.fromString("m123p400s789z1234").get_peng_mianzi("p0+"), "赤牌でポンできること");

            CollectionAssert.AreEqual(new string[] { "p505=", "p555=" },
                Shoupai.fromString("m123p055s789z1234").get_peng_mianzi("p5="), "赤牌なしのメンツも選択すること");
            CollectionAssert.AreEqual(new string[] { "p005=", "p505=" },
                Shoupai.fromString("m123p005s789z1234").get_peng_mianzi("p5="), "赤牌なしのメンツも選択すること");
            CollectionAssert.AreEqual(new string[] { "p005=" },
                Shoupai.fromString("m123p000s789z1234").get_peng_mianzi("p5="), "赤牌なしのメンツも選択すること");

            CollectionAssert.AreEqual(new string[] { "m111+" },
                Shoupai.fromString("m112p456s789z1234").get_peng_mianzi("m1_+"), "ツモ切りの牌をポンできること");

            CollectionAssert.AreEqual(new string[] { "m111+" },
                Shoupai.fromString("m112p456s789z1234").get_peng_mianzi("m1*+"), "リーチ宣言牌をポンできること");

            CollectionAssert.AreEqual(new string[] { "m111+" },
                Shoupai.fromString("m112p456s789z1234").get_peng_mianzi("m1_*+"), "ツモ切りリーチの宣言牌をポンできること");

            CollectionAssert.AreEqual(new string[] { },
                Shoupai.fromString("m112p456s789z1234*").get_peng_mianzi("m1+"), "リーチ後はポンできないこと");

            Assert.Throws<Exception>(() => Shoupai.fromString("m123p456s789z1234").get_peng_mianzi("mm+"),
                "不正な牌でポンできないこと");
            Assert.Throws<Exception>(() => Shoupai.fromString("m112p456s789z1234").get_peng_mianzi("m1"),
                "不正な牌でポンできないこと");
        }

        [Test, Description("get_gang_mianzi(p)")]
        public void TestGetGangMianzi()
        {
            Assert.IsNull(Shoupai.fromString("m111p456s789z12345").get_gang_mianzi("m1+"), "ツモ直後と副露の直後に大明槓できないこと");
            Assert.IsNull(Shoupai.fromString("m111p456s789z12,z333=,").get_gang_mianzi("m1+"), "ツモ直後と副露の直後に大明槓できないこと");
            Assert.IsNull(Shoupai.fromString("______________").get_gang_mianzi("m1-"), "ツモ直後と副露の直後に大明槓できないこと");

            CollectionAssert.AreEqual(new string[] { },
                Shoupai.fromString("m123p456s789z1122").get_gang_mianzi("z1+"), "大明槓できるメンツがない場合");
            CollectionAssert.AreEqual(new string[] { },
                Shoupai.fromString("_____________").get_gang_mianzi("z1="), "大明槓できるメンツがない場合");

            CollectionAssert.AreEqual(new string[] { "m1111+" },
                Shoupai.fromString("m111p456s789z1234").get_gang_mianzi("m1+"), "下家から大明槓できること");

            CollectionAssert.AreEqual(new string[] { "p4444=" },
                Shoupai.fromString("m123p444s789z1234").get_gang_mianzi("p4="), "対面から大明槓できること");

            CollectionAssert.AreEqual(new string[] { "s7777-" },
                Shoupai.fromString("m123p456s777z1234").get_gang_mianzi("s7-"), "上家から大明槓できること");

            CollectionAssert.AreEqual(new string[] { "p5550+" },
                Shoupai.fromString("m123p555s789z1234").get_gang_mianzi("p0+"), "赤牌で大明槓できること");

            CollectionAssert.AreEqual(new string[] { "p5505+" },
                Shoupai.fromString("m123p055s789z1234").get_gang_mianzi("p5+"), "赤牌入りの大明槓ができること");
            CollectionAssert.AreEqual(new string[] { "p5005+" },
                Shoupai.fromString("m123p005s789z1234").get_gang_mianzi("p5+"), "赤牌入りの大明槓ができること");
            CollectionAssert.AreEqual(new string[] { "p0005+" },
                Shoupai.fromString("m123p000s789z1234").get_gang_mianzi("p5+"), "赤牌入りの大明槓ができること");

            CollectionAssert.AreEqual(new string[] { "m1111+" },
                Shoupai.fromString("m111p456s789z1234").get_gang_mianzi("m1_+"), "ツモ切りの牌を大明槓できること");

            CollectionAssert.AreEqual(new string[] { "m1111+" },
                Shoupai.fromString("m111p456s789z1234").get_gang_mianzi("m1*+"), "リーチ宣言牌を大明槓できること");

            CollectionAssert.AreEqual(new string[] { "m1111+" },
                Shoupai.fromString("m111p456s789z1234").get_gang_mianzi("m1_*+"), "ツモ切りリーチの宣言牌を大明槓できること");

            CollectionAssert.AreEqual(new string[] { },
                Shoupai.fromString("m111p456s789z1234*").get_gang_mianzi("m1+"), "リーチ後は大明槓できないこと");

            Assert.Throws<Exception>(() => Shoupai.fromString("m111p555s999z1234").get_gang_mianzi("mm+"),
                "不正な牌で大明槓できないこと");
            Assert.Throws<Exception>(() => Shoupai.fromString("m111p555s999z1234").get_gang_mianzi("m1"),
                "不正な牌で大明槓できないこと");

            Assert.IsNull(Shoupai.fromString("m1111p555s999z123").get_gang_mianzi(), "打牌と副露の直後には暗槓できないこと");
            Assert.IsNull(Shoupai.fromString("m1111p555s999,z333=").get_gang_mianzi(), "打牌と副露の直後には暗槓できないこと");
            Assert.IsNull(Shoupai.fromString("m11112p555s999,z333=,").get_gang_mianzi(), "打牌と副露の直後には暗槓できないこと");
            Assert.IsNull(Shoupai.fromString("_____________").get_gang_mianzi(), "打牌と副露の直後には暗槓できないこと");

            CollectionAssert.AreEqual(new string[] { },
                Shoupai.fromString("m123p456s789z12345").get_gang_mianzi(), "暗槓できるメンツがない場合");
            CollectionAssert.AreEqual(new string[] { },
                Shoupai.fromString("______________").get_gang_mianzi(), "暗槓できるメンツがない場合");

            CollectionAssert.AreEqual(new string[] { "m1111" },
                Shoupai.fromString("m1111p456s789z1234").get_gang_mianzi(), "萬子で暗槓できること");

            CollectionAssert.AreEqual(new string[] { "p4444" },
                Shoupai.fromString("m123p4444s789z1234").get_gang_mianzi(), "筒子で暗槓できること");

            CollectionAssert.AreEqual(new string[] { "s7777" },
                Shoupai.fromString("m123p456s7777z1234").get_gang_mianzi(), "索子で暗槓できること");

            CollectionAssert.AreEqual(new string[] { "z1111" },
                Shoupai.fromString("m123p456s789z11112").get_gang_mianzi(), "字牌で暗槓できること");

            CollectionAssert.AreEqual(new string[] { "p5550" },
                Shoupai.fromString("m123p0555s789z1234").get_gang_mianzi(), "赤牌入りで暗槓できること");
            CollectionAssert.AreEqual(new string[] { "p5500" },
                Shoupai.fromString("m123p0055s789z1234").get_gang_mianzi(), "赤牌入りで暗槓できること");
            CollectionAssert.AreEqual(new string[] { "p5000" },
                Shoupai.fromString("m123p0005s789z1234").get_gang_mianzi(), "赤牌入りで暗槓できること");
            CollectionAssert.AreEqual(new string[] { "p0000" },
                Shoupai.fromString("m123p0000s789z1234").get_gang_mianzi(), "赤牌入りで暗槓できること");

            CollectionAssert.AreEqual(new string[] { "m1111" },
                Shoupai.fromString("m111p456s789z1122m1*").get_gang_mianzi(), "リーチ後も暗槓できること");

            CollectionAssert.AreEqual(new string[] { },
                Shoupai.fromString("m111123p456s78z11m4*").get_gang_mianzi(), "リーチ後は送り槓できないこと");

            CollectionAssert.AreEqual(new string[] { "m1111", "z1111" },
                Shoupai.fromString("m1111p456s789z1111").get_gang_mianzi(), "暗槓できるメンツが複数の場合");

            Assert.IsNull(Shoupai.fromString("m1p555s999z123,m111-").get_gang_mianzi(), "打牌と副露の直後には加槓できないこと");
            Assert.IsNull(Shoupai.fromString("m1p555s999,z333=,m111-").get_gang_mianzi(), "打牌と副露の直後には加槓できないこと");
            Assert.IsNull(Shoupai.fromString("m12p555s999,z333=,m111-,").get_gang_mianzi(), "打牌と副露の直後には加槓できないこと");
            Assert.IsNull(Shoupai.fromString("__________,m111-,").get_gang_mianzi(), "打牌と副露の直後には加槓できないこと");

            CollectionAssert.AreEqual(new string[] { },
                Shoupai.fromString("m123p456s789z12,z777+").get_gang_mianzi(), "加槓できるメンツがない場合");
            CollectionAssert.AreEqual(new string[] { },
                Shoupai.fromString("___________,z777+").get_gang_mianzi(), "加槓できるメンツがない場合");

            CollectionAssert.AreEqual(new string[] { "m111+1" },
                Shoupai.fromString("m1p456s789z1234,m111+").get_gang_mianzi(), "萬子で加槓できること");

            CollectionAssert.AreEqual(new string[] { "p444=4" },
                Shoupai.fromString("m123p4s789z1234,p444=").get_gang_mianzi(), "筒子で加槓できること");

            CollectionAssert.AreEqual(new string[] { "s777-7" },
                Shoupai.fromString("m123p456s7z1234,s777-").get_gang_mianzi(), "索子で加槓できること");

            CollectionAssert.AreEqual(new string[] { "z111+1" },
                Shoupai.fromString("m123p456s789z12,z111+").get_gang_mianzi(), "字牌で加槓できること");

            CollectionAssert.AreEqual(new string[] { "p555=0" },
                Shoupai.fromString("m123p0s789z1234,p555=").get_gang_mianzi(), "赤牌で加槓できること");

            CollectionAssert.AreEqual(new string[] { "p550-5" },
                Shoupai.fromString("m123p5s789z1234,p550-").get_gang_mianzi(), "赤牌入りで加槓できること");

            CollectionAssert.AreEqual(new string[] { },
                Shoupai.fromString("p456s789z1234m1*,m111+").get_gang_mianzi(), "リーチ後は加槓できないこと");

            CollectionAssert.AreEqual(new string[] { "m111+1", "p444=4" },
                Shoupai.fromString("m1p4s789z123,m111+,p444=").get_gang_mianzi(), "加槓できるメンツが複数の場合");

            CollectionAssert.AreEqual(new string[] { "m111+1", "z1111" },
                Shoupai.fromString("m1p456s789z1111,m111+").get_gang_mianzi(), "暗槓と加槓が同時にできる場合");
        }
    }
}
