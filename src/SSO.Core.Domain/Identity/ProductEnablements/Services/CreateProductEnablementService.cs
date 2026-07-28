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
	public sealed class CreateProductEnablementServiceRequest : DomainServiceRequest<ProductEnablement>
	{
		public CreateProductEnablementServiceRequest(ProductEnablement payload) : base(payload) { }
	}

	public sealed class CreateProductEnablementServiceRequestHandler
		: DomainServiceRequestHandler<ProductEnablement, CreateProductEnablementServiceRequest>
	{
		private IIdentityDbContextWriter Writer { get; set; }
		public CreateProductEnablementServiceRequestHandler(
			IIdentityDbContextWriter writer,
			IStringLocalizer<ProductEnablement> localizer,
			ProductEnablementValidator entityValidator,
			CreateProductEnablementSpecificationsValidator domainValidator)
			: base(localizer, entityValidator, domainValidator)
		{
			Writer = writer;
		}

		override public async Task<ProductEnablement> Handle(CreateProductEnablementServiceRequest request, CancellationToken cancellationToken)
		{
			ValidateEntity(request.Payload);
			ValidateDomain(request.Payload);
			await Writer.AddAsync(request.Payload);
			return request.Payload;
		}
	}
}
