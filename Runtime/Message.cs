using System;
using System.Collections.Generic;
using System.Linq;

namespace Majiang
{
    public abstract class EntityBase : ICloneable
    {
        public override bool Equals(object obj)
        {
            return ToString() == obj?.ToString();
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public virtual object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    /// <summary>
    /// 開局
    /// </summary>
    public class Kaiju : EntityBase
    {
        /// <summary>
        /// 席順。(0: 仮東、1: 仮南、2: 仮西、3: 仮北)
        /// </summary>
        public int id;
        /// <summary>
        /// ルール
        /// </summary>
        public Rule rule;
        /// <summary>
        /// 牌譜のタイトル。
        /// </summary>
        public string title;
        /// <summary>
        /// 対局者情報。仮東から順に並べる。
        /// </summary>
        public List<string> player;
        /// <summary>
        /// 起家。(0: 仮東、1: 仮南、2: 仮西、3: 仮北)
        /// </summary>
        public int qijia;

        public override string ToString()
        {
            return $"Kaiju(id={id}, rule={rule}, title={title}, player={player.JoinJS()}, qijia={qijia})";
        }

        public override object Clone()
        {
            return new Kaiju
            {
                id = id,
                rule = (Rule)rule?.Clone(),
                title = title,
                player = player?.Concat(),
                qijia = qijia,
            };
        }
    }

    /// <summary>
    /// 配牌
    /// </summary>
    public class Qipai : EntityBase
    {
        /// <summary>
        /// 場風。(0: 東、1: 南、2: 西、3: 北)
        /// </summary>
        public int zhuangfeng;
        /// <summary>
        /// 局数。(0: 一局、1: 二局、2: 三局、3: 四局)
        /// </summary>
        public int jushu;
        /// <summary>
        /// 本場。
        /// </summary>
        public int changbang;
        /// <summary>
        /// その局開始時の供託リーチ棒の数。
        /// </summary>
        public int lizhibang;
        /// <summary>
        /// その局開始時の対局者の持ち点。その局の東家から順に並べる。
        /// </summary>
        public List<int> defen;
        /// <summary>
        /// ドラ表示牌。
        /// </summary>
        public string baopai;
        /// <summary>
        /// 配牌の 牌姿。その局の東家から順に並べる。他家の配牌はマスクして通知される。
        /// </summary>
        public List<string> shoupai;

        public override string ToString()
        {
            return $"Qipai(zhuangfeng={zhuangfeng}, jushu={jushu}, changbang={changbang}, lizhibang={lizhibang}, baopai={baopai}, shoupai={shoupai.JoinJS()}, defen={defen.JoinJS()})";
        }

        public override object Clone()
        {
            return new Qipai
            {
                zhuangfeng = zhuangfeng,
                jushu = jushu,
                changbang = changbang,
                lizhibang = lizhibang,
                baopai = baopai,
                shoupai = shoupai?.Concat(),
                defen = defen?.Concat(),
            };
        }
    }

    /// <summary>
    /// 自摸
    /// </summary>
    public class Zimo : EntityBase
    {
        /// <summary>
        /// 手番。(0: 東家、1: 南家、2: 西家、3: 北家)
        /// </summary>
        public int l;
        /// <summary>
        /// ツモった 牌。他家のツモ牌はマスクして通知される。
        /// </summary>
        public string p;

        public override string ToString()
        {
            return $"Zimo(l={l}, p={p})";
        }
    }

    /// <summary>
    /// 打牌
    /// </summary>
    public class Dapai : EntityBase
    {
        /// <summary>
        /// 手番。(0: 東家、1: 南家、2: 西家、3: 北家)
        /// </summary>
        public int l;
        /// <summary>
        /// 切った 牌。
        /// </summary>
        public string p;

        public override string ToString()
        {
            return $"Dapai(l={l}, p={p})";
        }
    }

    /// <summary>
    /// 副露
    /// </summary>
    public class Fulou : EntityBase
    {
        /// <summary>
        /// 手番。(0: 東家、1: 南家、2: 西家、3: 北家)
        /// </summary>
        public int l;
        /// <summary>
        /// 副露した 面子。
        /// </summary>
        public string m;

        public override string ToString()
        {
            return $"Fulou(l={l}, m={m})";
        }
    }

    /// <summary>
    /// 槓
    /// </summary>
    public class Gang : EntityBase
    {
        /// <summary>
        /// 手番。(0: 東家、1: 南家、2: 西家、3: 北家)
        /// </summary>
        public int l;
        /// <summary>
        /// 槓した 面子。大明槓は副露として扱うので、ここでの槓は暗槓もしくは加槓。
        /// </summary>
        public string m;

        public override string ToString()
        {
            return $"Gang(l={l}, m={m})";
        }
    }

    /// <summary>
    /// 開槓
    /// </summary>
    public class Kaigang : EntityBase
    {
        /// <summary>
        /// 槓ドラ表示牌。
        /// </summary>
        public string baopai;

        public override string ToString()
        {
            return $"Kaigang(baopai={baopai})";
        }
    }

    /// <summary>
    /// 和了
    /// </summary>
    public class Hule : EntityBase
    {
        /// <summary>
        /// 和了者。(0: 東家、1: 南家、2: 西家、3: 北家)
        /// </summary>
        public int l;
        /// <summary>
        /// 和了者の 牌姿。ロン和了の場合は和了牌をツモした牌姿にする。
        /// </summary>
        public string shoupai;
        /// <summary>
        /// 放銃者。ツモ和了の場合は null。
        /// </summary>
        public int? baojia;
        /// <summary>
        /// 裏ドラ表示牌の配列。リーチでない場合は null。
        /// </summary>
        public List<string> fubaopai;
        /// <summary>
        /// 符。役満の場合は undefined。
        /// </summary>
        public int? fu;
        /// <summary>
        /// 翻数。役満の場合は undefined。
        /// </summary>
        public int? fanshu;
        /// <summary>
        /// 役満複合数。
        /// 複合には四暗刻をダブル役満にする類のものと、大三元と字一色の複合のような役の複合のケースがある。
        /// 役満でない場合は undefined。
        /// </summary>
        public int? damanguan;
        /// <summary>
        /// 和了打点。供託収入は含まない。
        /// </summary>
        public int defen;
        /// <summary>
        /// 和了役の配列。
        /// それぞれの要素には役名を示す name と翻数を示す fanshu がある。
        /// 役満の場合 fanshu は数字ではなく、和了役それぞれの役満複合数分の * となる。
        /// また役満のパオがあった場合は baojia に責任者を設定する。
        /// 役名は任意の文字列なので、ローカル役の採用も可能。
        /// </summary>
        public List<Yaku> hupai;
        /// <summary>
        /// 供託を含めたその局の点数の収支。
        /// その局の東家から順に並べる。
        /// リーチ宣言による1000点減は収支に含めない。
        /// </summary>
        public List<int> fenpei;

        public override string ToString()
        {
            return $"Hule(l={l}, baojia={baojia}, fenpei={fenpei.JoinJS()}, fu={fu}, fanshu={fanshu}, damanguan={damanguan}, defen={defen}, hupai={hupai.JoinJS()}, shoupai={shoupai}, fubaopai={fubaopai.JoinJS()})";
        }

        public override object Clone()
        {
            return new Hule
            {
                l = l,
                baojia = baojia,
                fenpei = fenpei?.Concat(),
                fu = fu,
                fanshu = fanshu,
                damanguan = damanguan,
                defen = defen,
                hupai = hupai.Clone(),
                shoupai = shoupai,
                fubaopai = fubaopai?.Concat(),
            };
        }
    }

    /// <summary>
    /// 流局
    /// </summary>
    public class Pingju : EntityBase
    {
        /// <summary>
        /// 流局理由。
        /// </summary>
        public string name;
        /// <summary>
        /// 流局時の手牌。
        /// その局の東家から順に並べる。
        /// ノーテンなどの理由により手牌を開示しなかった場合は空文字列とする。
        /// </summary>
        public List<string> shoupai;
        /// <summary>
        /// ノーテン罰符などその局の点数の収支。
        /// その局の東家から順に並べる。
        /// リーチ宣言による1000点減は収支に含めない。
        /// </summary>
        public List<int> fenpei;

        public override string ToString()
        {
            return $"Pingju(name={name}, shoupai={shoupai.JoinJS()}, fenpei={fenpei.JoinJS()})";
        }

        public override object Clone()
        {
            return new Pingju
            {
                name = name,
                shoupai = shoupai?.Concat(),
                fenpei = fenpei?.Concat(),
            };
        }
    }

    /// <summary>
    /// 倒牌
    /// </summary>
    public class Daopai : EntityBase
    {
    }

    /// <summary>
    /// メッセージの種類
    /// </summary>
    public enum MessageType
    {
        None,
        Kaiju,
        Qipai,
        Zimo,
        Dapai,
        Fulou,
        Gang,
        Kaigang,
        Hule,
        Pingju,
        Jieju,
        Daopai,
    }

    /// <summary>
    /// 通知メッセージ
    /// 牌譜のログにもなる
    /// </summary>
    public class Message : EntityBase
    {
        /// <summary>
        /// 開局
        /// </summary>
        public Kaiju kaiju;
        /// <summary>
        /// 配牌
        /// </summary>
        public Qipai qipai;
        /// <summary>
        /// 自摸
        /// </summary>
        public Zimo zimo;
        /// <summary>
        /// 打牌
        /// </summary>
        public Dapai dapai;
        /// <summary>
        /// 副露
        /// </summary>
        public Fulou fulou;
        /// <summary>
        /// 槓
        /// </summary>
        public Gang gang;
        /// <summary>
        /// 槓自摸
        /// </summary>
        public Zimo gangzimo;
        /// <summary>
        /// 開槓
        /// </summary>
        public Kaigang kaigang;
        /// <summary>
        /// 和了
        /// </summary>
        public Hule hule;
        /// <summary>
        /// 流局
        /// </summary>
        public Pingju pingju;
        /// <summary>
        /// 終局
        /// </summary>
        public Paipu jieju;
        /// <summary>
        /// 倒牌
        /// </summary>
        public Daopai daopai;

        /// <summary>
        /// メッセージの種類を取得
        /// </summary>
        public MessageType type
        {
            get
            {
                if (kaiju != null) return MessageType.Kaiju;
                if (qipai != null) return MessageType.Qipai;
                if (zimo != null) return MessageType.Zimo;
                if (dapai != null) return MessageType.Dapai;
                if (fulou != null) return MessageType.Fulou;
                if (gang != null) return MessageType.Gang;
                if (gangzimo != null) return MessageType.Zimo;
                if (kaigang != null) return MessageType.Kaigang;
                if (hule != null) return MessageType.Hule;
                if (pingju != null) return MessageType.Pingju;
                if (jieju != null) return MessageType.Jieju;
                if (daopai != null) return MessageType.Daopai;
                return MessageType.None;
            }
        }

        public override string ToString()
        {
            var parameters = new List<string>
            {
                kaiju != null ? $"kaiju={kaiju}" : null,
                qipai != null ? $"qipai={qipai}" : null,
                zimo != null ? $"zimo={zimo}" : null,
                dapai != null ? $"dapai={dapai}" : null,
                fulou != null ? $"fulou={fulou}" : null,
                gang != null ? $"gang={gang}" : null,
                gangzimo != null ? $"gangzimo={gangzimo}" : null,
                kaigang != null ? $"kaigang={kaigang}" : null,
                hule != null ? $"hule={hule}" : null,
                pingju != null ? $"pingju={pingju}" : null,
                jieju != null ? $"jieju={jieju}" : null,
                daopai != null ? $"daopai={daopai}" : null,
            };

            return $"Message({string.Join(", ", parameters.Where(p => p != null))})";
        }

        public override object Clone()
        {
            return new Message
            {
                kaiju = (Kaiju)kaiju?.Clone(),
                qipai = (Qipai)qipai?.Clone(),
                zimo = (Zimo)zimo?.Clone(),
                dapai = (Dapai)dapai?.Clone(),
                fulou = (Fulou)fulou?.Clone(),
                gang = (Gang)gang?.Clone(),
                gangzimo = (Zimo)gangzimo?.Clone(),
                kaigang = (Kaigang)kaigang?.Clone(),
                hule = (Hule)hule?.Clone(),
                pingju = (Pingju)pingju?.Clone(),
                jieju = (Paipu)jieju?.Clone(),
                daopai = (Daopai)daopai?.Clone(),
            };
        }
    }
}