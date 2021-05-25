using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace $rootnamespace$
{
	public class $notificationHandlerName$: INotificationHandler<$notificationName$>
	{
		public $notificationHandlerName$() { }

		public async Task Handle($notificationName$ request, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}
