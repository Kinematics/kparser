using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WaywardGamers.KParser.Parsing
{
    public class InternalTesting
    {
        public void RunA1LineTest()
        {
            string chatText = "1a,ef,91,80707070,00001487,00001772,004b,00,01,02,00,1 of the Hilltroll Red Mage's shadows absorbs the damage and disappears.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            MsgManager.Instance.Reset();

            Message msg = Parser.Parse(msgLine);

            //MsgManager.Instance.Reset();
            EntityManager.Instance.AddEntitiesFromMessage(msg);
            MsgManager.Instance.AddMessageToMessageCollection(msg);
        }

        public void RunA2LineTest()
        {
            string chatText1 = "3b,18,2d,80808050,00001ef4,000023b8,001d,00,01,02,00,Motenten uses Spectral Jig.";
            string chatText2 = "3b,18,2d,80808050,00001ef4,000023b9,0019,00,01,02,00,No effect on Motenten.1";
            ChatLine chatLine1 = new ChatLine(chatText1);
            MessageLine msgLine1 = new MessageLine(chatLine1);
            ChatLine chatLine2 = new ChatLine(chatText2);
            MessageLine msgLine2 = new MessageLine(chatLine2);

            MsgManager.Instance.Reset();

            Message msg1 = Parser.Parse(msgLine1);

            //MsgManager.Instance.Reset();
            EntityManager.Instance.AddEntitiesFromMessage(msg1);
            MsgManager.Instance.AddMessageToMessageCollection(msg1);

            // Save msg1 in MsgManager
            Message msg = Parser.Parse(msgLine2);
        }
    }
}
