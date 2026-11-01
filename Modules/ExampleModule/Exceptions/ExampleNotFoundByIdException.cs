using DotNetSkeleton.Shared.Exceptions;

namespace DotNetSkeleton.Modules.ExampleModule.Exceptions;

public sealed class ExampleNotFoundByIdException(int id)
    : AppException(
        StatusCodes.Status404NotFound,
        $"Không tìm thấy Example có Id = {id}");
