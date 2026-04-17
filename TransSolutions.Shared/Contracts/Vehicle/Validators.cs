using FluentValidation;

namespace TransSolutions.Shared.Contracts.Vehicle;

public class CreateVehicleValidator : AbstractValidator<CreateVehicleRequest>
{
    public CreateVehicleValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MinimumLength(3).MaximumLength(256);
        RuleFor(x => x.VehicleType).NotEmpty();
        RuleFor(x => x.RegistrationPlateNumber).NotEmpty().MinimumLength(3).MaximumLength(16);
    }
}

public class UpdateVehicleValidator : AbstractValidator<UpdateVehicleRequest>
{
    public UpdateVehicleValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MinimumLength(3).MaximumLength(256);
        RuleFor(x => x.VehicleType).NotEmpty();
        RuleFor(x => x.RegistrationPlateNumber).NotEmpty().MinimumLength(3).MaximumLength(16);
    }
}

public class DeleteVehicleValidator : AbstractValidator<DeleteVehicleRequest>
{
    public DeleteVehicleValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class GetVehicleValidator : AbstractValidator<GetVehicleRequest>
{
    public GetVehicleValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class GetVehiclesValidator : AbstractValidator<GetVehiclesRequest>
{
    public GetVehiclesValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
