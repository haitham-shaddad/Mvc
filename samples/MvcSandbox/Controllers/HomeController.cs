// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MvcSandbox.Controllers
{
    public class HomeController : Controller
    {
        [ModelBinder]
        public string Id { get; set; }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Integer([FromForm(Name ="totalCount")]int count)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok(count);
        }

        public IActionResult ComplexType(Customer customer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok(customer);
        }

        public IActionResult Array(string[] tags)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok(tags);
        }

        public IActionResult List(IList<string> tags)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok(tags);
        }

        public IActionResult Dictionary(Dictionary<string, string> states)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok(states);
        }

        public IActionResult KeyValuePair(KeyValuePair<string, string> kvp)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok(kvp);
        }
    }

    public class Customer
    {
        public int Id { get; set; }

        [FromHeader]
        public string Name { get; set; }

        public Address Address { get; set; }

        public decimal DecimalProperty { get; set; }

        public double DoubleProperty { get; set; }

        public float FloatProperty { get; set; }

        public List<string> Aliases { get; set; }
    }

    public class Address
    {
        [FromHeader]
        public string AddressType { get; set; }

        public List<StateInfo> States { get; set; }
    }

    public class StateInfo
    {
        public string ShortName { get; set; }
        public string LongName { get; set; }
    }
}
