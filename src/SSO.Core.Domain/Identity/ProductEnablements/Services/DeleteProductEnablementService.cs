using BAYSOFT.Abstractions.Core.Domain.Entities.Services;
using Microsoft.Extensions.Localization;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.ProductEnablements.Entity;
using SSO.Core.Domain.Identity.ProductEnablements.Validations.DomainValidations;
using SSO.Core.Domain.Identity.ProductEnablements.Validations.EntityValidations;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Domain.Identity.ProductEnablements.Services
{
	public sealed class DeleteProductEnablementServiceRequest : DomainServiceRequest<ProductEnablement>
	{
		public DeleteProductEnablementServiceRequest(ProductEnablement payload) : base(payload) { }
	}

	public sealed class DeleteProductEnablementServiceRequestHandler
		: DomainServiceRequestHandler<ProductEnablement, DeleteProductEnablementServiceRequest>
	{
		private IIdentityDbContextWriter Writer { get; set; }
		public DeleteProductEnablementServiceRequestHandler(
			IIdentityDbContextWriter writer,
			IStringLocalizer<ProductEnablement> localizer,
			ProductEnablementValidator entityValidator,
			DeleteProductEnablementSpecificationsValidator domainValidator)
			: base(localizer, entityValidator, domainValidator)
		{
			Writer = writer;
		}

		override public async Task<ProductEnablement> Handle(DeleteProductEnablementServiceRequest request, CancellationToken cancellationToken)
		{
			ValidateEntity(request.Payload);
			ValidateDomain(request.Payload);
			request.Payload.MarkDeleted();
			return request.Payload;
		}
	}
}
