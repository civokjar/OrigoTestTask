﻿using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TestTask.API.Model;
using TestTask.API.Validation.Helpers;
using TestTask.Core.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web.Http.Description;

namespace TestTask.API.Controllers
{
    [EnableCors]
    [Route("Customers")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly ILogger<CustomersController> _logger;
        private readonly IMapper _mapper;
        private readonly IMediator _mediatr;

        public CustomersController(ILogger<CustomersController> logger, IMapper mapper, IMediator mediatr)
        {
            _mediatr = mediatr;
            _logger = logger;
            _mapper = mapper;
        }
        [HttpGet]
        [ResponseType(typeof(CustomerSearchResponse))]
        public async Task<ActionResult<CustomerSearchResponse>> Get()
        {
            await Task.Delay(2000);
            CustomerSearchResponse apiResponse = new CustomerSearchResponse();

            var coreResponse =  await _mediatr.Send(new GetCustomersQuery { }).ConfigureAwait(false);

            apiResponse.Customers = _mapper.Map<IEnumerable<Customer>>(coreResponse);
          
            return Ok(apiResponse);

        }
        [ResponseType(typeof(CreateCustomerResponse))]
        [HttpPost]
        public async Task<ActionResult<CreateCustomerResponse>> Create([FromBody] CreateCustomerRequest request)
        {

            CreateCustomerResponse apiResponse = new CreateCustomerResponse();
           
            if (!ModelState.IsValid) 
            {
                var errors = new List<string>();

                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                apiResponse.ErrorMessages = errors;

                var options = new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
         
                return new JsonResult(apiResponse, options) { StatusCode = (int)HttpStatusCode.BadRequest };
            }
           
            var command = _mapper.Map<CreateCustomerCommand>(request);
     
            var personnummerHelper = new  Personnummer(command.SocialSecurityNumber);
            command.SocialSecurityNumber = personnummerHelper.Format(longFormat : false);

            var coreCustomer = await _mediatr.Send(command).ConfigureAwait(false);

            if (coreCustomer == null) {

                apiResponse.ErrorMessages = new List<string>{"Unexpected error occured.."};
                HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return apiResponse;
            }
            var apiCustomer = _mapper.Map<Customer>(coreCustomer);
            apiResponse.Customer = apiCustomer;
            return Ok(apiResponse);
        }
    }
}
