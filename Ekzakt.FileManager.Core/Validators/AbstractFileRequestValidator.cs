﻿using Ekzakt.FileManager.Core.Models.Requests;
using FluentValidation;

namespace Ekzakt.FileManager.Core.Validators;

internal sealed class AbstractFileRequestValidator : AbstractValidator<AbstractFileRequest>
{
    public AbstractFileRequestValidator()
    {
        RuleFor(x => x.BaseLocation)
            .NotNull()
            .NotEmpty();
    }
}
