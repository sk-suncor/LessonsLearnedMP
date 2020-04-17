using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
namespace Suncor.LessonsLearnedMP.Framework
{
    public class DistinguishedNameFilterConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("distinguishedName", IsRequired = true)]
        public string DistinguishedName
        {
            get
            {
                return (string)this["distinguishedName"];
            }
            set
            {
                this["distinguishedName"] = value;
            }
        }
    }
}