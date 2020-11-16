using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Granny.Email.WebApp.Models.Predict
{
    public class PredictionResultViewModel
    {
        public double BodyPredictionResult { get; set; }
        public double HeaderPredictionResult { get; set; }
        public double SubjectPredictionResult { get; set; }
    }
}