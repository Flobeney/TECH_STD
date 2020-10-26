using System;

namespace ex_magasin {
    /// <summary>
    /// Arguments lorsque l'événement CustomerDoneAtCheckout est déclenché
    /// </summary>
    class CustomerDoneAtCheckoutEventArgs : EventArgs {
        //Propriétés
        public Customer customerArgs { get; set; }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="c">Client</param>
        public CustomerDoneAtCheckoutEventArgs(Customer c) {
            customerArgs = c;
        }
    }
}
