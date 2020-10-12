using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ex_magasin {
    /// <summary>
    /// Représentation d'une caisse
    /// </summary>
    class Checkout {
        //Constantes
        const int OFFEST_WAITING = 10;
        readonly Brush COLOR_CLOSE = Brushes.Red;
        readonly Brush COLOR_OPEN = Brushes.Green;

        //Champs
        private List<Customer> customersWaiting;

        //Propriétés
        public PointF Location { get; set; }
        public bool IsOpen { get; set; }
        public RectangleF RectToDraw { get; private set; }

        /// <summary>
        /// Constructeur désigné
        /// </summary>
        /// <param name="pLocation">Position</param>
        public Checkout(PointF pLocation, bool pIsOpen = false) {
            customersWaiting = new List<Customer>();
            Location = pLocation;
            IsOpen = pIsOpen;
            RectToDraw = new RectangleF(Location, Properties.Settings.Default.SIZE_CHECKOUT);
        }

        /// <summary>
        /// Ajouter un client à la file d'attente de la caisse
        /// </summary>
        /// <param name="customer">Le client</param>
        public void AddCustomer(Customer customer) {
            customer.GoTo(GetNextWaitingLocation(customersWaiting.Count), PointF.Empty);
            customersWaiting.Add(customer);
        }

        private PointF GetNextWaitingLocation(int place) {
            return new PointF(
                Location.X,
                Location.Y - OFFEST_WAITING - (place * (OFFEST_WAITING + Properties.Settings.Default.SIZE_CHECKOUT.Height))
            );
        }

        /// <summary>
        /// Dessin du sprite
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Paint(object sender, PaintEventArgs e) {
            e.Graphics.FillRectangle(IsOpen ? COLOR_OPEN : COLOR_CLOSE, RectToDraw);
        }
    }
}
