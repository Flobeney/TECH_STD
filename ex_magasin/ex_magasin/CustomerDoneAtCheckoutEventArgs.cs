using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ex_magasin {
    class CustomerDoneAtCheckoutEventArgs : EventArgs {
        public Customer customer { get; set; }
        public CustomerDoneAtCheckoutEventArgs(Customer c) {
            customer = c;
        }
    }
}
