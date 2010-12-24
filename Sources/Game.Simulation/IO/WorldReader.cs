using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Collections;
using System.IO;

namespace Orion.Game.Simulation.IO
{
    public class WorldReader
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
        private readonly List<ResourceNodeTemplate> resourceNodes;
        private readonly List<List<UnitTemplate>> unitsByFaction = new List<List<UnitTemplate>>();
        #endregion
        #endregion

        #region Constructors
        public WorldReader(BinaryReader reader)
            : this(reader, true)
        { }

        public WorldReader(BinaryReader reader, bool disposeOfReader)
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
                resourceNodes = new List<ResourceNodeTemplate>();
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

                short numberOfUnits = reader.ReadInt16();
                for (int i = 0; i < numberOfUnits; i++)
                {
                    byte player = reader.ReadByte();
                    short x = reader.ReadInt16();
                    short y = reader.ReadInt16();
                    int nameLength = reader.ReadInt16();
                    string name = new string(reader.ReadChars(nameLength));

                    UnitTemplate template = new UnitTemplate(x, y, name);
                    unitsByFaction[player].Add(template);
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
        #endregion
    }
}
