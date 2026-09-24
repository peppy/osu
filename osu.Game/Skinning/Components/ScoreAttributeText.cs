// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation.SkinComponents;
using osu.Game.Scoring;

namespace osu.Game.Skinning.Components
{
    [UsedImplicitly]
    public partial class ScoreAttributeText : FontAdjustableSkinComponent
    {
        public const float DEFAULT_TEXT_SIZE = 40;

        [SettingSource(typeof(BeatmapAttributeTextStrings), nameof(BeatmapAttributeTextStrings.Attribute))]
        public Bindable<ScoreAttribute> Attribute { get; } = new Bindable<ScoreAttribute>();

        [SettingSource(typeof(BeatmapAttributeTextStrings), nameof(BeatmapAttributeTextStrings.Template), nameof(BeatmapAttributeTextStrings.TemplateDescription))]
        public Bindable<string> Template { get; } = new Bindable<string>("{Label}: {Value}");

        [Resolved]
        private IBindable<IScoreInfo> score { get; set; } = null!;

        private readonly OsuSpriteText text;

        public ScoreAttributeText()
        {
            AutoSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                text = new OsuSpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Attribute.BindValueChanged(_ => updateText());
            Template.BindValueChanged(_ => updateText());

            score.BindValueChanged(_ => updateText());
            updateText();
        }

        private void updateText()
        {
            string numberedTemplate = Template.Value
                                              .Replace("{", "{{")
                                              .Replace("}", "}}")
                                              .Replace(@"{{Label}}", "{0}")
                                              .Replace(@"{{Value}}", "{1}");

            List<object?> values = new List<object?>
            {
                getLabelString(Attribute.Value),
                getValueString(Attribute.Value)
            };

            foreach (var type in Enum.GetValues<ScoreAttribute>())
            {
                string replaced = numberedTemplate.Replace($@"{{{{{type}}}}}", $@"{{{values.Count}}}");

                if (numberedTemplate != replaced)
                {
                    numberedTemplate = replaced;
                    values.Add(getValueString(type));
                }
            }

            text.Text = LocalisableString.Format(numberedTemplate, values.ToArray());
        }

        private LocalisableString getLabelString(ScoreAttribute attribute)
        {
            switch (attribute)
            {
                case ScoreAttribute.Username:
                    return "Played by";

                case ScoreAttribute.Date:
                    return "Date";

                default:
                    return string.Empty;
            }
        }

        private LocalisableString getValueString(ScoreAttribute attribute)
        {
            switch (attribute)
            {
                case ScoreAttribute.Username:
                    return score.Value.User.Username;

                case ScoreAttribute.Date:
                    return score.Value.Date.ToString(@"G");

                default:
                    return string.Empty;
            }
        }

        protected override void SetFont(FontUsage font) => text.Font = font.With(size: DEFAULT_TEXT_SIZE);

        protected override void SetTextColour(Colour4 textColour) => text.Colour = textColour;
    }

    // WARNING: DO NOT ADD ANY VALUES TO THIS ENUM ANYWHERE ELSE THAN AT THE END.
    // Doing so will break existing user skins.
    public enum ScoreAttribute
    {
        Username,
        Date,
    }
}
