using Microsoft.AspNetCore.Mvc;
using Otp.Application.Contracts;
using Otp.Application.Services;

namespace Otp.Api.Controllers;

[ApiController]                 // auto-validates DTO attributes → 400 ProblemDetails
[Route("api/v1/otp")]
public sealed class OtpController(IOtpService otpService) : ControllerBase
{
    [HttpPost("generate")]
    public ActionResult<GenerateOtpResponse> Generate(GenerateOtpRequest request) =>
        Ok(otpService.Generate(request));

    [HttpPost("validate")]
    public ActionResult<ValidateOtpResponse> Validate(ValidateOtpRequest request) =>
        Ok(otpService.Validate(request));

    [HttpGet("{userId}/status")]
    public ActionResult<OtpStatusResponse> Status(string userId) =>
        Ok(otpService.GetStatus(userId));
}