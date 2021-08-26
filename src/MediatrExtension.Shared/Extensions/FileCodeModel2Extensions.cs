using EnvDTE80;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MediatrExtension.Shared.Extensions
{
    public static class FileCodeModel2Extensions
    {
        public static FileCodeModel2 AddNotExistingImports(this FileCodeModel2 fileCodeModel, IEnumerable<string> importsToAdd)
        {
            if(null == fileCodeModel)
            {
                throw new ArgumentNullException(nameof(fileCodeModel));
            }

            var currentImports = fileCodeModel.CodeElements
                .OfType<CodeImport>()
                .Select(x => x.Namespace)
                .ToList();

            foreach (var import in importsToAdd)
            {
                if (!currentImports.Any(x => x == import))
                {
                    fileCodeModel.AddImport(import, 0);
                }
            }

            return fileCodeModel;
        }
    }
}
