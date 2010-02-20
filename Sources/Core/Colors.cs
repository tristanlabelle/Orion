using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Orion
{
    /// <summary>
    /// Provides numerous typical colors.
    /// </summary>
    public static class Colors
    {
        #region Fields
        public static readonly ColorRgba TransparentBlack = new ColorRgba(0, 0, 0, 0);
        public static readonly ColorRgba TransparentWhite = new ColorRgba(1, 1, 1, 0);

        public static readonly ColorRgb PureRed = new ColorRgb(1, 0, 0);
        public static readonly ColorRgb PureBlue = new ColorRgb(0, 1, 0);
        public static readonly ColorRgb PureGreen = new ColorRgb(0, 0, 1);

        public static readonly ColorRgb PureYellow = new ColorRgb(1, 1, 0);
        public static readonly ColorRgb PureCyan = new ColorRgb(0, 1, 1);
        public static readonly ColorRgb PureMagenta = new ColorRgb(1, 0, 1);

        public static readonly ColorRgb AliceBlue = new ColorRgb(240 / 255f, 248 / 255f, 1);
        public static readonly ColorRgb AntiqueWhite = new ColorRgb(250 / 255f, 235 / 255f, 215 / 255f);
        public static readonly ColorRgb Aqua = new ColorRgb(0, 1, 1);
        public static readonly ColorRgb Aquamarine = new ColorRgb(127 / 255f, 1, 212 / 255f);
        public static readonly ColorRgb Azure = new ColorRgb(240 / 255f, 1, 1);
        public static readonly ColorRgb Beige = new ColorRgb(245 / 255f, 245 / 255f, 220 / 255f);
        public static readonly ColorRgb Bisque = new ColorRgb(1, 228 / 255f, 196 / 255f);
        public static readonly ColorRgb Black = new ColorRgb(0, 0, 0);
        public static readonly ColorRgb BlanchedAlmond = new ColorRgb(1, 235 / 255f, 205 / 255f);
        public static readonly ColorRgb Blue = new ColorRgb(0, 0, 1);
        public static readonly ColorRgb BlueViolet = new ColorRgb(138 / 255f, 43 / 255f, 226 / 255f);
        public static readonly ColorRgb Brown = new ColorRgb(165 / 255f, 42 / 255f, 42 / 255f);
        public static readonly ColorRgb BurlyWood = new ColorRgb(222 / 255f, 184 / 255f, 135 / 255f);
        public static readonly ColorRgb CadetBlue = new ColorRgb(95 / 255f, 158 / 255f, 160 / 255f);
        public static readonly ColorRgb Chartreuse = new ColorRgb(127 / 255f, 1, 0);
        public static readonly ColorRgb Chocolate = new ColorRgb(210 / 255f, 105 / 255f, 30 / 255f);
        public static readonly ColorRgb Coral = new ColorRgb(1, 127 / 255f, 80 / 255f);
        public static readonly ColorRgb CornflowerBlue = new ColorRgb(100 / 255f, 149 / 255f, 237 / 255f);
        public static readonly ColorRgb Cornsilk = new ColorRgb(1, 248 / 255f, 220 / 255f);
        public static readonly ColorRgb Crimson = new ColorRgb(220 / 255f, 20 / 255f, 60 / 255f);
        public static readonly ColorRgb Cyan = new ColorRgb(0, 1, 1);
        public static readonly ColorRgb DarkBlue = new ColorRgb(0, 0, 139 / 255f);
        public static readonly ColorRgb DarkCyan = new ColorRgb(0, 139 / 255f, 139 / 255f);
        public static readonly ColorRgb DarkGoldenrod = new ColorRgb(184 / 255f, 134 / 255f, 11 / 255f);
        public static readonly ColorRgb DarkGray = new ColorRgb(169 / 255f, 169 / 255f, 169 / 255f);
        public static readonly ColorRgb DarkGreen = new ColorRgb(0, 100 / 255f, 0);
        public static readonly ColorRgb DarkKhaki = new ColorRgb(189 / 255f, 183 / 255f, 107 / 255f);
        public static readonly ColorRgb DarkMagenta = new ColorRgb(139 / 255f, 0, 139 / 255f);
        public static readonly ColorRgb DarkOliveGreen = new ColorRgb(85 / 255f, 107 / 255f, 47 / 255f);
        public static readonly ColorRgb DarkOrange = new ColorRgb(1, 140 / 255f, 0);
        public static readonly ColorRgb DarkOrchid = new ColorRgb(153 / 255f, 50 / 255f, 204 / 255f);
        public static readonly ColorRgb DarkRed = new ColorRgb(139 / 255f, 0, 0);
        public static readonly ColorRgb DarkSalmon = new ColorRgb(233 / 255f, 150 / 255f, 122 / 255f);
        public static readonly ColorRgb DarkSeaGreen = new ColorRgb(143 / 255f, 188 / 255f, 139 / 255f);
        public static readonly ColorRgb DarkSlateBlue = new ColorRgb(72 / 255f, 61 / 255f, 139 / 255f);
        public static readonly ColorRgb DarkSlateGray = new ColorRgb(47 / 255f, 79 / 255f, 79 / 255f);
        public static readonly ColorRgb DarkTurquoise = new ColorRgb(0, 206 / 255f, 209 / 255f);
        public static readonly ColorRgb DarkViolet = new ColorRgb(148 / 255f, 0, 211 / 255f);
        public static readonly ColorRgb DeepPink = new ColorRgb(1, 20 / 255f, 147 / 255f);
        public static readonly ColorRgb DeepSkyBlue = new ColorRgb(0, 191 / 255f, 1);
        public static readonly ColorRgb DimGray = new ColorRgb(105 / 255f, 105 / 255f, 105 / 255f);
        public static readonly ColorRgb DodgerBlue = new ColorRgb(30 / 255f, 144 / 255f, 1);
        public static readonly ColorRgb Firebrick = new ColorRgb(178 / 255f, 34 / 255f, 34 / 255f);
        public static readonly ColorRgb FloralWhite = new ColorRgb(1, 250 / 255f, 240 / 255f);
        public static readonly ColorRgb ForestGreen = new ColorRgb(34 / 255f, 139 / 255f, 34 / 255f);
        public static readonly ColorRgb Fuchsia = new ColorRgb(1, 0, 1);
        public static readonly ColorRgb Gainsboro = new ColorRgb(220 / 255f, 220 / 255f, 220 / 255f);
        public static readonly ColorRgb GhostWhite = new ColorRgb(248 / 255f, 248 / 255f, 1);
        public static readonly ColorRgb Gold = new ColorRgb(1, 215 / 255f, 0);
        public static readonly ColorRgb Goldenrod = new ColorRgb(218 / 255f, 165 / 255f, 32 / 255f);
        public static readonly ColorRgb Gray = new ColorRgb(128 / 255f, 128 / 255f, 128 / 255f);
        public static readonly ColorRgb Green = new ColorRgb(0, 128 / 255f, 0);
        public static readonly ColorRgb GreenYellow = new ColorRgb(173 / 255f, 1, 47 / 255f);
        public static readonly ColorRgb Honeydew = new ColorRgb(240 / 255f, 1, 240 / 255f);
        public static readonly ColorRgb HotPink = new ColorRgb(1, 105 / 255f, 180 / 255f);
        public static readonly ColorRgb IndianRed = new ColorRgb(205 / 255f, 92 / 255f, 92 / 255f);
        public static readonly ColorRgb Indigo = new ColorRgb(75 / 255f, 0, 130 / 255f);
        public static readonly ColorRgb Ivory = new ColorRgb(1, 1, 240 / 255f);
        public static readonly ColorRgb Khaki = new ColorRgb(240 / 255f, 230 / 255f, 140 / 255f);
        public static readonly ColorRgb Lavender = new ColorRgb(230 / 255f, 230 / 255f, 250 / 255f);
        public static readonly ColorRgb LavenderBlush = new ColorRgb(1, 240 / 255f, 245 / 255f);
        public static readonly ColorRgb LawnGreen = new ColorRgb(124 / 255f, 252 / 255f, 0);
        public static readonly ColorRgb LemonChiffon = new ColorRgb(1, 250 / 255f, 205 / 255f);
        public static readonly ColorRgb LightBlue = new ColorRgb(173 / 255f, 216 / 255f, 230 / 255f);
        public static readonly ColorRgb LightCoral = new ColorRgb(240 / 255f, 128 / 255f, 128 / 255f);
        public static readonly ColorRgb LightCyan = new ColorRgb(224 / 255f, 1, 1);
        public static readonly ColorRgb LightGoldenrodYellow = new ColorRgb(250 / 255f, 250 / 255f, 210 / 255f);
        public static readonly ColorRgb LightGreen = new ColorRgb(144 / 255f, 238 / 255f, 144 / 255f);
        public static readonly ColorRgb LightGray = new ColorRgb(211 / 255f, 211 / 255f, 211 / 255f);
        public static readonly ColorRgb LightPink = new ColorRgb(1, 182 / 255f, 193 / 255f);
        public static readonly ColorRgb LightSalmon = new ColorRgb(1, 160 / 255f, 122 / 255f);
        public static readonly ColorRgb LightSeaGreen = new ColorRgb(32 / 255f, 178 / 255f, 170 / 255f);
        public static readonly ColorRgb LightSkyBlue = new ColorRgb(135 / 255f, 206 / 255f, 250 / 255f);
        public static readonly ColorRgb LightSlateGray = new ColorRgb(119 / 255f, 136 / 255f, 153 / 255f);
        public static readonly ColorRgb LightSteelBlue = new ColorRgb(176 / 255f, 196 / 255f, 222 / 255f);
        public static readonly ColorRgb LightYellow = new ColorRgb(1, 1, 224 / 255f);
        public static readonly ColorRgb Lime = new ColorRgb(0, 1, 0);
        public static readonly ColorRgb LimeGreen = new ColorRgb(50 / 255f, 205 / 255f, 50 / 255f);
        public static readonly ColorRgb Linen = new ColorRgb(250 / 255f, 240 / 255f, 230 / 255f);
        public static readonly ColorRgb Magenta = new ColorRgb(1, 0, 1);
        public static readonly ColorRgb Maroon = new ColorRgb(128 / 255f, 0, 0);
        public static readonly ColorRgb MediumAquamarine = new ColorRgb(102 / 255f, 205 / 255f, 170 / 255f);
        public static readonly ColorRgb MediumBlue = new ColorRgb(0, 0, 205 / 255f);
        public static readonly ColorRgb MediumOrchid = new ColorRgb(186 / 255f, 85 / 255f, 211 / 255f);
        public static readonly ColorRgb MediumPurple = new ColorRgb(147 / 255f, 112 / 255f, 219 / 255f);
        public static readonly ColorRgb MediumSeaGreen = new ColorRgb(60 / 255f, 179 / 255f, 113 / 255f);
        public static readonly ColorRgb MediumSlateBlue = new ColorRgb(123 / 255f, 104 / 255f, 238 / 255f);
        public static readonly ColorRgb MediumSpringGreen = new ColorRgb(0, 250 / 255f, 154 / 255f);
        public static readonly ColorRgb MediumTurquoise = new ColorRgb(72 / 255f, 209 / 255f, 204 / 255f);
        public static readonly ColorRgb MediumVioletRed = new ColorRgb(199 / 255f, 21 / 255f, 133 / 255f);
        public static readonly ColorRgb MidnightBlue = new ColorRgb(25 / 255f, 25 / 255f, 112 / 255f);
        public static readonly ColorRgb MintCream = new ColorRgb(245 / 255f, 1, 250 / 255f);
        public static readonly ColorRgb MistyRose = new ColorRgb(1, 228 / 255f, 225 / 255f);
        public static readonly ColorRgb Moccasin = new ColorRgb(1, 228 / 255f, 181 / 255f);
        public static readonly ColorRgb NavajoWhite = new ColorRgb(1, 222 / 255f, 173 / 255f);
        public static readonly ColorRgb Navy = new ColorRgb(0, 0, 128 / 255f);
        public static readonly ColorRgb OldLace = new ColorRgb(253 / 255f, 245 / 255f, 230 / 255f);
        public static readonly ColorRgb Olive = new ColorRgb(128 / 255f, 128 / 255f, 0);
        public static readonly ColorRgb OliveDrab = new ColorRgb(107 / 255f, 142 / 255f, 35 / 255f);
        public static readonly ColorRgb Orange = new ColorRgb(1, 165 / 255f, 0);
        public static readonly ColorRgb OrangeRed = new ColorRgb(1, 69 / 255f, 0);
        public static readonly ColorRgb Orchid = new ColorRgb(218 / 255f, 112 / 255f, 214 / 255f);
        public static readonly ColorRgb PaleGoldenrod = new ColorRgb(238 / 255f, 232 / 255f, 170 / 255f);
        public static readonly ColorRgb PaleGreen = new ColorRgb(152 / 255f, 251 / 255f, 152 / 255f);
        public static readonly ColorRgb PaleTurquoise = new ColorRgb(175 / 255f, 238 / 255f, 238 / 255f);
        public static readonly ColorRgb PaleVioletRed = new ColorRgb(219 / 255f, 112 / 255f, 147 / 255f);
        public static readonly ColorRgb PapayaWhip = new ColorRgb(1, 239 / 255f, 213 / 255f);
        public static readonly ColorRgb PeachPuff = new ColorRgb(1, 218 / 255f, 185 / 255f);
        public static readonly ColorRgb Peru = new ColorRgb(205 / 255f, 133 / 255f, 63 / 255f);
        public static readonly ColorRgb Pink = new ColorRgb(1, 192 / 255f, 203 / 255f);
        public static readonly ColorRgb Plum = new ColorRgb(221 / 255f, 160 / 255f, 221 / 255f);
        public static readonly ColorRgb PowderBlue = new ColorRgb(176 / 255f, 224 / 255f, 230 / 255f);
        public static readonly ColorRgb Purple = new ColorRgb(128 / 255f, 0, 128 / 255f);
        public static readonly ColorRgb Red = new ColorRgb(1, 0, 0);
        public static readonly ColorRgb RosyBrown = new ColorRgb(188 / 255f, 143 / 255f, 143 / 255f);
        public static readonly ColorRgb RoyalBlue = new ColorRgb(65 / 255f, 105 / 255f, 225 / 255f);
        public static readonly ColorRgb SaddleBrown = new ColorRgb(139 / 255f, 69 / 255f, 19 / 255f);
        public static readonly ColorRgb Salmon = new ColorRgb(250 / 255f, 128 / 255f, 114 / 255f);
        public static readonly ColorRgb SandyBrown = new ColorRgb(244 / 255f, 164 / 255f, 96 / 255f);
        public static readonly ColorRgb SeaGreen = new ColorRgb(46 / 255f, 139 / 255f, 87 / 255f);
        public static readonly ColorRgb SeaShell = new ColorRgb(1, 245 / 255f, 238 / 255f);
        public static readonly ColorRgb Sienna = new ColorRgb(160 / 255f, 82 / 255f, 45 / 255f);
        public static readonly ColorRgb Silver = new ColorRgb(192 / 255f, 192 / 255f, 192 / 255f);
        public static readonly ColorRgb SkyBlue = new ColorRgb(135 / 255f, 206 / 255f, 235 / 255f);
        public static readonly ColorRgb SlateBlue = new ColorRgb(106 / 255f, 90 / 255f, 205 / 255f);
        public static readonly ColorRgb SlateGray = new ColorRgb(112 / 255f, 128 / 255f, 144 / 255f);
        public static readonly ColorRgb Snow = new ColorRgb(1, 250 / 255f, 250 / 255f);
        public static readonly ColorRgb SpringGreen = new ColorRgb(0, 1, 127 / 255f);
        public static readonly ColorRgb SteelBlue = new ColorRgb(70 / 255f, 130 / 255f, 180 / 255f);
        public static readonly ColorRgb Tan = new ColorRgb(210 / 255f, 180 / 255f, 140 / 255f);
        public static readonly ColorRgb Teal = new ColorRgb(0, 128 / 255f, 128 / 255f);
        public static readonly ColorRgb Thistle = new ColorRgb(216 / 255f, 191 / 255f, 216 / 255f);
        public static readonly ColorRgb Tomato = new ColorRgb(1, 99 / 255f, 71 / 255f);
        public static readonly ColorRgb Turquoise = new ColorRgb(64 / 255f, 224 / 255f, 208 / 255f);
        public static readonly ColorRgb Violet = new ColorRgb(238 / 255f, 130 / 255f, 238 / 255f);
        public static readonly ColorRgb Wheat = new ColorRgb(245 / 255f, 222 / 255f, 179 / 255f);
        public static readonly ColorRgb White = new ColorRgb(1, 1, 1);
        public static readonly ColorRgb WhiteSmoke = new ColorRgb(245 / 255f, 245 / 255f, 245 / 255f);
        public static readonly ColorRgb Yellow = new ColorRgb(1, 1, 0);
        public static readonly ColorRgb YellowGreen = new ColorRgb(154 / 255f, 205 / 255f, 50 / 255f);

        private static readonly Dictionary<ColorRgba, string> names;
        #endregion

        #region Constructor
        static Colors()
        {
            names = new Dictionary<ColorRgba, string>();

            typeof(Colors).GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(field => !field.Name.StartsWith("Pure"))
                .ForEach(field => 
                    {
                        object value = field.GetValue(null);
                        ColorRgba color = value is ColorRgb ? (ColorRgb)value : (ColorRgba)value;
                        names[color] = field.Name;
                    });
        }
        #endregion

        #region Methods
        public static string GetName(ColorRgba color)
        {
            return names[color];
        }
        #endregion
    }
}
