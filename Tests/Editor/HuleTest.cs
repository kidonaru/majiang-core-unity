using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Majiang.Test
{
    public class HuleTest
    {
        // {"in":{"shoupai":"m345p40677s67s8,m34-0","param":{"zhuangfeng":0,"menfeng":3,"hupai":{},"baopai":["s4"],"fubaopai":[],"jicun":{"changbang":0,"lizhibang":0}}},"out":{"hupai":[{"fanshu":1,"name":"断幺九"},{"fanshu":2,"name":"赤ドラ"}],"fu":30,"fanshu":3,"damanguan":null,"defen":4000,"fenpei":[-2000,-1000,-1000,4000]}}
        [Serializable]
        public class HuleTestInput
        {
            public string shoupai;
            public HuleParam param;
            public string rongpai;
        }

        [Serializable]
        public class HuleTestEntity
        {
            public HuleTestInput @in;
            public HuleResult @out;
        }

        private List<HuleTestEntity> data;

        [SetUp]
        public void SetUp()
        {
            data = TestUtil.LoadDataFromJsonl<HuleTestEntity>("Packages/com.kidonaru.majiang-core-unity/Tests/Editor/data/hule.jsonl");
        }

        [Test, Description("hule_mianzi(shoupai, rongpai)")]
        public void TestHuleMianzi()
        {
            CollectionAssert.AreEquivalent(new List<List<string>> { new List<string> { "z22_!", "m123", "p555", "s789", "z111" } }, Majiang.Util.hule_mianzi(Majiang.Shoupai.fromString("m123p055s789z11122"), null), "一般手(ツモ和了)");
            CollectionAssert.AreEquivalent(new List<List<string>> { new List<string> { "z22+!", "m123", "p555", "s789", "z111" } }, Majiang.Util.hule_mianzi(Majiang.Shoupai.fromString("m123p055s789z1112"), "z2+"), "一般手(ロン和了)");
            CollectionAssert.AreEquivalent(new List<List<string>> { new List<string> { "z22=!", "m123", "p555", "z111", "s7-89" } }, Majiang.Util.hule_mianzi(Majiang.Shoupai.fromString("m123p055z1112,s7-89"), "z2="), "一般手(副露あり)");
            CollectionAssert.AreEquivalent(new List<List<string>> { new List<string> { "m22", "m55-!", "p44", "p66", "s11", "s99", "z33" } }, Majiang.Util.hule_mianzi(Majiang.Shoupai.fromString("m225p4466s1199z33"), "m0-"), "七対子形");
            CollectionAssert.AreEquivalent(new List<List<string>> { new List<string> { "z77", "m1_!", "m9", "p1", "p9", "s1", "s9", "z1", "z2", "z3", "z4", "z5", "z6" } }, Majiang.Util.hule_mianzi(Majiang.Shoupai.fromString("m9p19s19z12345677m1"), null), "国士無双形(ツモ)");
            CollectionAssert.AreEquivalent(new List<List<string>> { new List<string> { "m99+!", "m1", "p1", "p9", "s1", "s9", "z1", "z2", "z3", "z4", "z5", "z6", "z7" } }, Majiang.Util.hule_mianzi(Majiang.Shoupai.fromString("m19p19s19z1234567"), "m9+"), "国士無双形(13面待ちロン)");
            CollectionAssert.AreEquivalent(new List<List<string>> { new List<string> { "m55=!", "m111", "m234", "m678", "m999" }, new List<string> { "m11123456789995=!" } }, Majiang.Util.hule_mianzi(Majiang.Shoupai.fromString("m1112345678999"), "m0="), "九蓮宝燈形");
            CollectionAssert.AreEquivalent(new List<List<string>> { }, Majiang.Util.hule_mianzi(Majiang.Shoupai.fromString("m123p055s789z1122")), "和了形以外(少牌)");
            CollectionAssert.AreEquivalent(new List<List<string>> { }, Majiang.Util.hule_mianzi(Majiang.Shoupai.fromString("___m123p055z2,s7-89"), "z2="), "和了形以外(三面子)");
            CollectionAssert.AreEquivalent(new List<List<string>> { }, Majiang.Util.hule_mianzi(Majiang.Shoupai.fromString("m22")), "和了形以外(一対子)");
            CollectionAssert.AreEquivalent(new List<List<string>> { }, Majiang.Util.hule_mianzi(Majiang.Shoupai.fromString("m19p19s19z123456"), "z7="), "和了形以外(国士無双テンパイ)");
            CollectionAssert.AreEquivalent(new List<List<string>> { }, Majiang.Util.hule_mianzi(Majiang.Shoupai.fromString("m111234567899"), "m9="), "和了形以外(九蓮宝燈テンパイ)");
            CollectionAssert.AreEquivalent(new List<List<string>> { new List<string> { "z11_!", "m123", "m111", "p789", "p999" } }, Majiang.Util.hule_mianzi(Majiang.Shoupai.fromString("m111123p789999z1z1"), null), "複数の和了形としない(順子優先)");
            CollectionAssert.AreEquivalent(new List<List<string>> { new List<string> { "s88_!", "m234", "m234", "p567", "p567" }, new List<string> { "m22", "m33", "m44", "p55", "p66", "p77", "s88_!" } }, Majiang.Util.hule_mianzi(Majiang.Shoupai.fromString("m223344p556677s88")), "複数の和了形(二盃口か七対子か)");
            CollectionAssert.AreEquivalent(new List<List<string>> { new List<string> { "p99", "m123", "m123", "m123", "p7_!89" }, new List<string> { "p99", "m111", "m222", "m333", "p7_!89" } }, Majiang.Util.hule_mianzi(Majiang.Shoupai.fromString("m111222333p89997")), "複数の和了形(順子か刻子か)");
            CollectionAssert.AreEquivalent(new List<List<string>> { new List<string> { "m22", "m3_!45", "m345", "p234", "s234" }, new List<string> { "m55", "m23_!4", "m234", "p234", "s234" } }, Majiang.Util.hule_mianzi(Majiang.Shoupai.fromString("m2234455p234s234m3")), "複数の和了形(雀頭の選択、平和かサンショクか)");
            CollectionAssert.AreEquivalent(new List<List<string>> { new List<string> { "s33", "m1_!23", "p567", "s345", "s666" }, new List<string> { "s66", "m1_!23", "p567", "s333", "s456" } }, Majiang.Util.hule_mianzi(Majiang.Shoupai.fromString("m23p567s33345666m1")), "複数の和了形(暗刻を含む形)");
            CollectionAssert.AreEquivalent(new List<List<string>> { new List<string> { "s99", "s111", "s2_!34", "s456", "s789" }, new List<string> { "s11134456789992_!" } }, Majiang.Util.hule_mianzi(Majiang.Shoupai.fromString("s1113445678999s2")), "複数の和了形(九蓮宝燈形)");
            CollectionAssert.AreEquivalent(new List<List<string>> { new List<string> { "s99", "s456", "s78_!9", "z444", "s8888" } }, Majiang.Util.hule_mianzi(Majiang.Shoupai.fromString("s4067999z444s8,s8888")), "バグ: 暗槓しているの5枚目の牌で和了");
        }

        private HuleParam param(HuleInput input = null)
        {
            return Util.hule_param(input);
        }

        [Test, Description("hule(shoupai, rongpai, param)")]
        public void TestHule()
        {
            Assert.Throws<Exception>(() => Majiang.Util.hule(Majiang.Shoupai.fromString(), "m1", param()), "パラメータ不正");
            Assert.IsNull(Majiang.Util.hule(Majiang.Shoupai.fromString(), null, param()), "和了形以外");
            Assert.AreEqual(20, Majiang.Util.hule(Majiang.Shoupai.fromString("m345567p234s33789"), null, param()).fu, "符計算: 平和ツモは20符");
            Assert.AreEqual(30, Majiang.Util.hule(Majiang.Shoupai.fromString("m345567p234s3378"), "s9=", param()).fu, "符計算: 平和ロンは30符");
            Assert.AreEqual(30, Majiang.Util.hule(Majiang.Shoupai.fromString("m112233p456z33s78"), "s9=", param()).fu, "符計算: オタ風の雀頭に符はつかない");
            Assert.AreEqual(40, Majiang.Util.hule(Majiang.Shoupai.fromString("m112233p456z11s78"), "s9=", param()).fu, "符計算: 場風の雀頭は2符");
            Assert.AreEqual(40, Majiang.Util.hule(Majiang.Shoupai.fromString("m112233p456z22s78"), "s9=", param()).fu, "符計算: 自風の雀頭は2符");
            Assert.AreEqual(40, Majiang.Util.hule(Majiang.Shoupai.fromString("m112233p456z55s78"), "s9=", param()).fu, "符計算: 三元牌の雀頭は2符");
            Assert.AreEqual(50, Majiang.Util.hule(Majiang.Shoupai.fromString("m112233z444z11s78"), "s9=", param(new HuleInput { menfeng = 0 })).fu, "符計算: 連風牌の雀頭は4符");
            Assert.AreEqual(30, Majiang.Util.hule(Majiang.Shoupai.fromString("m123z11m88,p888+,s888-"), "m8=", param(new HuleInput { menfeng = 0 })).fu, "符計算: 中張牌の明刻は2符");
            Assert.AreEqual(40, Majiang.Util.hule(Majiang.Shoupai.fromString("m123p22s99,z222+,p111-"), "s9=", param()).fu, "符計算: 幺九牌の明刻は4符");
            Assert.AreEqual(50, Majiang.Util.hule(Majiang.Shoupai.fromString("z33p222777s888m23"), "m4=", param()).fu, "符計算: 中張牌の暗刻は4符");
            Assert.AreEqual(60, Majiang.Util.hule(Majiang.Shoupai.fromString("s33p111999z555m23"), "m4=", param()).fu, "符計算: 幺九牌の暗刻は8符");
            Assert.AreEqual(40, Majiang.Util.hule(Majiang.Shoupai.fromString("p33m22245667,s444+4"), "m8=", param()).fu, "符計算: 中張牌の明槓は8符");
            Assert.AreEqual(40, Majiang.Util.hule(Majiang.Shoupai.fromString("p33m23445667,z6666-"), "m8=", param()).fu, "符計算: 幺九牌の明槓は16符");
            Assert.AreEqual(50, Majiang.Util.hule(Majiang.Shoupai.fromString("p33m23445667,s4444"), "m8=", param()).fu, "符計算: 中張牌の暗槓は16符");
            Assert.AreEqual(70, Majiang.Util.hule(Majiang.Shoupai.fromString("p33m23445667,z7777"), "m8=", param()).fu, "符計算: 幺九牌の暗槓は32符");
            Assert.AreEqual(40, Majiang.Util.hule(Majiang.Shoupai.fromString("p33m222s222345,s888-"), null, param()).fu, "符計算: ツモ和了は2符加算");
            Assert.AreEqual(40, Majiang.Util.hule(Majiang.Shoupai.fromString("m222s222345p3,s888-"), "p3=", param()).fu, "符計算: 単騎待ちは2符加算");
            Assert.AreEqual(40, Majiang.Util.hule(Majiang.Shoupai.fromString("p33m222s22235,s888-"), "s4=", param()).fu, "符計算: 嵌張待ちは2符加算");
            Assert.AreEqual(40, Majiang.Util.hule(Majiang.Shoupai.fromString("p33z111m12389,s222-"), "m7=", param()).fu, "符計算: 辺張待ちは2符加算");
            Assert.AreEqual(30, Majiang.Util.hule(Majiang.Shoupai.fromString("m22p345678s34,s67-8"), "s5=", param()).fu, "符計算: 喰い平和は30符");
            Assert.AreEqual(25, Majiang.Util.hule(Majiang.Shoupai.fromString("m2255p88s1166z1155"), null, param()).fu, "符計算: 七対子は25符");
            Assert.IsNull(Majiang.Util.hule(Majiang.Shoupai.fromString("m19p19s1z12345677s9"), null, param()).fu, "符計算: 国士無双は符なし");
            Assert.IsNull(Majiang.Util.hule(Majiang.Shoupai.fromString("m11123456789995"), null, param()).fu, "符計算: 九蓮宝燈は符なし");
            Assert.IsNull(Majiang.Util.hule(Majiang.Shoupai.fromString("m344556s24678z66"), "s3=", param()).hupai, "和了役: 役なし");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "立直", fanshu = "1" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m344556s24678z66*"), "s3=", param(new HuleInput { lizhi = 1 })).hupai, "和了役: 立直");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "ダブル立直", fanshu = "2" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m344556s24678z66*"), "s3=", param(new HuleInput { lizhi = 2 })).hupai, "和了役: ダブル立直");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "立直", fanshu = "1" }, new Yaku { name = "一発", fanshu = "1" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m344556s24678z66*"), "s3=", param(new HuleInput { lizhi = 1, yifa = true })).hupai, "和了役: 立直・一発");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "海底摸月", fanshu = "1" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m344556s24z66s3,s6-78"), null, param(new HuleInput { haidi = 1 })).hupai, "和了役: 海底摸月");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "河底撈魚", fanshu = "1" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m344556s24678z66"), "s3=", param(new HuleInput { haidi = 2 })).hupai, "和了役: 河底撈魚");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "嶺上開花", fanshu = "1" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m344556s24z66s3,s777+7"), null, param(new HuleInput { lingshang = true })).hupai, "和了役: 嶺上開花");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "槍槓", fanshu = "1" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m344556s24678z66"), "s3=", param(new HuleInput { qianggang = true })).hupai, "和了役: 槍槓");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "天和", fanshu = "*" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m344556s24678z66s3"), null, param(new HuleInput { tianhu = 1 })).hupai, "和了役: 天和");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "地和", fanshu = "*" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m344556s24678z66s3"), null, param(new HuleInput { tianhu = 2 })).hupai, "和了役: 地和");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "門前清自摸和", fanshu = "1" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m344556s24678z66s3"), null, param(new HuleInput { })).hupai, "和了役: 門前清自摸和");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "場風 東", fanshu = "1" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m345567s3378z111"), "s9=", param(new HuleInput { })).hupai, "和了役: 場風 東");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "自風 西", fanshu = "1" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m345567s33789,z333+"), null, param(new HuleInput { menfeng = 2 })).hupai, "和了役: 自風 西");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "場風 南", fanshu = "1" }, new Yaku { name = "自風 南", fanshu = "1" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m345567s33z22,s789-"), "z2=", param(new HuleInput { zhuangfeng = 1 })).hupai, "和了役: 連風牌 南");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "翻牌 白", fanshu = "1" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m345567s33789,z555+5"), null, param(new HuleInput { })).hupai, "和了役: 翻牌 白");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "翻牌 發", fanshu = "1" }, new Yaku { name = "翻牌 中", fanshu = "1" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m345567s33,z6666+,z7777"), null, param(new HuleInput { })).hupai, "和了役: 翻牌 發・中");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "平和", fanshu = "1" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("z33m234456p78s123"), "p9=", param(new HuleInput { })).hupai, "和了役: 平和");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "門前清自摸和", fanshu = "1" }, new Yaku { name = "平和", fanshu = "1" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("z33m234456p78s123p9"), null, param(new HuleInput { })).hupai, "和了役: 平和・ツモ");
            Assert.IsNull(Majiang.Util.hule(Majiang.Shoupai.fromString("z33m234456p78,s1-23"), "p9=", param(new HuleInput { })).hupai, "和了役: 喰い平和(役なし)");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "断幺九", fanshu = "1" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m22555p234s78,p777-"), "s6=", param(new HuleInput { })).hupai, "和了役: 断幺九");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "断幺九", fanshu = "1" }, new Yaku { name = "七対子", fanshu = "2" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m2255p4488s33667"), "s7=", param(new HuleInput { })).hupai, "和了役: 断幺九(七対子形)");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "一盃口", fanshu = "1" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m33455p111s33789"), "m4=", param(new HuleInput { })).hupai, "和了役: 一盃口");
            Assert.IsNull(Majiang.Util.hule(Majiang.Shoupai.fromString("m33455p111s33,s78-9"), "m4=", param(new HuleInput { })).hupai, "和了役: 喰い一盃口(役なし)");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "三色同順", fanshu = "2" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m567p567s2256799"), "s9=", param(new HuleInput { })).hupai, "和了役: 三色同順");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "三色同順", fanshu = "1" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m567s2256799,p56-7"), "s9=", param(new HuleInput { })).hupai, "和了役: 三色同順(喰い下がり)");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "一気通貫", fanshu = "2" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m12456789s33789"), "m3=", param(new HuleInput { })).hupai, "和了役: 一気通貫");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "一気通貫", fanshu = "1" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m12789s33789,m4-56"), "m3=", param(new HuleInput { })).hupai, "和了役: 一気通貫(喰い下がり)");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "混全帯幺九", fanshu = "2" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m123999p789z33s12"), "s3=", param(new HuleInput { })).hupai, "和了役: 混全帯幺九");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "混全帯幺九", fanshu = "1" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m123p789z33s12,m999+"), "s3=", param(new HuleInput { })).hupai, "和了役: 混全帯幺九(喰い下がり)");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "七対子", fanshu = "2" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m115599p2233s8z22"), "s8=", param(new HuleInput { })).hupai, "和了役: 七対子");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "対々和", fanshu = "2" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m55888z333s22,p111="), "s2=", param(new HuleInput { })).hupai, "和了役: 対々和");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "三暗刻", fanshu = "2" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("p99s111m555,p345-,s3333"), null, param(new HuleInput { })).hupai, "和了役: 三暗刻");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "三槓子", fanshu = "2" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("p11m45,s2222+,m888=8,z4444"), "m3=", param(new HuleInput { })).hupai, "和了役: 三槓子");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "三色同刻", fanshu = "2" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("s12377m22,p222-,s222-"), "m2=", param(new HuleInput { })).hupai, "和了役: 三色同刻");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "対々和", fanshu = "2" }, new Yaku { name = "混老頭", fanshu = "2" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("z11p11199,m111=,z333+"), "p9=", param(new HuleInput { })).hupai, "和了役: 混老頭(対々和形)");

            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "七対子", fanshu = "2" }, new Yaku { name = "混老頭", fanshu = "2" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m1199p11s99z11335"), "z5=", param(new HuleInput { })).hupai, "和了役: 混老頭(七対子形)");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "翻牌 白", fanshu = "1" }, new Yaku { name = "翻牌 發", fanshu = "1" }, new Yaku { name = "小三元", fanshu = "2" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("z55577m567p22,z666-"), "p2=", param(new HuleInput { })).hupai, "和了役: 小三元");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "混一色", fanshu = "3" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m111234789z1133"), "z3=", param(new HuleInput { })).hupai, "和了役: 混一色");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "混一色", fanshu = "2" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("z11333p23478,p111+"), "p9=", param(new HuleInput { })).hupai, "和了役: 混一色(喰い下がり)");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "七対子", fanshu = "2" }, new Yaku { name = "混一色", fanshu = "3" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("s11224488z22557"), "z7=", param(new HuleInput { })).hupai, "和了役: 混一色(七対子形)");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "純全帯幺九", fanshu = "3" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m11s123p789s789m99"), "m9=", param(new HuleInput { })).hupai, "和了役: 純全帯幺九");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "純全帯幺九", fanshu = "2" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m11s123p789s78,m999="), "s9=", param(new HuleInput { })).hupai, "和了役: 純全帯幺九(喰い下がり)");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "二盃口", fanshu = "3" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m223344p667788s9"), "s9=", param(new HuleInput { })).hupai, "和了役: 二盃口");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "二盃口", fanshu = "3" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m222233334444s9"), "s9=", param(new HuleInput { })).hupai, "和了役: 二盃口(4枚使い)");
            Assert.IsNull(Majiang.Util.hule(Majiang.Shoupai.fromString("m223344p678s9,p678-"), "s9=", param(new HuleInput { })).hupai, "和了役: 喰い二盃口(役なし)");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "清一色", fanshu = "6" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m1113456677778"), "m9=", param(new HuleInput { })).hupai, "和了役: 清一色");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "清一色", fanshu = "5" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("p2344555,p12-3,p7-89"), "p1=", param(new HuleInput { })).hupai, "和了役: 清一色(喰い下がり)");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "七対子", fanshu = "2" }, new Yaku { name = "清一色", fanshu = "6" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("s1122445577889"), "s9=", param(new HuleInput { })).hupai, "和了役: 清一色(七対子形)");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "国士無双", fanshu = "*" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m119p19s19z1234567"), null, param(new HuleInput { })).hupai, "和了役: 国士無双");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "国士無双十三面", fanshu = "**" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m19p19s19z1234567m1"), null, param(new HuleInput { })).hupai, "和了役: 国士無双十三面");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "四暗刻", fanshu = "*" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m33m111p333s777z111"), null, param(new HuleInput { })).hupai, "和了役: 四暗刻");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "四暗刻単騎", fanshu = "**" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m111p333s777z111m3"), "m3=", param(new HuleInput { })).hupai, "和了役: 四暗刻単騎");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "大三元", fanshu = "*" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("z555m456p22z66,z777+"), "z6=", param(new HuleInput { })).hupai, "和了役: 大三元");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "大三元", fanshu = "*", baojia = "+" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m2234,z555-5,z6666,z777+"), "m5=", param(new HuleInput { })).hupai, "和了役: 大三元(パオ)");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "小四喜", fanshu = "*" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m234z2244,z333+,z111-"), "z4=", param(new HuleInput { })).hupai, "和了役: 小四喜");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "大四喜", fanshu = "**" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m22z22244,z333+,z111-"), "z4=", param(new HuleInput { })).hupai, "和了役: 大四喜");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "大四喜", fanshu = "**", baojia = "-" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m2,z222+,z4444,z333+,z111-"), "m2=", param(new HuleInput { })).hupai, "和了役: 大四喜(パオ)");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "字一色", fanshu = "*" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("z1112277,z555=,z444+"), "z7=", param(new HuleInput { })).hupai, "和了役: 字一色");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "字一色", fanshu = "*" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("z1122334455667"), "z7=", param(new HuleInput { })).hupai, "和了役: 字一色(七対子形)");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "緑一色", fanshu = "*" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("s22334466z66,s888+"), "z6=", param(new HuleInput { })).hupai, "和了役: 緑一色");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "緑一色", fanshu = "*" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("s4466,s222=,s333+,s888-"), "s6=", param(new HuleInput { })).hupai, "和了役: 緑一色(發なし)");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "清老頭", fanshu = "*" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("s11p111m11,s999-,m999="), "m1=", param(new HuleInput { })).hupai, "和了役: 清老頭");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "四槓子", fanshu = "*" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m1,z5555,p222+2,p777-7,s1111-"), "m1=", param(new HuleInput { })).hupai, "和了役: 四槓子");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "九蓮宝燈", fanshu = "*" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m1112235678999"), "m4=", param(new HuleInput { })).hupai, "和了役: 九蓮宝燈");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "純正九蓮宝燈", fanshu = "**" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m1112345678999"), "m2=", param(new HuleInput { })).hupai, "和了役: 純正九蓮宝燈");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "場風 東", fanshu = "1" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("p55m234s78,m4-56,z111+"), "s9=", param(new HuleInput { baopai = new List<string> { "s1" } })).hupai, "ドラ: ドラなし");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "場風 東", fanshu = "1" }, new Yaku { name = "ドラ", fanshu = "1" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("p55m234s78,m4-56,z111+"), "s9=", param(new HuleInput { baopai = new List<string> { "m2" } })).hupai, "ドラ: 手牌内: 1");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "場風 東", fanshu = "1" }, new Yaku { name = "ドラ", fanshu = "2" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("p55m234s78,m4-56,z111+"), "s9=", param(new HuleInput { baopai = new List<string> { "p4" } })).hupai, "ドラ: 手牌内: 2");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "場風 東", fanshu = "1" }, new Yaku { name = "ドラ", fanshu = "2" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("p55m23s789,m4-56,z111+"), "m4=", param(new HuleInput { baopai = new List<string> { "m3" } })).hupai, "ドラ: 手牌内: 1, 副露内: 1");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "場風 東", fanshu = "1" }, new Yaku { name = "ドラ", fanshu = "1" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("p55m234s78,m4-56,z111+"), "s9=", param(new HuleInput { baopai = new List<string> { "s1", "m2" } })).hupai, "ドラ: 槓ドラ: 1");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "場風 東", fanshu = "1" }, new Yaku { name = "赤ドラ", fanshu = "2" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("p50m234s78,m4-06,z111+"), "s9=", param(new HuleInput { baopai = new List<string> { "s1" } })).hupai, "ドラ: 赤ドラ: 2");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "場風 東", fanshu = "1" }, new Yaku { name = "ドラ", fanshu = "1" }, new Yaku { name = "赤ドラ", fanshu = "1" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("p55m234s78,m4-06,z111+"), "s9=", param(new HuleInput { baopai = new List<string> { "m4" } })).hupai, "ドラ: 赤のダブドラ");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "場風 東", fanshu = "1" }, new Yaku { name = "ドラ", fanshu = "1" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("p55m234s78,m4-56,z111+"), "s9=", param(new HuleInput { baopai = new List<string> { "m0" } })).hupai, "ドラ: ドラ表示牌が赤牌");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "立直", fanshu = "1" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m344556s24678z66*"), "s3=", param(new HuleInput { lizhi = 1, baopai = new List<string> { "s9" }, fubaopai = new List<string> { "s9" } })).hupai, "ドラ: 裏ドラなし");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "立直", fanshu = "1" }, new Yaku { name = "裏ドラ", fanshu = "1" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m344556s24678z66*"), "s3=", param(new HuleInput { lizhi = 1, baopai = new List<string> { "s9" }, fubaopai = new List<string> { "m2" } })).hupai, "ドラ: 裏ドラ: 1");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "立直", fanshu = "1" }, new Yaku { name = "ドラ", fanshu = "1" }, new Yaku { name = "裏ドラ", fanshu = "1" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m344556s24678z66*"), "s3=", param(new HuleInput { lizhi = 1, baopai = new List<string> { "m2" }, fubaopai = new List<string> { "m2" } })).hupai, "ドラ: ドラ: 1, 裏ドラ: 1");
            Assert.IsNull(Majiang.Util.hule(Majiang.Shoupai.fromString("m344556s24678z66"), "s3=", param(new HuleInput { baopai = new List<string> { "m2" } })).hupai, "ドラ: ドラのみでの和了は不可");
            CollectionAssert.AreEqual(new List<Yaku> { new Yaku { name = "国士無双", fanshu = "*" } }, Majiang.Util.hule(Majiang.Shoupai.fromString("m119p19s19z1234567"), null, param(new HuleInput { baopai = new List<string> { "m9" } })).hupai, "ドラ: 役満にドラはつかない");

            HuleResult hule;

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("z33m123p456s789m234"), null, param());
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
                                    new Yaku { name = "門前清自摸和", fanshu = "1" },
                                    new Yaku { name = "平和",        fanshu = "1" }
                                },
                fu = 20,
                fanshu = 2,
                damanguan = null,
                defen = 1500,
                fenpei = new List<int> { -700, 1500, -400, -400 }
            },
                            hule, "点計算: 20符 2翻 子 ツモ → 400/700");
            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("z33m123p456s789m231"), null, param(new HuleInput { menfeng = 0 }));
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
                    new Yaku { name = "門前清自摸和", fanshu = "1" },
                    new Yaku { name = "平和", fanshu = "1" },
                    new Yaku { name = "一盃口", fanshu = "1" }
                },
                fu = 20,
                fanshu = 3,
                damanguan = null,
                defen = 3900,
                fenpei = new List<int> { 3900, -1300, -1300, -1300 }
            }, hule, "点計算: 20符 3翻 親 ツモ → 1300∀");
            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("z33m123p234s234m234"), null, param());
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
            new Yaku { name = "門前清自摸和", fanshu = "1" },
            new Yaku { name = "平和", fanshu = "1" },
            new Yaku { name = "三色同順", fanshu = "2" }
        },
                fu = 20,
                fanshu = 4,
                damanguan = null,
                defen = 5200,
                fenpei = new List<int> { -2600, 5200, -1300, -1300 }
            }, hule, "点計算: 20符 4翻 子 ツモ → 1300/2600");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("m1122p3344s5566z7"), "z7-", param(new HuleInput { lizhibang = 1, changbang = 1 }));
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
            new Yaku { name = "七対子", fanshu = "2" }
        },
                fu = 25,
                fanshu = 2,
                damanguan = null,
                defen = 1600,
                fenpei = new List<int> { -1900, 2900, 0, 0 }
            }, hule, "点計算: 25符 2翻 子 ロン → 1600");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("m1122p3344s5566z77"), null, param(new HuleInput { menfeng = 0, lizhibang = 1, changbang = 1 }));
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
            new Yaku { name = "門前清自摸和", fanshu = "1" },
            new Yaku { name = "七対子", fanshu = "2" }
        },
                fu = 25,
                fanshu = 3,
                damanguan = null,
                defen = 4800,
                fenpei = new List<int> { 6100, -1700, -1700, -1700 }
            }, hule, "点計算: 25符 3翻 親 ツモ → 1600∀");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("m2277p3344s556688"), null, param(new HuleInput { lizhibang = 1, changbang = 1 }));
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
            new Yaku { name = "門前清自摸和", fanshu = "1" },
            new Yaku { name = "断幺九", fanshu = "1" },
            new Yaku { name = "七対子", fanshu = "2" }
        },
                fu = 25,
                fanshu = 4,
                damanguan = null,
                defen = 6400,
                fenpei = new List<int> { -3300, 7700, -1700, -1700 }
            }, hule, "点計算: 25符 4翻 子 ツモ → 1600/3200");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("m77234p456s67,m34-5"), "s8=", param(new HuleInput { menfeng = 0 }));
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
            new Yaku { name = "断幺九", fanshu = "1" }
        },
                fu = 30,
                fanshu = 1,
                damanguan = null,
                defen = 1500,
                fenpei = new List<int> { 1500, 0, -1500, 0 }
            }, hule, "点計算: 30符 1翻 親 ロン → 1500");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("m77234p345s34,m34-5"), "s5-", param());
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
            new Yaku { name = "断幺九", fanshu = "1" },
            new Yaku { name = "三色同順", fanshu = "1" }
        },
                fu = 30,
                fanshu = 2,
                damanguan = null,
                defen = 2000,
                fenpei = new List<int> { -2000, 2000, 0, 0 }
            }, hule, "点計算: 30符 2翻 子 ロン → 2000");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("m22z111p445566s789"), null, param(new HuleInput { zhuangfeng = 1, menfeng = 0 }));
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
            new Yaku { name = "門前清自摸和", fanshu = "1" },
            new Yaku { name = "自風 東", fanshu = "1" },
            new Yaku { name = "一盃口", fanshu = "1" }
        },
                fu = 30,
                fanshu = 3,
                damanguan = null,
                defen = 6000,
                fenpei = new List<int> { 6000, -2000, -2000, -2000 }
            }, hule, "点計算: 30符 3翻 親 ツモ → 2000∀");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("m11z111p123789s789"), null, param());
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
            new Yaku { name = "門前清自摸和", fanshu = "1" },
            new Yaku { name = "場風 東", fanshu = "1" },
            new Yaku { name = "混全帯幺九", fanshu = "2" }
        },
                fu = 30,
                fanshu = 4,
                damanguan = null,
                defen = 7900,
                fenpei = new List<int> { -3900, 7900, -2000, -2000 }
            }, hule, "点計算: 30符 4翻 子 ツモ → 2000/3900");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("m11234234p456s89"), "s7=", param(new HuleInput { menfeng = 0 }));
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
            new Yaku { name = "一盃口", fanshu = "1" }
        },
                fu = 40,
                fanshu = 1,
                damanguan = null,
                defen = 2000,
                fenpei = new List<int> { 2000, 0, -2000, 0 }
            }, hule, "点計算: 40符 1翻 親 ロン → 2000");


            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("m22334455p456s68"), "s7-", param());
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
            new Yaku { name = "断幺九", fanshu = "1" },
            new Yaku { name = "一盃口", fanshu = "1" }
        },
                fu = 40,
                fanshu = 2,
                damanguan = null,
                defen = 2600,
                fenpei = new List<int> { -2600, 2600, 0, 0 }
            }, hule, "点計算: 40符 2翻 子 ロン → 2600");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("z33222m222,s222=,p999+"), null, param(new HuleInput { zhuangfeng = 1, menfeng = 0 }));
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
            new Yaku { name = "場風 南", fanshu = "1" },
            new Yaku { name = "対々和", fanshu = "2" }
        },
                fu = 40,
                fanshu = 3,
                damanguan = null,
                defen = 7800,
                fenpei = new List<int> { 7800, -2600, -2600, -2600 }
            }, hule, "点計算: 40符 3翻 親 ツモ → 2600∀");


            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("z33222m222,s222=,p999+"), null, param(new HuleInput { zhuangfeng = 1 }));
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
            new Yaku { name = "場風 南", fanshu = "1" },
            new Yaku { name = "自風 南", fanshu = "1" },
            new Yaku { name = "対々和", fanshu = "2" }
        },
                fu = 40,
                fanshu = 4,
                damanguan = null,
                defen = 8000,
                fenpei = new List<int> { -4000, 8000, -2000, -2000 }
            }, hule, "点計算: 40符 4翻 子 ツモ → 2000/4000");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("m123p456s789z2227"), "z7=", param(new HuleInput { zhuangfeng = 1, menfeng = 0 }));
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
            new Yaku { name = "場風 南", fanshu = "1" }
        },
                fu = 50,
                fanshu = 1,
                damanguan = null,
                defen = 2400,
                fenpei = new List<int> { 2400, 0, -2400, 0 }
            }, hule, "点計算: 50符 1翻 親 ロン → 2400");


            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("m123p456s789z2227"), "z7-", param(new HuleInput { zhuangfeng = 1 }));
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
        new Yaku { name = "場風 南", fanshu = "1" },
        new Yaku { name = "自風 南", fanshu = "1" }
    },
                fu = 50,
                fanshu = 2,
                damanguan = null,
                defen = 3200,
                fenpei = new List<int> { -3200, 3200, 0, 0 }
            }, hule, "点計算: 50符 2翻 子 ロン → 3200");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("z33m222z222,p8888,s789-"), null, param(new HuleInput { zhuangfeng = 1, menfeng = 0 }));
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
        new Yaku { name = "場風 南", fanshu = "1" },
        new Yaku { name = "三暗刻", fanshu = "2" }
    },
                fu = 50,
                fanshu = 3,
                damanguan = null,
                defen = 9600,
                fenpei = new List<int> { 9600, -3200, -3200, -3200 }
            }, hule, "点計算: 50符 3翻 親 ツモ → 3200∀");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("z33m222z222,p8888,s789-"), null, param(new HuleInput { zhuangfeng = 1 }));
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
        new Yaku { name = "場風 南", fanshu = "1" },
        new Yaku { name = "自風 南", fanshu = "1" },
        new Yaku { name = "三暗刻", fanshu = "2" }
    },
                fu = 50,
                fanshu = 4,
                damanguan = null,
                defen = 8000,
                fenpei = new List<int> { -4000, 8000, -2000, -2000 }
            }, hule, "点計算: 50符 4翻 子 ツモ → 2000/4000");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("s789z2227,m2222,p111="), "z7=", param(new HuleInput { zhuangfeng = 1, menfeng = 0 }));
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
        new Yaku { name = "場風 南", fanshu = "1" }
    },
                fu = 60,
                fanshu = 1,
                damanguan = null,
                defen = 2900,
                fenpei = new List<int> { 2900, 0, -2900, 0 }
            }, hule, "点計算: 60符 1翻 親 ロン → 2900");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("s789z2227,m2222,p111="), "z7-", param(new HuleInput { zhuangfeng = 1 }));
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
        new Yaku { name = "場風 南", fanshu = "1" },
        new Yaku { name = "自風 南", fanshu = "1" }
    },
                fu = 60,
                fanshu = 2,
                damanguan = null,
                defen = 3900,
                fenpei = new List<int> { -3900, 3900, 0, 0 }
            }, hule, "点計算: 60符 2翻 子 ロン → 3900");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("m11222789,z2222,m444="), null, param(new HuleInput { zhuangfeng = 1, menfeng = 0 }));
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
        new Yaku { name = "場風 南", fanshu = "1" },
        new Yaku { name = "混一色", fanshu = "2" }
    },
                fu = 60,
                fanshu = 3,
                damanguan = null,
                defen = 11700,
                fenpei = new List<int> { 11700, -3900, -3900, -3900 }
            }, hule, "点計算: 60符 3翻 親 ツモ → 3900∀");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("m11222789,z2222,m444="), null, param(new HuleInput { zhuangfeng = 1 }));
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
        new Yaku { name = "場風 南", fanshu = "1" },
        new Yaku { name = "自風 南", fanshu = "1" },
        new Yaku { name = "混一色", fanshu = "2" }
    },
                fu = 60,
                fanshu = 4,
                damanguan = null,
                defen = 8000,
                fenpei = new List<int> { -4000, 8000, -2000, -2000 }
            }, hule, "点計算: 60符 4翻 子 ツモ → 2000/4000");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("m12377p456s78,z2222"), "s9=", param(new HuleInput { zhuangfeng = 1, menfeng = 0 }));
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
        new Yaku { name = "場風 南", fanshu = "1" }
    },
                fu = 70,
                fanshu = 1,
                damanguan = null,
                defen = 3400,
                fenpei = new List<int> { 3400, 0, -3400, 0 }
            }, hule, "点計算: 70符 1翻 親 ロン → 3400");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("m12377p456s78,z2222"), "s9-", param(new HuleInput { zhuangfeng = 1 }));
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
        new Yaku { name = "場風 南", fanshu = "1" },
        new Yaku { name = "自風 南", fanshu = "1" }
    },
                fu = 70,
                fanshu = 2,
                damanguan = null,
                defen = 4500,
                fenpei = new List<int> { -4500, 4500, 0, 0 }
            }, hule, "点計算: 70符 2翻 子 ロン → 4500");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("p77s223344,z2222,m2222"), null, param(new HuleInput { zhuangfeng = 1, menfeng = 0 }));
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
        new Yaku { name = "門前清自摸和", fanshu = "1" },
        new Yaku { name = "場風 南", fanshu = "1" },
        new Yaku { name = "一盃口", fanshu = "1" }
    },
                fu = 70,
                fanshu = 3,
                damanguan = null,
                defen = 12000,
                fenpei = new List<int> { 12000, -4000, -4000, -4000 }
            }, hule, "点計算: 70符 3翻 親 ツモ → 4000∀");


            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("m22s888p34,z222+2,z4444"), "p5=", param(new HuleInput { zhuangfeng = 1, menfeng = 0 }));
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
        new Yaku { name = "場風 南", fanshu = "1" }
    },
                fu = 80,
                fanshu = 1,
                damanguan = null,
                defen = 3900,
                fenpei = new List<int> { 3900, 0, -3900, 0 }
            }, hule, "点計算: 80符 1翻 親 ロン → 3900");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("m22s888p34,z222+2,z4444"), "p5-", param(new HuleInput { zhuangfeng = 1 }));
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
        new Yaku { name = "場風 南", fanshu = "1" },
        new Yaku { name = "自風 南", fanshu = "1" }
    },
                fu = 80,
                fanshu = 2,
                damanguan = null,
                defen = 5200,
                fenpei = new List<int> { -5200, 5200, 0, 0 }
            }, hule, "点計算: 80符 2翻 子 ロン → 5200");


            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("m11p999s123,z222+2,z1111"), null, param(new HuleInput { zhuangfeng = 1, menfeng = 0 }));
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
        new Yaku { name = "場風 南", fanshu = "1" },
        new Yaku { name = "自風 東", fanshu = "1" },
        new Yaku { name = "混全帯幺九", fanshu = "1" }
    },
                fu = 80,
                fanshu = 3,
                damanguan = null,
                defen = 12000,
                fenpei = new List<int> { 12000, -4000, -4000, -4000 }
            }, hule, "点計算: 80符 3翻 親 ツモ → 4000∀");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("p88m123s99,s6666,z2222"), "s9=", param(new HuleInput { zhuangfeng = 1, menfeng = 0 }));
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
        new Yaku { name = "場風 南", fanshu = "1" }
    },
                fu = 90,
                fanshu = 1,
                damanguan = null,
                defen = 4400,
                fenpei = new List<int> { 4400, 0, -4400, 0 }
            }, hule, "点計算: 90符 1翻 親 ロン → 4400");


            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("p88m123s99,s6666,z2222"), "s9-", param(new HuleInput { zhuangfeng = 1 }));
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
        new Yaku { name = "場風 南", fanshu = "1" },
        new Yaku { name = "自風 南", fanshu = "1" }
    },
                fu = 90,
                fanshu = 2,
                damanguan = null,
                defen = 5800,
                fenpei = new List<int> { -5800, 5800, 0, 0 }
            }, hule, "点計算: 90符 2翻 子 ロン → 5800");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("m22s345,z5555,z2222,z666-"), null, param(new HuleInput { zhuangfeng = 1, menfeng = 0 }));
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
                    new Yaku { name = "場風 南", fanshu = "1" },
                    new Yaku { name = "翻牌 白", fanshu = "1" },
                    new Yaku { name = "翻牌 發", fanshu = "1" }
                },
                fu = 90,
                fanshu = 3,
                damanguan = null,
                defen = 12000,
                fenpei = new List<int> { 12000, -4000, -4000, -4000 }
            }, hule, "点計算: 90符 3翻 親 ツモ → 4000∀");


            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("m22p345s67,z2222,s9999"), "s8=", param(new HuleInput { zhuangfeng = 1, menfeng = 0 }));
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
                    new Yaku { name = "場風 南", fanshu = "1" }
                },
                fu = 100,
                fanshu = 1,
                damanguan = null,
                defen = 4800,
                fenpei = new List<int> { 4800, 0, -4800, 0 }
            }, hule, "点計算: 100符 1翻 親 ロン → 4800");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("m22p345s67,z2222,s9999"), "s8-", param(new HuleInput { zhuangfeng = 1 }));
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
                    new Yaku { name = "場風 南", fanshu = "1" },
                    new Yaku { name = "自風 南", fanshu = "1" }
                },
                fu = 100,
                fanshu = 2,
                damanguan = null,
                defen = 6400,
                fenpei = new List<int> { -6400, 6400, 0, 0 }
            }, hule, "点計算: 100符 2翻 子 ロン → 6400");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("z11m999p243,s1111,s9999"), null, param(new HuleInput { zhuangfeng = 1, menfeng = 0 }));
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
                    new Yaku { name = "門前清自摸和", fanshu = "1" },
                    new Yaku { name = "三暗刻", fanshu = "2" }
                },
                fu = 100,
                fanshu = 3,
                damanguan = null,
                defen = 12000,
                fenpei = new List<int> { 12000, -4000, -4000, -4000 }
            }, hule, "点計算: 100符 3翻 親 ツモ → 4000∀");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("m234z1177,p1111,s9999"), "z7=", param(new HuleInput { menfeng = 0 }));
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
                    new Yaku { name = "翻牌 中", fanshu = "1" }
                },
                fu = 110,
                fanshu = 1,
                damanguan = null,
                defen = 5300,
                fenpei = new List<int> { 5300, 0, -5300, 0 }
            }, hule, "点計算: 110符 1翻 親 ロン → 5300");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("m234z2277,p1111,z5555"), "z7-", param(new HuleInput { zhuangfeng = 1 }));
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
                    new Yaku { name = "翻牌 白", fanshu = "1" },
                    new Yaku { name = "翻牌 中", fanshu = "1" }
                },
                fu = 110,
                fanshu = 2,
                damanguan = null,
                defen = 7100,
                fenpei = new List<int> { -7100, 7100, 0, 0 }
            }, hule, "点計算: 110符 2翻 子 ロン → 7100");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("m243z11,p1111,s9999,z555+5"), null, param(new HuleInput { zhuangfeng = 1, menfeng = 0 }));
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
                    new Yaku { name = "翻牌 白", fanshu = "1" },
                    new Yaku { name = "三槓子", fanshu = "2" }
                },
                fu = 110,
                fanshu = 3,
                damanguan = null,
                defen = 12000,
                fenpei = new List<int> { 12000, -4000, -4000, -4000 }
            }, hule, "点計算: 110符 3翻 親 ツモ → 4000∀");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("m22456p456s44556"), "s6=", param(new HuleInput { menfeng = 0 }));
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
                    new Yaku { name = "平和", fanshu = "1" },
                    new Yaku { name = "断幺九", fanshu = "1" },
                    new Yaku { name = "一盃口", fanshu = "1" },
                    new Yaku { name = "三色同順", fanshu = "2" }
                },
                fu = 30,
                fanshu = 5,
                damanguan = null,
                defen = 12000,
                fenpei = new List<int> { 12000, 0, -12000, 0 }
            }, hule, "点計算: 5翻 親 ロン → 12000");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("m22456p456s445566"), null, param());
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
                    new Yaku { name = "門前清自摸和", fanshu = "1" },
                    new Yaku { name = "平和", fanshu = "1" },
                    new Yaku { name = "断幺九", fanshu = "1" },
                    new Yaku { name = "一盃口", fanshu = "1" },
                    new Yaku { name = "三色同順", fanshu = "2" }
                },
                fu = 20,
                fanshu = 6,
                damanguan = null,
                defen = 12000,
                fenpei = new List<int> { -6000, 12000, -3000, -3000 }
            }, hule, "点計算: 6翻 子 ツモ → 3000/6000");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("m111z3334,z222=,m999-"), "z4=", param(new HuleInput { zhuangfeng = 1, menfeng = 0 }));
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
                    new Yaku { name = "場風 南", fanshu = "1" },
                    new Yaku { name = "対々和", fanshu = "2" },
                    new Yaku { name = "混老頭", fanshu = "2" },
                    new Yaku { name = "混一色", fanshu = "2" }
                },
                fu = 50,
                fanshu = 7,
                damanguan = null,
                defen = 18000,
                fenpei = new List<int> { 18000, 0, -18000, 0 }
            }, hule, "点計算: 7翻 親 ロン → 18000");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("m111z333444,z222=,m999-"), null, param(new HuleInput { zhuangfeng = 1 }));
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
                    new Yaku { name = "場風 南", fanshu = "1" },
                    new Yaku { name = "自風 南", fanshu = "1" },
                    new Yaku { name = "対々和", fanshu = "2" },
                    new Yaku { name = "混老頭", fanshu = "2" },
                    new Yaku { name = "混一色", fanshu = "2" }
                },
                fu = 50,
                fanshu = 8,
                damanguan = null,
                defen = 16000,
                fenpei = new List<int> { -8000, 16000, -4000, -4000 }
            }, hule, "点計算: 8翻 子 ツモ → 4000/8000");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("s2223334455567"), "s8=", param(new HuleInput { menfeng = 0 }));
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
                    new Yaku { name = "断幺九", fanshu = "1" },
                    new Yaku { name = "三暗刻", fanshu = "2" },
                    new Yaku { name = "清一色", fanshu = "6" }
                },
                fu = 50,
                fanshu = 9,
                damanguan = null,
                defen = 24000,
                fenpei = new List<int> { 24000, 0, -24000, 0 }
            }, hule, "点計算: 9翻 親 ロン → 24000");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("s22233344555678"), null, param());
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
                    new Yaku { name = "門前清自摸和", fanshu = "1" },
                    new Yaku { name = "断幺九", fanshu = "1" },
                    new Yaku { name = "三暗刻", fanshu = "2" },
                    new Yaku { name = "清一色", fanshu = "6" }
                },
                fu = 40,
                fanshu = 10,
                damanguan = null,
                defen = 16000,
                fenpei = new List<int> { -8000, 16000, -4000, -4000 }
            }, hule, "点計算: 10翻 子 ツモ → 4000/8000");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("p2233445566778"), "p8=", param(new HuleInput { menfeng = 0 }));
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
                    new Yaku { name = "平和", fanshu = "1" },
                    new Yaku { name = "断幺九", fanshu = "1" },
                    new Yaku { name = "二盃口", fanshu = "3" },
                    new Yaku { name = "清一色", fanshu = "6" }
                },
                fu = 30,
                fanshu = 11,
                damanguan = null,
                defen = 36000,
                fenpei = new List<int> { 36000, 0, -36000, 0 }
            }, hule, "点計算: 11翻 親 ロン → 36000");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("p22334455667788"), null, param());
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
                    new Yaku { name = "門前清自摸和", fanshu = "1" },
                    new Yaku { name = "平和", fanshu = "1" },
                    new Yaku { name = "断幺九", fanshu = "1" },
                    new Yaku { name = "二盃口", fanshu = "3" },
                    new Yaku { name = "清一色", fanshu = "6" }
                },
                fu = 20,
                fanshu = 12,
                damanguan = null,
                defen = 24000,
                fenpei = new List<int> { -12000, 24000, -6000, -6000 }
            }, hule, "点計算: 12翻 子 ツモ → 6000/12000");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("m1177778888999"), "m9=", param(new HuleInput { menfeng = 0 }));
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
                    new Yaku { name = "平和", fanshu = "1" },
                    new Yaku { name = "純全帯幺九", fanshu = "3" },
                    new Yaku { name = "二盃口", fanshu = "3" },
                    new Yaku { name = "清一色", fanshu = "6" }
                },
                fu = 30,
                fanshu = 13,
                damanguan = null,
                defen = 48000,
                fenpei = new List<int> { 48000, 0, -48000, 0 }
            }, hule, "点計算: 13翻 親 ロン → 48000");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("z77111z444,z222+,z333-"), null, param());
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
                    new Yaku { name = "大四喜", fanshu = "**" },
                    new Yaku { name = "字一色", fanshu = "*" }
                },
                fu = null,
                fanshu = null,
                damanguan = 3,
                defen = 96000,
                fenpei = new List<int> { -48000, 96000, -24000, -24000 }
            }, hule, "点計算: 役満複合 子 ツモ → 24000/48000");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("m11p456,z555+,z666=,z777-"), null, param(new HuleInput { menfeng = 0, lizhibang = 1, changbang = 1 }));
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
                    new Yaku { name = "大三元", fanshu = "*", baojia = "-" }
                },
                fu = null,
                fanshu = null,
                damanguan = 1,
                defen = 48000,
                fenpei = new List<int> { 49300, 0, 0, -48300 }
            }, hule, "点計算: 役満パオ 放銃者なし、責任払い");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("m11p45,z555+,z666=,z777-"), "p6=", param(new HuleInput { menfeng = 0, lizhibang = 1, changbang = 1 }));
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
                    new Yaku { name = "大三元", fanshu = "*", baojia = "-" }
                },
                fu = null,
                fanshu = null,
                damanguan = 1,
                defen = 48000,
                fenpei = new List<int> { 49300, 0, -24300, -24000 }
            }, hule, "点計算: 役満パオ 放銃者あり、放銃者と折半");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("m11p45,z555+,z666=,z777-"), "p6-", param(new HuleInput { menfeng = 0, lizhibang = 1, changbang = 1 }));
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
                    new Yaku { name = "大三元", fanshu = "*", baojia = "-" }
                },
                fu = null,
                fanshu = null,
                damanguan = 1,
                defen = 48000,
                fenpei = new List<int> { 49300, 0, 0, -48300 }
            }, hule, "点計算: 役満パオ パオが放銃、全額責任払い");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("z77,z111-,z2222,z333=3,z444+"), null, param(new HuleInput { lizhibang = 1, changbang = 1 }));
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
                    new Yaku { name = "大四喜", fanshu = "**", baojia = "+" },
                    new Yaku { name = "字一色", fanshu = "*" }
                },
                fu = null,
                fanshu = null,
                damanguan = 3,
                defen = 96000,
                fenpei = new List<int> { -16100, 97300, -72100, -8100 }
            }, hule, "点計算: ダブル役満パオ 放銃者なし、関連役満のみ責任払い");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("z7,z111-,z2222,z333=3,z444+"), "z7-", param(new HuleInput { lizhibang = 1, changbang = 1 }));
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
                    new Yaku { name = "大四喜", fanshu = "**", baojia = "+" },
                    new Yaku { name = "字一色", fanshu = "*" }
                },
                fu = null,
                fanshu = null,
                damanguan = 3,
                defen = 96000,
                fenpei = new List<int> { -64300, 97300, -32000, 0 }
            }, hule, "点計算: ダブル役満パオ 放銃者あり、関連役満のみ放銃者と折半");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("z7,z111-,z2222,z333=3,z444+"), "z7+", param(new HuleInput { lizhibang = 1, changbang = 1 }));
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
                    new Yaku { name = "大四喜", fanshu = "**", baojia = "+" },
                    new Yaku { name = "字一色", fanshu = "*" }
                },
                fu = null,
                fanshu = null,
                damanguan = 3,
                defen = 96000,
                fenpei = new List<int> { 0, 97300, -96300, 0 }
            }, hule, "点計算: ダブル役満パオ パオが放銃、全額責任払い");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("m223344p556677s8"), "s8=", param());
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
                    new Yaku { name = "断幺九", fanshu = "1" },
                    new Yaku { name = "二盃口", fanshu = "3" }
                },
                fu = 40,
                fanshu = 4,
                damanguan = null,
                defen = 8000,
                fenpei = new List<int> { 0, 8000, 0, -8000 }
            }, hule, "高点法: 七対子と二盃口の選択");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("m2234455p234s234"), "m3=", param());
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
                    new Yaku { name = "断幺九", fanshu = "1" },
                    new Yaku { name = "一盃口", fanshu = "1" },
                    new Yaku { name = "三色同順", fanshu = "2" }
                },
                fu = 40,
                fanshu = 4,
                damanguan = null,
                defen = 8000,
                fenpei = new List<int> { 0, 8000, 0, -8000 }
            }, hule, "高点法: 雀頭候補2つの選択");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("m111222333p8999"), "p7=", param());
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
                    new Yaku { name = "一盃口", fanshu = "1" },
                    new Yaku { name = "純全帯幺九", fanshu = "3" }
                },
                fu = 40,
                fanshu = 4,
                damanguan = null,
                defen = 8000,
                fenpei = new List<int> { 0, 8000, 0, -8000 }
            }, hule, "高点法: 順子と刻子の選択");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("m12334p567z11z777"), "m2=", param());
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
                    new Yaku { name = "翻牌 中", fanshu = "1" }
                },
                fu = 50,
                fanshu = 1,
                damanguan = null,
                defen = 1600,
                fenpei = new List<int> { 0, 1600, 0, -1600 }
            }, hule, "高点法: 嵌張待ち両面待ちの選択");


            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("m111222333p7899"), "p9=", param());
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
                    new Yaku { name = "平和", fanshu = "1" },
                    new Yaku { name = "一盃口", fanshu = "1" },
                    new Yaku { name = "純全帯幺九", fanshu = "3" }
                },
                fu = 30,
                fanshu = 5,
                damanguan = null,
                defen = 8000,
                fenpei = new List<int> { 0, 8000, 0, -8000 }
            }, hule, "高点法: 得点が同じ場合は翻数が多い方を選択");

            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("s1112223335578"), "s9=", param());
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
                    new Yaku { name = "三暗刻", fanshu = "2" },
                    new Yaku { name = "清一色", fanshu = "6" }
                },
                fu = 50,
                fanshu = 8,
                damanguan = null,
                defen = 16000,
                fenpei = new List<int> { 0, 16000, 0, -16000 }
            }, hule, "高点法: 得点・翻数が同じ場合は符が多い方を選択");


            hule = Majiang.Util.hule(Majiang.Shoupai.fromString("m11123457899996"), null, param(new HuleInput { lizhi = 1, yifa = true, baopai = new List<string> { "m2" }, fubaopai = new List<string> { "m5" } }));
            Assert.AreEqual(new HuleResult
            {
                hupai = new List<Yaku>{
                    new Yaku { name = "九蓮宝燈", fanshu = "*" }
                },
                fu = null,
                fanshu = null,
                damanguan = 1,
                defen = 32000,
                fenpei = new List<int> { -16000, 32000, -8000, -8000 }
            }, hule, "高点法: 役満と数え役満では役満を選択");

            TestUtil.ProcessInParallel(data, t =>
            {
                t.@in.param.rule = new Majiang.Rule();
                var hule = Majiang.Util.hule(Majiang.Shoupai.fromString(t.@in.shoupai), t.@in.rongpai, t.@in.param);
                Assert.AreEqual(t.@out, hule, t.@in.shoupai);
            });
        }
    }
}