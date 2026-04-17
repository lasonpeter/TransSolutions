using FluentValidation;

namespace TransSolutions.Shared.Contracts.Driver;

public class CreateDriverValidator : AbstractValidator<CreateDriverRequest>
{
    public CreateDriverValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.DrivingLicenseCategories).NotEmpty();
    }
}

public class UpdateDriverValidator : AbstractValidator<UpdateDriverRequest>
{
    public UpdateDriverValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.DrivingLicenseCategories).NotEmpty();
    }
}

public class DeleteDriverValidator : AbstractValidator<DeleteDriverRequest>
{
    public DeleteDriverValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class GetDriverValidator : AbstractValidator<GetDriverRequest>
{
    public GetDriverValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class GetDriversValidator : AbstractValidator<GetDriversRequest>
{
    public GetDriversValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
