using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Math;

namespace Orion.Graphics
{
    partial class GraphicsContext
    {
        /*TextPrinter printer = new TextPrinter();
        Font sans_serif = new Font(FontFamily.GenericSansSerif, 18.0f);

        public void DrawTextInView(string text, View view)
        {
            printer.Begin();
            GL.Translate(view.Frame.Origin.X, view.Frame.Origin.Y, 0);
            printer.Print(text, sans_serif, Color.Black);
            printer.End();
        }*/

        // ----------------------------------------------------------------------------------

        /*

        // Crée un TextPrinter à chaque opération de dessin.
        // Utilise la property 'Font' comme font à utiliser dans la méthode Print.
        public Font Font { get; set; }
        // Pour la couleur, utilise la property 'Color' définie quelque part dans GraphicsContext.cs.

        public void FillText(string text) { } // FillText(0,0, text)
        public void FillText(Vector2 position, string text) { } // FillText(position.X, position.Y, text);
        public void FillText(float x, float y, string text) { } // implémentation complète ici

        // supprime les commentaires d'instructions, et fais des tags de documentation
        //  (si possible à la mode de ceux des autres méthodes de la classe).

        // merci :)
         * 
         * */
    }
}