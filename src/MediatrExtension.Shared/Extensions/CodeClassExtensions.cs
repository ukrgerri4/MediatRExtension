using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediatrExtension.Shared.Extensions
{
    public static class CodeClassExtensions
    {
        public static CodeClass InsertInterfaceToTheEnd(this CodeClass codeClass, string interfaceValue)
        {
            if (null == codeClass) throw new ArgumentNullException(nameof(codeClass));

            var editPoint = codeClass.StartPoint.CreateEditPoint();
            editPoint.EndOfLine();
            editPoint.Insert(interfaceValue);
            return codeClass;
        }
    }
}
