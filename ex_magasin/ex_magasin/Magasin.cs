using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ex_magasin {
    /// <summary>
    /// Représentation d'un magasin
    /// </summary>
    class Magasin : Control {
        //Constantes
        private const int FPS_INTERVAL = 8;
        private const int NB_CUSTOMER = 5;
        private const int V_CUSTOMER = 200;
        private const int SPACE_BETWEEN_CHECKOUT = 15;
        private const int TIME_MIN_TO_STAY = 5;
        private const int TIME_MAX_TO_STAY = 20;

        //Champs
        Bitmap bmp = null;
        Graphics g = null;
        Timer fps;
        Random rnd;
        List<Checkout> checkouts;

        /// <summary>
        /// Constructeur
        /// </summary>
        public Magasin() : base() {
            checkouts = new List<Checkout>();
            //Random
            rnd = new Random();
            //Timer
            fps = new Timer();
            fps.Interval = FPS_INTERVAL;
            fps.Enabled = true;
            fps.Tick += new EventHandler(OnTick);
            //Pour la fluidité de l'image
            DoubleBuffered = true;

            //Clients
            for (int i = 0; i < NB_CUSTOMER; i++) {
                Customer currentCustomer = new Customer(
                    //Position de départ centrée
                    new PointF(Properties.Settings.Default.SIZE_FORM.Width / 2, Properties.Settings.Default.SIZE_FORM.Height / 2),
                    //Vitesse aléatoire
                    new PointF(rnd.Next(-V_CUSTOMER, V_CUSTOMER), rnd.Next(-V_CUSTOMER, V_CUSTOMER)),
                    rnd.Next(TIME_MIN_TO_STAY, TIME_MAX_TO_STAY)
                );
                Paint += currentCustomer.Paint;
                currentCustomer.TimeEnded += c_TimeEnded;
            }
            //Caisses
            //Position Y des caisses
            int checkoutY = Properties.Settings.Default.SIZE_FORM.Height - Properties.Settings.Default.SIZE_CHECKOUT.Height;
            //Ajouter autant de caisse que l'espace disponible permet
            for (int i = 0; i < Properties.Settings.Default.SIZE_FORM.Width / Properties.Settings.Default.SIZE_CHECKOUT.Width; i++) {
                Checkout currentCheckout = new Checkout(
                    new PointF(
                        (Properties.Settings.Default.SIZE_CHECKOUT.Width * i) + (SPACE_BETWEEN_CHECKOUT * i), 
                        checkoutY
                    ),
                    //Ouvrir la 1ère caisse
                    i == 0
                );
                Paint += currentCheckout.Paint;
                checkouts.Add(currentCheckout);
            }
        }

        /// <summary>
        /// Évènement appelé lors du dessinage de la Scene
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e) {
            //Initialiser les variables si elles sont nulles (notation C# 8.0, https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-8#null-coalescing-assignment)
            bmp ??= new Bitmap(Size.Width, Size.Height);
            g ??= Graphics.FromImage(bmp);
            //Pour la netteté de l'image
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            PaintEventArgs p = new PaintEventArgs(g, e.ClipRectangle);

            //Nettoyer l'écran
            g.Clear(BackColor);
            //Appeler l'event de base
            base.OnPaint(p);
            //Dessiner la nouvelle image sur l'écran
            e.Graphics.DrawImage(bmp, new Point(0, 0));
        }

        /// <summary>
        /// Pour chaque Tick du timer
        /// </summary>
        protected void OnTick(object sender, EventArgs e) {
            Invalidate(true);
        }

        /// <summary>
        /// Event lorsque le client a fini ses courses
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void c_TimeEnded(object sender, EventArgs e) {
            //Récupérer le client
            Customer currentCustomer = sender as Customer;
            //Récupérer une caisse ouverte
            Checkout currentCheckout = checkouts.Find(x => x.IsOpen);
            //Faire aller le client vers la caisse ouverte
            currentCustomer.GoTo(currentCheckout);
        }
    }
}
