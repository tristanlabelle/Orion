﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using Orion.Engine;
using System.IO;

namespace Orion.Game.Matchmaking
{
    /// <summary>
    /// Stores the information needed to setupa deathmatch.
    /// </summary>
    [Serializable]
    public sealed class MatchSettings
    {
        #region Instance
        #region Fields
        private Action<Player> onColorChanged;

        private Size mapSize = new Size(150, 150);
        
        private int foodLimit = 200;
        private int initialAladdiumAmount = 200;
        private int initialAlageneAmount;
        private int randomSeed = (int)Environment.TickCount;

        private bool revealTopology;
        private bool startNomad;
        private bool areCheatsEnabled;
        private bool areRandomHeroesEnabled = true;

        private List<Player> players = new List<Player>();
        #endregion

        #region Constructors
        public MatchSettings()
        {
            onColorChanged = OnColorChanged;
        }

        public MatchSettings(IEnumerable<Player> players)
            : this()
        {
            this.players.AddRange(players);
        }
        #endregion

        #region Events
        public event Action<MatchSettings> MapSizeChanged;
        public event Action<MatchSettings> FoodLimitChanged;
        public event Action<MatchSettings> InitialAladdiumAmountChanged;
        public event Action<MatchSettings> InitialAlageneAmountChanged;
        public event Action<MatchSettings> RandomSeedChanged;
        public event Action<MatchSettings> RevealTopologyChanged;
        public event Action<MatchSettings> StartNomadChanged;
        public event Action<MatchSettings> AreCheatsEnabledChanged;
        public event Action<MatchSettings> AreRandomHeroesEnabledChanged;
        public event Action<MatchSettings, Player> PlayerJoined;
        public event Action<MatchSettings, Player> PlayerLeft;
        public event Action<MatchSettings, Player> PlayerChanged;
        public event Action<MatchSettings> Changed;
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the size of the world map.
        /// </summary>
        public Size MapSize
        {
            get { return mapSize; }
            set
            {
                if (value == mapSize) return;
                mapSize = value;
                TriggerEvent(MapSizeChanged);
            }
        }

        /// <summary>
        /// Accesses the maximum amount of food that can be used by factions.
        /// </summary>
        public int FoodLimit
        {
            get { return foodLimit; }
            set
            {
                if (value == foodLimit) return;
                Argument.EnsurePositive(value, "FoodLimit");
                foodLimit = value;
                TriggerEvent(FoodLimitChanged);
            }
        }

        /// <summary>
        /// Accesses the amount of aladdium units factions start with.
        /// </summary>
        public int InitialAladdiumAmount
        {
            get { return initialAladdiumAmount; }
            set
            {
                if (value == initialAladdiumAmount) return;
                Argument.EnsurePositive(value, "InitialAladdiumAmount");
                initialAladdiumAmount = value;
                TriggerEvent(InitialAladdiumAmountChanged);
            }
        }

        /// <summary>
        /// Accesses the amount of alagene units factions start with.
        /// </summary>
        public int InitialAlageneAmount
        {
            get { return initialAlageneAmount; }
            set
            {
                if (value == initialAlageneAmount) return;
                Argument.EnsurePositive(value, "InitialAlageneAmount");
                initialAlageneAmount = value;
                TriggerEvent(InitialAlageneAmountChanged);
            }
        }

        /// <summary>
        /// Accesses the value used as the game's random seed.
        /// </summary>
        public int RandomSeed
        {
            get { return randomSeed; }
            set
            {
                if (value == randomSeed) return;
                randomSeed = value;
                TriggerEvent(RandomSeedChanged);
            }
        }

        /// <summary>
        /// Accesses a value indicating if the topology of the map is visible when the game starts.
        /// </summary>
        public bool RevealTopology
        {
            get { return revealTopology; }
            set
            {
                if (value == revealTopology) return;
                revealTopology = value;
                TriggerEvent(RevealTopologyChanged);
            }
        }

        /// <summary>
        /// Accesses a value indicating if the factions should start nomad,
        /// without any buildings.
        /// </summary>
        public bool StartNomad
        {
            get { return startNomad; }
            set
            {
                if (value == startNomad) return;
                startNomad = value;
                TriggerEvent(StartNomadChanged);
            }
        }

        /// <summary>
        /// Accesses a value indicating if cheats can be used in the game.
        /// </summary>
        public bool AreCheatsEnabled
        {
            get { return areCheatsEnabled; }
            set
            {
                if (value == areCheatsEnabled) return;
                areCheatsEnabled = value;
                TriggerEvent(AreCheatsEnabledChanged);
            }
        }

        /// <summary>
        /// Accesses a value indicating if heroes can randomly spawn instead of their basic unit type.
        /// </summary>
        public bool AreRandomHeroesEnabled
        {
            get { return areRandomHeroesEnabled; }
            set
            {
                if (value == areRandomHeroesEnabled) return;
                areRandomHeroesEnabled = value;
                TriggerEvent(AreRandomHeroesEnabledChanged);
            }
        }

        /// <summary>
        /// Accesses the list of players that joined the game.
        /// </summary>
        public IEnumerable<Player> Players
        {
            get { return players; }
        }
        #endregion

        #region Methods
        public void AddPlayer(Player player)
        {
            players.Add(player);
            player.ColorChanged += onColorChanged;
            TriggerEvent(PlayerJoined, player);
        }

        public void RemovePlayer(Player player)
        {
            players.Remove(player);
            player.ColorChanged -= onColorChanged;
            TriggerEvent(PlayerLeft, player);
        }

        /// <summary>
        /// Serializes this object to its binary representation.
        /// </summary>
        /// <param name="writer">The binary writer to which to write the serialized data.</param>
        public void Serialize(BinaryWriter writer)
        {
            Argument.EnsureNotNull(writer, "writer");
            writer.Write(mapSize.Width);
            writer.Write(mapSize.Height);
            writer.Write(foodLimit);
            writer.Write(initialAladdiumAmount);
            writer.Write(initialAlageneAmount);
            writer.Write(randomSeed);
            writer.Write(revealTopology);
            writer.Write(startNomad);
            writer.Write(areCheatsEnabled);
            writer.Write(areRandomHeroesEnabled);

            writer.Write(players.Count);
            foreach (float component in players.SelectMany(player => player.Color.ToArray()))
                writer.Write(component);
        }

        /// <summary>
        /// Deserializes the binary representation of this object in the current instance.
        /// </summary>
        /// <param name="reader">The data reader to be used.</param>
        public void Deserialize(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            MapSize = new Size(reader.ReadInt32(), reader.ReadInt32());
            FoodLimit = reader.ReadInt32();
            InitialAladdiumAmount = reader.ReadInt32();
            InitialAlageneAmount = reader.ReadInt32();
            RandomSeed = reader.ReadInt32();
            RevealTopology = reader.ReadBoolean();
            StartNomad = reader.ReadBoolean();
            AreCheatsEnabled = reader.ReadBoolean();
            AreRandomHeroesEnabled = reader.ReadBoolean();

            int numberOfPlayers = reader.ReadInt32();
            players = new List<Player>(numberOfPlayers);
            for(int i = 0; i < numberOfPlayers; i++)
            {
                float r = reader.ReadSingle();
                float g = reader.ReadSingle();
                float b = reader.ReadSingle();
                players.Add(new LocalPlayer(new ColorRgb(r, g, b)));
            }
        }

        #region Events Handling
        private void OnColorChanged(Player player)
        {
            TriggerEvent(PlayerChanged, player);
        }

        private void TriggerEvent(Action<MatchSettings> eventHandler)
        {
            eventHandler.Raise(this);
            Changed.Raise(this);
        }

        private void TriggerEvent(Action<MatchSettings, Player> eventHandler, Player player)
        {
            eventHandler.Raise(this, player);
            Changed.Raise(this);
        }
        #endregion
        #endregion
        #endregion

        #region Static
        #region Fields
#if DEBUG
        public static readonly Size SuggestedMinimumMapSize = new Size(1, 1);
#else
        public static readonly Size SuggestedMinimumMapSize = new Size(50, 50);
#endif
        public const int SuggestedMinimumPopulation = 4;
        public const int SuggestedMinimumAlagene = 0;
        public const int SuggestedMinimumAladdium = 0;
        #endregion
        #endregion
    }
}
