using FluentValidation;

namespace TransSolutions.Shared.Contracts.RoadTrip;

public class CreateRoadTripValidator : AbstractValidator<CreateRoadTripRequest>
{
    public CreateRoadTripValidator()
    {
        RuleFor(x => x.CarId).NotEmpty();
        RuleFor(x => x.DeviceId).NotEmpty();
        RuleFor(x => x.Distance).GreaterThan(0);
        RuleFor(x => x.AverageFuelConsumption).GreaterThan(0);
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate).NotEmpty().GreaterThan(x => x.StartDate);
    }
}

public class GetRoadTripsByDeviceIdValidator : AbstractValidator<GetRoadTripsByDeviceIdRequest>
{
    public GetRoadTripsByDeviceIdValidator()
    {
        RuleFor(x => x.DeviceId).NotEmpty();
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
