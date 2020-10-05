using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ex_magasin {
    /// <summary>
    /// Représentation d'une caisse
    /// </summary>
    class Checkout {
        //Champs
        PointF location;
        SizeF size;
        Brush initialColor;
        Brush currentCcolor;

        /// <summary>
        /// Constructeur désigné
        /// </summary>
        /// <param name="pLocation">Position</param>
        public Checkout(PointF pLocation) {
            location = pLocation;
            initialColor = Brushes.Red;
            currentCcolor = initialColor;
            size = Properties.Settings.Default.SIZE_CHECKOUT;
        }

        /// <summary>
        /// Dessin du sprite
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Paint(object sender, PaintEventArgs e) {
            e.Graphics.FillRectangle(currentCcolor, new RectangleF(location, size));
        }
    }
}
