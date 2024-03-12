﻿using Ekzakt.FileManager.Core.Models.Requests;
using FluentValidation;

namespace Ekzakt.FileManager.Core.Validators;

public class ReadFileRequestValidator : AbstractValidator<ReadFileAsStringRequest>
{
    public ReadFileRequestValidator()
    {
        // TODO: Make this simpler!
        RuleFor(request => request.BaseLocation).NotEmpty().WithMessage("Container name cannot be empty.");
        RuleFor(request => request.FileName).NotEmpty().WithMessage("Blob name cannot be empty.");
    }
}