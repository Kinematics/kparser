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
            string chatText1 = "a3,00,00,80c08080,000003ec,00000485,003c,00,01,02,00,Vixx hits the Vanguard Enchanter for 90 points of damage.1";
            string chatText2 = "bb,00,00,8090c0f0,0000040b,000004ad,003f,00,01,02,00,Additional effect: 3 TP drained from the Vanguard Enchanter.1";
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
