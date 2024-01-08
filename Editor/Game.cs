using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Majiang.Editor
{
    public class Player : Majiang.Player
    {
        public Player()
        {
            this._reply = new List<Reply>();
        }

        public override void action(Message msg, Action<Reply> callback = null)
        {
            callback?.Invoke(this._reply.Shift());
        }
    }

    public class Game : Majiang.Game
    {
        public Majiang.Shan make_shan(Rule rule, List<Message> log)
        {
            var shan = new Majiang.Shan(rule);

            for (int i = 0; i < shan._pai.Count; i++) { shan._pai[i] = "_"; }

            int zimo_idx = shan._pai.Count;
            int gang_idx = 0;
            var baopai = new List<string>();
            var fubaopai = new List<string>();

            void set_qipai(string paistr)
            {
                foreach (var suitstr in Regex.Matches(paistr, @"[mpsz]\d+"))
                {
                    var s = suitstr.ToString()[0];
                    foreach (var n in Regex.Matches(suitstr.ToString(), @"\d"))
                    {
                        shan._pai[--zimo_idx] = $"{s}{n}";
                    }
                }
            }

            foreach (var data in log)
            {
                if (data.qipai != null)
                {
                    for (int l = 0; l < 4; l++) { set_qipai(data.qipai.shoupai[l]); }
                    baopai.Add(data.qipai.baopai);
                }
                else if (data.zimo != null) shan._pai[--zimo_idx] = data.zimo.p;
                else if (data.gangzimo != null) shan._pai[gang_idx++] = data.gangzimo.p;
                else if (data.kaigang != null) baopai.Add(data.kaigang.baopai);
                else if (data.hule != null && data.hule.fubaopai != null)
                    fubaopai = data.hule.fubaopai;
            }

            for (int i = 0; i < baopai.Count; i++) { shan._pai[4 + i] = baopai[i]; }
            for (int i = 0; i < fubaopai.Count; i++) { shan._pai[9 + i] = fubaopai[i]; }

            shan._baopai = new List<string> { shan._pai[4] };
            shan._fubaopai = new List<string> { shan._pai[9] };

            return shan;
        }

        public List<Reply> make_reply(int l, List<Message> log)
        {
            var reply = new List<Reply>();

            foreach (var data in log)
            {
                if (data.zimo != null || data.gangzimo != null) reply.Add(new Reply());
                else if (data.dapai != null)
                    reply.Add(l == data.dapai.l ? new Reply { dapai = data.dapai.p } : new Reply());
                else if (data.fulou != null)
                    reply.Add(l == data.fulou.l ? new Reply { fulou = data.fulou.m } : new Reply());
                else if (data.gang != null)
                    reply.Add(l == data.gang.l ? new Reply { gang = data.gang.m } : new Reply());
                else if (data.pingju != null)
                {
                    if (data.pingju.shoupai[l] != null)
                    {
                        if (Regex.IsMatch(data.pingju.name, @"^三家和"))
                            reply.Add(new Reply { hule = "-" });
                        else
                            reply.Add(new Reply { daopai = "-" });
                    }
                }
                else if (data.hule != null)
                {
                    if (l == data.hule.l) reply.Add(new Reply { hule = "-" });
                }
            }

            return reply;
        }

        private Paipu _script;

        public Game(Paipu paipu, Rule rule) : base(Enumerable.Range(0, 4).Select(x => new Player()).ToList<Majiang.Player>(), null, rule)
        {
            this._model.title = paipu.title;
            this._model.player = paipu.player;
            this._script = paipu;
        }

        public override void kaiju(int? qijia = null)
        {
            base.kaiju(this._script.qijia);
        }

        public override void qipai(Shan shan = null)
        {
            var log = this._script.log.Shift();
            for (int l = 0; l < 4; l++)
            {
                var id = (this._model.qijia + this._model.jushu + l) % 4;
                this._players[id]._reply = make_reply(l, log);
            }
            base.qipai(make_shan(this._rule, log));
        }
    }
}