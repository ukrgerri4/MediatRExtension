using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace $rootnamespace$
{
	public class $queryHandlerName$: IRequestHandler<$queryName$, $queryViewModelName$>
	{
		public $queryHandlerName$() { }

		public async Task<$queryViewModelName$> Handle($queryName$ request, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}
