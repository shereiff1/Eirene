using BLL.Services.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.Implementation
{
    public class GenericServices<T> : IGenericServices<T> where T : class
    {
    }
}
