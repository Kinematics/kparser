using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WaywardGamers.KParser.Monitoring.Packet
{
    public class Packet
    {
        RawPacket rawPacket;

        #region Constructors
        public Packet(byte[] packetBytes)
        {
            this.rawPacket = new RawPacket(packetBytes);
            ReadRawPacket();
        }

        public Packet(string packetString, RawPacketStringType stringType)
        {
            this.rawPacket = new RawPacket(packetString, stringType);
            ReadRawPacket();
        }
        #endregion

        private void ReadRawPacket()
        {
            try
            {
                PacketID = rawPacket.ReadByte();
                PacketLength = rawPacket.ReadByte();
                PacketNum1 = rawPacket.ReadByte();
                PacketNum2 = rawPacket.ReadByte();
                PacketNum3 = rawPacket.ReadByte();
                ActorID = rawPacket.ReadInt();
                TargetCount = rawPacket.ReadInt(10);
                Category = rawPacket.ReadInt(4);
                Param = rawPacket.ReadInt(10);
                AnimationCategory = rawPacket.ReadInt(6);
                Animation = rawPacket.ReadInt(16);
                Unknown1 = rawPacket.ReadInt(32);

                Targets = new List<Target>(TargetCount);
                for (int i = 0; i < TargetCount; i++)
                {
                    Targets.Add(new Target(rawPacket));
                }

                Unknown2 = rawPacket.ReadShort(16);

                rawPacket.ReportStatus();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public byte PacketID { get; private set; }
        public byte PacketLength { get; private set; }
        public byte PacketNum1 { get; private set; }
        public byte PacketNum2 { get; private set; }
        public byte PacketNum3 { get; private set; }
        public int ActorID { get; private set; }
        public int TargetCount { get; private set; }

        public int Category { get; private set; }
        public int Param { get; private set; }
        public int AnimationCategory { get; private set; }
        public int Animation { get; private set; }
        public int Unknown1 { get; private set; }
        public short Unknown2 { get; private set; }

        public List<Target> Targets { get; private set; }
    }

    public class Target
    {
        public Target(RawPacket rawPacket)
        {
            ReadTargetData(rawPacket);
        }

        private void ReadTargetData(RawPacket rawPacket)
        {
            TargetID = rawPacket.ReadInt();
            ActionCount = rawPacket.ReadInt(4);

            Actions = new List<Action>(ActionCount);
            for (int i = 0; i < ActionCount; i++)
            {
                Actions.Add(new Action(rawPacket));
            }
        }

        public int TargetID { get; private set; }
        public int ActionCount { get; private set; }

        public List<Action> Actions { get; private set; }
    }


    public class Action
    {
        public Action(RawPacket rawPacket)
        {
            ReadActionData(rawPacket);
        }

        private void ReadActionData(RawPacket rawPacket)
        {
            Unknown1 = rawPacket.ReadBool();

            Reaction = rawPacket.ReadInt(5);
            Animation = rawPacket.ReadInt(11);
            Effect = rawPacket.ReadInt(4);
            Stagger = rawPacket.ReadInt(5);
            Param = rawPacket.ReadInt(17);
            Message = rawPacket.ReadInt(10);

            Unknown2 = rawPacket.ReadInt(32);

            HasAdditionalEffect = rawPacket.ReadBool();
            if (HasAdditionalEffect)
            {
                AdditionalEffect = new ExtraEffect(rawPacket);
            }
            else
            {
                AdditionalEffect = null;
            }

            HasSpikeEffect = rawPacket.ReadBool();
            if (HasSpikeEffect)
            {
                SpikeEffect = new ExtraEffect(rawPacket);
            }
            else
            {
                SpikeEffect = null;
            }


        }

        public int Reaction { get; private set; }
        public int Animation { get; private set; }
        public int Effect { get; private set; }
        public int Stagger { get; private set; }
        public int Param { get; private set; }
        public int Message { get; private set; }

        public bool HasAdditionalEffect { get; private set; }
        public ExtraEffect AdditionalEffect { get; private set; }

        public bool HasSpikeEffect { get; private set; }
        public ExtraEffect SpikeEffect { get; private set; }

        public bool Unknown1 { get; private set; }
        public int Unknown2 { get; private set; }

    }

    public class ExtraEffect
    {
        public ExtraEffect(RawPacket rawPacket)
        {
            ReadEffectData(rawPacket);
        }

        private void ReadEffectData(RawPacket rawPacket)
        {
            Animation = rawPacket.ReadInt(6);
            Effect = rawPacket.ReadInt(4);
            Param = rawPacket.ReadInt(14);
            Message = rawPacket.ReadInt(13);
        }

        public int Animation { get; private set; }
        public int Effect { get; private set; }
        public int Param { get; private set; }
        public int Message { get; private set; }
    }

}
