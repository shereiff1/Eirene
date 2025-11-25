using DAL.Entities.Core;
using DAL.Repository.Abstraction.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository.Implementation.Core
{
    public class DoctorRepository : GenericRepository<Doctor>, IDoctorRepository
    {
    }
}
