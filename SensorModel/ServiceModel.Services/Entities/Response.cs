using Repository.Mongo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorModel.Services.Entities
{
    /// <summary>
    ///  thingspeakden channel  json veriyi modele çevirmek için kullanılan classs'tır.
    ///  Response modeli entity türeyerek mongo db 'ye kaydedilen entity'dir.
    /// </summary>
    public class Response : Entity
    {
        public Channel channel { get; set; }
        public List<Feed> feeds { get; set; }
    }
}
