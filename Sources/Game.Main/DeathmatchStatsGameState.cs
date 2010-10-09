using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Engine.Graphics;
using Orion.Engine.Gui;
using Orion.Game.Presentation;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Utilities;

namespace Orion.Game.Main
{
    internal sealed class FactionFoodChartModel : ChartModel
    {
        #region Fields
        private readonly WorldFoodSampler sampler;
        #endregion

        #region Constructors
        public FactionFoodChartModel(WorldFoodSampler sampler)
        {
            Argument.EnsureNotNull(sampler, "sampler");
            this.sampler = sampler;
        }
        #endregion

        #region Properties
        public override string Title
        {
            get { return "Nourriture par faction dans le temps"; }
        }

        public override int CategoryCount
        {
            get { return sampler.SampledFactions.Count(); }
        }

        public override int EntryCount
        {
            get { return sampler.SampleCount; }
        }
        #endregion

        #region Methods
        public override string GetCategoryLabel(int index)
        {
            return sampler.SampledFactions.ElementAt(index).Name;
        }

        public override ColorRgb GetCategoryColor(int index)
        {
            return sampler.SampledFactions.ElementAt(index).Color;
        }

        public override string GetEntryLabel(int index)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(sampler.SamplingPeriod * index);
            return timeSpan.ToString();
        }

        public override float GetValue(int categoryIndex, int entryIndex)
        {
            Faction faction = sampler.SampledFactions.ElementAt(categoryIndex);
            return sampler.GetFactionFoodUsageSample(faction, entryIndex);
        }
        #endregion
    }

    /// <summary>
    /// A game state which presents statistics on a deathmatch game.
    /// </summary>
    public sealed class DeathmatchStatsGameState : GameState
    {
        #region Fields
        private const int sampledPointsPerCategory = 50;

        private readonly GameGraphics graphics;
        private readonly ChartModel chartModel;
        private readonly NormalizedZoneChartRenderer renderer;
        #endregion

        #region Constructors
        public DeathmatchStatsGameState(GameStateManager manager, GameGraphics graphics, WorldFoodSampler sampler)
            : base(manager)
        {
            Argument.EnsureNotNull(graphics, "graphics");
            Argument.EnsureNotNull(sampler, "sampler");

            this.graphics = graphics;
            this.chartModel = new FactionFoodChartModel(sampler);
            this.renderer = new NormalizedZoneChartRenderer(this.chartModel, 50);
        }
        #endregion

        #region Properties
        #endregion

        #region Methods
        protected internal override void Draw(GameGraphics graphics)
        {
            var context = graphics.Context;
            renderer.Draw(context, context.ProjectionBounds);
        }
        #endregion
    }
}
