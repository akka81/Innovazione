using System;
using System.Runtime.Serialization;

namespace SkillNet.Services.Models
{
    [DataContract]
    public class TechSkill
    {
        [DataMember]
        public int ResourceId { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public Domain.Level Level { get; set; }
        [DataMember]
        public string Notes { get; set; }

    }
}
