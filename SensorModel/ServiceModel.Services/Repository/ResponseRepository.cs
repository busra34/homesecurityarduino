using Repository.Mongo;
using SensorModel.Services.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorModel.Services.Repository
{
    /// <summary>
    /// generic repository olan response repository'dir.  ekleme çıkarma listeleme bu repository üezrinden olur.
    /// </summary>
    public class ResponseRepository : Repository<Response>
    {
        public ResponseRepository(string connectionString) : base(connectionString, "Response")
        {

        }
    }
}
