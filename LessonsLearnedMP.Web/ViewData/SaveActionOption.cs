using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Suncor.LessonsLearnedMP.Framework;

namespace Suncor.LessonsLearnedMP.Web.ViewData
{
    public class SaveActionOption
    {
        public ButtonType ButtonType { get; set; }
        public Enumerations.SaveAction SaveAction { get; set; }
        public string IconClass { get; set; }
        public string ButtonText { get; set; }
        public string Url { get; set; }
        public int SortOrder { get; set; }

        public SaveActionOption()
        {
            ButtonType = ViewData.ButtonType.DropDownOption;
        }
    }

    public enum ButtonType
    {
        MainDropDown,
        SecondaryAction,
        DropDownOption
    }
}