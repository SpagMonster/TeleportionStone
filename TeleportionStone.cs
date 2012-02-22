//TeleportionStone by Archangel v2
//Drag and drop to /Scripts/Customs
using System;
using Server;
using Server.Network;
using Server.Spells;

namespace Server.Items
{
    public class TeleportionStone : Item
    {
        private bool m_Active, m_Creatures, m_CombatCheck;
        private Point3D m_PointDest;
        private Map m_MapDest;
      private string m_MissingRItemMsg;
      private string m_TeleportingMessage;
        private bool m_RequiredItemEnabled;
        private Type m_RequiredItem;
      private bool m_DeleteRequiredItem;
        private bool m_SourceEffect;
        private bool m_DestEffect;
        private int m_SoundID;
        private TimeSpan m_Delay;

       [CommandProperty(AccessLevel.GameMaster)]
        public string TeleportingMessage
        {
            get { return m_TeleportingMessage; }
            set { m_TeleportingMessage = value; InvalidateProperties(); }
        }

       [CommandProperty(AccessLevel.GameMaster)]
        public string MissingRItemMsg
        {
            get { return m_MissingRItemMsg; }
            set { m_MissingRItemMsg = value; InvalidateProperties(); }
        }
     
        [CommandProperty(AccessLevel.GameMaster)]
        public bool DeleteRequiredItem
        {
            get { return m_DeleteRequiredItem; }
            set { m_DeleteRequiredItem = value; InvalidateProperties(); }
        }
     
        [CommandProperty(AccessLevel.GameMaster)]
        public bool RequiredItemEnabled
        {
            get { return m_RequiredItemEnabled; }
            set { m_RequiredItemEnabled = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Type RequiredItem
        {
            get { return m_RequiredItem; }
            set { m_RequiredItem = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool SourceEffect
        {
            get { return m_SourceEffect; }
            set { m_SourceEffect = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool DestEffect
        {
            get { return m_DestEffect; }
            set { m_DestEffect = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SoundID
        {
            get { return m_SoundID; }
            set { m_SoundID = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan Delay
        {
            get { return m_Delay; }
            set { m_Delay = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Active
        {
            get { return m_Active; }
            set { m_Active = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Point3D PointDest
        {
            get { return m_PointDest; }
            set { m_PointDest = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Map MapDest
        {
            get { return m_MapDest; }
            set { m_MapDest = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Creatures
        {
            get { return m_Creatures; }
            set { m_Creatures = value; InvalidateProperties(); }
        }


        [CommandProperty(AccessLevel.GameMaster)]
        public bool CombatCheck
        {
            get { return m_CombatCheck; }
            set { m_CombatCheck = value; InvalidateProperties(); }
        }

        public override int LabelNumber { get { return 1026095; } } // teleporter

        [Constructable]
        public TeleportionStone()
            : this(new Point3D(0, 0, 0), null, false)
        {
        }

        [Constructable]
        public TeleportionStone(Point3D pointDest, Map mapDest)
            : this(pointDest, mapDest, false)
        {
        }

        [Constructable]
        public TeleportionStone(Point3D pointDest, Map mapDest, bool creatures)
            : base(0xED4)
        {
            Hue = (Utility.RandomMinMax(2, 2430));
            Movable = false;
            Visible = true;
            Name = "Teleportion Stone";
            m_Active = true;
            m_PointDest = pointDest;
            m_MapDest = mapDest;
            m_Creatures = creatures;
            m_RequiredItem = typeof(Apple);
            m_CombatCheck = false;
        }

        //Used mostly for debugging..allows users to see if active/map dest/point dest etc.
        /*public override void GetProperties( ObjectPropertyList list )
        {
            base.GetProperties( list );

            if ( m_Active )
                list.Add( 1060742 ); // active
            else
                list.Add( 1060743 ); // inactive

            if ( m_MapDest != null )
                list.Add( 1060658, "Map\t{0}", m_MapDest );

            if ( m_PointDest != Point3D.Zero )
                list.Add( 1060659, "Coords\t{0}", m_PointDest );

            list.Add( 1060660, "Creatures\t{0}", m_Creatures ? "Yes" : "No" );
         
        }*/


        public override void OnDoubleClick(Mobile m)
        {
            base.OnDoubleClick(m);
            if (m_RequiredItemEnabled)
            {
                if (m.Backpack.FindItemByType(m_RequiredItem) != null)
                {
                    StartTeleport(m);
                    m.SendMessage(62, m_TeleportingMessage );
                  if (m_DeleteRequiredItem == true)
                  {
                     m.Backpack.ConsumeTotal( ( m_RequiredItem ), 1 );
                  }
                    return;
                }
                m.SendMessage(38, m_MissingRItemMsg );

                return;
            }
            else
                m.SendMessage(62, m_TeleportingMessage );
            StartTeleport(m);
        }

        public virtual void StartTeleport(Mobile m)
        {
            if (m_Delay == TimeSpan.Zero)
                DoTeleport(m);
            else
                Timer.DelayCall(m_Delay, new TimerStateCallback(DoTeleport_Callback), m);
        }

        private void DoTeleport_Callback(object state)
        {
            DoTeleport((Mobile)state);
        }

        public virtual void DoTeleport(Mobile m)
        {
            Map map = m_MapDest;

            if (map == null || map == Map.Internal)
                map = m.Map;

            Point3D p = m_PointDest;

            if (p == Point3D.Zero)
                p = m.Location;

            Server.Mobiles.BaseCreature.TeleportPets(m, p, map);

            bool sendEffect = (!m.Hidden || m.AccessLevel == AccessLevel.Player);

            if (m_SourceEffect && sendEffect)
                Effects.SendLocationEffect(m.Location, m.Map, 0x3728, 10, 10);

            m.MoveToWorld(p, map);

            if (m_DestEffect && sendEffect)
                Effects.SendLocationEffect(m.Location, m.Map, 0x3728, 10, 10);

            if (m_SoundID > 0 && sendEffect)
                Effects.PlaySound(m.Location, m.Map, m_SoundID);
        }

        public TeleportionStone(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)3); // version

            writer.Write((bool)m_CombatCheck);

         writer.Write((string)m_MissingRItemMsg);
         writer.Write((string)m_TeleportingMessage);
            writer.Write((bool)m_RequiredItemEnabled);
            writer.Write((string)m_RequiredItem.ToString());
         writer.Write((bool)m_DeleteRequiredItem);
            writer.Write((bool)m_SourceEffect);
            writer.Write((bool)m_DestEffect);
            writer.Write((TimeSpan)m_Delay);
            writer.WriteEncodedInt((int)m_SoundID);

            writer.Write(m_Creatures);

            writer.Write(m_Active);
            writer.Write(m_PointDest);
            writer.Write(m_MapDest);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 3:
                    {
                        m_CombatCheck = reader.ReadBool();
                        goto case 2;
                    }
                case 2:
                    {
                  m_MissingRItemMsg = reader.ReadString();
                  m_TeleportingMessage = reader.ReadString();
                        m_RequiredItemEnabled = reader.ReadBool();
                        m_RequiredItem = Type.GetType(reader.ReadString());
                        if (m_RequiredItem == null) m_RequiredItem = typeof(Apple);
                  m_DeleteRequiredItem = reader.ReadBool();
                        m_SourceEffect = reader.ReadBool();
                        m_DestEffect = reader.ReadBool();
                        m_Delay = reader.ReadTimeSpan();
                        m_SoundID = reader.ReadEncodedInt();

                        goto case 1;
                    }
                case 1:
                    {
                        m_Creatures = reader.ReadBool();

                        goto case 0;
                    }
                case 0:
                    {
                        m_Active = reader.ReadBool();
                        m_PointDest = reader.ReadPoint3D();
                        m_MapDest = reader.ReadMap();

                        break;
                    }
            }
        }
    }
}