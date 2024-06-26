﻿using FluentValidation.Results;

namespace TibiaStalker.Application.Exceptions;

public class TibiaValidationException : TibiaStalkerException
{
    public IEnumerable<ValidationFailure> Errors { get; private set; }

    public TibiaValidationException(string message) : base(message)
    {
    }
    public TibiaValidationException(params ValidationFailure[] errors) : base(BuildErrorMessage(errors))
    {
        Errors = errors;
    }

    private static string BuildErrorMessage(IEnumerable<ValidationFailure> errors)
    {
        return string.Join(",", errors.Select(x => x.ErrorMessage));
    }
}