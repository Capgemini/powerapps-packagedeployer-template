using Capgemini.Xrm.Deployment.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capgemini.Xrm.Deployment.Extensions
{
    public static class ExceptionExtensions
    {
        public static void ThrowArgumentNullExceptionIfNull(this object input, string argumentName)
        {
            if (null == input)
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        public static void ThrowIfNull(this object input, string message = "Input is null!")
        {
            if (null == input)
            {
                throw new ArgumentNullException(message);
            }
        }
    }
}
