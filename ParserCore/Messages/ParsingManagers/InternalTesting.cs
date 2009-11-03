using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WaywardGamers.KParser.Parsing
{
    public class InternalTesting
    {
        public void RunATest()
        {

            string chatText1 = "3b,ee,d0,80808050,000011db,000014f1,0022,00,01,02,00,The Qiqirn Astrologer uses Faze.";
            string chatText2 = "3b,ee,d0,80808050,000011db,000014f2,0019,00,01,02,00,No effect on Motenten.1";
            ChatLine chatLine1 = new ChatLine(chatText1);
            MessageLine msgLine1 = new MessageLine(chatLine1);
            ChatLine chatLine2 = new ChatLine(chatText2);
            MessageLine msgLine2 = new MessageLine(chatLine2);

            Message msg1 = Parser.Parse(msgLine1);

            MsgManager.Instance.Reset();
            EntityManager.Instance.AddEntitiesFromMessage(msg1);
            MsgManager.Instance.AddMessageToMessageCollection(msg1);

            // Save msg1 in MsgManager
            Message msg = Parser.Parse(msgLine2);

            int i = 0;
        }
    }
}
