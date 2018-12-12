using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Ubisoft.Interview.SessionFeedback.BL;

namespace Ubisoft.Interview.SessionFeedback.Controllers
{
    public class CreateUserFeedbackDTO
    {
        [Range(1, 5)]
        [JsonProperty("rate"), Required]
        public UserRate Rate { get; set; }
        [JsonProperty("comment"), Required]
        public string Comment { get; set; }
    }
}
