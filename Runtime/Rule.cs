using System;
using System.Collections.Generic;

namespace Majiang
{
    /// <summary>
    /// 赤ドラ
    /// </summary>
    [Serializable]
    public class Hongpai
    {
        public int m;
        public int p;
        public int s;

        public int this[char key]
        {
            get
            {
                switch (key)
                {
                    case 'm': return this.m;
                    case 'p': return this.p;
                    case 's': return this.s;
                    case 'z': return 0;
                    default: throw new Exception("invalid key");
                }
            }
        }
    }

    /// <summary>
    /// ルール
    /// </summary>
    [Serializable]
    public class Rule : EntityBase
    {
        public int 配給原点 = 25000;
        public List<string> 順位点 = new List<string> { "20.0", "10.0", "-10.0", "-20.0" };
        public bool 連風牌は2符 = false;
        public Hongpai 赤牌 = new Hongpai { m = 1, p = 1, s = 1 };
        public bool クイタンあり = true;
        public int 喰い替え許可レベル = 0;
        public int 場数 = 2;
        public bool 途中流局あり = true;
        public bool 流し満貫あり = true;
        public bool ノーテン宣言あり = false;
        public bool ノーテン罰あり = true;
        public int 最大同時和了数 = 2;
        public int 連荘方式 = 2;
        public bool トビ終了あり = true;
        public bool オーラス止めあり = true;
        public int 延長戦方式 = 1;
        public bool 一発あり = true;
        public bool 裏ドラあり = true;
        public bool カンドラあり = true;
        public bool カン裏あり = true;
        public bool カンドラ後乗せ = true;
        public bool ツモ番なしリーチあり = false;
        public int リーチ後暗槓許可レベル = 2;
        public bool 役満の複合あり = true;
        public bool ダブル役満あり = true;
        public bool 数え役満あり = true;
        public bool 役満パオあり = true;
        public bool 切り上げ満貫あり = false;

        public override string ToString()
        {
            return $"Rule(配給原点={配給原点}, 順位点={順位点.JoinJS()}, 連風牌は2符={連風牌は2符}, 赤牌={赤牌.m}{赤牌.p}{赤牌.s}, クイタンあり={クイタンあり}, 喰い替え許可レベル={喰い替え許可レベル}, 場数={場数}, 途中流局あり={途中流局あり}, 流し満貫あり={流し満貫あり}, ノーテン宣言あり={ノーテン宣言あり}, ノーテン罰あり={ノーテン罰あり}, 最大同時和了数={最大同時和了数}, 連荘方式={連荘方式}, トビ終了あり={トビ終了あり}, オーラス止めあり={オーラス止めあり}, 延長戦方式={延長戦方式}, 一発あり={一発あり}, 裏ドラあり={裏ドラあり}, カンドラあり={カンドラあり}, カン裏あり={カン裏あり}, カンドラ後乗せ={カンドラ後乗せ}, ツモ番なしリーチあり={ツモ番なしリーチあり}, リーチ後暗槓許可レベル={リーチ後暗槓許可レベル}, 役満の複合あり={役満の複合あり}, ダブル役満あり={ダブル役満あり}, 数え役満あり={数え役満あり}, 役満パオあり={役満パオあり}, 切り上げ満貫あり={切り上げ満貫あり})";
        }
    }
}
