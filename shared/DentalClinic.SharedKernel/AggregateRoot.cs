using System;
using System.Collections.Generic;
using System.Text;

namespace DentalClinic.SharedKernel
{
    public abstract class AggregateRoot : Entity
    {
        protected AggregateRoot(Guid id) : base(id) { }
    }
}
