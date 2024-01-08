using System.Collections.Generic;
using System.Linq;

namespace Majiang
{
    /// <summary>
    /// 牌譜
    /// 1戦分の対局記録を表すJSON形式データ
    /// </summary>
    public class Paipu : EntityBase
    {
        /// <summary>
        /// 牌譜のタイトル。
        /// 改行を含めてもよい。改行を含む場合は1行目をリーグ戦名、2行目以降を節名や開催日などにすることを推奨する。
        /// </summary>
        public string title;
        /// <summary>
        /// 対局者情報。
        /// 仮東から順に並べる。
        /// 個々の対局者情報には改行を含めてもよい。
        /// 改行を含む場合は1行目を個人を特定できる情報とし、2行目以降に段位や通算獲得ポイントなどの付加情報とすることを推奨する。
        /// </summary>
        public List<string> player;
        /// <summary>
        /// 起家。0: 仮東、1: 仮南、2: 仮西、3: 仮北。
        /// 仮東の概念のない対局では第一局の東家を仮東とし、qijia に 0 を設定すればよい。
        /// </summary>
        public int qijia;
        /// <summary>
        /// 対局情報。
        /// 各局の局情報の配列。
        /// </summary>
        public List<List<Message>> log;
        /// <summary>
        /// 終了時の持ち点。
        /// 仮東から順に並べる。
        /// </summary>
        public List<int> defen;
        /// <summary>
        /// 各プレイヤーのポイントを表す文字列のリスト
        /// </summary>
        public List<string> point;
        /// <summary>
        /// 着順。
        /// 1: トップ ～ 4: ラス。仮東から順に並べる。
        /// </summary>
        public List<int> rank;

        public override string ToString()
        {
            var logStr = "";
            if (log != null)
            {
                foreach (var l in log)
                {
                    logStr += "[";
                    foreach (var ll in l)
                    {
                        logStr += ll.ToString();
                    }
                    logStr += "]";
                }
            }

            var parameters = new List<string>
            {
                title != null ? $"title={title}" : null,
                player != null ? $"player={player.JoinJS()}" : null,
                $"qijia={qijia}",
                log != null ? $"log={logStr}" : null,
                defen != null ? $"defen={defen.JoinJS()}" : null,
                point != null ? $"point={point.JoinJS()}" : null,
                rank != null ? $"rank={rank.JoinJS()}" : null
            };

            return $"Paipu({string.Join(", ", parameters.Where(p => p != null))})";
        }

        public override object Clone()
        {
            List<List<Message>> newLog = null;
            if (log != null)
            {
                newLog = new List<List<Message>>(log.Count);
                foreach (var l in log)
                {
                    var newList = new List<Message>(l.Count);
                    foreach (var ll in l)
                    {
                        newList.Add((Message)ll.Clone());
                    }
                    newLog.Add(newList);
                }
            }

            return new Paipu
            {
                title = title,
                player = player?.Concat(),
                qijia = qijia,
                log = newLog,
                defen = defen?.Concat(),
                point = point?.Concat(),
                rank = rank?.Concat(),
            };
        }
    }
}
