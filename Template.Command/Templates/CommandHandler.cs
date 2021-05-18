using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace $rootnamespace$
{
	public class $commandHandlerName$: IRequestHandler<$commandName$>
	{
		public $commandHandlerName$() { }

		public async Task<Unit> Handle($commandName$ request, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}
