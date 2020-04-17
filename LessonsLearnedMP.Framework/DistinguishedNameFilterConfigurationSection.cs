using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
namespace Suncor.LessonsLearnedMP.Framework
{
    public class DistinguishedNameFilterConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("filters", IsRequired = true)]
        [ConfigurationCollection(typeof(DistinguishedNameFilterConfigurationElement), AddItemName = "add", ClearItemsName = "clear", RemoveItemName = "remove")]
        public GenericConfigurationElementCollection<DistinguishedNameFilterConfigurationElement> Filters
        {
            get { return (GenericConfigurationElementCollection<DistinguishedNameFilterConfigurationElement>)this["filters"]; }
        }
    }
}