﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrdersBackgroundWorker.Models
{
    public class Order
    {
        public Guid OrderId { get; set; }
        public int Quantity { get; set; }
        public int AccountNumber { get; set; }
    }
}
