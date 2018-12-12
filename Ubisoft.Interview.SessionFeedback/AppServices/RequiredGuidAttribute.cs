using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Ubisoft.Interview.SessionFeedback.AppServices
{
    public class RequiredGuidAttribute : RequiredAttribute

    {
        object _currentValue;
        public override bool IsValid(object value)
        {
            _currentValue = value;
            try
            {
                return Guid.TryParse(Convert.ToString(value),out Guid dummy);
            }
            catch
            {
                return false;
            }

        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} needs to be a valid GUID: {Convert.ToString(_currentValue)} is not valid.";
        }
    }
}

