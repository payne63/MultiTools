using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTools.Base
{
	public class ValidationItem
	{
		public enum SeverityValidEnum  { NoProblem , Low, Medium, High };
		private string _Description;
		public string Description
        {
			get { return _Description; }
			set { _Description = value; }
		}

		private string _ErrorDescription;
		public string ErrorDescription
        {
			get { return _ErrorDescription; }
			set { _ErrorDescription = value; }
		}


		public Func<DataI, SeverityValidEnum> CheckValidation;

        public ValidationItem(string description, string errorDescription, Func<DataI,SeverityValidEnum> checkValidation)
        {
            Description = description;
            ErrorDescription = errorDescription;
            CheckValidation = checkValidation;
        }
    }
}
