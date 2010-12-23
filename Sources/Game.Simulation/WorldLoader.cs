using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Collections;
using OpenTK;
using System.IO;
using System.Diagnostics;

namespace Orion.Game.Simulation
{
    public class WorldLoader : WorldGenerator
    {
        #region Nested types
        private class UnitInstance
        {
            public readonly Point Location;
            public readonly string UnitName;

            public UnitInstance(short x, short y, string name)
                : this(new Point(x, y), name)
            { }

            public UnitInstance(Point location, string unitName)
            {
                Location = location;
                UnitName = unitName;
            }
        }
        #endregion

        #region Static
        #region Fields
        private const int mapFileMagic = 0xdefaced;
        #endregion

        #region Methods
        public bool TryCreate(string filePath, out WorldLoader loader)
        {
            try
            {
                loader = new WorldLoader(filePath);
                return true;
            }
            catch
            {
                loader = null;
                return false;
            }
        }
        #endregion
        #endregion

        #region Instance
        #region Fields
        private readonly int numberOfPlayers;
        private readonly Size size;
        private readonly BitArray2D terrain;
        private readonly List<Point> aladdiumNodes;
        private readonly List<Point> alageneNodes;
        private readonly List<List<UnitInstance>> unitsByFaction = new List<List<UnitInstance>>();
        #endregion

        #region Constructors
        public WorldLoader(string filePath)
        {
            Stream fileStream = new FileStream(filePath, FileMode.Open);
            // XXX messemble que BinaryReader Dispose() son stream quand il est lui-même disposé
            using (BinaryReader reader = new BinaryReader(fileStream))
            {
                int magic = reader.ReadInt32();
                if (magic != mapFileMagic)
                    throw new InvalidOperationException("File is not an Orion map file");

                short width = reader.ReadInt16();
                short height = reader.ReadInt16();
                numberOfPlayers = reader.ReadByte();

                size = new Size(width, height);
                terrain = new BitArray2D(size);

                // terrain bits
                byte[] rawTerrain = reader.ReadBytes(size.Width * size.Height);

                for (int i = 0; i < size.Width; i++)
                    for (int j = 0; j < size.Height; j++)
                        terrain[i, j] = rawTerrain[j * size.Width + i] == 0;

                // positioning of aladdium and alagene nodes
                aladdiumNodes = new List<Point>();
                alageneNodes = new List<Point>();
                short aladdiumNodesCount = reader.ReadInt16();
                short alageneNodesCount = reader.ReadInt16();
                for (int i = 0; i < aladdiumNodesCount; i++)
                {
                    short x = reader.ReadInt16();
                    short y = reader.ReadInt16();
                    aladdiumNodes.Add(new Point(x, y));
                }

                for (int i = 0; i < alageneNodesCount; i++)
                {
                    short x = reader.ReadInt16();
                    short y = reader.ReadInt16();
                    alageneNodes.Add(new Point(x, y));
                }

                // units already on the map
                for (int i = 0; i < numberOfPlayers; i++)
                    unitsByFaction.Add(new List<UnitInstance>());

                short numberOfUnits = reader.ReadInt16();
                for (int i = 0; i < numberOfUnits; i++)
                {
                    byte player = reader.ReadByte();
                    short x = reader.ReadInt16();
                    short y = reader.ReadInt16();
                    int nameLength = reader.ReadInt16();
                    string name = new string(reader.ReadChars(nameLength));

                    UnitInstance instance = new UnitInstance(x, y, name);
                    unitsByFaction[player].Add(instance);
                }
            }
        }
        #endregion

        #region Methods
        public override Terrain GenerateTerrain()
        {
            return new Terrain(terrain);
        }

        public override void PrepareWorld(World world, UnitTypeRegistry unitTypes)
        {
            // create resource nodes
            foreach (Point nodeLocation in aladdiumNodes)
                world.Entities.CreateResourceNode(ResourceType.Aladdium, nodeLocation);

            foreach (Point nodeLocation in alageneNodes)
                world.Entities.CreateResourceNode(ResourceType.Alagene, nodeLocation);

            // place units
            Debug.Assert(world.Factions.Count() <= unitsByFaction.Count,
                "There are more factions than this map supports.");

            IEnumerator<Faction> factionEnumerator = world.Factions.GetEnumerator();
            for (int i = 0; i < unitsByFaction.Count; i++)
            {
                factionEnumerator.MoveNext();
                Faction currentFaction = factionEnumerator.Current;
                foreach (UnitInstance instance in unitsByFaction[i])
                {
                    UnitType type = unitTypes.FromName(instance.UnitName);
                    currentFaction.CreateUnit(type, instance.Location);
                }
            }
        }
        #endregion
        #endregion
    }
}
