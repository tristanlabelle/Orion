using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Collections;
using System.IO;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Simulation.IO
{
    public class WorldData
    {
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
        private readonly int numberOfFactions;
        private readonly Terrain terrain;
        private readonly List<ResourceNodeTemplate> resourceNodes = new List<ResourceNodeTemplate>();
        private readonly List<List<UnitTemplate>> unitsByFaction = new List<List<UnitTemplate>>();
        #endregion
        #endregion

        #region Constructors
        public WorldData(World world)
        {
            numberOfFactions = world.Factions.Count();
            terrain = world.Terrain;

            IEnumerable<Entity> concreteResourceNodes = world.Entities.Where(e => e.HasComponent<Harvestable>());
            foreach (Entity node in concreteResourceNodes)
            {
                Harvestable harvestData = node.GetComponent<Harvestable>();
                ResourceNodeTemplate template = new ResourceNodeTemplate(harvestData.Type, Point.Truncate(node.Position), harvestData.AmountRemaining);
                resourceNodes.Add(template);
            }

            foreach (Faction faction in world.Factions)
            {
                List<UnitTemplate> units = new List<UnitTemplate>();
                foreach (Unit unit in faction.Units)
                {
                    UnitTemplate template = new UnitTemplate(Point.Truncate(unit.Position), unit.Type.Name);
                    units.Add(template);
                }
                unitsByFaction.Add(units);
            }
        }

        public WorldData(BinaryReader reader)
            : this(reader, true)
        { }

        public WorldData(BinaryReader reader, bool disposeOfReader)
        {
            try
            {
                int magic = reader.ReadInt32();
                if (magic != mapFileMagic)
                    throw new InvalidOperationException("File is not an Orion map file");

                short width = reader.ReadInt16();
                short height = reader.ReadInt16();
                numberOfFactions = reader.ReadByte();

                Size size = new Size(width, height);
                BitArray2D terrainBits = new BitArray2D(size);

                byte[] rawTerrain = reader.ReadBytes(size.Width * size.Height);
                for (int i = 0; i < size.Width; i++)
                    for (int j = 0; j < size.Height; j++)
                        terrainBits[i, j] = rawTerrain[j * size.Width + i] == 0;

                // resources
                short aladdiumNodesCount = reader.ReadInt16();
                short alageneNodesCount = reader.ReadInt16();
                for (int i = 0; i < aladdiumNodesCount; i++)
                {
                    short x = reader.ReadInt16();
                    short y = reader.ReadInt16();
                    int amountRemaining = reader.ReadInt32();
                    ResourceNodeTemplate node = new ResourceNodeTemplate(ResourceType.Aladdium, new Point(x, y), amountRemaining);
                    resourceNodes.Add(node);
                }

                for (int i = 0; i < alageneNodesCount; i++)
                {
                    short x = reader.ReadInt16();
                    short y = reader.ReadInt16();
                    int amountRemaining = reader.ReadInt32();
                    ResourceNodeTemplate node = new ResourceNodeTemplate(ResourceType.Alagene, new Point(x, y), amountRemaining);
                    resourceNodes.Add(node);
                }

                // units
                for (int i = 0; i < numberOfFactions; i++)
                    unitsByFaction.Add(new List<UnitTemplate>());

                for (int i = 0; i < numberOfFactions; i++)
                {
                    short numberOfUnits = reader.ReadInt16();
                    for (int j = 0; j < numberOfUnits; j++)
                    {
                        short x = reader.ReadInt16();
                        short y = reader.ReadInt16();
                        int nameLength = reader.ReadInt16();
                        string name = new string(reader.ReadChars(nameLength));

                        UnitTemplate template = new UnitTemplate(x, y, name);
                        unitsByFaction[i].Add(template);
                    }
                }
            }
            finally
            {
                if (disposeOfReader)
                    reader.Close();
            }
        }
        #endregion

        #region Properties
        public int NumberOfFactions
        {
            get { return numberOfFactions; }
        }

        public Terrain Terrain
        {
            get { return terrain; }
        }

        public IEnumerable<ResourceNodeTemplate> AladdiumNodes
        {
            get
            {
                return resourceNodes.Where(r => r.ResourceType == ResourceType.Aladdium);
            }
        }

        public IEnumerable<ResourceNodeTemplate> AlageneNodes
        {
            get
            {
                return resourceNodes.Where(r => r.ResourceType == ResourceType.Alagene);
            }
        }
        #endregion

        #region Methods
        public IEnumerable<UnitTemplate> GetUnitsForFaction(int faction)
        {
            Argument.EnsureLower(faction, unitsByFaction.Count, "faction");
            return unitsByFaction[faction];
        }

        public void Write(string filePath)
        {
            using (FileStream stream = new FileStream(filePath, FileMode.Create))
                Write(stream);
        }

        public void Write(Stream stream)
        {
            // we have no right to close the underlying stream since it isn't ours
            // so we won't dispose the Writer.
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(mapFileMagic);
            writer.Write((short)terrain.Width);
            writer.Write((short)terrain.Height);
            writer.Write((byte)unitsByFaction.Count);

            for (int i = 0; i < terrain.Height; i++)
                for (int j = 0; j < terrain.Width; j++)
                    writer.Write(terrain.IsWalkable(new Point(j, i)));

            writer.Write((short)AladdiumNodes.Count());
            writer.Write((short)AlageneNodes.Count());

            foreach (ResourceNodeTemplate node in AladdiumNodes)
            {
                writer.Write((short)node.Location.X);
                writer.Write((short)node.Location.Y);
                writer.Write(node.RemainingAmount);
            }

            foreach (ResourceNodeTemplate node in AlageneNodes)
            {
                writer.Write((short)node.Location.X);
                writer.Write((short)node.Location.Y);
                writer.Write(node.RemainingAmount);
            }

            foreach (List<UnitTemplate> units in unitsByFaction)
            {
                writer.Write(units.Count);
                foreach (UnitTemplate unit in units)
                {
                    writer.Write((short)unit.Location.X);
                    writer.Write((short)unit.Location.Y);
                    writer.Write((int)unit.UnitTypeName.Length);
                    writer.Write(unit.UnitTypeName);
                }
            }
        }
        #endregion
    }
}
